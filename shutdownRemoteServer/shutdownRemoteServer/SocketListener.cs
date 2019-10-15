using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace shutdownRemoteServer
{
    public static class SocketListener
    {
        private static Socket handler;
        private static Socket listener;

        public static void StartListening()
        {

            IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = null;
            foreach (var ip in iPHostEntry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    iPAddress = ip;
                    break;
                }
            }
            IPEndPoint localEndPoint = new IPEndPoint(iPAddress, 8111);

            listener = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void Accept()
        {

            while (true)
            {
                 handler = listener.Accept();
                
            }
        }

        public static byte[] recv()
        {
            byte[] bytes = new byte[1024];
            while ((handler == null || !handler.Connected));
            int byteRec = handler.Receive(bytes);
            return bytes;
        }

        public static void Sent(byte[] msg)
        {
            handler.Send(msg);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        public static string getServerIp()
        {
            return listener.LocalEndPoint.ToString();
        }

        public static void makeDiscovrable()
        {
            UdpClient discovrable = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 8112);
            string msg = "," + Dns.GetHostName();
            byte[] bytes = Encoding.ASCII.GetBytes(msg);
            while (true)
            {
                discovrable.Send(bytes, bytes.Length, ip);
                Thread.Sleep(1000);
            }
        }


        public static string getServerDetails()
        {
            string ip = getServerIp();
            ip = ip.Substring(0, ip.IndexOf(':'));
            string hostname = Dns.GetHostName();
            return ip + ',' + hostname;
        }
    }
}
