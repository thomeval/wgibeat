using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WGiBeat.Drawing.Sets;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.GameManagers
{
    public class GameManager
    {

        public NoteBarSet NoteBarSet;
        public Player[] Players;
        
        public virtual void ApplyJudgement(BeatlineNoteJudgement judgement, int player, double multiplier)
        {
            
        }

        public virtual void ApplyJudgementLife()
        {
            
        }

        public virtual void ApplyJudgementLevel()
        {
            
        }

        public virtual void ApplyJudgementScore()
        {
            
        }

        public virtual void ApplyJudgementNotes()
        {
            
        }
    }
}
