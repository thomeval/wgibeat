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
        public static GraphicsDevice GraphicsDevice;
        public static LogManager Log;

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

}
