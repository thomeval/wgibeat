using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using WGiBeat.Managers;

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
        private string _restrictionMessage = "";
        private bool _showWarningIcon;
        private Sprite _messageBorder;
        private Sprite _restrictionIcon;

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
                    _playerOptions[frameCount].Position = (Core.Metrics["PlayerOptionsFrame", frameCount]);
                    frameCount++;
                }
            }
            SetRestrictionMessage("Press left or right to \nchoose a team. Press start \nto confirm selection.", false);

        }
        public void InitSprites()
        {
            _headingSprite = new Sprite
                                 {
                                     SpriteTexture = TextureManager.Textures("TeamSelectHeader"),
                                     Position = (Core.Metrics["TeamSelectScreenHeader", 0])
                                 };

            _backgroundSprite = new Sprite 
            { 
                SpriteTexture = TextureManager.Textures("AllBackground"), 
                Height = Core.Window.ClientBounds.Height, 
                Width = Core.Window.ClientBounds.Width, 
            };

            _playerReadyMarkers = new SpriteMap { SpriteTexture = TextureManager.Textures("PlayerReady"), Columns = 1, Rows = 2 };
            _teamBorderSprite = new Sprite
                                    {
                                        SpriteTexture = TextureManager.Textures("TeamScreenBackground"),
                                        Position = (Core.Metrics["TeamScreenBackground", 0])
                                    };

            _playerMarkers = new SpriteMap
                                 {
                                     SpriteTexture = TextureManager.Textures("PlayerTeamMarkers"), Columns = 1, Rows = 4
                                 };
            _messageBorder = new Sprite
                                 {
                                     SpriteTexture = TextureManager.Textures("MessageBorder"),
                                     Position = (Core.Metrics["MessageBorder", 0])
                                 };
            _restrictionIcon = new Sprite {SpriteTexture = TextureManager.Textures("RestrictionIcon"), Width = 48, Height = 48};
            _restrictionIcon.SetPosition(_messageBorder.X + 7, _messageBorder.Y + 7);
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            DrawMarkers(spriteBatch);
            DrawPlayerOptions(spriteBatch);
            DrawRestrictionMessage(spriteBatch);
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

        private void DrawRestrictionMessage(SpriteBatch spriteBatch)
        {
            if (_restrictionMessage != "")
            {
                _messageBorder.Draw(spriteBatch);
                TextureManager.DrawString(spriteBatch, _restrictionMessage, "DefaultFont", Core.Metrics["RestrictionMessage", 0], Color.White, FontAlign.LEFT);
                if (_showWarningIcon)
                    _restrictionIcon.Draw(spriteBatch);
            }

        }

        public override void PerformAction(InputAction inputAction)
        {

            var playerIdx = inputAction.Player - 1;
            var optionsFrame = (from e in _playerOptions where e.PlayerIndex == playerIdx select e).SingleOrDefault();

            //Ignore inputs from players not playing EXCEPT for system keys.
            if ((inputAction.Player > 0) && (optionsFrame == null))
            {
                return;
            }
            switch (inputAction.Action)
            {
                case "LEFT":
                    if (!optionsFrame.OptionChangeActive) 
                    {
                        if (!_ready[playerIdx])
                        {
                            Core.Players[playerIdx].Team = Core.Players[playerIdx].Team == 2 ? 0 : 1;
                        }
                    }
                    else
                    {
                        optionsFrame.AdjustDifficulty(-1);   
                    }
                    break;
                case "RIGHT":
                    if (!optionsFrame.OptionChangeActive)
                    {
                        if (!_ready[playerIdx])
                        {
                            Core.Players[playerIdx].Team = Core.Players[playerIdx].Team == 1 ? 0 : 2;
                        }
                    }
                    else
                    {
                        optionsFrame.AdjustDifficulty(1);   
                    }
                    break;
                case "UP":
                    if (optionsFrame.OptionChangeActive)
                    {
                        optionsFrame.AdjustSpeed(1);
                    }
                    break;
                case "DOWN":
                    if (optionsFrame.OptionChangeActive)
                    {
                        optionsFrame.AdjustSpeed(-1);
                    }
                    break;
                case "START":
                    if (Core.Players[playerIdx].Team != 0)
                    {
                        _ready[playerIdx] = !_ready[playerIdx];
                        TryToStart();
                    }
                    break;
                case "SELECT":
                    optionsFrame.OptionChangeActive = true;
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

        public override void PerformActionReleased(InputAction inputAction)
        {

            var playerOptions = (from e in _playerOptions where e.PlayerIndex == inputAction.Player -1 select e).SingleOrDefault();
            switch (inputAction.Action)
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
            if (!canStart)
            {
                SetRestrictionMessage("Press left or right to \nchoose a team. Press start \nto confirm selection.", false);
                return;
            }
            var blueTeamCount = (from e in Core.Players where e.Team == 1 select e).Count();
            var redTeamCount = (from e in Core.Players where e.Team == 2 select e).Count();

            canStart = (blueTeamCount > 0);
            canStart = canStart && (redTeamCount > 0);

            if (!canStart)
            {
                SetRestrictionMessage("One team has no players.", true);
            }  
            else
            {
                Core.ScreenTransition("SongSelect");
            }
        }

        private void SetRestrictionMessage(string message, bool isProblem)
        {
            _restrictionMessage = message;
            _showWarningIcon = isProblem;
        }
    }
}
