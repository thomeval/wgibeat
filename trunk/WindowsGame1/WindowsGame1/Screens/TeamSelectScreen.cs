using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using WGiBeat.Players;

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
        private PlayerOptionsSet _playerOptionsSet;

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

            for (int x = 0; x < 4; x++ )
            {
                _ready[x] = false;
            }
                _playerOptionsSet = new PlayerOptionsSet { Players = Core.Players, Positions = Core.Metrics["PlayerOptionsFrame"], CurrentGameType = (GameType)Core.Cookies["CurrentGameType"] };
            _playerOptionsSet.CreatePlayerOptionsFrames();

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
                Height = 600,
                Width = 800
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
            DrawBackground(spriteBatch, gameTime);
            DrawMarkers(spriteBatch);
            DrawPlayerOptions(spriteBatch);
            DrawRestrictionMessage(spriteBatch);
        }

        private void DrawPlayerOptions(SpriteBatch spriteBatch)
        {
            _playerOptionsSet.Draw(spriteBatch);
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

        private void DrawBackground(SpriteBatch spriteBatch, GameTime gameTime)
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

            var pass = _playerOptionsSet.PerformAction(inputAction);
            if (pass)
            {
                RaiseSoundTriggered(SoundEvent.PLAYER_OPTIONS_CHANGE);
                TryToStart();
                return;
            }

            //Ignore inputs from players not playing EXCEPT for system keys.
            if ((inputAction.Player > 0) && (!Core.Players[inputAction.Player - 1].IsHumanPlayer))
            {
                return;
            }

            var playerIdx = inputAction.Player - 1;
            switch (inputAction.Action)
            {
                case "LEFT":
                        if (!_ready[playerIdx])
                        {
                            Core.Players[playerIdx].Team = Core.Players[playerIdx].Team == 2 ? 0 : 1;
                        }
                        RaiseSoundTriggered(SoundEvent.TEAM_CHANGE);
                    break;
                case "RIGHT":
                        if (!_ready[playerIdx])
                        {
                            Core.Players[playerIdx].Team = Core.Players[playerIdx].Team == 1 ? 0 : 2;
                        }
                    RaiseSoundTriggered(SoundEvent.TEAM_CHANGE);
                    break;
                case "START":
                    if (Core.Players[playerIdx].Team != 0)
                    {
                        _ready[playerIdx] = !_ready[playerIdx];
                        RaiseSoundTriggered(SoundEvent.TEAM_DECIDE);
                        TryToStart();
                    }
                    break;
                case "SELECT":
                    _playerOptionsSet.SetChangeMode(inputAction.Player, true);
                    RaiseSoundTriggered(SoundEvent.PLAYER_OPTIONS_DISPLAY);
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

            switch (inputAction.Action)
            {
                case "SELECT":
                    _playerOptionsSet.SetChangeMode(inputAction.Player, false);
                    RaiseSoundTriggered(SoundEvent.PLAYER_OPTIONS_HIDE);
                    break;
            }
        }

        private void TryToStart()
        {

            var canStart = true;
            for (int x = 0; x < 4; x++)
            {
                canStart = canStart && (!Core.Players[x].Playing || _ready[x]);
            }
            if (!canStart)
            {
                SetRestrictionMessage("Press left or right to \nchoose a team. Press start \nto confirm selection.", false);
                return;
            }
            var blueTeamCount = (from e in Core.Players where e.Playing && e.Team == 1 select e).Count();
            var redTeamCount = (from e in Core.Players where e.Playing && e.Team == 2 select e).Count();

            canStart = (blueTeamCount > 0);
            canStart = canStart && (redTeamCount > 0);

            if (!canStart)
            {
                SetRestrictionMessage("One team has no players.", true);
            }  
            else
            {
                RaiseSoundTriggered(SoundEvent.MENU_DECIDE);
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
