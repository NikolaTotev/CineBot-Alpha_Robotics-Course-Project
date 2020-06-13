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

namespace CameraStreamTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        private MPlayer Player;
        private StreamWriter PlayerInput;

       

        private void Stop_Clicked(object sender, EventArgs e)
        {
            Player.End();

        }

        private void Base_Load(object sender, EventArgs e)
        {
            Player = new MPlayer((int)VideoPanel.Handle);
            Player.Start();
        }
    }
}
