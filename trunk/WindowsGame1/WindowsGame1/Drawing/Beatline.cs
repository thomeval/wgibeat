﻿using System;
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
        private SpriteMap _markerSprite;
        private Sprite _pulseSprite;
        private SpriteMap _baseSprite;
        private SpriteMap _largeBaseSprite;
        private SpriteMap _beatlineEffects;

        public bool Large { get; set; }

        //Used by endpoint markers.

        public const int HIT_IGNORE_CUTOFF = 1000;
        //How distant the beatline notes are from each other.
        //Increase this to make them move faster and more apart.
        const int BEAT_ZOOM_DISTANCE = 200;

        //Prevent notes from being drawn outside the beatline base.
        //Higher means longer visibility range.
        public const double BEAT_VISIBILITY = 1.1;

        private const int NORMAL_HEIGHT = 34;
        private const int LARGE_HEIGHT = 68;
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
                SpriteTexture = TextureManager.Textures("BeatMarkers"),

            };

            _pulseSprite = new Sprite
            {
                SpriteTexture = TextureManager.Textures("BeatFlame")
            };
            _baseSprite = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures("BeatMeter"),
                Columns = 1,
                Rows = 5
            };
            _largeBaseSprite = new SpriteMap
            {
                SpriteTexture = TextureManager.Textures("BeatMeterLarge"),
                Columns = 1,
                Rows = 5
            };
            _beatlineEffects = new SpriteMap
                                   {
                                       SpriteTexture = TextureManager.Textures("BeatlineEffectIcons"),
                                       Columns = 1,
                                       Rows = 4
                                   };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 0.0);
        }

        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            this.Height = Large ? 80 : 40;
            DrawPulse(spriteBatch, phraseNumber);
            DrawBase(spriteBatch);
            DrawNotes(spriteBatch, phraseNumber);
         //   DrawEndPoints(spriteBatch, phraseNumber);
        }

/*
        private void DrawEndPoints(SpriteBatch spriteBatch, double phraseNumber)
        {
            var endpointBeatOffset = (int)(Speed * BEAT_ZOOM_DISTANCE * (phraseNumber - EndPhrase));

            //Dont render endpoint if outside the visibility range.


            var markerPosition = new Vector2 { X = this.X + 28 - (endpointBeatOffset) };
            var markerHeight = Large ? LARGE_HEIGHT : NORMAL_HEIGHT;
            markerPosition.Y = Large ? this.Y + 6 : this.Y + 3;
            _markerSprite.ColorShading.A = 255;
            _markerSprite.Draw(spriteBatch, 4, 1, markerHeight, markerPosition);
        }
*/

        private void DrawPulse(SpriteBatch spriteBatch, double phraseNumber)
        {
            if (DisablePulse)
            {
                return;
            }
            _pulseSprite.Width = (int)(80 * (Math.Ceiling(phraseNumber) - (phraseNumber)));
            _pulseSprite.ColorShading.A = (byte)(_pulseSprite.Width * 255 / 80);
            _pulseSprite.Height = this.Height;
            _pulseSprite.SetPosition(this.X + 30, this.Y - 3);
            _pulseSprite.DrawTiled(spriteBatch, 83 - _pulseSprite.Width, 0, _pulseSprite.Width, 36);
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

                var markerPosition = new Vector2 ();
                markerPosition.Y = Large ? this.Y + 6 : this.Y + 3;
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
                var markerHeight = Large ? LARGE_HEIGHT : NORMAL_HEIGHT;
                var effectHeight = Large ? LARGE_HEIGHT : NORMAL_HEIGHT;

                int noteIdx = 0;
                int width = 5;

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

                _markerSprite.Draw(spriteBatch,noteIdx, width, markerHeight, markerPosition);
                var effectType = (int)bn.NoteType;
                if (effectType != 0)
                {
                    _beatlineEffects.ColorShading.A = (byte) (_markerSprite.ColorShading.A  /2);
                    _beatlineEffects.Draw(spriteBatch, effectType - 1,effectHeight, effectHeight, (int)(markerPosition.X - effectHeight/2), (int)markerPosition.Y);
                }
            }
        }

        private void DrawBase(SpriteBatch spriteBatch)
        {
            if (Large)
            {
                           
                _largeBaseSprite.Draw(spriteBatch,Id, this.Width, this.Height, this.X, this.Y);

            }
            else
            {
                _baseSprite.Draw(spriteBatch, Id, this.Width, this.Height, this.X, this.Y);
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
