using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using WGiBeat.Managers;
//using WGiBeat.NetSystem;

namespace WGiBeat.Screens
{
    public class NetLobbyScreen : GameScreen
    {
        private string _mode = "";

        private Sprite _backgroundSprite;
        private Menu _netMainMenu;

        private string _host = "127.0.0.1";
        private TextEntry _textEntry;
        private bool _textEntryActive;

        private LobbyCursorPosition _cursorPosition;
        private string _textEntryDestination;

        public NetLobbyScreen(GameCore core) : base(core)
        {
            InitSprites();
            BuildMenu();
        }

        private void BuildMenu()
        {
            _netMainMenu = new Menu{Position = Core.Metrics["NetMainMenu",0], Width = 375};
            _netMainMenu.AddItem(new MenuItem{ItemText = "Server Address", ItemValue = 0});
            _netMainMenu.AddItem(new MenuItem{ItemText = "Start Server", ItemValue = 2});
            _netMainMenu.AddItem(new MenuItem { ItemText = "Join Server", ItemValue = 3 });
            _netMainMenu.AddItem(new MenuItem { ItemText = "Main Menu",ItemValue = 4});
            _textEntry = new TextEntry {Width = 375, Height = 400, Position = Core.Metrics["NetMainMenu", 1]};
            _textEntry.EntryComplete += TextEntryComplete;
            _textEntry.EntryCancelled += TextEntryCancelled;
        }

        private void TextEntryCancelled(object sender, EventArgs e)
        {
            _textEntryActive = false;
            _textEntry.Clear();
        }

        private void TextEntryComplete(object sender, EventArgs e)
        {
            switch (_textEntryDestination)
            {
                case "Server Address":
                    _host = _textEntry.EnteredText;
                    break;
            }
            _textEntryActive = false;
            _textEntry.Clear();
        }

        private void InitSprites()
        {
            _backgroundSprite = new Sprite
                                    {
                                        SpriteTexture = TextureManager.Textures("AllBackground"),
                                        Height = 600,
                                        Width = 800
                                    };

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _backgroundSprite.Draw(spriteBatch);
            switch (_cursorPosition)
            {
                case LobbyCursorPosition.MAIN_MENU:
                    DrawMenu(spriteBatch);
                    break;
                    case LobbyCursorPosition.CLIENT:
                                  TextureManager.DrawString(spriteBatch, "" + Core.Net.Client.GetClientConnectionStatus(), "DefaultFont",
                          new Vector2(50, 250), Color.Black, FontAlign.LEFT);
                    break;
                    case LobbyCursorPosition.SERVER:
                    var output = "";
                    foreach (string s in Core.Net.Server.GetServerConnections())
                    {
                        output += s + "\n";
                    }
                    TextureManager.DrawString(spriteBatch, "" + output, "DefaultFont",
                   new Vector2(50, 250), Color.Black, FontAlign.LEFT); 
                    break;
                    
            }
            if (_textEntryActive)
            {
                _textEntry.Draw(spriteBatch);
            }

        }

        private void DrawMenu(SpriteBatch spriteBatch)
        {
            _netMainMenu.GetByItemText("Server Address").ClearOptions();
            _netMainMenu.GetByItemText("Server Address").AddOption(_host,0);
            _netMainMenu.Draw(spriteBatch);
        }

        public override void PerformAction(InputAction inputAction)
        {
            if (_textEntryActive)
            {
                return;
            }
          switch (inputAction.Action)
          {
                  
              case "START":
                  if (_cursorPosition == LobbyCursorPosition.MAIN_MENU)
                  MenuOptionSelected((int) _netMainMenu.SelectedItem().ItemValue);
                  else if (_cursorPosition == LobbyCursorPosition.SERVER)
                  {
                      StartGame();
                  }
                  break;
              case "UP":
                  _netMainMenu.MoveSelected(-1);
                  break;
              case "DOWN":
                  _netMainMenu.MoveSelected(1);
                  break;
              case "BACK":
                  Core.Net.Disconnect();
                  _cursorPosition = LobbyCursorPosition.MAIN_MENU;
                  Core.ScreenTransition("MainMenu");
                  break;
          }
        }

        private void StartGame()
        {
            Core.Net.Server.BroadcastMessage(new NetMessage {MessageType = MessageType.LOBBY_START},null);
            Core.ScreenTransition("NewGame");
        }

        public override void PerformKey(Microsoft.Xna.Framework.Input.Keys key)
        {
            if (_textEntryActive)
            {
                _textEntry.PerformKey(key);
            }
        }
        private void MenuOptionSelected(int index)
        {
            switch (index)
            {
                case 0:
                    _textEntryDestination = "Server Address";
                    _textEntryActive = true;
                    break;
                case 1:
                    _textEntryDestination = "Port";
                    _textEntryActive = true;
                    break;
                case 2:
                    _cursorPosition = LobbyCursorPosition.SERVER;
                      Core.Net.StartServer();
                    break;
                case 3:
                    _cursorPosition = LobbyCursorPosition.CLIENT;
                    Core.Net.ClientConnect(_host,3334);
                    break;
                case 4:
                    Core.Net.Disconnect();
                    _cursorPosition = LobbyCursorPosition.MAIN_MENU;
                    Core.ScreenTransition("MainMenu");
                    break;
            }
 
        }

        public override void NetMessageReceived(NetMessage message)
        {
           switch (message.MessageType)
           {
               case MessageType.LOBBY_START:
                   Core.ScreenTransition("NewGame");
                   break;
           }
        }

    }
    enum LobbyCursorPosition
    {
        MAIN_MENU = 0,
        CLIENT = 1,
        SERVER = 2,
    }
}
