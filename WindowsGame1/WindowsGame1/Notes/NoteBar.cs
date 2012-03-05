using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing;

namespace WGiBeat.Notes
{
    /// <summary>
    /// A NoteBar consists of a number of arrows that must be pressed in sequence. A NoteBar is responsible for drawing
    /// all of the arrows it contains, as well as its background. To create a NoteBar with a random sequence of notes,
    /// use CreateNoteBar().
    /// </summary>
    public class NoteBar : DrawableObject
    {
        private Sprite _barSpriteMiddle;
        private Sprite _barSpriteLeft;
        private Sprite _barSpriteRight;

        private Sprite _barSpriteMiddleGlow;
        private Sprite _barSpriteLeftGlow;
        private Sprite _barSpriteRightGlow;

        private SpriteMap _noteBarCursor;
        public double Redness;
        public List<Note> Notes { get; set; }
        public int ID { get; set; }
        public NoteBar()
        {
            Notes = new List<Note>();
            InitSprites();
        }

        public void InitSprites()
        {
            _barSpriteMiddle = new Sprite { Height = 40, SpriteTexture = TextureManager.Textures("NoteBarMiddle") };
            _barSpriteLeft = new Sprite
            {
                Height = 40,
                Width = 16,

                SpriteTexture = TextureManager.Textures("NoteBarLeft")
            };
            _barSpriteRight = new Sprite
            {
                Height = 40,
                Width = 16,
                SpriteTexture = TextureManager.Textures("NoteBarRight")
            };
            _barSpriteLeftGlow = new Sprite
                                     {
                                         SpriteTexture = TextureManager.CreateWhiteMask("NoteBarLeft"),
                                         Height = 40,
                                         Width = 16,
                                         ColorShading = Color.Red                                      
                                     };
            _barSpriteRightGlow = new Sprite
            {
                SpriteTexture = TextureManager.CreateWhiteMask("NoteBarRight"),
                Height = 40,
                Width = 16,
                ColorShading = Color.Red
            };
            _barSpriteMiddleGlow = new Sprite
            {
                SpriteTexture = TextureManager.CreateWhiteMask("NoteBarMiddle"),
                Height = 40,
                ColorShading = Color.Red
            };
            _noteBarCursor = new SpriteMap
                                 {
                                     SpriteTexture = TextureManager.Textures("NoteBarCombinedCursors"),
                                     Columns = 1,
                                     Rows = 4
                                 };
        }
        public int NumberCompleted()
        {
            return (from e in Notes where e.Completed select e).Count();
        }
        public int NumberReverse()
        {
            return (from e in Notes where e.Reverse select e).Count();
        }

        public Note CurrentNote()
        {
            return (from e in Notes where !e.Completed select e).FirstOrDefault();
        }
        public void MarkCurrentCompleted()
        {
            Note currentNote = CurrentNote();
            if (currentNote == null)
            {
                throw new InvalidOperationException("Note bar doesn't have a current note to mark as complete.");
            }
            currentNote.Completed = true;
        }

        public void MarkAllCompleted()
        {
            foreach (Note note in Notes)
            {
                note.Completed = true;
            }
        }

        public void CancelReverse()
        {
            foreach (Note note in Notes)
            {
                note.Reverse = false;
            }
        }


        public static NoteBar CreateNoteBar(int numNotes, int numReverse)
        {
            return CreateNoteBar(numNotes, numReverse, 0, 0);
        }
        public static NoteBar CreateNoteBar(int numNotes, int numReverse, Vector2 position)
        {
            return CreateNoteBar(numNotes, numReverse, (int) position.X, (int) position.Y);
        }

        private static Random _rnd = new Random();


        public static NoteBar CreateNoteBar(int numNotes, int numReverse, int posX, int posY)
        {
            var newNoteBar = new NoteBar();

            for (int x = 0; x < numNotes; x++)
            {
                var direction = (NoteDirection)_rnd.Next((int)NoteDirection.COUNT);
                newNoteBar.Notes.Add(new Note { Completed = false, Direction = direction, Reverse = false });
            }
            for (int x = 0; x < Math.Min(numReverse,numNotes); x++)
            {
                int idx = _rnd.Next(newNoteBar.Notes.Count);
                while (newNoteBar.Notes[idx].Reverse)
                {
                    idx = _rnd.Next(newNoteBar.Notes.Count);
                }
                newNoteBar.Notes[idx].Reverse = true;
            }
            newNoteBar.X = posX;
            newNoteBar.Y = posY;

            CheckForTriples(newNoteBar);
            return newNoteBar;
        }

        private static void CheckForTriples(NoteBar newNoteBar)
        {
            if (newNoteBar.Notes.Count < 3)
            {
                return;
            }
           for (int x = 0; x <= newNoteBar.Notes.Count - 3; x++)
           {
               if ((newNoteBar.Notes[x].Direction == newNoteBar.Notes[x + 1].Direction) && (newNoteBar.Notes[x].Direction == newNoteBar.Notes[x + 2].Direction))
               {
                   var temp = (int) newNoteBar.Notes[x + 1].Direction;
                   var adj = _rnd.Next(1, 4);
                   newNoteBar.Notes[x + 1].Direction = (NoteDirection) ((temp + adj) % (int) NoteDirection.COUNT);
               }
               
           }
        }

        public NoteBar Clone()
        {
            var result = new NoteBar();

            foreach (Note n in Notes)
            {
                result.Notes.Add(new Note{Completed = n.Completed, Direction = n.Direction, Reverse = n.Reverse});
            }
            return result;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
           
            int posX = this.X - (16 * Notes.Count);
            int posY = this.Y;

            _barSpriteMiddle.Width = _barSpriteMiddleGlow.Width =  (32 * Notes.Count) + 4;
            _barSpriteMiddle.X = _barSpriteMiddleGlow.X =  posX - 2;
            _barSpriteMiddle.Y = _barSpriteMiddleGlow.Y = posY - 4;
            _barSpriteLeft.X = _barSpriteLeftGlow.X = _barSpriteMiddle.X - 16;
            _barSpriteLeft.Y = _barSpriteLeftGlow.Y = _barSpriteMiddle.Y;
            _barSpriteRight.X = _barSpriteRightGlow.X = _barSpriteMiddle.X + _barSpriteMiddle.Width;
            _barSpriteRight.Y = _barSpriteRightGlow.Y = _barSpriteMiddle.Y;
          
            _barSpriteLeft.Draw(spriteBatch);
            _barSpriteRight.Draw(spriteBatch);
            _barSpriteMiddle.Draw(spriteBatch);

            _barSpriteLeftGlow.ColorShading.A =
                _barSpriteMiddleGlow.ColorShading.A = _barSpriteRightGlow.ColorShading.A = Convert.ToByte(Redness);
            _barSpriteLeftGlow.Draw(spriteBatch);
            _barSpriteRightGlow.Draw(spriteBatch);
            _barSpriteMiddleGlow.Draw(spriteBatch);

            var sprite = new SpriteMap { SpriteTexture = TextureManager.Textures("Arrows"), Columns = 4, Rows = 3 };
            foreach (Note note in Notes)
            {

                var cell = ((int) note.Direction);
                if (note.Completed)
                {
                    cell += 8;
                }
                else if (note.Reverse)
                {
                    cell += 4;
                }

                sprite.Draw(spriteBatch, cell, 32, 32, posX, posY);
                posX += 32;
            }

            posX = this.X - (16 * Notes.Count) +32*NumberCompleted() - (_noteBarCursor.SpriteTexture.Width/2);
            posY -= 16;
            _noteBarCursor.Draw(spriteBatch,ID, new Vector2(posX,posY));

        }

        public bool AllCompleted()
        {
            return !((from e in Notes where !e.Completed select e).Any());
        }

        public void ResetAll()
        {
            foreach (Note note in Notes)
            {
                note.Completed = false;
            }
        }

        public void TruncateNotes(int level)
        {
            while (Notes.Count > level)
            {
                Redness = 255;
                Notes.RemoveAt(Notes.Count-1);
            }
        }

        public void PlayerFaulted()
        {
            Redness = 255;
            ResetAll();
        }
    }
}
