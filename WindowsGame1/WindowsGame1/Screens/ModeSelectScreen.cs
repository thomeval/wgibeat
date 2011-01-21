using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Action = WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class ModeSelectScreen : GameScreen
    {
        private int _selectedGameType;
        private SpriteMap _optionBaseSpriteMap;
        private SpriteMap _optionsSpriteMap;
        private SpriteMap _edgeSpriteMap;
        private SpriteMap _arrowSpriteMap;
        private Sprite _background;
        private Sprite _headerSprite;
        private Sprite _descriptionBaseSprite;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();
        private Sprite _restrictionSprite;
        private readonly List<PlayerOptionsFrame> _playerOptions = new List<PlayerOptionsFrame>();
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
            var frameCount = 0;
            _playerOptions.Clear();
            RemoveCPUPlayers();
            for (int x = 3; x >= 0; x--)
            {
                if (Core.Players[x].Playing)
                {
                    _playerOptions.Add(new PlayerOptionsFrame { Player = Core.Players[x], PlayerIndex = x });
                    _playerOptions[frameCount].Position = (Core.Metrics["PlayerOptionsFrame", frameCount]);
                    frameCount++;
                }
            }

            base.Initialize();
        }

        private void InitSprites()
        {
            _background = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["AllBackground"],
                Width = Core.Window.ClientBounds.Width,
            };

            _headerSprite = new Sprite { SpriteTexture = TextureManager.Textures["modeSelectHeader"] };
            _headerSprite.Position = (Core.Metrics["ModeSelectScreenHeader", 0]);

            _optionBaseSpriteMap = new SpriteMap
            {
                Columns = 2,
                Rows = 1,
                SpriteTexture = TextureManager.Textures["ModeOptionBase"]
            };

            _optionsSpriteMap = new SpriteMap
            {
                Columns = 1,
                Rows = (int)GameType.COUNT,
                SpriteTexture = TextureManager.Textures["ModeOptions"]
            };


            _edgeSpriteMap = new SpriteMap { Columns = 2, Rows = 1, SpriteTexture = TextureManager.Textures["ModeSelectEdge"] };
            _arrowSpriteMap = new SpriteMap { Columns = 4, Rows = 1, SpriteTexture = TextureManager.Textures["IndicatorArrows"] };

            _descriptionBaseSprite = new Sprite { SpriteTexture = TextureManager.Textures["ModeDescriptionBase"] };
            _descriptionBaseSprite.Position = (Core.Metrics["ModeDescriptionBase", 0]);
            _messageBorderSprite = new Sprite { SpriteTexture = TextureManager.Textures["MessageBorder"] };
            _messageBorderSprite.Position = (Core.Metrics["MessageBorder", 0]);
            _restrictionSprite = new Sprite { SpriteTexture = TextureManager.Textures["RestrictionIcon"], Width = 48, Height = 48 };
            _restrictionSprite.SetPosition(_messageBorderSprite.X + 7, _messageBorderSprite.Y + 7);
        }

        #endregion

        #region Drawing

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            _field.Draw(spriteBatch);
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
            var message = "Select CPU Skill level:";
            _messageBorderSprite.Draw(spriteBatch);
            TextureManager.DrawString(spriteBatch, message, "DefaultFont", Core.Metrics["RestrictionMessage", 0], Color.White, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, Core.CPUManager.SkillNames[_selectedCPUSkill], "DefaultFont", Core.Metrics["SelectedCPUDifficulty", 0], Color.White, FontAlign.LEFT);
            var position = Core.Metrics["SelectedCPUDifficulty", 0];
            _arrowSpriteMap.Draw(spriteBatch, 1, 24, 24, (int)position.X - 25, (int)position.Y);
            _arrowSpriteMap.Draw(spriteBatch, 0, 24, 24, (int)position.X + 220, (int)position.Y);
        }

        private const int LIST_ITEMS_DRAWN = 4;
        public const double MODE_CHANGE_SPEED = 0.9;
        private static readonly Color DisabledColor = new Color(255, 255, 255, 64);

        private void DrawModeOptions(SpriteBatch spriteBatch)
        {

            var midpoint = Core.Metrics["ModeSelectOptions", 0].Clone();
            midpoint.X += _listDrawOffset;

            int index = _selectedGameType;
            //Draw selected game type.
            _optionBaseSpriteMap.Draw(spriteBatch, 1, 250, 160, midpoint);
            var allowed = String.IsNullOrEmpty(GameTypeAllowed((GameType)index));
            _optionsSpriteMap.ColorShading = allowed ? Color.White : DisabledColor;
            _optionsSpriteMap.Draw(spriteBatch, index, 229, 139, (int)midpoint.X + 10, (int)midpoint.Y + 10);

            //Draw Mode options to the right of (after) the selected one.
            for (int x = 1; x <= LIST_ITEMS_DRAWN; x++)
            {
                index = (index + 1) % (int)GameType.COUNT;
                midpoint.X += 255;
                _optionBaseSpriteMap.Draw(spriteBatch, 0, 250, 160, midpoint);
                allowed = String.IsNullOrEmpty(GameTypeAllowed((GameType)index));
                _optionsSpriteMap.ColorShading = allowed ? Color.White : DisabledColor;
                _optionsSpriteMap.Draw(spriteBatch, index, 229, 139, (int)midpoint.X + 10, (int)midpoint.Y + 10);
            }

            midpoint.X -= 255 * LIST_ITEMS_DRAWN;


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
                _optionsSpriteMap.ColorShading = allowed ? Color.White : DisabledColor;
                _optionsSpriteMap.Draw(spriteBatch, index, 229, 139, (int)midpoint.X + 10, (int)midpoint.Y + 10);
            }

            midpoint.X -= _listDrawOffset;
            _listDrawOffset = (int)(_listDrawOffset * MODE_CHANGE_SPEED);

        }


        private void DrawBackground(SpriteBatch spriteBatch)
        {
            _background.Draw(spriteBatch);
        }

        private void DrawPlayerOptions(SpriteBatch spriteBatch)
        {
            foreach (PlayerOptionsFrame pof in _playerOptions)
            {
                pof.Draw(spriteBatch);
            }
        }

        #endregion

        #region Input Handling

        public override void PerformAction(Action action)
        {
            int player = -1;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            var playerOptions = (from e in _playerOptions where e.PlayerIndex == player select e).SingleOrDefault();

            //Ignore inputs from players not playing EXCEPT for system keys.
            if ((player > -1) && (playerOptions == null))
            {
                return;
            }
            switch (paction)
            {
                case "LEFT":
                    if (playerOptions.OptionChangeActive)
                    {
                        playerOptions.AdjustDifficulty(-1);
                    }
                    else if (_selectingCPUSkill)
                    {
                        ChangeSelectedCPUDifficulty(-1);
                    }
                    else
                    {
                        ChangeGameType(-1);
                    }

                    break;
                case "RIGHT":
                    if (playerOptions.OptionChangeActive)
                    {
                        playerOptions.AdjustDifficulty(1);
                    }
                    else if (_selectingCPUSkill)
                    {
                        ChangeSelectedCPUDifficulty(1);
                    }
                    else
                    {
                        ChangeGameType(1);
                    }
                    break;
                case "UP":
                    if (playerOptions.OptionChangeActive)
                    {
                        playerOptions.AdjustSpeed(1);
                    }
                    break;
                case "DOWN":
                    if (playerOptions.OptionChangeActive)
                    {
                        playerOptions.AdjustSpeed(-1);
                    }
                    break;
                case "BACK":
                    Core.ScreenTransition("NewGame");
                    break;

                case "START":
                    DoAction();
                    break;
                case "SELECT":
                    playerOptions.OptionChangeActive = true;
                    break;

            }
        }

        public override void PerformActionReleased(Action action)
        {
            int player;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            var playerOptions = (from e in _playerOptions where e.PlayerIndex == player select e).SingleOrDefault();
            switch (paction)
            {
                case "SELECT":
                    if (playerOptions != null)
                    {
                        playerOptions.OptionChangeActive = false;
                    }
                    break;
            }
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
                if (((GameType)_selectedGameType) == GameType.VS_CPU)
                {
                    if (!_selectingCPUSkill)
                    {
                        SetupVSCPUMode();
                        return;
                    }
                    Core.Cookies["CPUSkillLevel"] = Core.CPUManager.SkillNames[_selectedCPUSkill];
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
            switch (gameType)
            {
                case GameType.NORMAL:
                    return "1 to 4 players:\nEach player plays independantly and \nis evaluated individually.";
                case GameType.COOPERATIVE:
                    return
                        "2 to 4 players:\nPlay as a team to achieve high scores. \nPlayer scores and life bars are combined.";

                case GameType.TEAM:
                    return "2 to 4 players:\nTwo teams play competitively.\nThe team with the higher score wins.";
                case GameType.VS_CPU:
                    return "1 to 3 players:\nPlayers compete against a computer opponent.\nThe opponent is scaled to match the number of players.";
                default:
                    return "Game type not recognized.";
            }
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
            var cpuDifficulty = (from e in Core.Players where e.Playing select e.PlayDifficulty).Max();
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    Core.Players[x].Playing = true;
                    Core.Players[x].CPU = true;
                    Core.Players[x].Profile = null;
                    Core.Players[x].Team = 2;
                    Core.Players[x].PlayDifficulty = cpuDifficulty;
                    return;
                }
            }

            throw new Exception("Did not setup CPU Player.");
        }

        #endregion
    }
}