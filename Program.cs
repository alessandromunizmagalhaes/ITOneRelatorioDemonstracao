using System;

namespace ITOneRelatorioDemonstracao
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Addon addon = new Addon())
            {
                //  Starting the Application
                System.Windows.Forms.Application.Run();
            }
        }
    }
}
