using System;
using System.IO;
using Lidgren.Network;
using WGiBeat.Managers;

namespace WGiBeat.NetSystem
{
    public class NetClientManager
    {

        public NetManager Parent { get; set; }
        private NetClient _client;

        public bool IsActive
        {
            get; set;
        }
        public NetClientManager()
        {
            var rnd = new Random();
            var config = new NetPeerConfiguration("WGiBeat") {Port = rnd.Next(3000,4000)};
            _client = new NetClient(config);
        }

        public void Connect(string host, int port)
        {
            _client.Start();
            _client.Connect(host, port);
            IsActive = true;
            SendMessage(new NetMessage{MessageData = "Client Hello", MessageType = MessageType.CHAT_MESSAGE, PlayerID = -1});
            
        }

        public void Disconnect()
        {
            if (_client != null)
            {
                _client.Disconnect("Requested by user");
                IsActive = false;
            }
        }

        public void SendMessage(NetMessage message)
        {

            //Inefficient?
            var ms = new MemoryStream();
            Parent.Formatter.Serialize(ms, message);
            NetOutgoingMessage om = _client.CreateMessage();
            om.Write(ms.ToArray());
            _client.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
            System.Diagnostics.Debug.WriteLine("Sending '" + message.MessageData + "'");
        }

        public NetConnectionStatus GetClientConnectionStatus()
        {
            if (_client == null)
            {
                return NetConnectionStatus.None;
            }
            return _client.ConnectionStatus;
            
        }

        public void CheckForMessages()
        {
            if (!IsActive)
            {
                return;
            }
            NetIncomingMessage im = _client.ReadMessage();

            while (im != null)
            {
                var netMessage = Parent.ParseMessage(im);
                if (netMessage == null)
                {

                }
                else
                {
                    
                    Parent.ActOnMessage(netMessage);
                    
                }
                im = _client.ReadMessage();
            }
        }
    }
}
