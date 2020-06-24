using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsClientGUI
{
    public partial class Form1 : Form
    {

        private ClientLogic m_Client;
        private bool m_ClientStarted;
        private bool m_CommandInProgress;
        private bool m_ClientConnected;
        private readonly string m_ServerIP = "192.168.12.133";
        private readonly int m_PortToUse = 4200;
        private InputUtil m_currentCommand;
        private Thread m_ClientThread;
        public Form1()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            m_Client = new ClientLogic(m_ServerIP, m_PortToUse);
            m_Client.StatusUpdate += HandleClientEvents;
            m_currentCommand = new InputUtil();
            m_currentCommand.Command = AvailableCommands.NoOp;
            m_Client.RequestInput += GetNextCommand;
        }

        private void GetNextCommand(InputUtil cmd)
        {
            m_currentCommand.Command = AvailableCommands.NoOp;
            Lb_CommandSent.BackColor = Color.LightBlue;

            while (m_currentCommand.Command == AvailableCommands.NoOp)
            {
                if (Lb_AwaitingResponse.BackColor == Color.DodgerBlue)
                {
                    Lb_AwaitingResponse.BackColor = Color.DeepSkyBlue;
                }
                else
                {
                    Lb_AwaitingResponse.BackColor = Color.DodgerBlue;
                }
            }

            cmd.Command = m_currentCommand.Command;
        }

        private void HandleClientEvents(StatusCodes code)
        {
            switch (code)
            {
                case StatusCodes.CL_START:
                    Lb_ClientStarted.BackColor = Color.LightGreen;
                    break;
                case StatusCodes.CL_RETRY:
                    Lb_ConnectionRetry.BackColor = Color.Orange;
                    break;
                case StatusCodes.CL_CONNECTED:
                    Lb_ClientConnected.BackColor = Color.LightGreen;
                    break;
                case StatusCodes.CL_SENDCMD:
                    Lb_CommandSent.BackColor = Color.Gold;
                    break;
                case StatusCodes.CL_DISCONNECTED:
                    Lb_ClientConnected.BackColor = Color.LightCoral;
                    break;
                case StatusCodes.CL_CONNECTIONABORT:
                    Lb_ClientConnected.BackColor = Color.DarkRed;
                    break;
                case StatusCodes.CL_ERROR:
                    Lb_ClientError.BackColor = Color.LightCoral;
                    Lb_ClientError.ForeColor = Color.White;
                    break;
                case StatusCodes.CL_SVRHANDLERR:
                    Lb_ClientError.BackColor = Color.DarkOrange;
                    break;
                
                case StatusCodes.CL_WAITING:
                    if (Lb_AwaitingResponse.BackColor == Color.Gold)
                    {
                        Lb_AwaitingResponse.BackColor = Color.Orange;
                    }
                    else
                    {
                        Lb_AwaitingResponse.BackColor = Color.Gold;
                    }
                    break;
                case StatusCodes.CL_CMDCOMPLETE:
                    break;
                default:
                    Lb_ClientConnected.BackColor = Color.Red;
                    Lb_ClientError.BackColor = Color.Red;
                    Lb_ClientStarted.BackColor = Color.Red;
                    Lb_CommandSent.BackColor = Color.Red;
                    Lb_ConnectionRetry.BackColor = Color.Red;
                    Lb_AwaitingResponse.BackColor = Color.Red;
                    Lb_AbortedConnection.BackColor = Color.Red;
                    throw new ArgumentOutOfRangeException(nameof(code), code, null);
            }
        }

        private void Btn_Connect_Click(object sender, EventArgs e)
        {
            m_ClientThread = new Thread(m_Client.Start);
            m_ClientThread.Start();
        }

        private void Btn_GimbalJog_Click(object sender, EventArgs e)
        {
            m_currentCommand.Command = AvailableCommands.GimbalJog;
        }

        private void Btn_StepperJog_Click(object sender, EventArgs e)
        {
            m_currentCommand.Command = AvailableCommands.StepperJog;
        }

        private void Btn_ObjTracking_Click(object sender, EventArgs e)
        {
            m_currentCommand.Command = AvailableCommands.ObjTracking;
        }

        private void Btn_PathFollow_Click(object sender, EventArgs e)
        {
            m_currentCommand.Command = AvailableCommands.PathFollow;
        }

        private void Btn_TestMode_Click(object sender, EventArgs e)
        {
            m_currentCommand.Command = AvailableCommands.TestMode;
        }

    }
}
