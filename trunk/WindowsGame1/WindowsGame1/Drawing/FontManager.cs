using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    static internal class FontManager
    {
        private static readonly Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();
        private const string DEFAULT_FONT = "DefaultFont";
        public static Matrix FontMatrix;
        public static SpriteBatch SpriteBatch { get; set; }
        public static SpriteFont Fonts(string id)
        {
            return _fonts[id.ToUpper()];
        }

        public static void AddFont(string name, SpriteFont font)
        {
            _fonts.Add(name.ToUpper(), font);
        }

        private static readonly Vector2 _noRotation = new Vector2(0, 0);
        private static readonly Vector2 _noScaling = new Vector2(1, 1);
        public static void DrawString(string text, string fontName, Vector2 position, Vector2 scale, Color color, FontAlign align)
        {
            if (SpriteBatch == null)
            {
                throw new Exception("FontManager requires a SpriteBatch to work.");
            }

            var measuredPosition = new Vector2(position.X, position.Y);
            var lines = text.Split('\n');
            fontName = fontName.ToUpper();
           
    
            SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None,FontMatrix);
            
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
               
                SpriteBatch.DrawString(_fonts[fontName], line, measuredPosition, color, 0.0f, _noRotation, scale, SpriteEffects.None, 0.0f);
          
            
                measuredPosition.Y += measurements.Y;
                measuredPosition.X = position.X;
            }
           
            SpriteBatch.End();
            
        }

        public static void DrawString(string text, Vector2 position)
        {
            DrawString(text,DEFAULT_FONT,position);
        }
        public static void DrawString(string text, string fontName, Vector2 position)
        {
            DrawString(text, fontName, position, _noScaling,Color.Black, FontAlign.Left);
        }

        public static void DrawString(string text, string fontName, Vector2 position, Color color)
        {
            DrawString(text, fontName, position, _noScaling, color, FontAlign.Left);
        }

        public static void DrawString(string text, string fontName, Vector2 position, Color color, FontAlign align)
        {
            DrawString(text,fontName,position,_noScaling,color,align);
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
    }

    public enum FontAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }
}