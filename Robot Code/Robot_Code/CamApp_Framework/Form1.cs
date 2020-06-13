using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamApp_Framework
{
    public partial class Form1 : Form
    {
        private MPlayer Player;
        private StreamWriter PlayerInput;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Player = new MPlayer((int)VideoPanel.Handle);
            Player.Start();
        }

        private void Stop_Clicked(object sender, EventArgs e)
        {
            Player.End();
        }
    }
}
