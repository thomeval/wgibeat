using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class UpdaterFrame : DrawableObject
    {

        //Expected to be like: 0.8.
        public string AvailableVersion { get; set; }
        //Expected to be like: 0.8 or 0.8 \s
        public string CurrentVersion { get; set; }
        public string NewsMessage { get; set; }
        public UpdaterStatus Status { get; set; }
        public bool Visible { get; set; }
        public int XOffset { get; set; }

        private Sprite _updaterFrame;
        private Vector2 _textPosition;
        public UpdaterFrame()
        {
            InitSprites();
        }

        private void InitSprites()
        {
            _updaterFrame = new Sprite
            {
                SpriteTexture = TextureManager.Textures("UpdaterFrame"),
            };
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var message = "";
            if (Visible)
            {
                switch (Status)
                {
                    case UpdaterStatus.DISABLED:
                        message = "Update checking is disabled. It can be enabled from the Options Menu.";
                        break;
                    case UpdaterStatus.CHECKING:
                        message = "Checking for updates...";
                        break;
                    case UpdaterStatus.FAILED:
                        message = "Failed to retrieve WGiBeat latest version information.";
                        break;
                    case UpdaterStatus.SUCCESSFUL:
                        message = DetermineVersionMessage();
                        break;
                }

            }

            message = ScrollText(message);
            
            _updaterFrame.Position = this.Position;
            _updaterFrame.Draw(spriteBatch);
            _textPosition.X = this.X + XOffset + 20;
            _textPosition.Y = this.Y + 25;


            TextureManager.DrawString(spriteBatch, message, "DefaultFont", _textPosition, Color.White,
              FontAlign.LEFT);
        }

        private string ScrollText(string message)
        {
            var messageLength = TextureManager.Fonts("DefaultFont").MeasureString(message);
            if (messageLength.X > 780)
            {
                XOffset -= 1;
                if (XOffset < 0-messageLength.X - 25)
                {
                    XOffset += (int)messageLength.X;
                }
                message = message + "" + message;
                
            }
            else
            {
                XOffset = 0;
            }
            return message;
        }

        private string DetermineVersionMessage()
        {
    
            switch (VersionUpToDate())
            {
                case 0:
                    return String.Format("This version of WGiBeat is up to date. (v{0})", CurrentVersion);
                case 1:
                    return String.Format("This version is newer than the official release. (You have v{1}, official is v{0})   ", AvailableVersion, CurrentVersion);
                default:
                    return String.Format("WGiBeat version {0} released! {1} Visit http://wgibeat.googlecode.com for more information.   ", AvailableVersion, NewsMessage);
            }
        }

        private int VersionUpToDate()
        {
            try
            {
                var temp = CurrentVersion.Split(' ');

                //Version ID's are EXACTLY the same (eg. 0.8 vs 0.8)
                if (CurrentVersion == AvailableVersion)
                {
                    return 0;
                }
                var availableNum = Convert.ToDouble(AvailableVersion, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                var currentNum = Convert.ToDouble(temp[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

                //Cases where the a pre-release version is used (eg. 0.85 pre vs 0.8)
                if (availableNum < currentNum)
                {
                    return 1;
                }

                //Cases where either an old release is used (eg 0.7 vs 0.8)
                //Or Version numbers don't quite match, (like 0.8 beta 1 vs 0.8)
                return -1;
            }
                // Show the new version available if anything goes wrong, just to be safe.
            catch (Exception)
            {

                return -1;
            }
            
        }

    }

    public enum UpdaterStatus
    {
        DISABLED = 0,
        CHECKING = 1,
        SUCCESSFUL = 2,
        FAILED = 3
    }
}
