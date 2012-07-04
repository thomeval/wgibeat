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
        private Sprite _instructionBaseSprite;
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
                _profileMenus[x].AddItem(new MenuItem { ItemText = "Main Menu", ItemValue = null });
            }
            _levelDisplay = new ProfileLevelDisplay{Width = 365};
        }

        private void InitSprites()
        {
            _backgroundSprite = new Sprite3D
                                    {
                                        Texture = TextureManager.Textures("StatsBackground"),
                                        Height = 600,
                                        Width = 800,
                                    };
            _headerSprite = new Sprite3D
                                {
                                    Texture = TextureManager.Textures("StatsHeader"),
                                    Position = (Core.Metrics["ScreenHeader", 0]),
                                    Size = Core.Metrics["ScreenHeader.Size", 0]
                                };

            _instructionBaseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("StatsInstructionBase"),
                Position = Core.Metrics["StatsInstructionBase", 0]
            };

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(gameTime);
            DrawBorder();
            DrawHeader(spriteBatch);
            DrawText(spriteBatch);
        }

        private void DrawBackground(GameTime gameTime)
        {
            _backgroundSprite.Draw();
            _field.Draw(gameTime);
        }

        private void DrawHeader(SpriteBatch spriteBatch)
        {
            _headerSprite.Draw();
            _instructionBaseSprite.Draw(spriteBatch);
        }

        private void DrawBorder()
        {
          
            var lineSegment = new RoundLine(400,0,400,600);
            RoundLineManager.Instance.Draw(lineSegment,1,Color.Black,0,null);
        }

        private void DrawText(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < 2; x++)
            {
                switch (GetState(x))
                {

                    case StatsScreenState.NOT_JOINED:
                        TextureManager.DrawString(spriteBatch, "Press start to join...", "LargeFont", Core.Metrics["StatsJoinMessage", x], Color.Black, FontAlign.CENTER);
                        TextureManager.DrawString(spriteBatch, "START: Join", "DefaultFont", Core.Metrics["StatsInstructionText", x], Color.White, FontAlign.CENTER);

                        break;
                    case StatsScreenState.SELECT_PROFILE:
                        TextureManager.DrawString(spriteBatch, "START: Select profile", "DefaultFont", Core.Metrics["StatsInstructionText", x], Color.White, FontAlign.CENTER);
                        _profileMenus[x].Draw(spriteBatch);
                        break;
                    case StatsScreenState.VIEWING_STATS:
                        DrawStats(spriteBatch, x, (Profile)_profileMenus[x].SelectedItem().ItemValue);
                        TextureManager.DrawString(spriteBatch, "START: Change profile", "DefaultFont", Core.Metrics["StatsInstructionText", x], Color.White, FontAlign.CENTER);
                        break;

                }
            }
        }

        private void DrawStats(SpriteBatch spriteBatch, int side, Profile profile)
        {
            var tempPlayer = new Player { Profile = profile };

            Vector2[] positions = { Core.Metrics["StatsColumns", (3 * side)].Clone(), Core.Metrics["StatsColumns", (3 * side) + 1].Clone(), Core.Metrics["StatsColumns", (3 * side) + 2].Clone() };
            TextureManager.DrawString(spriteBatch, profile.Name, "TwoTech36", positions[0],TextureManager.ScaleTextToFit(profile.Name,"TwoTech36",280,80), Color.Black, FontAlign.LEFT);
            positions[0].Y += 28;
            positions[1].Y += 28;

            DrawLevelBars(spriteBatch, positions[0], tempPlayer);
            positions[0].Y += 15;
            positions[1].Y += 15;

            TextureManager.DrawString(spriteBatch,"EXP: ", "DefaultFont",positions[0],Color.Black,FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch,
                          String.Format("{0}/{1}", profile.EXP, tempPlayer.GetNextEXPSafe()),
                          "DefaultFont", positions[1],Color.Black,
                          FontAlign.LEFT);
            positions[0].Y += 35;
            positions[1].Y += 35;
            TextureManager.DrawString(spriteBatch, "Total play time:", "LargeFont", positions[0], Color.Black, FontAlign.LEFT);
            var playTime = new TimeSpan(0, 0, 0, 0, (int)profile.TotalPlayTime);

            TextureManager.DrawString(spriteBatch, String.Format("{0}h:{1:00}m:{2:00}s ", playTime.Hours, playTime.Minutes, playTime.Seconds), "LargeFont", positions[1], Color.Black, FontAlign.LEFT);

            positions[0].Y += 25;
            positions[1].Y += 25;

            TextureManager.DrawString(spriteBatch, "Songs cleared:", "LargeFont", positions[0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + profile.SongsCleared, "LargeFont", positions[1], Color.Black, FontAlign.LEFT);
            positions[0].Y += 25;
            positions[1].Y += 25;
            TextureManager.DrawString(spriteBatch, "Songs failed:", "LargeFont", positions[0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + profile.SongsFailed, "LargeFont", positions[1], Color.Black, FontAlign.LEFT);

            positions[0].Y += 50;
            positions[1].Y += 50;
            var totalBeatlines = profile.JudgementCounts.Sum() - profile.JudgementCounts[(int)BeatlineNoteJudgement.COUNT];
            TextureManager.DrawString(spriteBatch, "Total beatlines:", "LargeFont", positions[0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + totalBeatlines, "LargeFont", positions[1], Color.Black, FontAlign.LEFT);
            positions[0].Y += 25;
            for (int x = 0; x < (int)BeatlineNoteJudgement.COUNT; x++)
            {

                positions[1].Y = positions[0].Y;
                positions[2].Y = positions[0].Y;
                TextureManager.DrawString(spriteBatch, ((BeatlineNoteJudgement)x).ToString(), "DefaultFont",
                                          positions[0], Color.Black, FontAlign.LEFT);
                TextureManager.DrawString(spriteBatch, profile.JudgementCounts[x] + "", "DefaultFont",
                                          positions[1], Color.Black, FontAlign.LEFT);
                TextureManager.DrawString(spriteBatch, String.Format("{0:P0}", 1.0 * profile.JudgementCounts[x] / totalBeatlines), "DefaultFont",
                                          positions[2], Color.Black, FontAlign.LEFT);
                positions[0].Y += 20;

            }
            positions[0].Y += 25;
            positions[1].Y = positions[0].Y;

            var totalArrows = profile.TotalHits + profile.JudgementCounts[(int)BeatlineNoteJudgement.COUNT];
            TextureManager.DrawString(spriteBatch, "Total arrows:", "LargeFont", positions[0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + totalArrows, "LargeFont", positions[1], Color.Black, FontAlign.LEFT);
            positions[0].Y += 25;
            positions[1].Y = positions[0].Y;
            positions[2].Y = positions[0].Y;

            TextureManager.DrawString(spriteBatch, "Hits", "DefaultFont", positions[0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + profile.TotalHits, "DefaultFont", positions[1], Color.Black, FontAlign.LEFT);
            string percentage = String.Format("{0:P0}", 1.0 * profile.TotalHits / totalArrows);
            TextureManager.DrawString(spriteBatch, percentage, "DefaultFont", positions[2], Color.Black, FontAlign.LEFT);
            positions[0].Y += 20;
            positions[1].Y = positions[0].Y;
            positions[2].Y = positions[0].Y;

            TextureManager.DrawString(spriteBatch, "Faults", "DefaultFont", positions[0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + profile.JudgementCounts[(int)BeatlineNoteJudgement.COUNT], "DefaultFont",
                positions[1], Color.Black, FontAlign.LEFT);
            percentage = String.Format("{0:P0}", 1.0 * profile.JudgementCounts[(int)BeatlineNoteJudgement.COUNT] / totalArrows);
            TextureManager.DrawString(spriteBatch, percentage, "DefaultFont", positions[2], Color.Black, FontAlign.LEFT);
            positions[0].Y += 20;
            positions[1].Y = positions[0].Y;
            positions[2].Y = positions[0].Y;

        }

        private void DrawLevelBars(SpriteBatch spriteBatch, Vector2 position, Player player)
        {
            _levelDisplay.Player = player;
            _levelDisplay.Position = position;
            _levelDisplay.Draw(spriteBatch);
        }


        public override void PerformAction(InputAction inputAction)
        {

            switch (inputAction.Action)
            {
                case "START":
                    StartPressed(inputAction.Player);
                    break;
                case "UP":
                    MoveScrollPosition(inputAction.Player, -1);
                    break;
                case "DOWN":
                    MoveScrollPosition(inputAction.Player, 1);
                    break;
                case "BACK":
                    Core.ScreenTransition("MainMenu");
                    RaiseSoundTriggered(SoundEvent.MENU_BACK);
                    break;
            }
        }

        private void MoveScrollPosition(int player, int amount)
        {
            var idx = _activePlayers.IndexOf(player);
            if (idx == -1)
            {
                return;
            }

            switch (GetState(idx))
            {
                case StatsScreenState.SELECT_PROFILE:
                    _profileMenus[idx].MoveSelected(amount);
                    if (amount > 0)
                    {
                        RaiseSoundTriggered(SoundEvent.MENU_SELECT_DOWN);
                    }
                    else
                    {
                        RaiseSoundTriggered(SoundEvent.MENU_SELECT_UP);
                    }
                    break;
                case StatsScreenState.VIEWING_STATS:
                    _scrollPosition[idx] += amount;
                    _scrollPosition[idx] = Math.Min(MAX_SCROLL, _scrollPosition[idx]);
                    _scrollPosition[idx] = Math.Max(0, _scrollPosition[idx]);
                    break;
            }
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
                    if (_profileMenus[side].SelectedItem().ItemValue != null)
                    {
                        _activeProfiles[side] = (Profile)_profileMenus[side].SelectedItem().ItemValue;
                        RaiseSoundTriggered(SoundEvent.MENU_DECIDE);
                    }
                    else
                    {
                        Core.ScreenTransition("MainMenu");
                        RaiseSoundTriggered(SoundEvent.MENU_BACK);
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
