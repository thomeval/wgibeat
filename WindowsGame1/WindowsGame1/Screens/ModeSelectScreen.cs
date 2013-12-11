using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using WGiBeat.Players;

namespace WGiBeat.Screens
{
    public class ModeSelectScreen : GameScreen
    {
        private int _selectedGameType;
        private SpriteMap3D _optionBaseSpriteMap;
        private SpriteMap3D _optionsSpriteMap;
        private SpriteMap3D _edgeSpriteMap;
        private SpriteMap3D _arrowSpriteMap;
        private SpriteMap3D _previewsSpriteMap;
        private Sprite3D _background;
        private Sprite3D _headerSprite;
        private Sprite3D _descriptionBaseSprite;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();
        private Sprite3D _restrictionSprite;
        private PlayerOptionsSet _playerOptionsSet;
        private Sprite3D _messageBorderSprite;
        private double _listDrawOffset;

        private bool _selectingCPUSkill;
        private int _selectedCPUSkill;

        public ModeSelectScreen(GameCore core)
            : base(core)
        {
        }

        #region Initialization

        public override void Initialize()
        {
            InitSprites();
            _selectingCPUSkill = false;
            Core.Cookies["CurrentGameType"] = GameType.NORMAL;
            RemoveCPUPlayers();
            ResetTeams();
            _playerOptionsSet = new PlayerOptionsSet { Players = Core.Players, Positions = Core.Metrics["PlayerOptionsFrame"], Size = Core.Metrics["PlayerOptionsFrame.Size",0], 
                DrawAttract = true, StackableFrames = true };
            _playerOptionsSet.CreatePlayerOptionsFrames();
   
            base.Initialize();
        }

        private void ResetTeams()
        {
            for (int x = 0; x < 4; x++)
            {
                Core.Players[x].Team = 0;
            }
        }


        private void InitSprites()
        {
            _background = new Sprite3D
            {
                Size = Core.Metrics["ScreenBackground.Size", 0],
                Position = Core.Metrics["ScreenBackground", 0],
                Texture = TextureManager.Textures("AllBackground"),
            };

            _headerSprite = new Sprite3D
                                {
                                    Texture = TextureManager.Textures("ModeSelectHeader"),
                                    Position = (Core.Metrics["ScreenHeader", 0]),
                                    Size = Core.Metrics["ScreenHeader.Size",0]
                                };

            _optionBaseSpriteMap = new SpriteMap3D
            {
                Columns = 2,
                Rows = 1,
                Texture = TextureManager.Textures("ModeOptionBase")
            };

            _optionsSpriteMap = new SpriteMap3D
            {
                Columns = 1,
                Rows = (int)GameType.COUNT,
                Texture = TextureManager.Textures("ModeOptions")
            };

            _previewsSpriteMap = new SpriteMap3D
                                     {
                                         Columns = 1,
                                         Rows = (int) GameType.COUNT,
                                         Texture = TextureManager.Textures("ModeDescriptionPreviews")
                                     };

            _edgeSpriteMap = new SpriteMap3D { Columns = 2, Rows = 1, Texture = TextureManager.Textures("ModeSelectEdge") };
            _arrowSpriteMap = new SpriteMap3D { Columns = 4, Rows = 1, Texture = TextureManager.Textures("IndicatorArrows") };

            _descriptionBaseSprite = new Sprite3D
                                         {
                                             Texture = TextureManager.Textures("ModeDescriptionBase"),
                                             Position = (Core.Metrics["ModeDescriptionBase", 0]),
                                             Size = (Core.Metrics["ModeDescriptionBase.Size", 0])
                                         };
            _messageBorderSprite = new Sprite3D
                                       {
                                           Texture = TextureManager.Textures("MessageBorder"),
                                           Position = (Core.Metrics["ModeSelectMessageBorder", 0]),
                                           Size = (Core.Metrics["ModeSelectMessageBorder.Size", 0])
                                       };
            _restrictionSprite = new Sprite3D { Texture = TextureManager.Textures("RestrictionIcon"), Width = 48, Height = 48 };
            _restrictionSprite.X = _messageBorderSprite.X + 7;
            _restrictionSprite.Y = _messageBorderSprite.Y + 7;
            _textPosition = _messageBorderSprite.Position.Clone();
            _textPosition.X += 60;
            _textPosition.Y += 25;
        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(gameTime);

            DrawPlayerOptions();

            _headerSprite.Draw();

            DrawModeOptions();
            DrawModeDescription();
            DrawRestriction();

            if (_selectingCPUSkill)
            {
                DrawVSCPUDifficultySelect();
            }
            _edgeSpriteMap.Draw( 0, 20, 160, Core.Metrics["ModeSelectEdge", 0]);
            _edgeSpriteMap.Draw( 1, 20, 160, Core.Metrics["ModeSelectEdge", 1]);
        }

        private void DrawModeDescription()
        {
            _descriptionBaseSprite.Draw();
            var gameType = (GameType)_selectedGameType;
            FontManager.DrawString(GetModeDescription(gameType), "DefaultFont", Core.Metrics["ModeDescription", 0], Color.Black, FontAlign.Left);
            _previewsSpriteMap.Draw(_selectedGameType,Core.Metrics["ModeDescriptionPreview",0]);
        }

        private void DrawRestriction()
        {
            var restrictionMessage = GameTypeAllowed((GameType)_selectedGameType);
            if (restrictionMessage == "")
            {
                return;
            }
            _messageBorderSprite.Draw();
            _restrictionSprite.Draw();
            FontManager.DrawString(restrictionMessage, "DefaultFont", _textPosition, Color.White, FontAlign.Left);
        }

        private void DrawVSCPUDifficultySelect()
        {
            _messageBorderSprite.Draw();
            FontManager.DrawString(Core.Text["ModeSelectCPULevel"], "DefaultFont", Core.Metrics["RestrictionMessage", 0], Color.White, FontAlign.Left);
            FontManager.DrawString(Core.CPUManager.SkillNames[_selectedCPUSkill], "DefaultFont", Core.Metrics["SelectedCPUDifficulty", 0], Color.White, FontAlign.Left);
            var position = Core.Metrics["SelectedCPUDifficulty", 0];
            _arrowSpriteMap.Draw( 1, 24, 24, (int)position.X - 25, (int)position.Y);
            _arrowSpriteMap.Draw( 0, 24, 24, (int)position.X + 220, (int)position.Y);
        }

        private const int LIST_ITEMS_DRAWN = 4;
        private static readonly Color _disabledColor = new Color(255, 255, 255, 64);
        private Vector2 _textPosition;
        private const int MODE_CHANGE_SPEED = 6;

        private void DrawModeOptions()
        {

            var midpoint = Core.Metrics["ModeSelectOptions", 0].Clone();
            midpoint.X += (int) _listDrawOffset;
            
            var optionSize = Core.Metrics["ModeSelectOptions.Size", 0].Clone();
            var gap = optionSize.X + 5;
            int index = _selectedGameType;
            //Draw selected game type.
            _optionBaseSpriteMap.Draw( 1, optionSize, midpoint);
            var allowed = String.IsNullOrEmpty(GameTypeAllowed((GameType)index));
            _optionsSpriteMap.ColorShading = allowed ? Color.White : _disabledColor;
            _optionsSpriteMap.Draw(index, optionSize.X - 20, optionSize.Y - 20, (int)midpoint.X + 10, (int)midpoint.Y + 10);

            //Draw Mode options to the right of (after) the selected one.
            for (int x = 1; x <= LIST_ITEMS_DRAWN; x++)
            {
                index = (index + 1) % (int)GameType.COUNT;
                midpoint.X += gap;
                _optionBaseSpriteMap.Draw( 0, optionSize , midpoint);
                allowed = String.IsNullOrEmpty(GameTypeAllowed((GameType)index));
                _optionsSpriteMap.ColorShading = allowed ? Color.White : _disabledColor;
                _optionsSpriteMap.Draw( index, optionSize.X - 20, optionSize.Y - 20, (int)midpoint.X + 10, (int)midpoint.Y + 10);
            }

            midpoint.X -= gap * LIST_ITEMS_DRAWN;

            index = _selectedGameType;
            //Draw Mode options to the left of (before) the selected one.
            for (int x = 1; x <= LIST_ITEMS_DRAWN; x++)
            {
                index -= 1;
                if (index < 0)
                {
                    index = (int)GameType.COUNT - 1;
                }
                midpoint.X -= gap;
                _optionBaseSpriteMap.Draw(0, optionSize, midpoint);
                allowed = String.IsNullOrEmpty(GameTypeAllowed((GameType)index));
                _optionsSpriteMap.ColorShading = allowed ? Color.White : _disabledColor;
                _optionsSpriteMap.Draw(index, optionSize.X - 20, optionSize.Y - 20, (int)midpoint.X + 10, (int)midpoint.Y + 10);
            }

            midpoint.X -= (int) _listDrawOffset;
            
            var changeMx = Math.Min(1.0, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds*MODE_CHANGE_SPEED);
            _listDrawOffset -= (_listDrawOffset*(changeMx));
        }

        private void DrawBackground(GameTime gameTime)
        {
            _background.Draw();
            _field.Draw(gameTime);
        }

        private void DrawPlayerOptions()
        {
            _playerOptionsSet.Draw();
        }

        #endregion

        #region Input Handling

        public override void PerformAction(InputAction inputAction)
        {
            
            var pass = _playerOptionsSet.PerformAction(inputAction);
            if (pass)
            {
                //NetHelper.Instance.BroadcastPlayerOptions(inputAction.Player);
                return;
            }
            
            //Ignore inputs from players not playing EXCEPT for system keys.
            if ((inputAction.Player > 0) && (!Core.Players[inputAction.Player -1].IsHumanPlayer))
            {
                return;
            }
            //NetHelper.Instance.BroadcastAction(inputAction);
            switch (inputAction.Action)
            {
                case "LEFT":
                    if (_selectingCPUSkill)
                    {
                        ChangeSelectedCPUDifficulty(-1);
                        RaiseSoundTriggered(SoundEvent.MENU_OPTION_SELECT_LEFT);
                    }
                    else
                    {
                        ChangeGameType(-1);
                        RaiseSoundTriggered(SoundEvent.MENU_SELECT_UP);
                    }
                    
                    break;
                case "RIGHT":
                    if (_selectingCPUSkill)
                    {
                        ChangeSelectedCPUDifficulty(1);
                        RaiseSoundTriggered(SoundEvent.MENU_OPTION_SELECT_RIGHT);
                    }
                    else
                    {
                        ChangeGameType(1);
                        RaiseSoundTriggered(SoundEvent.MENU_SELECT_DOWN);
                    }
                    
                    break;
                case "BACK":
                    Core.ScreenTransition("NewGame");
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
                    break;
                case "START":
                    DoAction();
                    break;
                case "SELECT":
                    _playerOptionsSet.SetChangeMode(inputAction.Player,true);
                    break;

            }
        }

        public override void PerformActionReleased(InputAction inputAction)
        {
            switch (inputAction.Action)
            {
                case "SELECT":
                    _playerOptionsSet.SetChangeMode(inputAction.Player, false);
                    break;
            }
            //NetHelper.Instance.BroadcastActionReleased(inputAction);

        }

        private void ChangeGameType(int amount)
        {
            _listDrawOffset += (amount * 255);
            _selectedGameType += amount;
            if (_selectedGameType < 0)
            {
                _selectedGameType += (int)GameType.COUNT;
            }
            if (_selectedGameType >= (int)GameType.COUNT)
            {
                _selectedGameType -= (int)GameType.COUNT;
            }
        }
        private void ChangeSelectedCPUDifficulty(int amount)
        {
            _selectedCPUSkill += amount;
            if (_selectedCPUSkill < 0)
            {
                _selectedCPUSkill += Core.CPUManager.SkillLevels.Count;
            }
            if (_selectedCPUSkill >= Core.CPUManager.SkillLevels.Count)
            {
                _selectedCPUSkill -= Core.CPUManager.SkillLevels.Count;
            }
        }

        private void DoAction()
        {
            if (GameTypeAllowed((GameType)_selectedGameType) == "")
            {

                Core.Cookies["CurrentGameType"] = (GameType)_selectedGameType;
                RaiseSoundTriggered(SoundEvent.MENU_DECIDE);
                if (((GameType)_selectedGameType) == GameType.VS_CPU)
                {
                    if (!_selectingCPUSkill)
                    {
                        SetupVSCPUMode();
                        
                        return;
                    }
                    Core.Cookies["CPUSkillLevel"] = Core.CPUManager.SkillNames[_selectedCPUSkill];
                }
                if (((GameType) _selectedGameType ) == GameType.SYNC_PRO)
                {
                        _playerOptionsSet.CheckSyncDifficulty();
                }
                if (((GameType)_selectedGameType) == GameType.TEAM)
                {
                    Core.ScreenTransition("TeamSelect");
                }
                else
                {
                    Core.ScreenTransition("SongSelect");
                }
            }
        }

        #endregion

        #region Helper Methods

        private int PlayerCount()
        {
            return (from e in Core.Players where (e.IsHumanPlayer) select e).Count();
        }

        private string GetModeDescription(GameType gameType)
        {
            return Core.Text["ModeSelect" + (int) gameType];
        }

        public string GameTypeAllowed(GameType gameType)
        {
            switch (gameType)
            {
                
            case GameType.COOPERATIVE:
                    
                if (PlayerCount() < 2)
                {
                   return "Requires at least two players.";
                }
                     
                break;
                 
                case GameType.TEAM:
                    if (PlayerCount() < 2)
                    {
                        return "Requires at least two players.";
                    }
                    break;

                case GameType.VS_CPU:
                    if (PlayerCount() > 3)
                    {
                        return "Requires at most three players.";
                    }
                    break;
                    case GameType.SYNC_PRO:
                    case GameType.SYNC_PLUS:
                    if (PlayerCount() < 2)
                    {
                        return "Requires at least two players.";
                    }
                    break;

            }
            return "";
        }

        private void RemoveCPUPlayers()
        {
            foreach (Player player in (from e in Core.Players select e))
            {
                if (player.CPU)
                {
                    player.CPU = false;
                    player.Playing = false;
                }
            }
        }

        private void SetupVSCPUMode()
        {
            foreach (Player player in (from e in Core.Players where e.Playing select e))
            {
                player.Team = 1;
            }

            _selectingCPUSkill = true;
            var cpuDifficulty = (from e in Core.Players where e.Playing select e.PlayerOptions.PlayDifficulty).Max();
            //Find a free player slot for the CPU player.
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    Core.Players[x].Playing = true;
                    Core.Players[x].CPU = true;
                    Core.Players[x].Profile = null;
                    Core.Players[x].ApplyDefaultOptions();
                    Core.Players[x].Team = 2;
                    Core.Players[x].PlayerOptions.PlayDifficulty = cpuDifficulty;
                    _playerOptionsSet.CreatePlayerOptionsFrames();
                    return;
                }
            }
            
            Core.Log.AddMessage("Failed to add CPU Player.",LogLevel.ERROR);
        }

        #endregion

        #region Netplay Code
        /*
        public override void NetMessageReceived(NetMessage message)
        {
            switch (message.MessageType)
            {

                case MessageType.PLAYER_ACTION:
                    PerformAction((InputAction) message.MessageData);
                    break;
                case MessageType.PLAYER_ACTION_RELEASED:
                    PerformAction((InputAction) message.MessageData);
                    break;
                case MessageType.PLAYER_JOIN:
                    break;
                case MessageType.PLAYER_LEAVE:
                    break;
                case MessageType.PLAYER_OPTIONS:
                    Core.Players[message.PlayerID].PlayerOptions = ((PlayerOptions)message.MessageData);
                    break;
                case MessageType.SCREEN_TRANSITION:
                    var screen = message.MessageData.ToString();
                    Core.ScreenTransition(screen);
                    break;
            }

        }
         */ 
        #endregion
    }
}