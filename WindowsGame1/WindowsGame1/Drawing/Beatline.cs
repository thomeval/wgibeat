using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Drawing.Sets;
using WGiBeat.Notes;

namespace WGiBeat.Drawing
{
    public class Beatline : DrawableObject
    {
        private readonly List<BeatlineNote> _beatlineNotes;
        private readonly List<BeatlineNote> _notesToRemove;
        public double Speed { get; set; }
        public double Bpm { get; set; }
        public int Id { get; set; }
        public bool DisablePulse { get; set; }
        public double EndPhrase { get; set; }
        private SpriteMap _markerSprite;
        private Sprite _pulseSprite;
        private SpriteMap _baseSprite;

        //Used by endpoint markers.
        private const int BEHIND_VISIBILITY = 23;

        public const int HIT_IGNORE_CUTOFF = 1000;
        //How distant the beatline notes are from each other.
        //Increase this to make them move faster and more apart.
        const int BEAT_ZOOM_DISTANCE = 200;

        //Prevent notes from being drawn outside the beatline base.
        //Higher means longer visibility range.
        public const double BEAT_VISIBILITY = 1.1;
        public Beatline()
        {
            _beatlineNotes = new List<BeatlineNote>();
            _notesToRemove = new List<BeatlineNote>();
            InitSprites();
        }

        private void InitSprites()
        {
            this.Height = 40;
            this.Width = 250;
            _markerSprite = new SpriteMap
            {
                Columns = 1,
                Rows = 5,
                SpriteTexture = TextureManager.Textures["beatMarkers"],

            };

            _pulseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures["BeatFlame"]
            };
            _baseSprite = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures["BeatMeter"],
                Columns = 1,
                Rows = 4
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch,0.0);
        }

        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            DrawPulse(spriteBatch, phraseNumber);
            DrawBase(spriteBatch);
            DrawNotes(spriteBatch, phraseNumber);
            DrawEndPoints(spriteBatch, phraseNumber);
        }

        private void DrawEndPoints(SpriteBatch spriteBatch, double phraseNumber)
        {
            var endpointBeatOffset = (int)(Speed * BEAT_ZOOM_DISTANCE * (phraseNumber - EndPhrase));

            //Dont render endpoint if outside the visibility range.
            if (((-1 * endpointBeatOffset) > BEAT_ZOOM_DISTANCE * BEAT_VISIBILITY) || (endpointBeatOffset > BEHIND_VISIBILITY))
            {
                return;
            }

            var markerPosition = new Vector2 { X = this.X + 28 - (endpointBeatOffset), Y = (this.Y + 3) };
            _markerSprite.ColorShading.A = 255;
            _markerSprite.Draw(spriteBatch, 4, 1, 34, markerPosition);
        }

        private void DrawPulse(SpriteBatch spriteBatch, double phraseNumber)
        {
            if (DisablePulse)
            {
                return;
            }
            _pulseSprite.Width = (int)(80 * (Math.Ceiling(phraseNumber) - (phraseNumber)));
            _pulseSprite.ColorShading.A = (byte)(_pulseSprite.Width * 255 / 80);
            _pulseSprite.Height = 42;
            _pulseSprite.SetPosition(this.X + 30, this.Y - 5);
            _pulseSprite.DrawTiled(spriteBatch, 83 - _pulseSprite.Width, 0, _pulseSprite.Width, 34);
        }


        private void DrawNotes(SpriteBatch spriteBatch, double phraseNumber)
        {
            foreach (BeatlineNote bn in _beatlineNotes)
            {
                var markerBeatOffset = (int)(Speed * BEAT_ZOOM_DISTANCE * (phraseNumber - bn.Position));

                //Dont render notes outside the visibility range.
                if (((-1 * markerBeatOffset) > BEAT_ZOOM_DISTANCE * BEAT_VISIBILITY) && (!bn.Hit))
                {
                    continue;
                }

                var markerPosition = new Vector2 { Y = (this.Y + 3) };

                if (bn.Hit)
                {
                    markerPosition.X = this.X + 28 + (bn.DisplayPosition);
                    _markerSprite.ColorShading.A = 128;
                }
                else
                {
                    markerPosition.X = this.X + 28 - (markerBeatOffset);

                    if (markerBeatOffset > 0)
                    {
                        _markerSprite.ColorShading.A = (byte)(Math.Max(0, 255 - 10 * markerBeatOffset));
                    }
                    else
                    {
                        _markerSprite.ColorShading.A = 255;
                    }
                }
                _markerSprite.Draw(spriteBatch, bn.Player, 5, 34, markerPosition);

            }
        }

        private void DrawBase(SpriteBatch spriteBatch)
        {
             _baseSprite.Draw(spriteBatch, Id, this.Width, this.Height, this.X, this.Y);
        }

        public void AddBeatlineNote(BeatlineNote bln)
        {
            _beatlineNotes.Add(bln);
        }

        public void RemoveAll()
        {
            _beatlineNotes.Clear();
        }

        public int TrimExpired(double phraseNumber)
        {
            _notesToRemove.Clear();
            foreach (BeatlineNote bn in _beatlineNotes)
            {
                if ((CalculateHitOffset(bn,phraseNumber) < -200))
                {
                    _notesToRemove.Add(bn);
                }
            }
        
            foreach (BeatlineNote bnr in _notesToRemove)
            {
                _beatlineNotes.Remove(bnr);
            }
            return (from e in _notesToRemove where (!e.Hit) select e).Count();
        }

        public double CalculateHitOffset(double phraseNumber)
        {
            if (NearestBeatlineNote(phraseNumber) != null)
            {
                return CalculateHitOffset(NearestBeatlineNote(phraseNumber), phraseNumber);
            }
            return 9999;
        }

        public double CalculateHitOffset(BeatlineNote bln, double phraseNumber)
        {
            return (bln.Position - phraseNumber) * 1000 * 240 / Bpm;
        }

        public BeatlineNote NearestBeatlineNote(double phraseNumber)
        {
            return (from e in _beatlineNotes where (!e.Hit) orderby CalculateHitOffset(e,phraseNumber) select e).FirstOrDefault();
        }

        private int CalculateAbsoluteBeatlinePosition(double position, double phraseNumber)
        {
            return (int)((position - phraseNumber) * Speed * BEAT_ZOOM_DISTANCE);
        }

        public BeatlineNoteJudgement DetermineJudgement(double phraseNumber, bool completed)
        {
            var nearest = NearestBeatlineNote(phraseNumber);
            double offset = (nearest == null) ? 9999 : CalculateHitOffset(nearest,phraseNumber);
            offset = Math.Abs(offset);

            if (offset > HIT_IGNORE_CUTOFF)
            {
                //Note that the COUNT judgement is essentially an 'ignored' judgement.
                return BeatlineNoteJudgement.COUNT;
            }
            BeatlineNoteJudgement result = BeatlineNoteJudgement.FAIL;
            if (completed)
            {
                for (int x = 0; x < NoteJudgementSet.JudgementCutoffs.Count(); x++)
                {
                    if (offset < NoteJudgementSet.JudgementCutoffs[x])
                    {
                        result = (BeatlineNoteJudgement) x;
                        break;
                    }
                }
            }
            //Mark the beatlinenote as hit (it will be displayed differently and hold position)
            if (nearest != null)
            {
                nearest.Hit = true;
                nearest.DisplayPosition = CalculateAbsoluteBeatlinePosition(nearest.Position, phraseNumber);
                nearest.Position = phraseNumber + 0.3;
            }

            return result;
        }


    }
}
