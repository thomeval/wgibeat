﻿using System;
using System.Linq;
using WGiBeat.Notes;
using WGiBeat.Players;

namespace WGiBeat.Helpers
{


    public class BeatlineHitAggregator
    {
        private Player[] Players { get; set; }
        private GameType GameType { get; set; }

        private readonly BeatlineNoteJudgement[] _lastJudgements;

        public BeatlineHitAggregator(Player[] players, GameType gameType)
        {
            Players = players;
            GameType = gameType;
            _lastJudgements = new BeatlineNoteJudgement[4];
            for (int x = 0; x < 4; x++)
            {
                _lastJudgements[x] = BeatlineNoteJudgement.Count;
            }
        }

        public event EventHandler<ObjectEventArgs> HitsAggregated;

        public void RegisterHit(int player, BeatlineNoteJudgement judgement)
        {
            if ((player < 0) || (player > 3))
            {
                return;
            }
            _lastJudgements[player] = judgement;

            switch (GameType)
            {
                case GameType.SYNC_PRO:
                    case GameType.SYNC_PLUS:
                    if (!AllHitsReceived())
                    {
                        return;
                    }
                    //For Sync Pro, multiply the judgement given (for scoring purposes). For Sync Plus, the number of arrows means that 
                    //the judgement should *not* be multiplied.
                    var mx = GameType == GameType.SYNC_PRO ? PlayerCount() : 1;
                    SendResponse(new AggregatorResponse
                                     {Judgement = GetWorstJudgement(), Multiplier = mx, Player = (AggregatorPlayerID)0});
                    ResetReceivedHits();
                    break;
                    default:
                        SendResponse(new AggregatorResponse { Judgement = judgement, Multiplier = 1, Player = (AggregatorPlayerID)player });
                    break;
            }

        }

        private BeatlineNoteJudgement GetWorstJudgement()
        {
            var worst = BeatlineNoteJudgement.Ideal;
            for (int x = 0; x < 4; x++)
            {
                if (Players[x].Playing)
                {
                    if (_lastJudgements[x] > worst)
                    {
                        worst = _lastJudgements[x];
                    }
                }
            }
            
            return worst;
        }

        private void ResetReceivedHits()
        {
            for (int x = 0; x < 4; x++)
            {
                _lastJudgements[x] = BeatlineNoteJudgement.Count;
            }
        }

        private int PlayerCount()
        {
            return (from e in Players where e.Playing select e).Count();
        }

        private bool AllHitsReceived()
        {
            for (int x = 0; x < 4; x++)
            {
                if ((_lastJudgements[x] == BeatlineNoteJudgement.Count) && Players[x].Playing)
                {
                    return false;
                }
            }
            return true;
        }

        private void SendResponse(AggregatorResponse aggregatorResponse)
        {
            if (HitsAggregated != null)
            {
                HitsAggregated(this, new ObjectEventArgs {Object = aggregatorResponse});
            }
        }
    }

    public struct AggregatorResponse
    {
        public int Multiplier { get; set; }
        public BeatlineNoteJudgement Judgement { get; set; }
        public AggregatorPlayerID Player { get; set; }
    }

    public enum AggregatorPlayerID
    {
        P1 = 0,
        P2 = 1,
        P3 = 2,
        P4 = 3,
        ALL = -1,
        BLUE_TEAM = -2,
        RED_TEAM = -3,
    }
}
