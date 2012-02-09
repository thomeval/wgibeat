﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;
using WGiBeat.Managers;
using WGiBeat.Notes;

namespace WGiBeat.Screens
{
    public class InstructionScreen : GameScreen
    {
        public int PageNumber = 1;
        public const int TOTAL_PAGES = 5;
        private MovingBackground _background;
        private Sprite _baseSprite;
        private Sprite[] _instructionPages;
        private double _startTime;
        private Beatline _beatline;
        private double _phraseNumber;
        private int _lastBeatline;

        private const int BEATLINE_BPM = 100;
        public InstructionScreen(GameCore core) : base(core)
        {
        }

        public override void Initialize()
        {
            
            InitSprites();
            PageNumber = 1;
            _startTime = TextureManager.LastGameTime.TotalRealTime.TotalSeconds;
            _lastBeatline = -1;
            base.Initialize();
        }

        private void InitSprites()
        {
            _background = new MovingBackground
                              {Direction = Math.PI / 4, Speed = 30, SpriteTexture = TextureManager.Textures("MovingBackground1"), Width = 800, Height = 600};
            _instructionPages = new Sprite[TOTAL_PAGES];
            for (int x = 0; x < TOTAL_PAGES; x++)
            {
                _instructionPages[x] = new Sprite {SpriteTexture = TextureManager.Textures("InstructionPage" + (x + 1))};
            }
            _baseSprite = new Sprite
                              {
                                  SpriteTexture = TextureManager.Textures("LoadingMessageBase"),
                                  Position = (Core.Metrics["LoadMessageBase", 0])
                              };
            _beatline = new Beatline
                            {
                                Bpm = BEATLINE_BPM,
                                Id = 0,
                                Large = true,
                                ReverseDirection = false,
                                Position = Core.Metrics["InstructionBeatline", 0],
                                Speed = 1.0,
                                Width = 320
                            };
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _background.Draw(spriteBatch,gameTime);
            _baseSprite.Draw(spriteBatch);      
            _instructionPages[PageNumber-1].Draw(spriteBatch);
            DrawBeatline(spriteBatch, gameTime);
            TextureManager.DrawString(spriteBatch, "Press start to continue.", "LargeFont", Core.Metrics["LoadMessage", 0], Color.White, FontAlign.LEFT);
            TextureManager.DrawString(spriteBatch, String.Format("Page {0} of {1}",PageNumber, TOTAL_PAGES), "DefaultFont", Core.Metrics["LoadErrorCount", 0], Color.White, FontAlign.LEFT);

        }

        private void DrawBeatline(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (PageNumber != 2)
            {
                return;
            }
            var diff = gameTime.TotalRealTime.TotalSeconds - _startTime;

           
            _phraseNumber = diff*BEATLINE_BPM/240;
            TextureManager.LastDrawnPhraseNumber = _phraseNumber;
            if (_phraseNumber  + 2> _lastBeatline)
            {
                _beatline.AddBeatlineNote(
                    new BeatlineNote {Position = _lastBeatline + 1, NoteType = BeatlineNoteType.NORMAL});
                 _lastBeatline++;
            }
            
           _beatline.Draw(spriteBatch,_phraseNumber);
            _beatline.TrimExpired(_phraseNumber);
        }


        public override void PerformAction(InputAction inputAction)
        {

            var firstLoad = Core.Cookies.ContainsKey("FirstScreen") && (bool) Core.Cookies["FirstScreen"];
            string nextScreen = firstLoad ? "InitialLoad" : "MainMenu";
            switch (inputAction.Action)
            {
                case "START":
                    if (PageNumber < TOTAL_PAGES)
                    {
                        PageNumber++;
                    }
                    else
                    {
                        Core.Cookies["FirstScreen"] = false;
                        Core.ScreenTransition(nextScreen);
                    }
                    break;
                case "BACK":
                    Core.Cookies["FirstScreen"] = false;
                    Core.ScreenTransition(nextScreen);
                    break;
                case "BEATLINE":
                    _beatline.DetermineJudgement(_phraseNumber, true);
                    break;
            }
        }
    }
}
