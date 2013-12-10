using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoundLineCode;
using WGiBeat.AudioSystem;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Screens
{
    public class StatsScreen : GameScreen
    {

        private readonly int[] _activePlayers;
        private readonly int[] _scrollPosition;
        private readonly Profile[] _activeProfiles;
        private readonly Menu[] _profileMenus;
        private Sprite3D _headerSprite;
        private Sprite3D _instructionBaseSprite;
        private Sprite3D _backgroundSprite;
 


        private readonly SineSwayParticleField _field = new SineSwayParticleField();
        private ProfileLevelDisplay _levelDisplay;

        private const int MAX_SCROLL = 5;
        public StatsScreen(GameCore core)
            : base(core)
        {
            _activePlayers = new[] { -1, -1 };
            _scrollPosition = new[] { 0, 0 };
            _activeProfiles = new Profile[2];
            _profileMenus = new Menu[2];
            
        }

        public override void Initialize()
        {
            _activePlayers[0] = -1;
            _activePlayers[1] = -1;
            InitSprites();
            InitObjects();
   

            base.Initialize();
        }

        private void InitObjects()
        {
            for (int x = 0; x < 2; x++)
            {
                _profileMenus[x] = new Menu
                                       {
                                           FontName = "LargeFont",
                                           Width = 350,
                                           MaxVisibleItems = 10,
                                           Position = Core.Metrics["StatsProfileMenu", x]
                                       };

                foreach (Profile profile in Core.Profiles.GetAll())
                {
                    _profileMenus[x].AddItem(new MenuItem { ItemText = profile.Name, ItemValue = profile });
                }
                _profileMenus[x].AddItem(new MenuItem { ItemText = "Main Menu", ItemValue = null, IsCancel = true});
            }
            _levelDisplay = new ProfileLevelDisplay{Width = 365};
        }

        private void InitSprites()
        {
            _backgroundSprite = new Sprite3D
                                    {
                                        Texture = TextureManager.Textures("StatsBackground"),
                                        Size = Core.Metrics["ScreenBackground.Size", 0],
                                        Position = Core.Metrics["ScreenBackground", 0]
                                    };
            _headerSprite = new Sprite3D
                                {
                                    Texture = TextureManager.Textures("StatsHeader"),
                                    Position = (Core.Metrics["ScreenHeader", 0]),
                                    Size = Core.Metrics["ScreenHeader.Size", 0]
                                };

            _instructionBaseSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("StatsInstructionBase"),
                Position = Core.Metrics["StatsInstructionBase", 0],
                Size = Core.Metrics["StatsInstructionBase.Size", 0]
            };

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(gameTime);
            DrawBorder();
            DrawHeader();
            DrawText();
        }

        private void DrawBackground(GameTime gameTime)
        {
            _backgroundSprite.Draw();
            _field.Draw(gameTime);
        }

        private void DrawHeader()
        {
            _headerSprite.Draw();
            _instructionBaseSprite.Draw();
        }

        private void DrawBorder()
        {

            var lineSegment = new RoundLine(GameCore.INTERNAL_WIDTH / 2, 0, GameCore.INTERNAL_WIDTH / 2, GameCore.INTERNAL_HEIGHT);
            RoundLineManager.Instance.Draw(lineSegment,1,Color.Black,0,null);
        }

        private void DrawText()
        {
            for (int x = 0; x < 2; x++)
            {
                switch (GetState(x))
                {

                    case StatsScreenState.NOT_JOINED:
                        FontManager.DrawString("Press start to join...", "LargeFont", Core.Metrics["StatsJoinMessage", x], Color.Black, FontAlign.Center);
                        FontManager.DrawString("START: Join", "DefaultFont", Core.Metrics["StatsInstructionText", x], Color.White, FontAlign.Center);

                        break;
                    case StatsScreenState.SELECT_PROFILE:
                        FontManager.DrawString("START: Select profile", "DefaultFont", Core.Metrics["StatsInstructionText", x], Color.White, FontAlign.Center);
                        _profileMenus[x].Draw();
                        break;
                    case StatsScreenState.VIEWING_STATS:
                        DrawStats( x, (Profile)_profileMenus[x].SelectedItem().ItemValue);
                        FontManager.DrawString("START: Change profile", "DefaultFont", Core.Metrics["StatsInstructionText", x], Color.White, FontAlign.Center);
                        break;

                }
            }
        }

        private void DrawStats( int side, Profile profile)
        {
            var tempPlayer = new Player { Profile = profile };

            Vector2[] positions = { Core.Metrics["StatsColumns", (3 * side)].Clone(), Core.Metrics["StatsColumns", (3 * side) + 1].Clone(), Core.Metrics["StatsColumns", (3 * side) + 2].Clone() };
            FontManager.DrawString(profile.Name, "TwoTech36", positions[0],FontManager.ScaleTextToFit(profile.Name,"TwoTech36",280,80),Color.Black,FontAlign.Left );
            positions[0].Y += 28;
            positions[1].Y += 28;

            DrawLevelBars( positions[0], tempPlayer);
            positions[0].Y += 15;
            positions[1].Y += 15;

            FontManager.DrawString("EXP: ", "DefaultFont",positions[0],Color.Black,FontAlign.Left);
            FontManager.DrawString(String.Format("{0}/{1}", profile.EXP, tempPlayer.GetNextEXPSafe()),
                          "DefaultFont", positions[1],Color.Black,
                          FontAlign.Left);
            positions[0].Y += 35;
            positions[1].Y += 35;
            FontManager.DrawString("Total play time:", "LargeFont", positions[0]  );
            var playTime = new TimeSpan(0, 0, 0, 0, (int)profile.TotalPlayTime);

            FontManager.DrawString(String.Format("{0:F0}:{1:00}:{2:00} ", playTime.TotalHours, playTime.Minutes, playTime.Seconds), "LargeFont", positions[1]  );

            positions[0].Y += 25;
            positions[1].Y += 25;

            FontManager.DrawString("Songs cleared:", "LargeFont", positions[0]  );
            FontManager.DrawString("" + profile.SongsCleared, "LargeFont", positions[1]  );
            positions[0].Y += 25;
            positions[1].Y += 25;
            FontManager.DrawString("Songs failed:", "LargeFont", positions[0]  );
            FontManager.DrawString("" + profile.SongsFailed, "LargeFont", positions[1]  );

            positions[0].Y += 50;
            positions[1].Y += 50;
            var totalBeatlines = profile.JudgementCounts.Sum() - profile.JudgementCounts[(int)BeatlineNoteJudgement.COUNT];
            FontManager.DrawString("Total beatlines:", "LargeFont", positions[0]  );
            FontManager.DrawString("" + totalBeatlines, "LargeFont", positions[1]  );
            positions[0].Y += 25;
            for (int x = 0; x < (int)BeatlineNoteJudgement.COUNT; x++)
            {

                positions[1].Y = positions[0].Y;
                positions[2].Y = positions[0].Y;
                FontManager.DrawString(((BeatlineNoteJudgement)x).ToString(), "DefaultFont",
                                          positions[0]  );
                FontManager.DrawString(profile.JudgementCounts[x] + "", "DefaultFont",
                                          positions[1]  );
                FontManager.DrawString(String.Format("{0:P0}", 1.0 * profile.JudgementCounts[x] / totalBeatlines), "DefaultFont",
                                          positions[2]  );
                positions[0].Y += 20;

            }
            positions[0].Y += 25;
            positions[1].Y = positions[0].Y;

            var totalArrows = profile.TotalHits + profile.JudgementCounts[(int)BeatlineNoteJudgement.COUNT];
            FontManager.DrawString("Total arrows:", "LargeFont", positions[0]  );
            FontManager.DrawString("" + totalArrows, "LargeFont", positions[1]  );
            positions[0].Y += 25;
            positions[1].Y = positions[0].Y;
            positions[2].Y = positions[0].Y;

            FontManager.DrawString("Hits", "DefaultFont", positions[0] );
            FontManager.DrawString("" + profile.TotalHits, "DefaultFont", positions[1]  );
            string percentage = String.Format("{0:P0}", 1.0 * profile.TotalHits / totalArrows);
            FontManager.DrawString(percentage, "DefaultFont", positions[2]  );
            positions[0].Y += 20;
            positions[1].Y = positions[0].Y;
            positions[2].Y = positions[0].Y;

            FontManager.DrawString("Faults", "DefaultFont", positions[0]);
            FontManager.DrawString("" + profile.JudgementCounts[(int)BeatlineNoteJudgement.COUNT], "DefaultFont",
                positions[1]);
            percentage = String.Format("{0:P0}", 1.0 * profile.JudgementCounts[(int)BeatlineNoteJudgement.COUNT] / totalArrows);
            FontManager.DrawString(percentage, "DefaultFont", positions[2]);
            positions[0].Y += 35;
            positions[1].Y = positions[0].Y;
            positions[2].Y = positions[0].Y;

            FontManager.DrawString("Best Hit Chain:", "LargeFont", positions[0]);
            FontManager.DrawString(profile.MostHitsEver + "", "LargeFont", positions[1]);
            positions[0].Y += 30;
            positions[1].Y = positions[0].Y;
            positions[2].Y = positions[0].Y;
            FontManager.DrawString("Best Streak:", "LargeFont", positions[0]);
            FontManager.DrawString(profile.MostStreakEver + "", "LargeFont", positions[1]);
        }

        private void DrawLevelBars( Vector2 position, Player player)
        {
            _levelDisplay.Player = player;
            _levelDisplay.Position = position;
            _levelDisplay.Draw();
        }


        public override void PerformAction(InputAction inputAction)
        {

            switch (inputAction.Action)
            {
                case "START":
                    StartPressed(inputAction.Player);
                    break;
                case "UP":
                case "DOWN":
                    var side = PlayerToSide(inputAction.Player);
                    if (side == -1)
                    {
                        return;
                    }
                    switch (GetState(side))
                    {
                        case StatsScreenState.SELECT_PROFILE:
                            _profileMenus[side].HandleAction(inputAction);
                            break;
                            case StatsScreenState.VIEWING_STATS:
                            MoveScrollPosition(side, -1);
                            break;
                    }                 
                    break;
                
                case "BACK":
                    Core.ScreenTransition("MainMenu");
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
                    break;
            }
        }

        private void MoveScrollPosition(int idx, int amount)
        {

                    _scrollPosition[idx] += amount;
                    _scrollPosition[idx] = Math.Min(MAX_SCROLL, _scrollPosition[idx]);
                    _scrollPosition[idx] = Math.Max(0, _scrollPosition[idx]);
        }

        private void StartPressed(int player)
        {
            int side = PlayerToSide(player);
            if (side == -1)
            {
                AddPlayer(player);
                RaiseSoundTriggered(SoundEvent.MENU_DECIDE);
                return;
            }

            switch (GetState(side))
            {
                case StatsScreenState.SELECT_PROFILE:
                    _profileMenus[side].HandleAction(new InputAction {Action = "START"});
                    if (_profileMenus[side].SelectedItem().ItemValue != null)
                    {
                        _activeProfiles[side] = (Profile)_profileMenus[side].SelectedItem().ItemValue;
                    }
                    else
                    {
                        Core.ScreenTransition("MainMenu");
                    }

                    break;
                case StatsScreenState.VIEWING_STATS:
                    _activeProfiles[side] = null;
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
                    break;
            }

        }

        private void AddPlayer(int player)
        {
            if (_activePlayers[0] == -1)
            {
                _activePlayers[0] = player;
            }
            else if (_activePlayers[1] == -1)
            {
                _activePlayers[1] = player;
            }
        }

        private int PlayerToSide(int player)
        {
            return (_activePlayers.IndexOf(player));
        }

        private StatsScreenState GetState(int side)
        {
            if (_activePlayers[side] == -1)
            {
                return StatsScreenState.NOT_JOINED;
            }
            if (_activeProfiles[side] == null)
            {
                return StatsScreenState.SELECT_PROFILE;
            }
            return StatsScreenState.VIEWING_STATS;
        }
    }

    public enum StatsScreenState
    {
        NOT_JOINED,
        SELECT_PROFILE,
        VIEWING_STATS
    }
}
