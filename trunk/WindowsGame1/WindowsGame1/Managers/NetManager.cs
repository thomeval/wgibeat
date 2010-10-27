using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Net;
using Microsoft.Xna.Framework.Net;

namespace WGiBeat.Managers
{
    public class NetManager : Manager
    {
        public void Connect(string address)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Rdm, ProtocolType.IP);
            socket.Listen(1024);
            Microsoft.Xna.Framework.Net.PacketWriter pw;
            pw = new PacketWriter();
            // http://www.switchonthecode.com/tutorials/csharp-tutorial-simple-threaded-tcp-server
            TcpListener tcpl = new TcpListener(IPAddress.Any, 3334);
            tcpl.Start();
        }
    }
}
