using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WindowsClientGUI
{
    public enum StatusCodes
    {
        CL_START,
        CL_RETRY,
        CL_CONNECTED,
        CL_SENDCMD,
        CL_CMDCOMPLETE,
        CL_WAITING,
        CL_DISCONNECTED,
        CL_CONNECTIONABORT,
        CL_ERROR,
        CL_SVRHANDLERR,

    }

    public enum AvailableCommands
    {
        NoOp,
        GimbalJog,
        StepperJog,
        ObjTracking,
        PathFollow,
        TestMode
    }

    public class ClientLogic
    {
        private StreamWriter m_Writer;
        private StreamReader m_Reader;
        private NetworkStream m_Stream;
        private TcpClient m_Server;
        private readonly string m_ClientVersion = "0.01";
        private int m_PortToUse;
        private readonly IPAddress m_IpToUse;
        private bool m_StopFlag;
        private bool m_Restart;

        public delegate void Messenger(string message);
        public delegate void Status(StatusCodes code);
        public delegate void GetInput(InputUtil config);
        public event Messenger ClientNotification;
        public event Status StatusUpdate;
        public event GetInput RequestInput;

        public ClientLogic(string ipToUse, int portToUse)
        {
            ClientNotification?.Invoke($"[{DateTime.Now}]: Client version {m_ClientVersion} created. Preparing to start.");
            m_PortToUse = portToUse;
            m_IpToUse = IPAddress.Parse(ipToUse);
            ClientNotification?.Invoke($"[{DateTime.Now}]: Client using IP {m_IpToUse}, Port {m_PortToUse}");
        }


        public void Start()
        {
            ClientNotification?.Invoke($"[{DateTime.Now}]: Starting client.");
            m_StopFlag = false;
            ClientNotification?.Invoke($"[{DateTime.Now}]: Setting stop flag to {m_StopFlag}");
            try
            {
                m_Server = new TcpClient();
                for (int i = 0; i < 4; i++)
                {
                    m_Server.Connect(m_IpToUse, m_PortToUse);
                    if (m_Server.Connected)
                    {
                        ClientNotification?.Invoke($"[{DateTime.Now}]: Connected to the server!");
                        StatusUpdate?.Invoke(StatusCodes.CL_CONNECTED);
                        break;
                    }

                    if (i < 2)
                    {
                        ClientNotification?.Invoke($"[{DateTime.Now}]: Failed to connect. Retrying in 3s...");
                        StatusUpdate?.Invoke(StatusCodes.CL_RETRY);
                        Thread.Sleep(TimeSpan.FromSeconds(3));
                    }
                    else
                    {
                        ClientNotification?.Invoke(
                            $"[{DateTime.Now}]: Maximum connection attempts (4) reached. Aborting connection.");
                        StatusUpdate?.Invoke(StatusCodes.CL_CONNECTIONABORT);
                        m_Server?.Close();
                        return;
                    }
                }

                ClientNotification?.Invoke($"\r\n[{DateTime.Now}] Main Thread: Preparing to start server handler. ");
                ServerHandler();
                StatusUpdate?.Invoke(StatusCodes.CL_START);
            }



            catch (Exception e)
            {
                ClientNotification?.Invoke(
                    $"[{DateTime.Now}] Error >> An exception occured during connection attempts. {e}");
                StatusUpdate?.Invoke(StatusCodes.CL_ERROR);
            }
        }


        public void ServerHandler()
        {
            ClientNotification?.Invoke($"\r\n[{DateTime.Now}] Handler: Starting server handler. ");

            m_Stream = m_Server.GetStream();
            m_Reader = new StreamReader(m_Stream, Encoding.ASCII);
            m_Writer = new StreamWriter(m_Stream, Encoding.ASCII);

            bool responseReceived = false;
            bool initialResponseReceived = false;

            try
            {
                while (!m_StopFlag || m_Server.Connected)
                {

                    if (initialResponseReceived)
                    {
                        InputUtil config = new InputUtil();
                        RequestInput?.Invoke(config);
                        ClientNotification?.Invoke($"[{DateTime.Now}] DEBUG >> Config object value is {config.Command}");

                        m_Writer.WriteLine(TranslateOption(config.Command));
                        m_Writer.Flush();
                        responseReceived = false;
                    }

                    while ((m_Server.Connected || !m_StopFlag) && !responseReceived)
                    {
                        if (m_Server.Available > 0)
                        {
                            string serverResponse = m_Reader.ReadLine();
                            ClientNotification?.Invoke($"\n[{DateTime.Now}] Server:  {serverResponse}");
                            responseReceived = true;
                            if (serverResponse.StartsWith("INITCONF"))
                            {
                                ClientNotification?.Invoke($"\n[{DateTime.Now}] Server: Initial command confirmation   {serverResponse}");

                                string initialMessage = $"INITCONF Hello server! I'm client version {m_ClientVersion}.";
                                m_Writer.WriteLine(initialMessage);
                                m_Writer.Flush();
                                initialResponseReceived = true;
                            }
                        }
                        else
                        {
                            ClientNotification?.Invoke($"\n[{DateTime.Now}] Awaiting response from server...");
                            StatusUpdate?.Invoke(StatusCodes.CL_WAITING);
                            Thread.Sleep(2000);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //ToDo Improve Handling
                StatusUpdate?.Invoke(StatusCodes.CL_SVRHANDLERR);
            }

        }

        private string TranslateOption(AvailableCommands command)
        {
            switch (command)
            {
                case AvailableCommands.NoOp:
                    return "NOOP";

                case AvailableCommands.GimbalJog:
                    return "GJ";

                case AvailableCommands.StepperJog:
                    return "SJ";

                case AvailableCommands.ObjTracking:
                    return "OBJT";

                case AvailableCommands.PathFollow:
                    return "PF";
                case AvailableCommands.TestMode:
                    return "TST";
                default:
                    return "NOOP";
            }
        }


        public void Restart()
        {
            m_StopFlag = true;
            ClientNotification?.Invoke($"\n[{DateTime.Now}] Restarting server...");
        }

        public void StopClient()
        {
            m_StopFlag = true;
            m_Server.Close();
        }
    }
}
