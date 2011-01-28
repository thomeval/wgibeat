using System;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using WGiBeat.Notes;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class StatsScreen : GameScreen
    {

        private readonly int[] _activePlayers;
        private readonly int[] _scrollPosition;
        private readonly Profile[] _activeProfiles;
        private readonly Menu[] _profileMenus;
        private Sprite _headerSprite;
        private Sprite _instructionBaseSprite;
        private Sprite _backgroundSprite;
        private PrimitiveLine _line;

        private const int MAX_SCROLL = 5;
        public StatsScreen(GameCore core) : base(core)
        {
            _activePlayers =  new []{-1,-1};
            _scrollPosition = new[]{0,0};
            _activeProfiles = new Profile[2];
            _profileMenus = new Menu[2];
            
        }

        public override void Initialize()
        {
            _activePlayers[0] = -1;
            _activePlayers[1] = -1;
            InitSprites();
            InitObjects();
            _line = new PrimitiveLine(Core.GraphicsDevice);
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
                    _profileMenus[x].AddItem(new MenuItem{ItemText = profile.Name, ItemValue = profile});
                }
                _profileMenus[x].AddItem(new MenuItem{ItemText = "Main Menu", ItemValue = null});
            }
        }

        private void InitSprites()
        {
            _backgroundSprite = new Sprite {SpriteTexture = TextureManager.Textures("StatsBackground")};
            _headerSprite = new Sprite {SpriteTexture = TextureManager.Textures("StatsHeader")};
            _instructionBaseSprite = new Sprite {SpriteTexture = TextureManager.Textures("StatsInstructionBase"),
            Position = Core.Metrics["StatsInstructionBase",0]
            };
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _backgroundSprite.Draw(spriteBatch);
            DrawBorder(spriteBatch);
            DrawHeader(spriteBatch);
            DrawText(spriteBatch);
        }

        private void DrawHeader(SpriteBatch spriteBatch)
        {
            _headerSprite.Draw(spriteBatch);
            _instructionBaseSprite.Draw(spriteBatch);
        }

        private void DrawBorder(SpriteBatch spriteBatch)
        {
            _line.Width = 1;
            _line.Colour = Color.Black;
            _line.ClearVectors();
            _line.AddVector(new Vector2(400,0));
            _line.AddVector(new Vector2(400,600));
            _line.Render(spriteBatch);
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
                        DrawStats(spriteBatch,x, (Profile) _profileMenus[x].SelectedItem().ItemValue);
                        TextureManager.DrawString(spriteBatch, "START: Change profile", "DefaultFont", Core.Metrics["StatsInstructionText", x], Color.White, FontAlign.CENTER);
                        break;
  
                }
            }
        }

        private void DrawStats(SpriteBatch spriteBatch, int side, Profile profile)
        {

            Vector2[] positions = { Core.Metrics["StatsColumns", (3 * side)].Clone(), Core.Metrics["StatsColumns", (3 * side)+1].Clone(), Core.Metrics["StatsColumns", (3 * side)+2].Clone() };
            TextureManager.DrawString(spriteBatch, profile.Name, "TwoTech36", positions[0], Color.Black, FontAlign.LEFT);
            positions[0].Y += 40;
            positions[1].Y += 40;
           TextureManager.DrawString(spriteBatch,"Total play time:","LargeFont",positions[0],Color.Black, FontAlign.LEFT);
            var playTime = new TimeSpan(0, 0, 0, 0, (int) profile.TotalPlayTime);
           // positions[1].X += 50;

           TextureManager.DrawString(spriteBatch, String.Format("{0}h:{1}m:{2}s ", playTime.Hours, playTime.Minutes,playTime.Seconds), "LargeFont", positions[1], Color.Black, FontAlign.LEFT);
           // positions[1].X -= 50;
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
            var totalBeatlines = profile.JudgementCounts.Sum() - profile.JudgementCounts[(int) BeatlineNoteJudgement.COUNT];
            TextureManager.DrawString(spriteBatch, "Total beatlines:","LargeFont", positions[0],Color.Black,FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + totalBeatlines, "LargeFont", positions[1], Color.Black, FontAlign.LEFT);
            positions[0].Y += 25;
            for (int x = 0; x < (int) BeatlineNoteJudgement.COUNT; x++)
            {

                positions[1].Y = positions[0].Y;
                positions[2].Y = positions[0].Y;
                TextureManager.DrawString(spriteBatch, ((BeatlineNoteJudgement) x).ToString(), "DefaultFont",
                                          positions[0], Color.Black, FontAlign.LEFT);
                TextureManager.DrawString(spriteBatch, profile.JudgementCounts[x] + "", "DefaultFont",
                                          positions[1], Color.Black, FontAlign.LEFT);
                TextureManager.DrawString(spriteBatch, "100%", "DefaultFont",
                                          positions[2], Color.Black, FontAlign.LEFT);
                positions[0].Y += 20;
                
            }
            positions[0].Y += 25;
            positions[1].Y = positions[0].Y;

            var totalArrows = profile.TotalHits + profile.JudgementCounts[(int) BeatlineNoteJudgement.COUNT];
            TextureManager.DrawString(spriteBatch, "Total arrows:", "LargeFont", positions[0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + totalArrows, "LargeFont", positions[1], Color.Black, FontAlign.LEFT);
            positions[0].Y += 25;
            positions[1].Y = positions[0].Y;
            positions[2].Y = positions[0].Y;

            TextureManager.DrawString(spriteBatch, "Hits", "DefaultFont", positions[0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + profile.TotalHits, "DefaultFont", positions[1], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "100%", "DefaultFont", positions[2], Color.Black, FontAlign.LEFT);
            positions[0].Y += 20;
            positions[1].Y = positions[0].Y;
            positions[2].Y = positions[0].Y;

            TextureManager.DrawString(spriteBatch, "Faults", "DefaultFont", positions[0], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "" + profile.JudgementCounts[(int)BeatlineNoteJudgement.COUNT], "DefaultFont", 
                positions[1], Color.Black, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, "100%", "DefaultFont", positions[2], Color.Black, FontAlign.LEFT);
            positions[0].Y += 20;
            positions[1].Y = positions[0].Y;
            positions[2].Y = positions[0].Y;
            
        }



        public override void PerformAction(Action action)
        {
            int player;
            Int32.TryParse("" + action.ToString()[1], out player);
            player--;
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            switch (paction)
            {
                case "START":
                    StartPressed(player);
                    break;
                case "UP":
                    MoveScrollPosition(player,-1);
                    break;
                case "DOWN":
                    MoveScrollPosition(player, 1);
                    break;
                case "BACK":
                    Core.ScreenTransition("MainMenu");
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
                return;
            }

            switch (GetState(side))
            {
                case StatsScreenState.SELECT_PROFILE:
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
