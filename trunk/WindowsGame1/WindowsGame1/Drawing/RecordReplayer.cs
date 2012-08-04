using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;

namespace WGiBeat.Drawing
{
    public class RecordReplayer : DrawableObject
    {
        public List<long> ScoreHistory { get; set; }

  
        public override void Draw(SpriteBatch spriteBatch)
        {
        Draw(spriteBatch,0);     
        }

        public void Draw(SpriteBatch spriteBatch, double phraseNumber)
        {
            if (ScoreHistory == null)
            {
                return;
            }

            var position = this.Position.Clone();

            var displayScore = GetScoreToDisplay(phraseNumber);         
            TextureManager.DrawString(spriteBatch,string.Format("Record: {0}",displayScore),"LargeFont",position,Color.Black, FontAlign.LEFT);

            for (int x = 0; x < 4; x++)
            {
                if (!GameCore.Instance.Players[x].Playing)
                {
                    continue;
                }
                position.Y += 25;               
                TextureManager.DrawString(spriteBatch,string.Format("P{0}: {1}",x+1,GameCore.Instance.Players[x].Score),"LargeFont",position,Color.Black,FontAlign.LEFT);
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
