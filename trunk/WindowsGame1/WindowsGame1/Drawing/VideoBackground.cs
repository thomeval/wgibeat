using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using SeeSharp.Xna.Video;

namespace WGiBeat.Drawing
{
    public class VideoBackground
    {
        private VideoPlayer _videoPlayer;
        private Sprite outputSprite;

        private string _filePath;
        public string FilePath
        {
            get { return _videoPlayer.FileName; }
            set
            {
                if (_videoPlayer != null)
                {
                    Dispose();
                }
                _videoPlayer = new VideoPlayer(value, GameCore.Instance.GraphicsDevice);
                _videoPlayer.OnVideoComplete += (s, e) =>
                                                    {
                                                        try
                                                        {
                                                            _videoPlayer.Stop();
                                                        }
                                                        catch (Exception)
                                                        {
                                                            
                                                            throw;
                                                        }
                                                        finally
                                                        {
                                                            _videoPlayer.Play();
                                                        }
                                                        
                                                    };

            }
        }

        public VideoBackground()
        {
            outputSprite = new Sprite
                               {
                                   Height = 600,
                                   Width = 800
                               };
        }
        public void Draw(SpriteBatch spriteBatch)
        {
          if (_videoPlayer == null)
          {
              return;
          }
           
            outputSprite.SpriteTexture = _videoPlayer.OutputFrame;
            outputSprite.Draw(spriteBatch);
        }

        public void Play()
        {
            _videoPlayer.Play();
        }

        public void Dispose()
        {
            _videoPlayer.Stop();
            _videoPlayer.Dispose();
        }

        public void Update()
        {
            _videoPlayer.Update();
        }
    }
}
