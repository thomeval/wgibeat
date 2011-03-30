using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lidgren.Network;
using WGiBeat.Managers;
using WGiBeat.NetSystem;

namespace WGiBeat.NetSystem
{
    public class NetManager : Manager
    {

        public NetServerManager Server;
        public NetClientManager Client;

        public BinaryFormatter Formatter;

        public event EventHandler<ObjectEventArgs> NetMessageReceived;

        public NetManager()
        {
            
            Formatter = new BinaryFormatter();
            Server = new NetServerManager {Parent = this};
            Client = new NetClientManager {Parent = this};
        }

        public bool NetplayActive
        {
            get
            {
                if (Server.IsActive)
                {
                    return true;
                }
                return Client.IsActive;
            }
  
        }


        public void StartServer()
        {
            Server.Host();   
        }
        public void ClientConnect(string host, int port)
        {
            Client.Connect(host,port);
        }
        public void Disconnect()
        {
            Server.StopHosting();
            Client.Disconnect();
        }
        public NetMessage ParseMessage(NetIncomingMessage im)
        {           
                // handle incoming message
            string text;
            switch (im.MessageType)
                {

                    case NetIncomingMessageType.ErrorMessage:
                        text = im.ReadString();
                        System.Diagnostics.Debug.WriteLine(text);
                        Log.AddMessage(text,LogLevel.ERROR);
                        break;
                    case NetIncomingMessageType.WarningMessage:
                        text = im.ReadString();
                        System.Diagnostics.Debug.WriteLine(text);
                        Log.AddMessage(text, LogLevel.WARN);
                        break;
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        text = im.ReadString();
                        System.Diagnostics.Debug.WriteLine(text);
                        Log.AddMessage(text, LogLevel.DEBUG);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        var status = (NetConnectionStatus)im.ReadByte();
                        string reason = im.ReadString();
                        System.Diagnostics.Debug.WriteLine(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason);
                        Server.ConnectionChanged(im.SenderConnection);
                        Server.UpdateConnectionsList();
                        break;
                    case NetIncomingMessageType.Data:
                        // incoming message from a client
                        byte[] data = im.ReadBytes(im.LengthBytes);
                        var ms = new MemoryStream(data);
                        var message = (NetMessage) Formatter.Deserialize(ms);

                        return message;
 
                    default:
                        throw new Exception("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);                      
            }
            return null;
        }


        public void ActOnMessage(NetMessage message)
        {
                    if (NetMessageReceived != null)
                    {
                        NetMessageReceived(this, new ObjectEventArgs { Object = message });
                    }
        }

        public void ListenForMessages()
        {
            Server.CheckForMessages();
            Client.CheckForMessages();
        }

        public void SendToPeers(NetMessage message)
        {
            if (Server.IsActive)
            {
                Server.BroadcastMessage(message, null);
            }
            if (Client.IsActive)
            {
                Client.SendMessage(message);
            }
        }
    }

    [Serializable]
    public class NetMessage
    {
        public MessageType MessageType
        { get; set; }

        public object MessageData
        {
            get; set;
        }
        public int PlayerID { get; set; }
          
    }

    public enum MessageType
    {
        PLAYER_SCORE_UPDATE,
        PLAYER_NOTEBAR_UPDATE,
        PLAYER_ACTION,
        PLAYER_NAME,
        PLAYER_PROFILE,
        PLAYER_NO_PROFILE,
        DISCONNECT,
        CHAT_MESSAGE,
        LOBBY_START,
        CURSOR_POSITION,
        PLAYER_OPTIONS
    }
}
