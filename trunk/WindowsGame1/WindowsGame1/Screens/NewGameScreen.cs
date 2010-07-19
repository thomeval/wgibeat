using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using Action=WGiBeat.Managers.Action;

namespace WGiBeat.Screens
{
    public class NewGameScreen : GameScreen
    {
        private int _playersJoined;
        private SineSwayParticleField _field = new SineSwayParticleField();
        private readonly int[] _cursorPositions = new int[4];

        private readonly Menu[] _playerMenus = new Menu[4];
        public NewGameScreen(GameCore core) : base(core)
        {

        }

        public override void Initialize()
        {
            for (int x = 0; x < Core.Players.Count(); x++)
            {
                _cursorPositions[x] = -1;
                Core.Players[x].Playing = false;

                    _playerMenus[x] = new Menu();
                    _playerMenus[x].AddItem(new MenuItem { ItemText = "Decision" });
                    var difficulty = new MenuItem { ItemText = "Difficulty" };
                    difficulty.AddOption("Beginner", 0);
                    difficulty.AddOption("Easy", 1);
                    difficulty.AddOption("Medium", 2);
                    difficulty.AddOption("Hard", 3);
                    difficulty.AddOption("Insane",4);
                    _playerMenus[x].AddItem(difficulty);

                var noteSpeed = new MenuItem {ItemText = "Beatline Speed"};
                noteSpeed.AddOption("0.5x",0.5);
                noteSpeed.AddOption("1x",1.0);
                noteSpeed.AddOption("1.5x",1.5);
                noteSpeed.AddOption("2x", 2.0);
                noteSpeed.AddOption("3x", 3.0);
                noteSpeed.AddOption("4x", 4.0);
                noteSpeed.AddOption("6x", 6.0);
                noteSpeed.SetSelectedByValue(1.0);
                _playerMenus[x].AddItem(noteSpeed);

                _playerMenus[x].SetPosition(Core.Metrics["NewGameMenuStart",x]);
                _playerMenus[x].AddItem(new MenuItem { ItemText = "Leave" });
            }
            _playersJoined = 0;
            base.Initialize();
        }

        private void DrawBackground(SpriteBatch spriteBatch)
        {
            var background = new Sprite
            {
                Height = Core.Window.ClientBounds.Height,
                SpriteTexture = TextureManager.Textures["allBackground"],
                Width = Core.Window.ClientBounds.Width,
                X = 0,
                Y = 0
            };

            background.Draw(spriteBatch);
            _field.Draw(spriteBatch);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawBackground(spriteBatch);
            

            DrawBorders(spriteBatch);
            DrawMenus(spriteBatch);
            for (int x = 0; x < 4; x++)
            {
                if (!Core.Players[x].Playing)
                {
                    spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Press Start to Join...",
                                           Core.Metrics["NewGameJoinNotification", x], Color.Black);
                }

            }
        }

        private void DrawMenus(SpriteBatch spriteBatch)
        {
            for (int x = 0; x < 4; x++ )
            {
                if (Core.Players[x].Playing)
                {
                    if (_cursorPositions[x] == 999)
                    {
                        spriteBatch.DrawString(TextureManager.Fonts["LargeFont"], "Ready",
                        Core.Metrics["NewGameJoinNotification", x], Color.Black);
                    }
                    else if (_cursorPositions[x] == 0)
                    {
                        _playerMenus[x].Draw(spriteBatch);
                    }

                }
            }

        }

        private void DrawBorders(SpriteBatch spriteBatch)
        {
            var brush = new PrimitiveLine(Core.GraphicsDevice) { Colour = Color.Black };
            brush.AddVector(new Vector2(400, 0));
            brush.AddVector(new Vector2(400, 600));
            brush.Render(spriteBatch);
            brush.ClearVectors();
            brush.AddVector(new Vector2(0, 275));
            brush.AddVector(new Vector2(800, 275));
            brush.Render(spriteBatch);
            brush.ClearVectors();
            brush.AddVector(new Vector2(0, 325));
            brush.AddVector(new Vector2(800, 325));
            brush.Render(spriteBatch);
            brush.ClearVectors();
        }

        public override void PerformAction(Action action)
        {
            switch (action)
            {
                case Action.P1_START:
                    StartPressed(0);
                    break;
                    case Action.P2_START:
                    StartPressed(1);
                    break;
                case Action.P3_START:
                    StartPressed(2);
                    break;
                case Action.P4_START:
                    StartPressed(3);
                    break;
                    case Action.P1_UP:
                    _playerMenus[0].DecrementSelected();
                    break;
                    case Action.P2_UP:
                    _playerMenus[1].DecrementSelected();
                    break;
                    case Action.P3_UP:
                    _playerMenus[2].DecrementSelected();
                    break;
                    case Action.P4_UP:
                    _playerMenus[3].DecrementSelected();
                    break;
                    case Action.P1_DOWN:
                    _playerMenus[0].IncrementSelected();
                    break;
                    case Action.P2_DOWN:
                    _playerMenus[1].IncrementSelected();
                    break;
                    case Action.P3_DOWN:
                    _playerMenus[2].IncrementSelected();
                    break;
                    case Action.P4_DOWN:
                    _playerMenus[3].IncrementSelected();
                    break;
                case Action.P1_LEFT:
                    _playerMenus[0].DecrementOption();
                    break;
                case Action.P2_LEFT:
                    _playerMenus[1].DecrementOption();
                    break;
                case Action.P3_LEFT:
                    _playerMenus[2].DecrementOption();
                    break;
                case Action.P4_LEFT:
                    _playerMenus[3].DecrementOption();
                    break;
                case Action.P1_RIGHT:
                    _playerMenus[0].IncrementOption();
                    break;
                case Action.P2_RIGHT:
                    _playerMenus[1].IncrementOption();
                    break;
                case Action.P3_RIGHT:
                    _playerMenus[2].IncrementOption();
                    break;
                case Action.P4_RIGHT:
                    _playerMenus[3].IncrementOption();
                    break;
                case Action.SYSTEM_BACK:
                    Core.ScreenTransition("MainMenu");
                    break;
            }
        }

        private void StartPressed(int number)
        {
            if (!Core.Players[number].Playing)
            {
                _cursorPositions[number] = 0;
                Core.Players[number].Playing = true;
                _playersJoined += 1;
            }
            else if (_cursorPositions[number] == 999)
            {
                //Player is already ready.
                return;
            }
            else
            {
                switch (_playerMenus[number].SelectedItem().ItemText)
                {
                    case "Leave":
                        _cursorPositions[number] = -1;
                        Core.Players[number].Playing = false;
                        TryToStart();
                        break;
                    case "Decision":
                        _cursorPositions[number] = 999;
                        TryToStart();
                        break;
                }
            }
        }

        private void TryToStart()
        {
            bool noPlayers = true;
            for (int x = 0; x < 4; x++)
            {
                noPlayers = noPlayers && (_cursorPositions[x] == -1);
            }
            if (noPlayers)
            {
                return;
            }

            bool everyoneReady = true;
            for (int x = 0; x < 4; x++)
            {
                everyoneReady = everyoneReady && (!(Core.Players[x].Playing ^ _cursorPositions[x] == 999));
            }

            if (everyoneReady)
            {
                StartGame();
            }
        }


        private void StartGame()
        {
            for (int x = 0; x < 4; x++ )
            {
                Core.Players[x].PlayDifficulty =
                    (Difficulty) (int) _playerMenus[x].GetByItemText("Difficulty").SelectedValue();
                Core.Players[x].BeatlineSpeed = (double) _playerMenus[x].GetByItemText("Beatline Speed").SelectedValue();
            }
                Core.ScreenTransition("ModeSelect");
        }
    }
}
