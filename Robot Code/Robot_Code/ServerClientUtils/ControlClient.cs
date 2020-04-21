using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UtilityClasses;

namespace ServerClientUtils
{
    public class ControlClient
    {
        private StreamWriter m_Writer;
        private StreamReader m_Reader;
        private NetworkStream m_Stream;
        private TcpClient m_Server;
        private Thread m_ListenerThread;
        private readonly string m_ClientVersion = "0.01";
        private int m_PortToUse;
        private readonly IPAddress m_IpToUse;
        private bool m_StopFlag;

        public delegate void Messenger(string message);
        public delegate void GetInput(Configuration config);
        public event Messenger ClientNotification;
        public event GetInput RequestInput;

        public ControlClient(string ipToUse, int portToUse)
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
                        break;
                    }

                    if (i < 2)
                    {
                        ClientNotification?.Invoke($"[{DateTime.Now}]: Failed to connect. Retrying in 3s...");
                        Thread.Sleep(TimeSpan.FromSeconds(3));
                    }
                    else
                    {
                        ClientNotification?.Invoke(
                            $"[{DateTime.Now}]: Maximum connection attempts (4) reached. Aborting connection.");
                        m_Server?.Close();
                        return;
                    }
                }

                ClientNotification?.Invoke($"\r\n[{DateTime.Now}] Main Thread: Preparing to start server handler. ");
                ServerHandler();
                //m_ListenerThread = new Thread(ServerHandler);
                //m_ListenerThread.Name = "Server handler thread";
                //m_ListenerThread.Start();
            }
            catch (Exception e)
            {
                ClientNotification?.Invoke(
                    $"[{DateTime.Now}] Error >> An exception occured during connection attempts. {e}");
            }
        }


        public void ServerHandler()
        {
            ClientNotification?.Invoke($"\r\n[{DateTime.Now}] Handler: Starting server handler. ");

            m_Stream = m_Server.GetStream();
            m_Reader = new StreamReader(m_Stream, Encoding.ASCII);
            m_Writer = new StreamWriter(m_Stream, Encoding.ASCII);
            bool responseReceived = false;

            try
            {
                while ((m_Server.Connected || !m_StopFlag) && !responseReceived)
                {
                    if (m_Server.Available > 0)
                    {
                        string serverResponse = m_Reader.ReadLine();
                        ClientNotification?.Invoke($"\n[{DateTime.Now}] Server:  {serverResponse}");
                        responseReceived = true;
                    }
                    else
                    {
                        ClientNotification?.Invoke($"\n[{DateTime.Now}] : Awaiting response from server");
                        Thread.Sleep(2000);
                    }
                }
            }
            catch (Exception e)
            {
                ClientNotification?.Invoke($"[{DateTime.Now}] Error >> Exception occured during communication.");
            }

            ClientNotification?.Invoke($"[{DateTime.Now}] Initial response received. Switching to dialog mode.");

            string initialMessage = $"Hello server! I'm client version {m_ClientVersion}.";
            m_Writer.WriteLine(initialMessage);
            m_Writer.Flush();


            for (int i = 0; i < 3; i++)
            {
                if (m_Server.Connected && m_Server.Available > 0)
                {
                    string serverResponse = m_Reader.ReadLine();
                    ClientNotification?.Invoke($"\n[{DateTime.Now}] Server: Initial command confirmation   {serverResponse}");
                    break;
                }
                else if (i < 3)
                {
                    ClientNotification?.Invoke($"\n[{DateTime.Now}] Failed to receive initial command confirmation. Trying again in 3 sec. ");
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                }
                else
                {
                    ClientNotification?.Invoke($"\n[{DateTime.Now}] Maximum retries reached. \n Terminating connection as it is unstable!");
                    Thread.Sleep(2000);
                }
            }

            while (!m_StopFlag || m_Server.Connected)
            {
                Configuration config = new Configuration();
                RequestInput?.Invoke(config);
                ClientNotification?.Invoke($"[{DateTime.Now}] DEBUG >> Config object value is {config.Debug}");

                m_Writer.WriteLine(config.Debug);
                m_Writer.Flush();
                responseReceived = false;

                while ((m_Server.Connected || !m_StopFlag) && !responseReceived)
                {
                    if (m_Server.Available > 0)
                    {
                        string serverResponse = m_Reader.ReadLine();
                        ClientNotification?.Invoke($"\n[{DateTime.Now}] Server:  {serverResponse}");
                        responseReceived = true;
                    }
                    else
                    {
                        ClientNotification?.Invoke($"\n[{DateTime.Now}] DIA Mode: Awaiting response from server");
                        Thread.Sleep(2000);
                    }
                }
            }
        }

        public void StopClient()
        {
            m_StopFlag = true;
            m_Server.Close();

        }
    }
}
