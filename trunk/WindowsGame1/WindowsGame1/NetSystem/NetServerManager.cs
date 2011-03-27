using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lidgren.Network;
using WGiBeat.Managers;

namespace WGiBeat.NetSystem
{
    public class NetServerManager
    {
        public NetManager Parent { get; set; }
        private NetServer _server;
        private readonly List<string> _connections;

        public bool IsActive
        {
            get; set;
        }
        public NetServerManager()
        {
            _connections = new List<string>();
            var config = new NetPeerConfiguration("WGiBeat") { MaximumConnections = 3, Port = 3334 };
            _server = new NetServer(config);
        }

        public void Host()
        {

            _server.Start();
            UpdateConnectionsList();
            IsActive = true;
        }

        public void StopHosting()
        {
            if (_server != null)
            {
                _server.Shutdown("Requested by user.");
                IsActive = false;
            }
        }

        //Use null to broadcast to every host?
        public void BroadcastMessage(NetMessage message, NetConnection source)
        {
            if (!IsActive)
            {
                return;
            }
            var connections = (from e in _server.Connections where e != source select e).ToList();

            var ms = new MemoryStream();
            Parent.Formatter.Serialize(ms, message);
            NetOutgoingMessage om = _server.CreateMessage();
            om.Write(ms.ToArray());
            _server.SendMessage(om, connections, NetDeliveryMethod.ReliableOrdered, 0);
            System.Diagnostics.Debug.WriteLine("Broadcasting '" + message.MessageData + "'");
        }

        public void UpdateConnectionsList()
        {
            _connections.Clear();
            foreach (NetConnection conn in _server.Connections)
            {
                string str = NetUtility.ToHexString(conn.RemoteUniqueIdentifier) + " from " + conn.RemoteEndpoint.ToString() + " [" + conn.Status + "]";
                _connections.Add(str);
            }

            if (_connections.Count == 0)
            {
                _connections.Add("No connections");
            }
        }
        
        public void CheckForMessages()
        {
            if (!IsActive)
            {
                return;
            }
            NetIncomingMessage im = _server.ReadMessage();

            while (im != null)
            {
                var netMessage = Parent.ParseMessage(im);
                if (netMessage == null)
                {
                   
                }
                else
                {
                    Parent.ActOnMessage(netMessage);
                    BroadcastMessage(netMessage, im.SenderConnection);
                }
                im = _server.ReadMessage();
            }
        }



        public List<string> GetServerConnections()
        {
            UpdateConnectionsList();
            return _connections;
        }

        public void ConnectionChanged(NetConnection connection)
        {
            if (connection.Status == NetConnectionStatus.Connected )
            {
                BroadcastMessage(new NetMessage{MessageData = connection.RemoteEndpoint.Address.ToString() + " connected successfully.",MessageType = MessageType.CHAT_MESSAGE}, null );
            }
        }
    }
}
