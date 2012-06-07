using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Notes;

namespace WGiBeat.Drawing
{
    public class CombinedNoteBar : NoteBar
    {

        private int _leftIdx;
        private int _rightIdx;


        public override void Draw(SpriteBatch spriteBatch)
        {
            int posX = this.X - (16 * Notes.Count);
            int posY = this.Y;
            DrawBackground(spriteBatch,posX,posY);
            DrawArrows(spriteBatch,posX,posY);
            DrawCursors(spriteBatch,posX,posY-16);
        }

        private void DrawCursors(SpriteBatch spriteBatch, int posX, int posY)
        {
            var myPosX = posX + 32 * _leftIdx - (_noteBarCursor.SpriteTexture.Width / 2);
            _noteBarCursor.Draw(spriteBatch,(2* ID),new Vector2(myPosX,posY));
            myPosX = posX + 32*(_rightIdx+1) - (_noteBarCursor.SpriteTexture.Width/2);
            _noteBarCursor.Draw(spriteBatch,(2*ID) + 1, new Vector2(myPosX,posY));

        }

        public override void ResetAll()
        {
            base.ResetAll();
            _leftIdx = 0;
            _rightIdx = Notes.Count-1;
        }

        public static CombinedNoteBar CreateCombinedNoteBar(int numArrow, int numReverse, Vector2 position)
        {
            return CreateCombinedNoteBar(numArrow, numReverse, (int) position.X, (int) position.Y);
        }
        public static CombinedNoteBar CreateCombinedNoteBar(int numNotes, int numReverse, int posX, int posY)
        {
            var result = new CombinedNoteBar();
            result.X = posX;
            result.Y = posY;
            result.AddNotes(numNotes, numReverse);
            result.CheckForTriples();
            result.ResetAll();
            
            return result;
        }

        public void MarkCurrentCompleted(int side)
        {
            if (_leftIdx > _rightIdx)
            {
                return;
            }
            switch (side)
            {
                case 0:
                    Notes[_leftIdx].Completed = true;
                    _leftIdx++;
                    break;
                case 1:
                    Notes[_rightIdx].Completed = true;
                    _rightIdx--;
                    break;
            }
        }

        public override void PlayerFaulted()
        {
            PlayerFaulted(0);
        }
        public void PlayerFaulted(int side)
        {
            switch (side)
            {
                case 0:
                    for (int x = 0; x < _leftIdx; x++)
                    {
                        Notes[x].Completed = false;
                    }
                    _leftIdx = 0;
                    break;
                case 1:
                    for (int x  = Notes.Count - 1; x > _rightIdx; x-- )
                    {
                        Notes[x].Completed = false;
                    }
                    _rightIdx = Notes.Count - 1;
                    break;
            }
        }

        public override void TruncateNotes(int level)
        {
            while (level > Notes.Count)
            {
                Notes.Remove(Notes[Notes.Count/2]);
            }
        }
        public override NoteBar Clone()
        {
            var result = new NoteBar();

            foreach (Note n in Notes)
            {
                result.Notes.Add(new Note { Completed = n.Completed, Direction = n.Direction, Reverse = n.Reverse });
            }

            result.ResetAll();
            return result;
        }

        public Note CurrentNote(int side)
        {

            switch (side)
            {
                case 0:
                    return Notes[_leftIdx];
                case 1:
                    return Notes[Math.Max(0,_rightIdx)];
         
            }
            return null;
        }

      
    }
}
