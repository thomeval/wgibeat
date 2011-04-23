using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Helpers
{


    public class BeatlineHitAggregator
    {
        public Player[] Players { get; set; }
        public GameType GameType { get; set; }

        public BeatlineNoteJudgement[] _lastJudgements;

        public BeatlineHitAggregator()
        {
            _lastJudgements = new BeatlineNoteJudgement[4];
        }

        
    }

    public class AggregatorResponse
    {
        double Multiplier { get; set; }
        BeatlineNoteJudgement Judgement { get; set;}
        int[] Players { get; set; }
    }
}
