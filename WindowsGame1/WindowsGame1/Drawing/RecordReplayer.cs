using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;

namespace WGiBeat.Drawing
{
    public class RecordReplayer : DrawableObject
    {

        private SpriteMap3D _partsSpriteMap;
        private SpriteMap3D _leftSpriteMap;
        private Sprite3D _middleSprite;
        private Sprite3D _rightSprite;
        private Sprite3D _headerSprite;

        private double _displayedRecordScore;
        private readonly double[] _displayedPlayerScore = new double[4];
        private readonly GameType _gameType;
        public List<long> ScoreHistory { get; set; }


        public double Opacity { get; set; }
        public bool AutoHide { get; set; }
        public RecordReplayer(GameType gameType)
        {
            _gameType = gameType;
            AutoHide = true;
        }

        private bool UseCombinedScoring
        {
            get { return _gameType == GameType.COOPERATIVE 
                || _gameType == GameType.SYNC_PRO
                || _gameType == GameType.SYNC_PLUS; }
         
        }

        private void InitSprites()
        {
            _partsSpriteMap = new SpriteMap3D
            {
                Texture = TextureManager.Textures("PerformanceBarParts"),
                Columns = 1,
                Rows = 6
            };
            _leftSpriteMap = new SpriteMap3D
            {
                Texture = TextureManager.Textures("PerformanceBarLeft"),
                Columns = 1,
                Rows = 6
            };
         
            _middleSprite = new Sprite3D { Texture = TextureManager.Textures("RecordReplayBarMiddle") };
            _rightSprite = new Sprite3D { Texture = TextureManager.Textures("PerformanceBarRight") };
            _headerSprite = new Sprite3D
                                {
                                    Texture = TextureManager.Textures("RecordReplayBarHeader"),
                                    Size = new Vector2(290,30)
                                };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        Draw(spriteBatch,0);     
        }

        private const int FADEIN_SPEED = 240;
        private const int FADEOUT_SPEED = 240;
        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            if (_middleSprite == null)
            {
                InitSprites();
            }
            if (ScoreHistory == null)
            {
                return;
            }

            UpdateDisplayedScores(phraseNumber);
            var position = this.Position.Clone();

         
            var maxScore = GetCombinedScore();
            if (_displayedRecordScore == 0)
            {
                return;
            }
            var ratio = 1.0 * maxScore / _displayedRecordScore;
          
            SetOpacity(ratio, phraseNumber);
            var drawColor = Color.Black;
            drawColor.A = (byte)Opacity;
            position.X += this.Width - _headerSprite.Width;
            _headerSprite.Position = position.Clone();
            _headerSprite.ColorShading.A = (byte) Opacity;
            _headerSprite.Draw();
     
            var textPosition = new Vector2(this.X + this.Width - RIGHT_SIDE_WIDTH/2, this.Y - 13 + (_headerSprite.Height/2));
            TextureManager.DrawString(spriteBatch, string.Format("{0:F0}", _displayedRecordScore), "DefaultFont", textPosition, drawColor, FontAlign.CENTER);
            position.X = this.X;
            position.Y += 30;
            DrawChallenges(spriteBatch, (long) _displayedRecordScore, position);
        }

        private void UpdateDisplayedScores(double phraseNumber)
        {

            AnimateNumber(ref _displayedRecordScore, GetScoreToDisplay(phraseNumber));

            if (UseCombinedScoring)
            {
                AnimateNumber(ref _displayedPlayerScore[0],GetCombinedScore());
                return;
            }

            for (int x = 0; x < 4; x++)
            {
                AnimateNumber(ref _displayedPlayerScore[x], GameCore.Instance.Players[x].Score);                   
            }
        }

        private const int SCORE_UPDATE_SPEED = 12;
        private void AnimateNumber(ref double displayedNumber, double acutalNumber)
        {
        
            var diff = acutalNumber - displayedNumber;
            if (diff < 0.5)
            {
                displayedNumber += diff;
            }
            else
            {
                displayedNumber += diff*(Math.Min(0.5, TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds*SCORE_UPDATE_SPEED));
            }
        }


        private void SetOpacity(double ratio, double phraseNumber)
        {
            if (!AutoHide)
            {
                return;
            }

            if ((phraseNumber >= 10) && (ratio > 0.8 && ratio <1.3))
            {
                Opacity += FADEIN_SPEED*TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds;
            }
            else
            {
                Opacity -= FADEOUT_SPEED*TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds;
            }
            Opacity = Math.Min(Math.Max(0, Opacity), 255);
        }

        private void DrawChallenges(SpriteBatch spriteBatch, long recordScore, Vector2 position)
        {
            var drawColor = Color.Black;
            drawColor.A = (byte) Opacity;

            if (UseCombinedScoring)
            {
                DrawSingleBar(spriteBatch, position, 5, (long) _displayedPlayerScore[0], recordScore);
            }
            else
            {
                for (int x = 0; x < 4; x++)
                {
                    if (!GameCore.Instance.Players[x].IsHumanPlayer)
                    {
                        continue;
                    }

                    DrawSingleBar(spriteBatch, position, x, (long) _displayedPlayerScore[x], recordScore);
                    position.Y += this.Height;
                }
            }
        }


        private const int LEFT_SIDE_WIDTH = 50;
        private const int RIGHT_SIDE_WIDTH = 90;
        private void DrawSingleBar(SpriteBatch spriteBatch, Vector2 position, int player, long challengeScore, long recordScore)
        {
            position = position.Clone();
            _rightSprite.ColorShading.A =
                _leftSpriteMap.ColorShading.A =
                _middleSprite.ColorShading.A = (byte)Opacity;
            _rightSprite.Position = position.Clone();
            _rightSprite.Size = new Vector2 (RIGHT_SIDE_WIDTH,this.Height);
            _rightSprite.X += this.Width - RIGHT_SIDE_WIDTH;
            var barWidth = this.Width - LEFT_SIDE_WIDTH - RIGHT_SIDE_WIDTH;

            var idx = (player < 5 && GameCore.Instance.Players[player].IsCPUPlayer) ? 4 : player;

            _leftSpriteMap.Draw(idx, LEFT_SIDE_WIDTH, this.Height, position);
            position.X += LEFT_SIDE_WIDTH;
            _middleSprite.Width = barWidth;
            _middleSprite.Height = this.Height;
            _middleSprite.Position = position.Clone();
 
            _partsSpriteMap.ColorShading.A = (byte) Opacity;

            var ratio = 1.0 * challengeScore/recordScore;
            ratio = Math.Max(Math.Min(1.2, ratio), 0.8);
            var width = barWidth*(ratio - 0.8) / 0.4;
                    _partsSpriteMap.Draw(GetBarColour(ratio), (float) width, this.Height, position);              

            _middleSprite.Draw();
            _rightSprite.Draw();
            _rightSprite.X += RIGHT_SIDE_WIDTH / 2;
            _rightSprite.Y += (this.Height / 2) - 10;
            var textColour = Color.Black;
            textColour.A = (byte)Opacity;
            TextureManager.DrawString(spriteBatch, "" + challengeScore, "DefaultFont", _rightSprite.Position, textColour,
                                      FontAlign.CENTER);

     
        }

        private int GetBarColour(double ratio)
        {
            if (ratio > 0.9999999 && ratio < 1.0000001)
            {
                return 1;
            }
            return ratio > 1 ? 0 : 3;
        }

        private long GetCombinedScore()
        {
            switch (_gameType)
            {
                case GameType.COOPERATIVE:
                    return (from e in GameCore.Instance.Players where e.Playing select e.Score).Sum();
                case GameType.SYNC_PLUS:
                case GameType.SYNC_PRO:
                    return GameCore.Instance.Players[0].Score;
                default:
                    return (from e in GameCore.Instance.Players where e.IsHumanPlayer select e.Score).Max();
            }
            
        }

        private long GetScoreToDisplay(double phraseNumber)
        {
            if (phraseNumber < 0 )
            {
                return 0;
            }
            var idx = (int)Math.Min(ScoreHistory.Count() - 1, Math.Floor(phraseNumber));
            idx = Math.Max(0, idx);
            return ScoreHistory[idx];
        }

        public void LoadRecord(int hashCode, GameType gameType)
        {
            try
            {
                gameType = HighScoreManager.TranslateGameType(gameType);
                var replaylocation = GameCore.Instance.WgibeatRootFolder + "\\Replays\\" + hashCode + "-" + gameType + ".wrp";
                if (!File.Exists(replaylocation))
                {
                    GameCore.Instance.Log.AddMessage(string.Format("{0} has no replay file available.", hashCode + ".wrp"), LogLevel.INFO);
                    ScoreHistory = null;
                    return;
                }
               

                var fs = new FileStream(replaylocation, FileMode.Open, FileAccess.Read);
                var bf = new BinaryFormatter();
                ScoreHistory = (List<long>)bf.Deserialize(fs);
                fs.Close();
                GameCore.Instance.Log.AddMessage(string.Format("{0} loaded successfully.", hashCode + ".wrp"), LogLevel.INFO);
            }
            catch (Exception ex)
            {
                GameCore.Instance.Log.AddMessage(string.Format("Could not load replay file: {0}. {1}",hashCode + ".wrp",ex.Message),LogLevel.WARN);
                ScoreHistory = null;
                throw;
            }
        
        }

        public static void SaveRecord(List<long> scoreHistory, int hashCode, GameType gameType )
        {
            try
            {
                var folder = GameCore.Instance.WgibeatRootFolder + "\\Replays\\";
                var replaylocation = folder + hashCode + "-" + gameType + ".wrp";

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }


                var fs = new FileStream(replaylocation, FileMode.Create, FileAccess.Write);
                var bf = new BinaryFormatter();
                bf.Serialize(fs, scoreHistory);
                fs.Close();
                GameCore.Instance.Log.AddMessage(string.Format("{0} saved successfully.", hashCode + ".wrp"), LogLevel.INFO);
            }
            catch (Exception ex)
            {
                GameCore.Instance.Log.AddMessage(string.Format("Could not save replay file: {0}. {1}", hashCode + ".wrp", ex.Message), LogLevel.WARN);
                throw;
            }
  
        }

   
    }
}
