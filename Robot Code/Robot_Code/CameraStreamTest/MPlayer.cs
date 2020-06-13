using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CameraStreamTest
{
    class MPlayer
    {
        private Process m_player;

        public MPlayer(int panelID)
        {
            m_player=new Process();
            m_player.StartInfo.UseShellExecute = false;
            m_player.StartInfo.RedirectStandardInput = true;
            m_player.StartInfo.FileName = "VideoStreamRecv.bash";
            m_player.StartInfo.Arguments = panelID.ToString();
        }

        public void Start()
        {
            m_player.Start();
        }

        public void End()
        {
            m_player.Kill();
            Process.Start("pkill mplayer");
        }
    }
}
