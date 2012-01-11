using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
//using WGiBeat.NetSystem;
using WGiBeat.Players;

namespace WGiBeat.Screens
{
    public class ModeSelectScreen : GameScreen
    {
        private int _selectedGameType;
        private SpriteMap _optionBaseSpriteMap;
        private SpriteMap _optionsSpriteMap;
        private SpriteMap _edgeSpriteMap;
        private SpriteMap _arrowSpriteMap;
        private SpriteMap _previewsSpriteMap;
        private Sprite _background;
        private Sprite _headerSprite;
        private Sprite _descriptionBaseSprite;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();
        private Sprite _restrictionSprite;
        private PlayerOptionsSet _playerOptionsSet;
        private Sprite _messageBorderSprite;
        private int _listDrawOffset;

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
            _playerOptionsSet = new PlayerOptionsSet { Players = Core.Players, Positions = Core.Metrics["PlayerOptionsFrame"] };
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
            _background = new Sprite
            {
                Height = 600,
                Width = 800,
                SpriteTexture = TextureManager.Textures("AllBackground"),
            };

            _headerSprite = new Sprite
                                {
                                    SpriteTexture = TextureManager.Textures("ModeSelectHeader"),
                                    Position = (Core.Metrics["ModeSelectScreenHeader", 0])
                                };

            _optionBaseSpriteMap = new SpriteMap
            {
                Columns = 2,
                Rows = 1,
                SpriteTexture = TextureManager.Textures("ModeOptionBase")
            };

            _optionsSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = (int)GameType.COUNT,
                SpriteTexture = TextureManager.Textures("ModeOptions")
            };

            _previewsSpriteMap = new SpriteMap
                                     {
                                         Columns = 1,
                                         Rows = (int) GameType.COUNT,
                                         SpriteTexture = TextureManager.Textures("ModeDescriptionPreviews")
                                     };

            _edgeSpriteMap = new SpriteMap { Columns = 2, Rows = 1, SpriteTexture = TextureManager.Textures("ModeSelectEdge") };
            _arrowSpriteMap = new SpriteMap { Columns = 4, Rows = 1, SpriteTexture = TextureManager.Textures("IndicatorArrows") };

            _descriptionBaseSprite = new Sprite
                                         {
                                             SpriteTexture = TextureManager.Textures("ModeDescriptionBase"),
                                             Position = (Core.Metrics["ModeDescriptionBase", 0])
                                         };
            _messageBorderSprite = new Sprite
                                       {
                                           SpriteTexture = TextureManager.Textures("MessageBorder"),
                                           Position = (Core.Metrics["MessageBorder", 0])
                                       };
            _restrictionSprite = new Sprite { SpriteTexture = TextureManager.Textures("RestrictionIcon"), Width = 48, Height = 48 };
            _restrictionSprite.SetPosition(_messageBorderSprite.X + 7, _messageBorderSprite.Y + 7);
        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch,gameTime);

            DrawPlayerOptions(spriteBatch);

            _headerSprite.Position = (Core.Metrics["ModeSelectScreenHeader", 0]);
            _headerSprite.Draw(spriteBatch);

            DrawModeOptions(spriteBatch);
            DrawModeDescription(spriteBatch);
            DrawRestriction(spriteBatch);

            if (_selectingCPUSkill)
            {
                DrawVSCPUDifficultySelect(spriteBatch);
            }
            _edgeSpriteMap.Draw(spriteBatch, 0, 20, 160, Core.Metrics["ModeSelectEdge", 0]);
            _edgeSpriteMap.Draw(spriteBatch, 1, 20, 160, Core.Metrics["ModeSelectEdge", 1]);
        }

        private void DrawModeDescription(SpriteBatch spriteBatch)
        {
            _descriptionBaseSprite.Draw(spriteBatch);
            var gameType = (GameType)_selectedGameType;
            TextureManager.DrawString(spriteBatch, GetModeDescription(gameType), "DefaultFont", Core.Metrics["ModeDescription", 0], Color.Black, FontAlign.LEFT);
            _previewsSpriteMap.Draw(spriteBatch,_selectedGameType,Core.Metrics["ModeDescriptionPreview",0]);
        }

        private void DrawRestriction(SpriteBatch spriteBatch)
        {
            var restrictionMessage = GameTypeAllowed((GameType)_selectedGameType);
            if (restrictionMessage == "")
            {
                return;
            }
            _messageBorderSprite.Draw(spriteBatch);
            _restrictionSprite.Draw(spriteBatch);
            TextureManager.DrawString(spriteBatch, restrictionMessage, "DefaultFont", Core.Metrics["RestrictionMessage", 0], Color.White, FontAlign.LEFT);
        }

        private void DrawVSCPUDifficultySelect(SpriteBatch spriteBatch)
        {
            _messageBorderSprite.Draw(spriteBatch);
            TextureManager.DrawString(spriteBatch, Core.Text["ModeSelectCPULevel"], "DefaultFont", Core.Metrics["RestrictionMessage", 0], Color.White, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, Core.CPUManager.SkillNames[_selectedCPUSkill], "DefaultFont", Core.Metrics["SelectedCPUDifficulty", 0], Color.White, FontAlign.LEFT);
            var position = Core.Metrics["SelectedCPUDifficulty", 0];
            _arrowSpriteMap.Draw(spriteBatch, 1, 24, 24, (int)position.X - 25, (int)position.Y);
            _arrowSpriteMap.Draw(spriteBatch, 0, 24, 24, (int)position.X + 220, (int)position.Y);
        }

        private const int LIST_ITEMS_DRAWN = 4;
        public const double MODE_CHANGE_SPEED = 0.9;
        private static readonly Color _disabledColor = new Color(255, 255, 255, 64);

        private void DrawModeOptions(SpriteBatch spriteBatch)
        {

            var midpoint = Core.Metrics["ModeSelectOptions", 0].Clone();
            midpoint.X += _listDrawOffset;

            int index = _selectedGameType;
            //Draw selected game type.
            _optionBaseSpriteMap.Draw(spriteBatch, 1, 250, 160, midpoint);
            var allowed = String.IsNullOrEmpty(GameTypeAllowed((GameType)index));
            _optionsSpriteMap.ColorShading = allowed ? Color.White : _disabledColor;
            _optionsSpriteMap.Draw(spriteBatch, index, 229, 139, (int)midpoint.X + 10, (int)midpoint.Y + 10);

            //Draw Mode options to the right of (after) the selected one.
            for (int x = 1; x <= LIST_ITEMS_DRAWN; x++)
            {
                index = (index + 1) % (int)GameType.COUNT;
                midpoint.X += 255;
                _optionBaseSpriteMap.Draw(spriteBatch, 0, 250, 160, midpoint);
                allowed = String.IsNullOrEmpty(GameTypeAllowed((GameType)index));
                _optionsSpriteMap.ColorShading = allowed ? Color.White : _disabledColor;
                _optionsSpriteMap.Draw(spriteBatch, index, 229, 139, (int)midpoint.X + 10, (int)midpoint.Y + 10);
            }

            midpoint.X -= 255 * LIST_ITEMS_DRAWN;

            index = _selectedGameType;
            //Draw Mode options to the left of (before) the selected one.
            for (int x = 1; x <= LIST_ITEMS_DRAWN; x++)
            {
                index -= 1;
                if (index < 0)
                {
                    index = (int)GameType.COUNT - 1;
                }
                midpoint.X -= 255;
                _optionBaseSpriteMap.Draw(spriteBatch, 0, 250, 160, midpoint);
                allowed = String.IsNullOrEmpty(GameTypeAllowed((GameType)index));
                _optionsSpriteMap.ColorShading = allowed ? Color.White : _disabledColor;
                _optionsSpriteMap.Draw(spriteBatch, index, 229, 139, (int)midpoint.X + 10, (int)midpoint.Y + 10);
            }

            midpoint.X -= _listDrawOffset;
            _listDrawOffset = (int)(_listDrawOffset * MODE_CHANGE_SPEED);

        }


        private void DrawBackground(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _background.Draw(spriteBatch);
            _field.Draw(spriteBatch, gameTime);
        }

        private void DrawPlayerOptions(SpriteBatch spriteBatch)
        {
            _playerOptionsSet.Draw(spriteBatch);
        }

        #endregion

        #region Input Handling

        public override void PerformAction(InputAction inputAction)
        {
            
            var pass = _playerOptionsSet.PerformAction(inputAction);
            if (pass)
            {
                //NetHelper.Instance.BroadcastPlayerOptions(inputAction.Player);
                RaiseSoundTriggered(SoundEvent.PLAYER_OPTIONS_CHANGE);
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
                    RaiseSoundTriggered(SoundEvent.PLAYER_OPTIONS_DISPLAY);
                    break;

            }
        }

        public override void PerformActionReleased(InputAction inputAction)
        {
            switch (inputAction.Action)
            {
                case "SELECT":
                    _playerOptionsSet.SetChangeMode(inputAction.Player, false);
                    RaiseSoundTriggered(SoundEvent.PLAYER_OPTIONS_HIDE);
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
                if (((GameType) _selectedGameType ) == GameType.SYNC)
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
                    case GameType.SYNC:
                    if (PlayerCount() < 2)
                    {
                        return "Requires at least two players.";
                    }
                    break;
                    
                default:
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