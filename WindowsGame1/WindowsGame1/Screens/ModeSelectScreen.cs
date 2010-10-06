using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class ModeSelectScreen : GameScreen
    {
        private int _selectedGameType = 0;
        private SpriteMap _optionBaseSpriteMap;
        private SpriteMap _optionsSpriteMap;
        private Sprite _background;
        private Sprite _headerSprite;
        private Sprite _descriptionBaseSprite;

        private readonly SineSwayParticleField _field = new SineSwayParticleField();
        private Sprite _restrictionSprite;
        private readonly List<PlayerOptionsFrame> _playerOptions = new List<PlayerOptionsFrame>();


        public ModeSelectScreen(GameCore core)
             : base(core)
        {
        }

        public override void Initialize()
        {
            InitSprites();

            var frameCount = 0;
            _playerOptions.Clear();
            for (int x = 3; x >= 0; x--)
            {
                if (Core.Players[x].Playing)
                {
                    _playerOptions.Add(new PlayerOptionsFrame { Player = Core.Players[x], PlayerIndex = x });
                    _playerOptions[frameCount].SetPosition(Core.Metrics["PlayerOptionsFrame", frameCount]);
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
                SpriteTexture = TextureManager.Textures["allBackground"],
                Width = Core.Window.ClientBounds.Width,
                X = 0,
                Y = 0
            };

            _headerSprite = new Sprite { SpriteTexture = TextureManager.Textures["modeSelectHeader"] };

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

            _descriptionBaseSprite = new Sprite() {SpriteTexture = TextureManager.Textures["ModeDescriptionBase"]};
            _descriptionBaseSprite.SetPosition(Core.Metrics["ModeDescriptionBase",0]);
            _restrictionSprite = new Sprite {SpriteTexture = TextureManager.Textures["RestrictionBorder"]};
            _restrictionSprite.SetPosition(Core.Metrics["RestrictionBase", 0]);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            _field.Draw(spriteBatch); 
            DrawPlayerOptions(spriteBatch);

            _headerSprite.SetPosition(Core.Metrics["ModeSelectScreenHeader", 0]);
            _headerSprite.Draw(spriteBatch);

            DrawModeOptions(spriteBatch);
            DrawModeDescription(spriteBatch);
            DrawRestriction(spriteBatch);
           
        }

        private void DrawModeDescription(SpriteBatch spriteBatch)
        {
            _descriptionBaseSprite.Draw(spriteBatch);
            var gameType = (GameType) _selectedGameType;
            TextureManager.DrawString(spriteBatch,GetModeDescription(gameType),"DefaultFont",Core.Metrics["ModeDescription",0],Color.Black, FontAlign.LEFT);
        }

        private void DrawRestriction(SpriteBatch spriteBatch)
        {
            var restrictionMessage = GameTypeAllowed((GameType) _selectedGameType);
            if (restrictionMessage == "")
            {
                return;
            }
            _restrictionSprite.Draw(spriteBatch);
            TextureManager.DrawString(spriteBatch, restrictionMessage,"DefaultFont",Core.Metrics["RestrictionMessage",0],Color.White, FontAlign.LEFT);
        }

        private void DrawModeOptions(SpriteBatch spriteBatch)
        {
            var posX = (int) Core.Metrics["ModeSelectOptions", 0].X;
            var posY = (int) Core.Metrics["ModeSelectOptions", 0].Y;

            for (int x = 0; x < (int) GameType.COUNT -1; x++)
            {
                int selected = (x == _selectedGameType) ? 1 : 0;
                _optionBaseSpriteMap.Draw(spriteBatch, selected, 250, 160, posX, posY);

                if (GameTypeAllowed((GameType)x) == "")
                {
                    _optionsSpriteMap.ColorShading.A = 255;
                }
                else
                {
                    _optionsSpriteMap.ColorShading.A = 64;
                }
                _optionsSpriteMap.Draw(spriteBatch, x, 229, 139, posX + 10, posY + 10);
                posX += 255;
            }

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
                    case GameType.SYNC:
                    return "2 to 4 players:\nPlayers share a single score and life bar\n All players must keep up, or fail as a group.";
                default:     
                    return "Game type not recognized.";
            }
        }
        public override void PerformAction(Action action)
        {
         int player;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            var playerOptions = (from e in _playerOptions where e.PlayerIndex == player select e).SingleOrDefault();
            switch (paction)
            {
                case "LEFT":
                    if (playerOptions.OptionChangeActive)
                    {
                        playerOptions.AdjustDifficulty(-1);
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
            _selectedGameType += amount;
            if (_selectedGameType < 0)
            {
                _selectedGameType += (int)GameType.COUNT -1;
            }
            if (_selectedGameType >= (int)GameType.COUNT -1)
            {
                _selectedGameType -= (int) GameType.COUNT -1;
            }
        }

        private int PlayerCount()
        {
            return (from e in Core.Players where e.Playing select e).Count();
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
                    if ((TeamCount(1) == 0 ) || TeamCount(2) == 0)
                    {
                        return "One team has no players.";
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

        private decimal TeamCount(int team)
        {
            return (from e in Core.Players where e.Playing && e.Team == team select e).Count();
        }

        private void DoAction()
        {
            if (GameTypeAllowed((GameType) _selectedGameType) == "")
            {
                if (((GameType)_selectedGameType ) != GameType.TEAM)
                {
                    foreach (Player player in Core.Players)
                    {
                        player.Team = 0;
                    }
                }
                Core.Cookies["CurrentGameType"] = (GameType) _selectedGameType;
                Core.ScreenTransition("SongSelect");
            }
        }
    }
}