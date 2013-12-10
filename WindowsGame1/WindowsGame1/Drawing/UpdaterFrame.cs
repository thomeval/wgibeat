using System;
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
        public string UpdateDetails { get; set; }
        public UpdaterStatus Status { get; set; }
        public bool Visible { get; set; }
        public double XOffset { get; set; }

        public string NewsMessage { get; set; }

        private Sprite3D _updaterFrame;
        private Vector2 _textPosition;

        public UpdaterFrame()
        {
            InitSprites();
            XOffset = 25;
        }

        private void InitSprites()
        {
            _updaterFrame = new Sprite3D
            {
                Texture = TextureManager.Textures("UpdaterFrame"),
            };
        }

        public override void Draw()
        {
            var message = "";
            if (!Visible)
            {
                return;
            }
           
                switch (Status)
                {
                    case UpdaterStatus.DISABLED:
                        message = "Update checking is disabled. It can be enabled from the Options Menu.";
                        break;
                    case UpdaterStatus.CHECKING:
                        message = "Checking for updates...";
                        break;
                    case UpdaterStatus.FAILED:
                        message = "Update check failed: " + UpdateDetails;
                        break;
                    case UpdaterStatus.SUCCESSFUL:
                        message = DetermineVersionMessage();
                        break;
                }

            message = ScrollText(message);
            
            _updaterFrame.Position = this.Position;
            _updaterFrame.Size = this.Size;
            _updaterFrame.Draw();
            _textPosition.X = this.X + (int) XOffset + 20;
            _textPosition.Y = this.Y + 25;


            FontManager.DrawString(message, "DefaultFont", _textPosition, Color.White,
              FontAlign.Left);
        }

        private const int SCROLL_SPEED = 60;
        private string ScrollText(string message)
        {
            var messageLength = FontManager.Fonts("DefaultFont").MeasureString(message);
            if (messageLength.X > 780)
            {
                XOffset -= TextureManager.LastGameTime.ElapsedRealTime.TotalSeconds * SCROLL_SPEED;
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
                    if (string.IsNullOrEmpty(NewsMessage))
                    {
                        return String.Format("This version of WGiBeat is up to date. (v{0})", CurrentVersion);   
                    }
                    return String.Format(NewsMessage+ "    ");

                case 1:
                    return String.Format("This version is newer than the official release. (You have v{1}, official is v{0})   ", AvailableVersion, CurrentVersion);
                default:
                    return String.Format("WGiBeat version {0} released! {1} Visit http://wgibeat.googlecode.com for more information.   ", AvailableVersion, UpdateDetails);
            }
        }

        private int VersionUpToDate()
        {
            try
            {
                var currentParts = CurrentVersion.Split(' ');
                var availableParts = AvailableVersion.Split(' ');
                //Version ID's are EXACTLY the same (eg. 0.8 vs 0.8)
                if (CurrentVersion == AvailableVersion)
                {
                    return 0;
                }
                var availableNum = Convert.ToDouble(availableParts[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                var currentNum = Convert.ToDouble(currentParts[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

                //Cases where the a pre-release version is used (eg. 0.85 pre vs 0.8)
                if (availableNum < currentNum)
                {
                    return 1;
                }

                //Cases where version number is the same, but suffix is different (eg. 2.0 a1 vs 2.0 a2)
                if (availableNum == currentNum && currentParts.Length > 1 && availableParts.Length > 1)
                {
                    return String.CompareOrdinal(currentParts[1], availableParts[1]);
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
