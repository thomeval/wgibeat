using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Threading;
using Microsoft.Xna.Framework.Net;

namespace WGiBeat.Managers
{
    public class NetManager : Manager
    {
        public Thread ListenThread;
        private TcpListener listener;
        private GameCore _core;

        public NetManager(GameCore core)
        {
            _core = core;
        }
        public void Host()
        {
        
            // http://www.switchonthecode.com/tutorials/csharp-tutorial-simple-threaded-tcp-server
            listener = new TcpListener(IPAddress.Any, 3334);
            ListenThread = new Thread(ListenForClients);
            ListenThread.Start();
        }

        private void ListenForClients()
        {
            listener.Start();
            
            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = listener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(HandleClientComm);
                clientThread.Start(client);
            }
        }


        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                
            }

            tcpClient.Close();
        }

    }

    [Serializable]
    class NetMessage
    {
        public MessageType MessageType
        { get; set; }
        public string MessageData
        {
            get; set;
        }
        public int PlayerID { get; set; }
          
    }

    public enum MessageType
    {
        PLAYER_SCORE_UPDATE,
        PLAYER_NAME,
        DISCONNECT,
        CHAT_MESSAGE,
    }
}
