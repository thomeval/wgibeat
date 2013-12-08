using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using WGiBeat.Notes;

namespace WGiBeat.Drawing
{
    /// <summary>
    /// A NoteBar consists of a number of arrows that must be pressed in sequence. A NoteBar is responsible for drawing
    /// all of the arrows it contains, as well as its background. To create a NoteBar with a random sequence of notes,
    /// use CreateNoteBar().
    /// </summary>
    public class NoteBar : DrawableObject
    {


        private SpriteMap3D _arrowsSpriteMap;

        public double Redness { get; set; }
        public Sprite3D RednessSprite { get; set; }

        public double XDisplayOffset { get; set; }
        public List<Note> Notes { get; set; }
        public int ID { get; set; }
        public int DisplayLimit { get; set; }
        public double Opacity { get; set; }

        public NoteBar()
        {
            Notes = new List<Note>();
            InitSprites();
        }

        public const int NOTE_SIZE = 44;

        private void InitSprites()
        {
            _arrowsSpriteMap = new SpriteMap3D { Texture = TextureManager.Textures("Arrows"), Columns = 4, Rows = 3 };
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
            if (NumberCompleted() >= DisplayLimit)
            {
                return null;
            }
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
            XDisplayOffset += NOTE_SIZE;
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
            result.RednessSprite = new Sprite3D
                                       {
                                           Position = RednessSprite.Position,
                                           Size = RednessSprite.Size,
                                           Texture = RednessSprite.Texture,
                                           ColorShading = RednessSprite.ColorShading

                                       };
            foreach (Note n in Notes)
            {
                result.Notes.Add(new Note{Completed = n.Completed, Direction = n.Direction, Reverse = n.Reverse});
            }
            return result;
        }

 
        public override void Draw()
        {

            int posX = this.X;
            int posY = this.Y;

            if (RednessSprite != null)
            {
                DrawRednessSprite();
            }

            var xdrawOffset = 0 - NumberCompleted() * NOTE_SIZE + (int)XDisplayOffset;
            var drawCount = 0;
            foreach (Note note in Notes)
            {
                if (drawCount >= DisplayLimit)
                {
                    return;
                }
                var completedOffset = 0;
                var heightOffset = (int) note.Direction * NOTE_SIZE / 3;
                var cell = ((int) note.Direction);
                if (note.Completed)
                {
                    cell += 8;
                    completedOffset = -6;
                }
                else if (note.Reverse)
                {
                    cell += 4;
                }

                _arrowsSpriteMap.ColorShading.A = (byte) (CalculateOpacity(xdrawOffset) * Opacity / 255);
                if (_arrowsSpriteMap.ColorShading.A > 0)
                {
                    _arrowsSpriteMap.Draw( cell, NOTE_SIZE, NOTE_SIZE, posX + xdrawOffset + completedOffset,
                                          posY + heightOffset);
                }
                xdrawOffset += NOTE_SIZE;
                drawCount++;
            }

        }

        private void DrawRednessSprite()
        {

            RednessSprite.ColorShading.A = Convert.ToByte(Redness);

            RednessSprite.Draw();
        }

        private const int NOTE_FADEOUT_START = 180;
        private byte CalculateOpacity(int drawOffset)
        {
            int result = 255;
            if (drawOffset < 0)
            {
                result = 255 + ((drawOffset) * 3);
            }
            else if (drawOffset > NOTE_FADEOUT_START)
            {
                result = 255 - ((drawOffset - NOTE_FADEOUT_START)*3);
            
            }
            result = Math.Max(0,Math.Min(255, result));
            return (byte) result;
            
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
