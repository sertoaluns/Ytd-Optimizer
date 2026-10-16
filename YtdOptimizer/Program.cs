using System;
using System.Windows.Forms;
using YtdOptimizer.UI;

namespace YtdOptimizer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var requestedName = new System.Reflection.AssemblyName(args.Name).Name;
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string targetPath = System.IO.Path.Combine(appDir, requestedName + ".dll");
                if (System.IO.File.Exists(targetPath))
                {
                    return System.Reflection.Assembly.LoadFrom(targetPath);
                }
            }
            catch { }
            return null;
        }
    }
}
