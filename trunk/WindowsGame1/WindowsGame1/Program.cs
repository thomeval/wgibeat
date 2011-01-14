using System;
using System.Windows.Forms;
using System.IO;

namespace WGiBeat
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
#if (DEBUG)
            using (GameCore game = new GameCore())
            {
                game.Run();
                game.Log.SaveToFile();
            }
#else
            try
            {
                using (GameCore game = new GameCore())
                {
                    game.Run();
                }
            }
             catch (Exception ex)
            {
                MessageBox.Show(
                    "A critical error has occurred. Please check the error.txt file created in the wgibeat folder for details. If this is a bug, please send the error.txt to a developer.");
                SaveErrorLog(ex);
            }
#endif
        }

        private static void SaveErrorLog(Exception ex)
        {
            try
            {
                var file = new StreamWriter(File.Open("error.txt", FileMode.OpenOrCreate, FileAccess.Write));
                file.WriteLine(ex.Message + "\n");
                file.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    file.WriteLine(ex.InnerException.Message);
                }
            file.Close();
            }
            catch (Exception ex2)
            {
                MessageBox.Show("Failed to write error.txt file. " + ex2.Message + "\n Original wgibeat error: " +
                                ex.Message + "\n" + ex.StackTrace);
            }

        }
    }

}

