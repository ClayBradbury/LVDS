using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace WinFormsApp1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize application settings (DPI, fonts, etc.)
            ApplicationConfiguration.Initialize();


            // Pass the loaded data to the mainScreen constructor
            Application.Run(new mainScreen());
        }

        
    }
}