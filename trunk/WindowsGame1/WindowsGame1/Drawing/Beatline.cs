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

        private bool _reverseDirection;
        public bool ReverseDirection
        {
            get { return _reverseDirection; }
            set
            {
                _reverseDirection = value;
            }
        }

        private SpriteMap _markerSprite;
        private SpriteMap _playerIdentifiers;

        private Sprite _baseSprite;
        private Sprite _largeBaseSprite;
        private SpriteMap _beatlineEffects;
        private SpriteMap _pulseFront;
        private SpriteMap _pulseBack;
        private byte _pulseFrontOpacity;
        private Vector2 _indicatorPosition;

        public bool Large { get; set; }


        //When a player presses the beatline key, only notes closer than this cutoff are considered.
        //Otherwise, the event is ignored completely.
        public const int HIT_IGNORE_CUTOFF = 1000;

        //How distant the beatline notes are from each other.
        //Increase this to make them move faster and more apart.
        const int BEAT_ZOOM_DISTANCE = 200;

        private const int NORMAL_HEIGHT = 34;
        private const int LARGE_HEIGHT = 68;
        private const int LEFT_SIDE = 40;
        private const int IMPACT_WIDTH = 5;

        public Beatline()
        {
            _beatlineNotes = new List<BeatlineNote>();
            _notesToRemove = new List<BeatlineNote>();
            InitSprites();
        }

        private void InitSprites()
        {
            this.Height = 40;
            this.Width = 350;
            _markerSprite = new SpriteMap
            {
                Columns = 1,
                Rows = 5,
                SpriteTexture = TextureManager.Textures("BeatMarkers"),

            };

            _pulseFront = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures("BeatMeterPulseFront"),
                Rows = 5,
                Columns = 1,
            };

            _pulseBack = new SpriteMap
                             {
                                 SpriteTexture = TextureManager.Textures("BeatMeterPulseBack"),
                                 Rows = 5,
                                 Columns = 1,
                             };
            _baseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("BeatMeter")
            };
            _largeBaseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("BeatMeter"),
            };

            _beatlineEffects = new SpriteMap
                                   {
                                       SpriteTexture = TextureManager.Textures("BeatlineEffectIcons"),
                                       Columns = 1,
                                       Rows = 4
                                   };
            _playerIdentifiers = new SpriteMap
                                    {
                                        SpriteTexture = TextureManager.Textures("BeatlinePlayerIdentifiers"),
                                        Columns = 1,
                                        Rows = 5
                                    };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 0.0);
        }

        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            this.Height = Large ? 80 : 40;
            
            DrawBase(spriteBatch);
            DrawPlayerIdentifier(spriteBatch);
            DrawPulses(spriteBatch, phraseNumber);
            DrawNotes(spriteBatch, phraseNumber);
        }

        private void DrawPlayerIdentifier(SpriteBatch spriteBatch)
        {
            _indicatorPosition.Y = this.Y + this.Height - 27;
            _indicatorPosition.X = _reverseDirection ? this.X + 25 : this.X + this.Width - 60;
            _playerIdentifiers.Draw(spriteBatch,Id,_indicatorPosition);
        }

        private void DrawPulses(SpriteBatch spriteBatch, double phraseNumber)
        {
            if (DisablePulse)
            {
                return;
            }

            var flip = _reverseDirection ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            var markerPosition = new Vector2 { X = this.X, Y = Large ? this.Y + 6 : this.Y + 3 };
            var markerHeight = Large ? LARGE_HEIGHT : NORMAL_HEIGHT;
            var phraseDecimal = (phraseNumber - (int)phraseNumber);
            phraseDecimal = Math.Max(1 - (phraseDecimal * 6), 0);

            _pulseFront.ColorShading.A = _pulseFrontOpacity;
            _pulseBack.ColorShading.A = (byte)(phraseDecimal * 255);
            _pulseFrontOpacity = (byte)Math.Max(0, _pulseFrontOpacity - 20);

            _pulseFront.Draw(spriteBatch, Id, this.Width, markerHeight, (int)markerPosition.X, (int)markerPosition.Y, flip);

            if (phraseNumber < 0.0)
            {
                return;
            }

            _pulseBack.Draw(spriteBatch, Id, this.Width, markerHeight, (int)markerPosition.X, (int)markerPosition.Y, flip);
        }

        private void DrawNotes(SpriteBatch spriteBatch, double phraseNumber)
        {
            foreach (BeatlineNote bn in _beatlineNotes)
            {
                var markerBeatOffset = (int)(Speed * BEAT_ZOOM_DISTANCE * (phraseNumber - bn.Position));

                //Dont render notes outside the visibility range.
                if (((-1 * markerBeatOffset) > this.Width - LEFT_SIDE) && (!bn.Hit))
                {
                    continue;
                }

                var markerPosition = new Vector2 {Y = Large ? this.Y + 6 : this.Y + 3, X = CalculateNotePosition(bn, markerBeatOffset)};
                 _markerSprite.ColorShading.A = CalculateNoteOpacity(bn,markerBeatOffset);

                var markerHeight = Large ? LARGE_HEIGHT : NORMAL_HEIGHT;

                int noteIdx = 0;
                int width = IMPACT_WIDTH;

                switch (bn.NoteType)
                {
                    case BeatlineNoteType.NORMAL:
                        noteIdx = bn.Player;
                        break;
                    case BeatlineNoteType.END_OF_SONG:
                    case BeatlineNoteType.BPM_INCREASE:
                    case BeatlineNoteType.BPM_DECREASE:
                        case BeatlineNoteType.STOP:
                        width = 1;
                        noteIdx = 4;
                        break;
                }

                _markerSprite.Draw(spriteBatch, noteIdx, width, markerHeight, (int)markerPosition.X, (int)markerPosition.Y);

                //Draw the effect icon on top of the marker if appropriate (such as a BPM change arrow)
                //TODO: Flip effect icons for reverse direction.
                if (bn.NoteType != 0)
                {
                    _beatlineEffects.ColorShading.A = (byte) (_markerSprite.ColorShading.A  * 0.8);
                    _beatlineEffects.Draw(spriteBatch, (int)bn.NoteType - 1, markerHeight, markerHeight, (int)(markerPosition.X - markerHeight / 2), (int)markerPosition.Y);
                }
            }
        }

        /// <summary>
        /// Calculates the opacity of a beatline note depending on where it is. If it isn't hit, it will fade
        /// in and out when at the edges of the beatline. If it is hit, it will fade out gradually and remain in position.
        /// </summary>
        /// <param name="bn">The BeatlineNote to calculate the opacity for.</param>
        /// <param name="markerBeatOffset">The calculated markerBeatOffset (calculated as beatline speed * Zoom Distance (constant) * (phrase number - BeatlineNote position value))</param>
        /// <returns>The opacity of the beatline note.</returns>
        private byte CalculateNoteOpacity(BeatlineNote bn, int markerBeatOffset)
        {
            byte result;
            if (bn.Hit)
            {
                bn.Opacity = (byte)Math.Max(0, bn.Opacity - 8);
                result = bn.Opacity;
                return result;
            }

            var distTravelled = this.Width - LEFT_SIDE + markerBeatOffset;

            if (markerBeatOffset > 0)
            {
                result = (byte)(Math.Max(0, 255 - 10 * markerBeatOffset));
            }
            else if (distTravelled < 35)
            {
                result  = (byte)(7 * distTravelled);
            }
            else
            {
                result = 255;
            }
            return result;
        }

        private float CalculateNotePosition(BeatlineNote bn, int markerBeatOffset)
        {
            float result;
            if (bn.Hit)
            {
                result = LEFT_SIDE + (bn.DisplayPosition);
                result = Math.Min(this.Width, result);
                result = Math.Max(0, result);
            }
            else
            {
                result = LEFT_SIDE - (markerBeatOffset);
            }

            if (_reverseDirection)
            {
                result = this.Width - result;
                result -= 4;
            }
            result += this.X;
            return result;
        }

        private void DrawBase(SpriteBatch spriteBatch)
        {
            var flip = _reverseDirection ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            //TODO: Optimize to use one variable (change Sprite used instead of having two)
            //TODO: Fix broken big sprite.
            if (Large)
            {
                _largeBaseSprite.SetPosition(this.X, this.Y);
                _largeBaseSprite.Height = this.Height;
                _largeBaseSprite.Width = this.Width;
                _largeBaseSprite.Draw(spriteBatch, flip);
            }
            else
            {
                _baseSprite.SetPosition(this.X, this.Y);
                _baseSprite.Height = this.Height;
                _baseSprite.Width = this.Width;
                _baseSprite.Draw(spriteBatch,flip);
            }
        }

        public void AddBeatlineNote(BeatlineNote bln)
        {
            _beatlineNotes.Add(bln);
        }

        public void InsertBeatlineNote(BeatlineNote bln, int index)
        {
            _beatlineNotes.Insert(index,bln);
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
                if ((CalculateHitOffset(bn, phraseNumber) < -200))
                {
                    _notesToRemove.Add(bn);
                }
            }

            foreach (BeatlineNote bnr in _notesToRemove)
            {
                _beatlineNotes.Remove(bnr);
            }
            return (from e in _notesToRemove where (!e.Hit) && (e.NoteType == BeatlineNoteType.NORMAL) select e).Count();
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
            return (from e in _beatlineNotes where (!e.Hit && e.NoteType == BeatlineNoteType.NORMAL) orderby CalculateHitOffset(e, phraseNumber) select e).FirstOrDefault();
        }

        private int CalculateAbsoluteBeatlinePosition(double position, double phraseNumber)
        {
            return (int)((position - phraseNumber) * Speed * BEAT_ZOOM_DISTANCE);
        }

        public BeatlineNoteJudgement DetermineJudgement(double phraseNumber, bool completed)
        {
            _pulseFrontOpacity = 255;
            var nearest = NearestBeatlineNote(phraseNumber);
            double offset = (nearest == null) ? 9999 : CalculateHitOffset(nearest, phraseNumber);
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
                        result = (BeatlineNoteJudgement)x;
                        break;
                    }
                }
            }
            //Mark the beatlinenote as hit (it will be displayed differently and hold position)
            if (nearest != null)
            {
                nearest.Hit = true;
                nearest.Opacity = 255;
                nearest.DisplayPosition = CalculateAbsoluteBeatlinePosition(nearest.Position, phraseNumber);
                nearest.Position = phraseNumber + 0.3;
            }

            
            return result;
        }


        public int AutoHit(double phraseNumber)
        {
            var passedNotes =
                (from e in _beatlineNotes where (!e.Hit) && (CalculateHitOffset(e, phraseNumber) < 0) 
                 && (e.NoteType == BeatlineNoteType.NORMAL ) select e);
            var result = passedNotes.Count();

            foreach (BeatlineNote bln in passedNotes)
            {
                bln.Hit = true;
                bln.DisplayPosition = CalculateAbsoluteBeatlinePosition(bln.Position, phraseNumber);
                bln.Position = phraseNumber + 0.3;
            }
            return result;
        }

        public void ClearNotes()
        {
            _beatlineNotes.Clear();
        }


    }
}
