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

        private static byte[] data;

        private static Socket listener;

        public static bool IsDataAvail { get; set; }

        public static byte[] Data
        {
            get { return data; }
        }

        public static void StartListening()
        {

            IPHostEntry iPHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress iPAddress = iPHostEntry.AddressList[1];
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
            while (handler == null || !handler.Connected) ;
            int byteRec = handler.Receive(bytes);
            return bytes;
        }

        public static void Sent(byte[] msg)
        {
            handler.Send(msg);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}
