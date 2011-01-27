using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public static class TextureManager
    {
        private static readonly Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, SpriteFont> _fonts = new Dictionary<string, SpriteFont>();
        public static GraphicsDevice GraphicsDevice;

        public static Texture2D Textures(string id)
        {
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

            foreach (string file in Directory.GetFiles(foldername))
            {
               if (validExtensions.Contains(Path.GetExtension(file)))
               {
                   CreateAndAddTexture(file);
               }
            }
        }


        public static void AddFont(string name, SpriteFont font)
        {
            _fonts.Add(name.ToUpper(), font);
        }

        private static readonly Vector2 _noRotation = new Vector2(0, 0);
        public static void DrawString(SpriteBatch spriteBatch, string text, string fontName, Vector2 position, Vector2 scale, Color color, FontAlign align)
        {
            var measuredPosition = new Vector2(position.X, position.Y);
            var lines = text.Split('\n');
            fontName = fontName.ToUpper();
            foreach (string line in lines)
            {
                var measurements = _fonts[fontName].MeasureString(line);
                measurements *= scale;
                switch (align)
                {
                    case FontAlign.CENTER:
                        measuredPosition.X -= measurements.X/2;
                        break;
                    case FontAlign.RIGHT:
                        measuredPosition.X -= measurements.X;
                        break;
                }
                spriteBatch.DrawString(_fonts[fontName], line, measuredPosition, color, 0.0f, _noRotation, scale, SpriteEffects.None, 0.0f);
                measuredPosition.Y += measurements.Y;
                measuredPosition.X = position.X;
            }
        }

        private static readonly Vector2 _noScaling = new Vector2(1, 1);
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
                var pixel = pixels[x];
                pixel.R = 255;
                pixel.B = 255;
                pixel.G = 255;
            }
            result.SetData(pixels);
            return result;
        }
        /*
        public static void DrawStringClipped(string text, string fontName, Vector2 position, Color color, FontAlign align, Rectangle clip)
        {
            var oldRect = GraphicsDevice.ScissorRectangle;
            var spriteBatch = new SpriteBatch(GraphicsDevice);
            GraphicsDevice.ScissorRectangle = clip;
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            DrawString(spriteBatch, text, fontName, position, color, align);
            GraphicsDevice.ScissorRectangle = oldRect;
            spriteBatch.End();
        }

        public static void SetClipRectangle(int x, int y, int width, int height)
        {
            GraphicsDevice.ScissorRectangle = new Rectangle(x, y, width, height);


        }
        public static void ResetClipRectangle()
        {
            GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, GraphicsDevice.DisplayMode.Width,
                                                            GraphicsDevice.DisplayMode.Height);
            GraphicsDevice.RenderState.ScissorTestEnable = false;
        }
         */

    }

    public enum FontAlign
    {
        LEFT = 0,
        CENTER = 1,
        RIGHT = 2
    }
}
