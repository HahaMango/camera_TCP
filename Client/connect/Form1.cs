using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace connect
{
    public partial class Form1 : Form
    {
        Byte[] bytes = new Byte[2048];
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Byte[] bytes = new Byte[1000000];
            try
            {
                //用对应的IP和端口初始化tcpclient
                TcpClient tcpclient = new TcpClient(textBox1.Text, Int32.Parse(textBox2.Text));
                NetworkStream ns = tcpclient.GetStream();
                //读取网络流的内容到数组里面
                ns.Read(bytes, 0, bytes.Length);
                //再利用内存流，将字节数组解码成图片
                MemoryStream ms1 = new MemoryStream(bytes);
                Bitmap bm = (Bitmap)Image.FromStream(ms1);
                pictureBox1.Image = bm;
                ms1.Close();
                ns.Close();
                tcpclient.Close();
            }
            catch
            {
                MessageBox.Show("连接出错，检查对方主机是否开启");
            }
        }
    }
}
