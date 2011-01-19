using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public static class TextureManager
    {
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        public static Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>();
        public static GraphicsDevice GraphicsDevice;

        public static void AddTexture(string name, Texture2D tex)
        {
            Textures.Add(name, tex);
        }

        public static void CreateAndAddTexture(string filename)
        {
            CreateAndAddTexture(filename, Path.GetFileNameWithoutExtension(filename));   
        }

        public static void CreateAndAddTexture(string filename, string assetName)
        {
            var newTexture = Texture2D.FromFile(GraphicsDevice, filename);
            if (!Textures.ContainsKey(assetName))
            {
                Textures.Add(assetName, newTexture);
            }
            else
            {
                Textures[assetName] = newTexture;
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
            Fonts.Add(name, font);
        }

        private static Vector2 noRotation = new Vector2(0, 0);
        public static void DrawString(SpriteBatch spriteBatch, string text, string fontName, Vector2 position, Vector2 scale, Color color, FontAlign align)
        {
            var measuredPosition = new Vector2(position.X, position.Y);
            var lines = text.Split('\n');

            foreach (string line in lines)
            {
                var measurements = Fonts[fontName].MeasureString(line);
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
                spriteBatch.DrawString(Fonts[fontName], line, measuredPosition, color, 0.0f, noRotation, scale, SpriteEffects.None, 0.0f);
                measuredPosition.Y += measurements.Y;
                measuredPosition.X = position.X;
            }
        }

        private static Vector2 noScaling = new Vector2(1, 1);
        public static void DrawString(SpriteBatch spriteBatch, string text, string fontName, Vector2 position, Color color, FontAlign align)
        {
            DrawString(spriteBatch,text,fontName,position,noScaling,color,align);
        }

        public static Vector2 ScaleTextToFit(string text, string fontName, Vector2 maxSize)
        {
            if (!Fonts.ContainsKey(fontName))
            {
                throw new ArgumentException("TextureManager does not contain a font called: " + fontName);
            }
            var actualSize =  Fonts[fontName].MeasureString(text);
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
