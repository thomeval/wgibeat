using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private double _displayedSpeed;

        public double Bpm { get; set; }
        public int Id { get; set; }
        public bool DisablePulse { get; set; }

    

        private SpriteMap _markerSprite;
        private SpriteMap _playerIdentifiers;

        private Sprite _baseSprite;
        private Sprite _speedScaleSprite;

        private SpriteMap _beatlineEffects;
        private SpriteMap _pulseFront;
        private SpriteMap _pulseBack;
        private double _pulseFrontOpacity;
        private Vector2 _indicatorPosition;
       

        private const double SPEED_CHANGE_SPEED = 4;
        private const double PULSE_FADEOUT_SPEED = 500;
        private const double HIT_NOTE_FADEOUT_SPEED = 900;


        //When a player presses the beatline key, only notes closer than this cutoff are considered.
        //Otherwise, the event is ignored completely.
        public const int HIT_IGNORE_CUTOFF = 1000;

        //How distant the beatline notes are from each other.
        //Increase this to make them move faster and more apart.
        const int BEAT_ZOOM_DISTANCE = 200;


        //The distance from the left edge of the beatline to the impact area of the beatline.
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
            this.Height = 125;
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
            _speedScaleSprite = new Sprite
                                    {
                                        SpriteTexture = TextureManager.Textures("BeatlineSpeedScale")
                                    };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 0.0);
        }

        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {

            DrawSpeedScale(spriteBatch, phraseNumber);
            DrawBase(spriteBatch);
            DrawPlayerIdentifier(spriteBatch);
            DrawPulses(spriteBatch, phraseNumber);
            DrawNotes(spriteBatch, phraseNumber);

            var diff = Speed - _displayedSpeed;
            var changeMx = Math.Min(1, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * SPEED_CHANGE_SPEED);
            _displayedSpeed += diff*(changeMx);
        }

        private void DrawSpeedScale(SpriteBatch spriteBatch, double phraseNumber)
        {
            const int TEXTURE_WIDTH = 240;
            var textureOffset = ((phraseNumber - Math.Floor(phraseNumber)) * TEXTURE_WIDTH) + 2;
            _speedScaleSprite.ColorShading.A = 160;
            _speedScaleSprite.Height = 5;
            _speedScaleSprite.Position = this.Position.Clone();
            _speedScaleSprite.Width = this.Width - LEFT_SIDE - 25;
            _speedScaleSprite.X += LEFT_SIDE + IMPACT_WIDTH;

            var flip = SpriteEffects.None; 
      
            var mxFactor = 1.0* _speedScaleSprite.Width/BEAT_ZOOM_DISTANCE / _displayedSpeed; 
            _speedScaleSprite.Y += this.Height/2 - _speedScaleSprite.Height/2;
            _speedScaleSprite.DrawTiled(spriteBatch, (int)textureOffset, 0, (int)(TEXTURE_WIDTH * mxFactor), 5, flip);
            
        }

        private void DrawPlayerIdentifier(SpriteBatch spriteBatch)
        {
            _indicatorPosition.Y = this.Y + this.Height - 27;
     
            _indicatorPosition.X = this.X + this.Width - 60;
            _playerIdentifiers.Draw(spriteBatch,Id,_indicatorPosition);
        }

        private void DrawPulses(SpriteBatch spriteBatch, double phraseNumber)
        {
            if (DisablePulse)
            {
                return;
            }

  
            var markerPosition = new Vector2 { X = this.X, Y = this.Y + 3 };
            var markerHeight = this.Height - 12;
            var phraseDecimal = (phraseNumber - (int)phraseNumber);
            phraseDecimal = Math.Max(1 - (phraseDecimal * 4), 0);

            
            _pulseBack.ColorShading.A = (byte)(phraseDecimal * 255);
            _pulseFrontOpacity = Math.Max(0, _pulseFrontOpacity - TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * PULSE_FADEOUT_SPEED);
            _pulseFront.ColorShading.A = (byte) _pulseFrontOpacity;

            _pulseFront.Draw(spriteBatch, Id, this.Width, markerHeight, (int)markerPosition.X, (int)markerPosition.Y);

            if (phraseNumber < 0.0)
            {
                return;
            }

            _pulseBack.Draw(spriteBatch, Id, this.Width, markerHeight, (int)markerPosition.X, (int)markerPosition.Y);
        }

        private void DrawNotes(SpriteBatch spriteBatch, double phraseNumber)
        {
            foreach (BeatlineNote bn in _beatlineNotes)
            {
                var markerBeatOffset = (int)(_displayedSpeed * BEAT_ZOOM_DISTANCE * (phraseNumber - bn.Position));

                //Dont render notes outside the visibility range.
                if (((-1 * markerBeatOffset) > this.Width - LEFT_SIDE) && (!bn.Hit))
                {
                    continue;
                }

                var markerPosition = new Vector2 {Y = this.Y + 6, X = CalculateNotePosition(bn, markerBeatOffset)};
                 _markerSprite.ColorShading.A = CalculateNoteOpacity(bn,markerBeatOffset);

                var markerHeight = this.Height-12;

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
                //TODO: Doesn't work in high frame rates. Must fix.
                bn.Opacity = Math.Max(0, bn.Opacity - TextureManager.LastDrawnPhraseDiff * HIT_NOTE_FADEOUT_SPEED);
  
                result = (byte) bn.Opacity;
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

            result += this.X;
            return result;
        }

        private void DrawBase(SpriteBatch spriteBatch)
        {

                _baseSprite.SetPosition(this.X, this.Y);
                _baseSprite.Height = this.Height;
                _baseSprite.Width = this.Width;
                _baseSprite.Draw(spriteBatch);
        }

        public void AddBeatlineNote(BeatlineNote bln)
        {
            _beatlineNotes.Insert(0,bln);
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

            if (_notesToRemove.Count > 1)
            {
                Debug.WriteLine("Trimmed:" + _notesToRemove.Count);
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
            return (int)((position - phraseNumber) * _displayedSpeed * BEAT_ZOOM_DISTANCE);
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
            BeatlineNoteJudgement result = GetJudgementResult(offset, completed);
            //Mark the beatlinenote as hit (it will be displayed differently and hold position)
            if (nearest != null)
            {
                MarkNoteAsHit(nearest, phraseNumber);
            }
          
            return result;
        }

        private BeatlineNoteJudgement GetJudgementResult(double offset, bool completed)
        {
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
            return result;
        }


        public int AutoHit(double phraseNumber)
        {
            var passedNotes =
                (from e in _beatlineNotes where (!e.Hit) && (CalculateHitOffset(e, phraseNumber) < 0) 
                 && (e.NoteType == BeatlineNoteType.NORMAL ) select e).ToList();
            var result = passedNotes.Count();

            foreach (BeatlineNote bln in passedNotes)
            {
                MarkNoteAsHit(bln, phraseNumber);
            }
            return result;
        }

        private void MarkNoteAsHit(BeatlineNote bln, double phraseNumber)
        {
            bln.Hit = true;
            bln.Opacity = 255;
            bln.DisplayPosition = CalculateAbsoluteBeatlinePosition(bln.Position, phraseNumber);
            bln.Position = phraseNumber + 0.3;
        }

        public void ClearNotes()
        {
            _beatlineNotes.Clear();
        }


    }
}
