using System;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.NetSystem
{
    public class NetHelper
    {
        private static NetHelper _instance;

        public static NetManager NetManager { get; set; }
        public static GameCore Core { get; set; }
        public static NetHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NetHelper();
                }
                return _instance;
            }
        }
        private NetHelper()
        {
        }

        public void BroadcastProfileChange(int number)
        {
            if (NetManager.NetplayActive)
            {
                var type = Core.Players[number].Profile == null
                               ? MessageType.PLAYER_NO_PROFILE
                               : MessageType.PLAYER_PROFILE;
                var message = new NetMessage
                {
                    PlayerID = number,
                    MessageData = Core.Players[number].Profile,
                    MessageType = type
                };
                Core.Net.SendToPeers(message);
            }
        }

        public void BroadcastCursorPosition(int number, object cursorPosition)
        {
            if (Core.Net.NetplayActive)
            {
                var message = new NetMessage
                {
                    PlayerID = number,
                    MessageData = cursorPosition,
                    MessageType = MessageType.CURSOR_POSITION
                };
                Core.Net.SendToPeers(message);
            }
        }

        public void BroadcastPlayerOptions(int number)
        {
            if (Core.Net.NetplayActive)
            {
                var message = new NetMessage
                                  {
                                      PlayerID = number,
                                      MessageData = Core.Players[number].PlayerOptions,
                                      MessageType = MessageType.PLAYER_OPTIONS
                                  };
                Core.Net.SendToPeers(message);
            }
        }

        public void BroadcastScreenTransition(string screen)
        {
            if (Core.Net.NetplayActive)
            {
                var message = new NetMessage
                {
                    PlayerID = 0,
                    MessageData = screen,
                    MessageType = MessageType.SCREEN_TRANSITION
                };
                Core.Net.SendToPeers(message);
            }
        }

        public void BroadcastAction(InputAction action)
        {
            if (Core.Net.NetplayActive)
            {
                var message = new NetMessage
                                  {
                                      PlayerID = action.Player,
                                      MessageData = action,
                                      MessageType = MessageType.PLAYER_ACTION
                                  };
                Core.Net.SendToPeers(message);
            }
        }
        public void BroadcastActionReleased(InputAction action)
        {
            if (Core.Net.NetplayActive)
            {
                var message = new NetMessage
                {
                    PlayerID = action.Player,
                    MessageData = action,
                    MessageType = MessageType.PLAYER_ACTION_RELEASED
                };
                Core.Net.SendToPeers(message);
            }
        }
    }
}
