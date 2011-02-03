﻿using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace WGiBeat.Drawing
{
    public class FileSelectDialog : DrawableObject
    {
        private string _currentFolder = "C:\\";

        private  string[] _patterns = {"*.*"};

        public string[] Patterns
        { 
            get
            {
                return _patterns;
            }
            set
            {
                _patterns = value;
                CreateFileList(_currentFolder);
            }
        }
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
                if (CurrentFolder == "")
                {
                    return FileList.SelectedItem().ItemText;
                }
                return CurrentFolder + "\\" + FileList.SelectedItem().ItemText;
            }
        }

        public readonly Menu FileList = new Menu();
 

 
        public FileSelectDialog()
        {
            FileList.TextColor = Color.Black;
            FileList.FontName = "DefaultFont";
            FileList.MaxVisibleItems = 15;
            FileList.ItemSpacing = 18;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            var shortPath = CurrentFolder;
            var position = this.Position;
            position.X += 5;


            var pathWidth = TextureManager.ScaleTextToFit(shortPath, "LargeFont", this.Width - 10, 50);
            TextureManager.DrawString(spriteBatch,shortPath,"LargeFont",position,pathWidth,Color.Black,FontAlign.LEFT);
            
            FileList.X = this.X;
            FileList.Y = this.Y + 35;
            FileList.Width = this.Width;
            FileList.Draw(spriteBatch);

            DrawControlHelp(spriteBatch);
        }

        private void DrawControlHelp(SpriteBatch spriteBatch)
        {
            var position = this.Position.Clone();
            position.X +=  (int) this.Width/2.0f;
            position.Y += FileList.ItemSpacing*FileList.MaxVisibleItems;
            position.Y += 110;

            string[] instructions =
                {
                    "Use UP and DOWN to select a file or folder.",
                    "\nUse START to pick a file or folder.",
                    "\nUse LEFT or the '..' option to go up a folder.",
                    "\nPress SELECT to pick a different drive.",
                };

            foreach (var line in instructions)
            {
                TextureManager.DrawString(spriteBatch, line, "DefaultFont", position, Color.Black,
                                          FontAlign.CENTER);
                position.Y += 20;
            }
        }

        public void MoveSelected(int amount)
        {
            FileList.MoveSelected(amount);
        }

        private void CreateFileList(string path)
        {
            if (CurrentFolder == "")
            {
                CreateDriveList();
                return;
            }
            if (Directory.Exists(path))
            {
                FileList.Clear();
                FileList.AddItem(new MenuItem{ItemText = "..", ItemValue = "DIR"});

                foreach (string dir in Directory.GetDirectories(path))
                {
               
                    var dirname = dir.Substring(dir.LastIndexOf("\\"));
                    FileList.AddItem(new MenuItem{ItemText = dirname, ItemValue = "DIR"});

                }

                
                foreach (string pattern in Patterns)
                {

                    foreach (string file in Directory.GetFiles(path,pattern))
                    {
                        FileList.AddItem(new MenuItem {ItemText = Path.GetFileName(file), ItemValue = "FILE"});
                    }
                }
            }
        }

        private void CreateDriveList()
        {
            FileList.Clear();
            foreach (string drive in Directory.GetLogicalDrives())
            {
                FileList.AddItem(new MenuItem {ItemText = drive, ItemValue = "DRIVE"});
            }
        }

        private bool CheckAccess(string path)
        {
            if (path == "")
            {
                return true;
            }
            try
            {
                Directory.GetFiles(path);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                FileList.SelectedItem().ClearOptions();
                FileList.SelectedItem().AddOption("Permission Denied",0);
                return false;
            }
        }

        public void PerformAction(Managers.InputAction inputAction)
        {
            
            switch (inputAction.Action)
            {
                case "UP":
                    FileList.DecrementSelected();
                    break;
                case "DOWN":
                    FileList.IncrementSelected();
                    break;
                case "LEFT":
                    CurrentFolder = Path.GetFullPath(CurrentFolder + "\\..");
                    break;
                case "START":
                    if (SelectedItemIsFolder())
                    {
                        if (IsRootFolder(CurrentFolder) && (FileList.SelectedItem().ItemText == ".."))
                        {
                            CurrentFolder = "";
                        }
                        else
                        {
                            CurrentFolder = Path.GetFullPath(SelectedFile);
                        }
                    }
                    else
                    {
                        
                        FileSelected(this, null);
                    }
                    break;
                case "SELECT":
                    CurrentFolder = "";
                    break;
                case "BACK":
                    if (FileSelectCancelled != null)
                    {
                        FileSelectCancelled(this, null);
                    }
                    break;
            }
        }

        private bool IsRootFolder(string folder)
        {
            return folder.EndsWith(":\\") && folder.Length == 3;
        }

        public bool SelectedItemIsFolder()
        {
            return Directory.Exists(SelectedFile);
        }

        public void ResetEvents()
        {
            FileSelected = null;
            FileSelectCancelled = null;
        }
    }
}