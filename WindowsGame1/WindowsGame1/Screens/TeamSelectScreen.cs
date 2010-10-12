using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class TeamSelectScreen : GameScreen
    {

        private Sprite _headingSprite;
        private Sprite _teamBorderSprite;
        private Sprite _backgroundSprite;
        private SpriteMap _playerMarkers;
        private SpriteMap _playerReadyMarkers;

        private bool[] _ready = new bool[4];
        private readonly List<PlayerOptionsFrame> _playerOptions = new List<PlayerOptionsFrame>();

        public TeamSelectScreen(GameCore core) : base(core)
        {
        }

        public override void Initialize()
        {
            InitSprites();
            foreach (Player player in Core.Players)
            {
                player.Team = 0;
            }

            var frameCount = 0;
            _playerOptions.Clear();
            for (int x = 3; x >= 0; x--)
            {
                _ready[x] = false;
                if (Core.Players[x].Playing)
                {
                    _playerOptions.Add(new PlayerOptionsFrame { Player = Core.Players[x], PlayerIndex = x });
                    _playerOptions[frameCount].SetPosition(Core.Metrics["PlayerOptionsFrame", frameCount]);
                    frameCount++;
                }
            }
        }
        public void InitSprites()
        {
            _headingSprite = new Sprite {SpriteTexture = TextureManager.Textures["TeamSelectHeader"]};
            _headingSprite.SetPosition(Core.Metrics["TeamSelectScreenHeader",0]);

            _backgroundSprite = new Sprite 
            { 
                SpriteTexture = TextureManager.Textures["allBackground"], 
                Height = Core.Window.ClientBounds.Height, 
                Width = Core.Window.ClientBounds.Width, 
            };

            _playerReadyMarkers = new SpriteMap { SpriteTexture = TextureManager.Textures["PlayerReady"], Columns = 1, Rows = 2 };
            _teamBorderSprite = new Sprite {SpriteTexture = TextureManager.Textures["TeamScreenBackground"]};
            _teamBorderSprite.SetPosition(Core.Metrics["TeamScreenBackground",0]);

            _playerMarkers = new SpriteMap
                                 {SpriteTexture = TextureManager.Textures["PlayerTeamMarkers"], Columns = 1, Rows = 4};
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            DrawBackground(spriteBatch);
            DrawMarkers(spriteBatch);
            DrawPlayerOptions(spriteBatch);
        }

        private void DrawPlayerOptions(SpriteBatch spriteBatch)
        {
            foreach (PlayerOptionsFrame pof in _playerOptions)
            {
                pof.Draw(spriteBatch);
            }
        }

        private void DrawMarkers(SpriteBatch spriteBatch)
        {
            for (int x =0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    continue;
                }
                var markerPosition = new Vector2(Core.Metrics["PlayerTeamMarkers", x].X,
                                                 Core.Metrics["PlayerTeamMarkers", x].Y);
                if (Core.Players[x].Team == 1)
                {
                    markerPosition.X -= 160;
                }
                if (Core.Players[x].Team == 2)
                {
                    markerPosition.X += 160;
                }
                if (_ready[x])
                {
                    markerPosition.X -= 55;
                    _playerReadyMarkers.Draw(spriteBatch, 2 - Core.Players[x].Team, 165, 60, markerPosition);
                    markerPosition.X += 55;
                }

                _playerMarkers.Draw(spriteBatch, x, 60,60, markerPosition);

            }
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            _backgroundSprite.Draw(spriteBatch);
            _headingSprite.Draw(spriteBatch);
            _teamBorderSprite.Draw(spriteBatch);
        }

        public override void PerformAction(Action action)
        {
            int player = -1;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);
            var pof = (from e in _playerOptions where e.PlayerIndex == player select e).SingleOrDefault();

            //Ignore inputs from players not playing EXCEPT for system keys.
            if ((player > -1) && (pof == null))
            {
                return;
            }
            switch (paction)
            {
                case "LEFT":
                    if (!pof.OptionChangeActive) 
                    {
                        if (!_ready[player])
                        {
                            Core.Players[player].Team = Core.Players[player].Team == 2 ? 0 : 1;
                        }
                    }
                    else
                    {
                        pof.AdjustDifficulty(-1);   
                    }
                    break;
                case "RIGHT":
                    if (!pof.OptionChangeActive)
                    {
                        if (!_ready[player])
                        {
                            Core.Players[player].Team = Core.Players[player].Team == 1 ? 0 : 2;
                        }
                    }
                    else
                    {
                        pof.AdjustDifficulty(1);   
                    }
                    break;
                case "UP":
                    if (pof.OptionChangeActive)
                    {
                        pof.AdjustSpeed(1);
                    }
                    break;
                case "DOWN":
                    if (pof.OptionChangeActive)
                    {
                        pof.AdjustSpeed(-1);
                    }
                    break;
                case "START":
                    if (Core.Players[player].Team != 0)
                    {
                        _ready[player] = !_ready[player];
                        TryToStart();
                    }
                    break;
                case "SELECT":
                    pof.OptionChangeActive = true;
                    break;
                case "BACK":
                    for (int x = 0; x < 4; x++ )
                    {
                        Core.Players[x].Team = 0;
                    }
                        Core.ScreenTransition("ModeSelect");
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

        private void TryToStart()
        {
            var canStart = true;
            for (int x = 0; x < 4; x++)
            {
                canStart = canStart && !(Core.Players[x].Playing ^ _ready[x]);
            }
            var blueTeamCount = (from e in Core.Players where e.Team == 1 select e).Count();
            var redTeamCount = (from e in Core.Players where e.Team == 2 select e).Count();

            canStart = canStart && (blueTeamCount > 0);
            canStart = canStart && (redTeamCount > 0);

            if (canStart)
            {
                Core.ScreenTransition("SongSelect");
            }
        }
    }
}
