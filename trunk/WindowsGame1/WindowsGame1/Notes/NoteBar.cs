using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WindowsGame1.Drawing;

namespace WindowsGame1.Notes
{
    public class NoteBar : DrawableObject
    {

        public List<Note> Notes { get; set; }
        public NoteBar()
        {
            Notes = new List<Note>();
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
        public static NoteBar CreateNoteBar(int numNotes, int numReverse)
        {
            return CreateNoteBar(numNotes, numReverse, 0, 0);
        }
        public static NoteBar CreateNoteBar(int numNotes, int numReverse, Vector2 position)
        {
            return CreateNoteBar(numNotes, numReverse, (int) position.X, (int) position.Y);
        }
        public static NoteBar CreateNoteBar(int numNotes, int numReverse, int posX, int posY)
        {
            var newNoteBar = new NoteBar();
            var rnd = new Random();
            for (int x = 0; x < numNotes; x++)
            {
                var direction = (NoteDirection)rnd.Next((int)NoteDirection.COUNT);
                newNoteBar.Notes.Add(new Note { Completed = false, Direction = direction, Reverse = false });
            }
            newNoteBar.X = posX;
            newNoteBar.Y = posY;
            return newNoteBar;
        }
 
        public override void Draw(SpriteBatch sb)
        {
           
            int posX = this.X - (16 * Notes.Count);
            int posY = this.Y;

            
            var barSpriteMiddle = new Sprite {Height = 40, Width = (32 * Notes.Count) + 4, X = posX - 2, Y = posY - 4, SpriteTexture = TextureManager.Textures["noteBarMiddle"]};
            var barSpriteLeft = new Sprite
                                    {
                                        Height = 40,
                                        Width = 16,
                                        X = barSpriteMiddle.X - 16,
                                        Y = barSpriteMiddle.Y,
                                        SpriteTexture = TextureManager.Textures["noteBarLeft"]
                                    };
            var barSpriteRight = new Sprite
            {
                Height = 40,
                Width = 16,
                X = barSpriteMiddle.X + barSpriteMiddle.Width,
                Y = barSpriteMiddle.Y,
                SpriteTexture = TextureManager.Textures["noteBarRight"]
            };

            
            barSpriteLeft.Draw(sb);
            barSpriteRight.Draw(sb);
            barSpriteMiddle.Draw(sb);
            foreach (Note note in Notes)
            {

                var sprite = new Sprite {Height = 32, Width = 32, X = posX, Y = posY};
                var textureName = "";
                switch (note.Direction)
                {
                    case NoteDirection.LEFT:
                        textureName = "arrowleft";
                        break;
                    case NoteDirection.RIGHT:
                        textureName = "arrowright";
                        break;
                    case NoteDirection.UP:
                        textureName = "arrowup";
                        break;
                    case NoteDirection.DOWN:
                        textureName = "arrowdown";
                        break;
                }
                if (note.Completed)
                {
                    textureName = textureName.Replace("arrow", "arrowc");
                }
                else if (note.Reverse)
                {
                    textureName = textureName.Replace("arrow", "arrowr");  
                }

                sprite.SpriteTexture = TextureManager.Textures[textureName];
                sprite.Draw(sb);
                posX += 32;
            }
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



    }
}
