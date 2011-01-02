using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class FileSelectDialog : DrawableObject
    {
        private string _currentFolder = "C:";

        public event EventHandler FileSelected;
        public event EventHandler FileSelectCancelled;
        public string CurrentFolder
        {
            get { return _currentFolder; }
             set
             {

                 if (!CheckAccess(value))
                 {
                     return;
                 }
                 _currentFolder = value;
                 CreateFileList(CurrentFolder);
             }
        }

        public string SelectedFile 
        {
            get
            {
                return CurrentFolder + "\\" + FileList.SelectedItem().ItemText;
            }
        }

        public Menu FileList = new Menu();

        public FileSelectDialog()
        {
            FileList.TextColor = Color.White;
            FileList.FontName = "DefaultFont";
            FileList.MaxVisibleItems = 15;
            FileList.ItemSpacing = 18;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            var shortPath = CurrentFolder;
            TextureManager.DrawString(spriteBatch,shortPath,"LargeFont",this.Position,Color.White,FontAlign.LEFT);
            FileList.X = this.X;
            FileList.Y = this.Y + 35;
            FileList.Width = this.Width;
            FileList.Draw(spriteBatch);
        }

        public void MoveSelected(int amount)
        {
            FileList.MoveSelected(amount);
        }

        private void CreateFileList(string path)
        {

            if (Directory.Exists(path))
            {
                FileList.Clear();
                FileList.AddItem(new MenuItem{ItemText = "..", ItemValue = "DIR"});

                foreach (string dir in Directory.GetDirectories(path))
                {
               
                    var dirname = dir.Substring(dir.LastIndexOf("\\"));
                    FileList.AddItem(new MenuItem{ItemText = dirname, ItemValue = "DIR"});

                }

                string[] patterns = {"*.mp3", "*.ogg", "*.wav"};
                foreach (string pattern in patterns)
                {

                    foreach (string file in Directory.GetFiles(path,pattern))
                    {
                        FileList.AddItem(new MenuItem {ItemText = Path.GetFileName(file), ItemValue = "FILE"});
                    }
                }
            }
        }

        private bool CheckAccess(string path)
        {
            try
            {
                Directory.GetFiles(path);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public void PerformAction(Managers.Action action)
        {
            var paction = action.ToString().Substring(action.ToString().IndexOf("_") + 1);

            switch (paction)
            {
                case "UP":
                    FileList.DecrementSelected();
                    break;
                case "DOWN":
                    FileList.IncrementSelected();
                    break;
                case "START":
                    if (SelectedItemIsFolder())
                    {
                        CurrentFolder = Path.GetFullPath(SelectedFile);
                    }
                    else
                    {
                        FileSelected(this, null);
                    }
                    break;
                case "BACK":
                    if (FileSelectCancelled != null)
                    {
                        FileSelectCancelled(this, null);
                    }
                    break;
            }
        }
        public bool SelectedItemIsFolder()
        {
            return Directory.Exists(SelectedFile);
        }
        
    }
}
