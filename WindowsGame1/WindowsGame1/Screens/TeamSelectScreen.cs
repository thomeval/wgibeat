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

        private Sprite3D _headingSprite;
        private Sprite3D _teamBorderSprite;
        private Sprite3D _backgroundSprite;
        private SpriteMap3D _playerMarkers;
        private SpriteMap3D _playerReadyMarkers;

        private readonly bool[] _ready = new bool[4];
        private PlayerOptionsSet _playerOptionsSet;

        private string _restrictionMessage = "";
        private bool _showWarningIcon;
        private Sprite3D _messageBorder;
        private Sprite3D _restrictionIcon;
        private Vector2 _textPosition;
        private readonly SineSwayParticleField _field = new SineSwayParticleField();

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
            _playerOptionsSet = new PlayerOptionsSet { Players = Core.Players, Positions = Core.Metrics["PlayerOptionsFrame"], 
                CurrentGameType = GameType.TEAM,  Size = Core.Metrics["PlayerOptionsFrame.Size",0], DrawAttract = true, StackableFrames = true};
            _playerOptionsSet.CreatePlayerOptionsFrames();

            SetRestrictionMessage("Press LEFT or RIGHT to choose \na team. Press START to confirm \nselection.", false);

        }
        private void InitSprites()
        {
            _headingSprite = new Sprite3D
                                 {
                                     Texture = TextureManager.Textures("TeamSelectHeader"),
                                     Position = Core.Metrics["ScreenHeader", 0],
                                     Size = Core.Metrics["ScreenHeader.Size", 0]
                                 };

            _backgroundSprite = new Sprite3D 
            { 
                Texture = TextureManager.Textures("AllBackground"),
                Size = Core.Metrics["ScreenBackground.Size", 0],
                Position = Core.Metrics["ScreenBackground", 0]
            };

            _playerReadyMarkers = new SpriteMap3D { Texture = TextureManager.Textures("PlayerReady"), Columns = 1, Rows = 2 };
            _teamBorderSprite = new Sprite3D
                                    {
                                        Texture = TextureManager.Textures("TeamScreenBackground"),
                                        Position = (Core.Metrics["TeamScreenBackground", 0]),
                                        Size = Core.Metrics["TeamScreenBackground.Size",0]
                                    };

            _playerMarkers = new SpriteMap3D
                                 {
                                     Texture = TextureManager.Textures("PlayerTeamMarkers"), Columns = 1, Rows = 4
                                 };
            _messageBorder = new Sprite3D
                                 {
                                     Texture = TextureManager.Textures("MessageBorder"),
                                     Position = (Core.Metrics["TeamScreenMessageBorder", 0]),
                                     Size = Core.Metrics["TeamScreenMessageBorder.Size",0]
                                 };
            _restrictionIcon = new Sprite3D {Texture = TextureManager.Textures("RestrictionIcon"), Width = 48, Height = 48};
            _restrictionIcon.X = _messageBorder.X + 7;
            _restrictionIcon.Y = _messageBorder.Y + 7;
            _textPosition = _messageBorder.Position.Clone();
            _textPosition.X += 60;
            _textPosition.Y += 25;

        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(gameTime);
            DrawMarkers();
            DrawPlayerOptions();
            DrawRestrictionMessage();
        }

        private void DrawPlayerOptions()
        {
            _playerOptionsSet.Draw();
        }


        private void DrawMarkers()
        {
            var markerSize = Core.Metrics["PlayerTeamMarkers.Size",0];
            var playerReadyMarkerSize = Core.Metrics["PlayerTeamReadyMarkers.Size", 0];
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
                    markerPosition -= Core.Metrics["PlayerTeamMarkers.Movement",0];
                }
                if (Core.Players[x].Team == 2)
                {
                    markerPosition += Core.Metrics["PlayerTeamMarkers.Movement", 0];
                }
                if (_ready[x])
                {
                    _playerReadyMarkers.Draw(2 - Core.Players[x].Team, playerReadyMarkerSize.X, playerReadyMarkerSize.Y, markerPosition.X - 65, markerPosition.Y);
    
                }

                _playerMarkers.Draw( x, markerSize, markerPosition);

            }
        }

        private void DrawBackground(GameTime gameTime)
        {
            _backgroundSprite.Draw();
            _field.Draw(gameTime);
            _headingSprite.Draw();
            _teamBorderSprite.Draw();
        }


        private void DrawRestrictionMessage()
        {
            if (_restrictionMessage == "")
            {
                return;
            }
            _messageBorder.Draw();
            FontManager.DrawString(_restrictionMessage, "DefaultFont", _textPosition, Color.White, FontAlign.Left);
            if (_showWarningIcon)
            {
                _restrictionIcon.Draw();
            }
        }

        public override void PerformAction(InputAction inputAction)
        {

            var pass = _playerOptionsSet.PerformAction(inputAction);
            if (pass)
            {
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
                    break;
                case "BACK":
                    for (int x = 0; x < 4; x++ )
                    {
                        Core.Players[x].Team = 0;
                    }
                        Core.ScreenTransition("ModeSelect");
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
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
                SetRestrictionMessage("Press LEFT or RIGHT to choose \na team. Press START to confirm \nselection.", false);
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
