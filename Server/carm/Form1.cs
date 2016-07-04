using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace carm
{
    public partial class Form1 : Form
    {
        network nt;
        Thread tr;
        public Form1()
        {
            InitializeComponent();
            //添加本地计算机所以的IP地址到列表框里
            for(int i =0;i<HIP.ip();i++)
            {
                comboBox1.Items.Add(HIP.a[i]);
            }
        }

        //关闭软件窗口发生的事件
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //调用nt.stop抛出异常，即可停止无限循环的线程
            try
            {
                nt.stop();
            }
            catch
            {//不作任何异常处理，直接关闭软件
            }
         
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (label1.Text == comboBox1.Items[comboBox1.SelectedIndex].ToString() + "已开启")
            {
                MessageBox.Show("请先关闭监控");
            }else
            {
                //用选中的IP地址作为监控端口
                nt = new network(comboBox1.Items[comboBox1.SelectedIndex].ToString(), 4000);
                tr = new Thread(new ThreadStart(nt.start));
                tr.Start();
                label1.Text = comboBox1.Items[comboBox1.SelectedIndex].ToString() + "已开启";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //调用nt.stop抛出异常，即可停止无限循环的线程
            try
            {
                nt.stop();
                label1.Text = comboBox1.Items[comboBox1.SelectedIndex].ToString() + "已关闭";
            }
           catch
            {
                MessageBox.Show("是否没有开启监控，或者没有选中需要关闭的IP地址");
            }
        }
    }

    //摄像机类
    public class carmem
    {
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        public int selectedDeviceIndex = 0;
        public string errorinfo;
        public Bitmap bmp;

        public FilterInfoCollection GetDevices()
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count != 0)
                {
                    errorinfo = "连接驱动成功";
                    return videoDevices;
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                errorinfo = "连接失败"+e.ToString();
                return null;
            }
        }

        //连接摄像机
        public VideoCaptureDevice VideoConnect(int deviceIndex = 0, int resoutionIndex = 0)
        {
            if (videoDevices.Count <= 0)
                return null;
            selectedDeviceIndex = deviceIndex;
            videoSource = new VideoCaptureDevice(videoDevices[deviceIndex].MonikerString);
            videoSource.VideoResolution = videoSource.VideoCapabilities[0];
            return videoSource;
        }
        //获取摄像头的截图
        public void GrabBitmap()
        {  
            if (videoSource == null)  
                return;
            videoSource.Start();
            videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);  
        }
        //刷新帧将触发该事件
    void videoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            bmp = (Bitmap)eventArgs.Frame.Clone();
            videoSource.NewFrame -= new NewFrameEventHandler(videoSource_NewFrame);
        }
        //关闭摄像机
        public void stopdown()
        {
            videoSource.SignalToStop();
        }
    }
}
//网络类
class network
{
    private carm.carmem ca = new carm.carmem();
    private IPAddress localAdder;
    private TcpListener tcplistener;

    //用ip地址和端口号初始化tcplistener类
    public network(string ip,Int32 port)
    {
        localAdder = IPAddress.Parse(ip);
        tcplistener = new TcpListener(localAdder, port);
    }

    public void start()
    {
        tcplistener.Start();
        while (true)
        {
            try
            {
                TcpClient tcpclient = tcplistener.AcceptTcpClient();
                ca.GetDevices();
                ca.VideoConnect();
                ca.GrabBitmap();
                //睡眠3秒，确保图片已经缓存
                Thread.Sleep(3000);
                NetworkStream ns = tcpclient.GetStream();
                MemoryStream ms = new MemoryStream();
                //将图片保存到内存流中，目的是下面可以对图片转换成字节。
                ca.bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                //转换成字节数组
                Byte[] bytes = ms.GetBuffer();
                //把数组写入到网络流
                ns.Write(bytes, 0, bytes.Length);
                ca.stopdown();
                ms.Close();
                ns.Close();
                tcpclient.Close();
            }
            catch      //利用关闭监听引发异常从而退出循环
            {
                break;
            }
        }

    }

    public void stop() //关闭监听
    {
        try
        {
            //利用关闭tcplistener.stop方法来在运行期间抛出异常，即可关闭上面无限循环的线程。
            tcplistener.Stop();
            tcplistener = null;
        }
        catch   //不作任何处理
        {
        }
    }
}


//获取本机ip
static class HIP
{
    public static string[] a = new string[10];

    public static int ip()
    {
        string s = Dns.GetHostName();
        IPHostEntry ipe = Dns.GetHostByName(s);
        int jis = 0;
        while(jis<ipe.AddressList.Length)
        {
            a[jis] = ipe.AddressList[jis].ToString();
            jis++;
        }
        return ipe.AddressList.Length;
    }
}