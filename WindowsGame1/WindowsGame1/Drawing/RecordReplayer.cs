using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;
using WGiBeat.Notes;

namespace WGiBeat.Drawing
{
    public class RecordReplayer : DrawableObject
    {

        private SpriteMap3D _partsSpriteMap;
        private SpriteMap3D _leftSpriteMap;
        private Sprite3D _middleSprite;
        private Sprite3D _rightSprite;
        private Sprite3D _headerSprite;

        private readonly GameType _gameType;
        public List<long> ScoreHistory { get; set; }


        public double Opacity { get; set; }

        public RecordReplayer(GameType gameType)
        {
            _gameType = gameType;
            
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
                Rows = 5
            };
            _middleSprite = new Sprite3D { Texture = TextureManager.Textures("PerformanceBarMiddle") };
            _rightSprite = new Sprite3D { Texture = TextureManager.Textures("PerformanceBarRight") };
            _headerSprite = new Sprite3D{ Texture = TextureManager.Textures("PerformanceBarHeader") };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

        Draw(spriteBatch,0);     
        }

        private const int FADEIN_SPEED = 120;
        private const int FADEOUT_SPEED = 120;
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

            
            var position = this.Position.Clone();

            var displayScore = GetScoreToDisplay(phraseNumber);
            var maxScore = GetMaxScore();
            if (displayScore == 0)
            {
                return;
            }
            var ratio = 1.0 * maxScore/displayScore;

            if (ratio > 0.8 && ratio < 1.3)
            {
                Opacity += FADEIN_SPEED*TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds;
            }
            else
            {
                Opacity -= FADEOUT_SPEED*TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds;
            }
            Opacity = Math.Min(Math.Max(0, Opacity), 255);
            var drawColor = Color.Black;
            drawColor.A = (byte)Opacity;  
            TextureManager.DrawString(spriteBatch,string.Format("Record: {0}",displayScore),"LargeFont",position,drawColor, FontAlign.LEFT);
            position.Y += 30;
            DrawChallenges(spriteBatch, displayScore, position);
        }

        private void DrawChallenges(SpriteBatch spriteBatch, long recordScore, Vector2 position)
        {
            var drawColor = Color.Black;
            drawColor.A = (byte) Opacity;
            switch (_gameType)
            {
                case GameType.COOPERATIVE:
                case GameType.SYNC_PLUS:
                case GameType.SYNC_PRO:
                   DrawSingleBar(spriteBatch,position,5,GetMaxScore(),recordScore);
                                      
                    break;
                default:
                    for (int x = 0; x < 4; x++)
                    {
                        if (!GameCore.Instance.Players[x].IsHumanPlayer)
                        {
                            continue;
                        }
                    
                        DrawSingleBar(spriteBatch,position,x,GameCore.Instance.Players[x].Score,recordScore);
                        position.Y += this.Height;
                    }
                    break;
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

            var idx = (GameCore.Instance.Players[player].IsCPUPlayer) ? 4 : player;

            _leftSpriteMap.Draw(idx, LEFT_SIDE_WIDTH, this.Height, position);
            position.X += LEFT_SIDE_WIDTH;
            _middleSprite.Width = barWidth;
            _middleSprite.Height = this.Height;
            _middleSprite.Position = position.Clone();
 
            _partsSpriteMap.ColorShading.A = (byte) Opacity;

            var ratio = 1.0 * challengeScore/recordScore;
            ratio = Math.Max(Math.Min(1.2, ratio), 0.8);
            var width = barWidth*(ratio - 0.8) / 0.4;
                    _partsSpriteMap.Draw(ratio >= 1.0 ? 0 : 3, (float) width, this.Height, position);              

            _middleSprite.Draw();
            _rightSprite.Draw();
            _rightSprite.X += RIGHT_SIDE_WIDTH / 2;
            _rightSprite.Y += (this.Height / 2) - 10;
            var textColour = Color.Black;
            textColour.A = (byte)Opacity;
            TextureManager.DrawString(spriteBatch, "" + challengeScore, "DefaultFont", _rightSprite.Position, textColour,
                                      FontAlign.CENTER);

     
        }

        private long GetMaxScore()
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
