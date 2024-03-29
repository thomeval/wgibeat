﻿using System;
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

    

        private SpriteMap3D _markerSprite;
        private SpriteMap3D _playerIdentifiers;

        private Sprite3D _baseSprite;

        private Sprite3D _speedScaleSprite;

        private SpriteMap3D _beatlineEffects;
        private SpriteMap3D _pulseFront;
        private SpriteMap3D _pulseBack;
        private double _pulseFrontOpacity;
        private Vector2 _indicatorPosition;
        public Vector2 IdentifierSize { get; set; }
        public Vector2 EffectIconSize { get; set; }


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
            Colour = Color.White;
        }

        private void InitSprites()
        {
     
            
            _markerSprite = new SpriteMap3D
            {
                Columns = 1,
                Rows = 6,
                Texture = TextureManager.Textures("BeatMarkers"),

            };

            _pulseFront = new SpriteMap3D
            {
                Texture = TextureManager.Textures("BeatMeterPulseFront"),
                Rows = 5,
                Columns = 1,
            };

            _pulseBack = new SpriteMap3D
                             {
                                 Texture = TextureManager.Textures("BeatMeterPulseBack"),
                                 Rows = 5,
                                 Columns = 1,
                             };
            _baseSprite = new Sprite3D
            {
                Texture = TextureManager.Textures("BeatMeter"),
                Position = this.Position,
                Size = this.Size
            };

            _beatlineEffects = new SpriteMap3D
                                   {
                                       Texture = TextureManager.Textures("BeatlineEffectIcons"),
                                       Columns = 1,
                                       Rows = 4
                                   };
            _playerIdentifiers = new SpriteMap3D
                                    {
                                        Texture = TextureManager.Textures("BeatlinePlayerIdentifiers"),
                                        Columns = 1,
                                        Rows = 5
                                    };
            _speedScaleSprite = new Sprite3D
                                    {
                                        Texture = TextureManager.Textures("BeatlineSpeedScale"),
                                        Size = new Vector2(this.Width - LEFT_SIDE - 25, 5),
                                        X =  (float) (this.Position.X + (LEFT_SIDE + IMPACT_WIDTH) * WidthRatio),
                                        Y =  (this.Position.Y + (this.Height / 2.0f) - 2)
                                    };
       
        }

        protected double WidthRatio
        {
            get { return this.Width/350.0; }
    
        }

        public Color Colour { get; set; }

        public override void Draw()
        {
            Draw(0.0);
        }

        public void Draw(double phraseNumber)
        {
            if (_baseSprite == null)
            {
                InitSprites();
                
            }
            _baseSprite.ColorShading = Colour;
            _speedScaleSprite.ColorShading = Colour;

            DrawSpeedScale(phraseNumber);
            DrawBase();
            DrawPlayerIdentifier();
            DrawPulses(phraseNumber);
            DrawNotes(phraseNumber);

            var diff = Speed - _displayedSpeed;
            var changeMx = Math.Min(1, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * SPEED_CHANGE_SPEED);
            _displayedSpeed += diff*(changeMx);
        }

        private void DrawSpeedScale(double phraseNumber)
        {
            const int TEXTURE_WIDTH = 240;
            var textureOffset = ((phraseNumber - Math.Floor(phraseNumber)) * TEXTURE_WIDTH) + 2;
            _speedScaleSprite.ColorShading.A = 160;

      
            var mxFactor = 1.0* _speedScaleSprite.Width/BEAT_ZOOM_DISTANCE / _displayedSpeed; 

            _speedScaleSprite.DrawTiled((float) textureOffset, 0, (float) (_speedScaleSprite.Texture.Width * mxFactor), _speedScaleSprite.Texture.Height);
            
        }

        private void DrawPlayerIdentifier()
        {
            _indicatorPosition.Y = this.Y + this.Height - IdentifierSize.Y - 5;
     
            _indicatorPosition.X = this.X + this.Width - IdentifierSize.X - 5;
            _playerIdentifiers.Draw(Id, IdentifierSize.X, IdentifierSize.Y,_indicatorPosition);
        }

        private void DrawPulses(double phraseNumber)
        {
            if (DisablePulse)
            {
                return;
            }


            var pulsePosition = new Vector2 {X = this.X, Y = this.Y + 6};
            var pulseHeight = this.Height - 12;
            var phraseDecimal = (phraseNumber - (int) phraseNumber);
            phraseDecimal = Math.Max(1 - (phraseDecimal*4), 0);


            _pulseBack.ColorShading.A = (byte) (phraseDecimal*255);
            _pulseFrontOpacity = Math.Max(0,
                                          _pulseFrontOpacity -
                                          TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds*PULSE_FADEOUT_SPEED);
            _pulseFront.ColorShading.A = (byte) _pulseFrontOpacity;

            _pulseFront.Draw(Id, this.Width, pulseHeight, pulsePosition);

            if (phraseNumber < 0.0)
            {
                return;
            }

            _pulseBack.Draw(Id, this.Width, pulseHeight, pulsePosition);
        }

        private void DrawNotes(double phraseNumber)
        {
            foreach (BeatlineNote bn in _beatlineNotes)
            {
                var markerBeatOffset = (_displayedSpeed * BEAT_ZOOM_DISTANCE * (phraseNumber - bn.Position));

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
                    case BeatlineNoteType.Normal:
                        noteIdx = this.Id;
                        break;
                    case BeatlineNoteType.Super:
                        noteIdx = 5;
                        break;
                    case BeatlineNoteType.EndOfSong:
                    case BeatlineNoteType.BPMIncrease:
                    case BeatlineNoteType.BPMDecrease:
                        case BeatlineNoteType.Stop:
                        width = 1;
                        noteIdx = 4;
                        break;
                }

                _markerSprite.Draw(noteIdx, width, markerHeight,markerPosition);
     
                //Draw the effect icon on top of the marker if appropriate (such as a BPM change arrow)
                if ((bn.NoteType != BeatlineNoteType.Normal) && (bn.NoteType != BeatlineNoteType.Super))
                {
                    _beatlineEffects.ColorShading.A = (byte) (_markerSprite.ColorShading.A  * 0.8);
                    _beatlineEffects.Draw((int)bn.NoteType - 1, EffectIconSize, new Vector2(markerPosition.X - EffectIconSize.X / 2.0f,markerPosition.Y));
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
        private byte CalculateNoteOpacity(BeatlineNote bn, double markerBeatOffset)
        {
            byte result;
            if (bn.Hit)
            {
         
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

        private float CalculateNotePosition(BeatlineNote bn, double markerBeatOffset)
        {
            float result;
            if (bn.Hit)
            {
                result = (float)((LEFT_SIDE * WidthRatio )+ (bn.DisplayPosition));
                result = Math.Min(this.Width, result);
                result = Math.Max(0, result);
            }
            else
            {
                result = (float) ((LEFT_SIDE * WidthRatio) - (markerBeatOffset));
            }

            result += this.X;
            return result;
        }

        private void DrawBase()
        {
           _baseSprite.Draw();
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
            foreach (var bn in _beatlineNotes)
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
            foreach (var bnr in _notesToRemove)
            {
                _beatlineNotes.Remove(bnr);
            }
            return (from e in _notesToRemove where (!e.Hit) && (e.NoteType == BeatlineNoteType.Normal) select e).Count();
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
            return (from e in _beatlineNotes where (!e.Hit && e.CanBeHit) orderby CalculateHitOffset(e, phraseNumber) select e).FirstOrDefault();
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
                return BeatlineNoteJudgement.Count;
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
            var result = BeatlineNoteJudgement.Fail;
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
                 && (e.NoteType == BeatlineNoteType.Normal ) select e).ToList();
            var result = passedNotes.Count();

            foreach (var bln in passedNotes)
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
