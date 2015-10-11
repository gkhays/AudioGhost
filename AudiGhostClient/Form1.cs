using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace AudiGhost
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Int32 port = Convert.ToInt32(tbPort.Text);
                string hostName = tbHost.Text;
                TcpClient tcpClient = new TcpClient(hostName, port);
                Byte[] data = Encoding.UTF8.GetBytes(tbMessage.Text);
                NetworkStream stream = tcpClient.GetStream();
                stream.Write(data, 0, data.Length);
                Console.WriteLine("Sent: {0}", tbMessage.Text);
                stream.Close();
                tcpClient.Close();
                tbMessage.Clear();
                button1.Enabled = false;
            }
            catch (SocketException se)
            {
                button1.Enabled = false;
                tbMessage.AppendText(string.Format("\n\n{0}", se.Message));
                Console.WriteLine("SocketException: {0}", se);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tbMessage.Enabled = false;
            button1.Enabled = false;
        }

        private void tbHost_TextChanged(object sender, EventArgs e)
        {
            tbMessage.Enabled = true;
        }

        private void tbMessage_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }
    }
}
