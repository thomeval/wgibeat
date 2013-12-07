using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WGiBeat.Managers;

namespace WGiBeat.Drawing
{
    public static class TextureManager
    {
        private static readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();
        public static GraphicsDevice GraphicsDevice;
        public static LogManager Log;
        public static Matrix FontMatrix;

        public static Texture2D Textures(string id)
        {
            if (!_textures.ContainsKey(id.ToUpper()))
            {
                Log.AddMessage(String.Format("Texture {0} is missing.", id.ToUpper()), LogLevel.ERROR);  
                
                if (_textures.ContainsKey("MISSINGGRAPHIC"))
                {
                    _textures.Add(id.ToUpper(),_textures["MISSINGGRAPHIC"]);
                }
                else
                {
                    _textures.Add(id.ToUpper(), new Texture2D(GraphicsDevice, 1, 1)); 
                }
                
            }
            return _textures[id.ToUpper()];
        }
        public static SpriteFont Fonts(string id)
        {
            return _fonts[id.ToUpper()];
        }
        public static void AddTexture(string name, Texture2D tex)
        {
            
            _textures.Add(name, tex);
        }

        public static void CreateAndAddTexture(string filename)
        {
            CreateAndAddTexture(filename, Path.GetFileNameWithoutExtension(filename));   
        }
   
        public static void CreateAndAddTexture(string filename, string assetName)
        {
            var newTexture = Texture2D.FromFile(GraphicsDevice, filename);
            assetName = assetName.ToUpper();
            if (!_textures.ContainsKey(assetName))
            {
                _textures.Add(assetName, newTexture);
            }
            else
            {
                _textures[assetName] = newTexture;
            }
        }

        public static void LoadTheme(string foldername)
        {
            string[] validExtensions = {".bmp", ".png", ".jpg", ".jpeg"};

            var folders = new Stack<string>();
            folders.Push(foldername);
            while (folders.Count > 0)
            {
                var currentFolder = folders.Pop();
                foreach (string folder in Directory.GetDirectories(currentFolder))
                {
                    folders.Push(folder);
                }

                foreach (string file in Directory.GetFiles(currentFolder))
                {
                    if (validExtensions.Contains(Path.GetExtension(file)))
                    {
                        CreateAndAddTexture(file);
                    }
                }               
            }
        }

        public static void AddFont(string name, SpriteFont font)
        {
            _fonts.Add(name.ToUpper(), font);
        }

        private static readonly Vector2 _noRotation = new Vector2(0, 0);
        private static readonly Vector2 _noScaling = new Vector2(1, 1);
        public static void DrawString(SpriteBatch spriteBatch, string text, string fontName, Vector2 position, Vector2 scale, Color color, FontAlign align)
        {
            
            var measuredPosition = new Vector2(position.X, position.Y);
            var lines = text.Split('\n');
            fontName = fontName.ToUpper();
           
    
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None,FontMatrix);
            
            foreach (string line in lines)
            {
                var measurements = _fonts[fontName].MeasureString(line);
                measurements *= scale;
                switch (align)
                {
                    case FontAlign.Center:
                        measuredPosition.X -= measurements.X/2;
                        break;
                    case FontAlign.Right:
                        measuredPosition.X -= measurements.X;
                        break;
                }
               
                spriteBatch.DrawString(_fonts[fontName], line, measuredPosition, color, 0.0f, _noRotation, scale, SpriteEffects.None, 0.0f);
          
            
                measuredPosition.Y += measurements.Y;
                measuredPosition.X = position.X;
            }
           
                spriteBatch.End();
            
        }

        public static void DrawString(SpriteBatch spriteBatch, string text, string fontName, Vector2 position)
        {
            DrawString(spriteBatch, text, fontName, position, _noScaling,Color.Black, FontAlign.Left);
        }

        public static void DrawString(SpriteBatch spriteBatch, string text, string fontName, Vector2 position, Color color)
        {
            DrawString(spriteBatch, text, fontName, position, _noScaling, color, FontAlign.Left);
        }

        public static void DrawString(SpriteBatch spriteBatch, string text, string fontName, Vector2 position, Color color, FontAlign align)
        {
            DrawString(spriteBatch,text,fontName,position,_noScaling,color,align);
        }

        public static Vector2 ScaleTextToFit(string text, string fontName, Vector2 maxSize)
        {
            fontName = fontName.ToUpper();
            if (!_fonts.ContainsKey(fontName))
            {
                throw new ArgumentException("TextureManager does not contain a font called: " + fontName);
            }
            var actualSize = _fonts[fontName].MeasureString(text);
            var result = new Vector2(maxSize.X/actualSize.X, maxSize.Y/actualSize.Y);

            result.X = Math.Min(1.0f, result.X);
            result.Y = Math.Min(1.0f, result.Y);
            return result;
        }

        public static Vector2 ScaleTextToFit(string text, string fontName, int width, int height)
        {
            return ScaleTextToFit(text, fontName, new Vector2(width, height));
        }

        public static Texture2D BlankTexture()
        {
            var tex = new Texture2D(GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            var pixels = new Color[1];
            pixels[0] = Color.White;
            tex.SetData(pixels);
            return tex;
        }

        public static Texture2D CreateWhiteMask(string assetName)
        {
            var source = Textures(assetName);
            var result = new Texture2D(GraphicsDevice,source.Width,source.Height);
            var pixels = new Color[source.Width*source.Height];
            Textures(assetName).GetData(pixels);
            for (int x = 0; x < pixels.Length; x++)
            {
                pixels[x].R = 255;
                pixels[x].B = 255;
                pixels[x].G = 255;
            }
            result.SetData(pixels);
            return result;
        }

        private static GameTime _lastGameTime = new GameTime();
        public static GameTime LastGameTime
        {
            get { return _lastGameTime; }
            set { _lastGameTime = value; }
        }

        private static double _lastDrawnPhraseNumber;
        public static double LastDrawnPhraseNumber
        {
            get { return _lastDrawnPhraseNumber; }
            set
            {
                LastDrawnPhraseDiff = Math.Max(0,value - LastDrawnPhraseNumber);
                _lastDrawnPhraseNumber = value;
            }
        }

        public static double LastDrawnPhraseDiff { get; private set; }
  
    }

    public enum FontAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }
}
