using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using System.IO;

namespace TFTPServerApp
{
    static class Program
    {
        public const string CustomEventLog = "TFTPServerLog";
        public const string CustomEventSource = "TFTPServerSource";

        private const string Switch_Install = "/install";
        private const string Switch_Uninstall = "/uninstall";
        private const string Switch_Service = "/service";

        public static string GetConfigurationPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "JPMikkers\\TFTP Server\\Configuration.xml");
        }

        public static bool HasAdministrativeRight()
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool RunElevated(string fileName,string args)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = fileName;
            processInfo.Arguments = args;
            try
            {
                Process.Start(processInfo);
                return true;
            }
            catch (Exception)
            {
                //Do nothing. Probably the user canceled the UAC window
            }
            return false;
        }

        public static bool RunElevated(string args)
        {
            return RunElevated(Application.ExecutablePath, args);
        }

        private static void Install()
        {
            if (!HasAdministrativeRight())
            {
                RunElevated(Switch_Install);
                return;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Installing TFTP service");

                try
                {
                    System.Configuration.Install.AssemblyInstaller Installer = new System.Configuration.Install.AssemblyInstaller(Assembly.GetExecutingAssembly(), new string[] { });
                    Installer.UseNewContext = true;
                    Installer.Install(null);
                    Installer.Commit(null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(String.Format("Exception: {0}", ex));
                }
            }
        }

        private static void Uninstall()
        {
            if (!HasAdministrativeRight())
            {
                RunElevated(Switch_Uninstall);
                return;
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Uninstalling TFTP service");

                try
                {
                    System.Configuration.Install.AssemblyInstaller Installer = new System.Configuration.Install.AssemblyInstaller(Assembly.GetExecutingAssembly(), new string[] { });
                    Installer.UseNewContext = true;
                    Installer.Uninstall(null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(String.Format("Exception: {0}", ex));
                }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0].ToLower() == Switch_Service)
            {
                ServiceBase.Run(new ServiceBase[] { new TFTPService() });
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (args.Length == 0)
                {
                    ServiceController serviceController = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == "TFTPServer");

                    if (serviceController == null)
                    {
                        if (MessageBox.Show("Service has not been installed yet, install?", "TFTP Server", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Install();
                        }
                    }
                    else
                    {
                        Application.Run(new FormMain(serviceController));
                    }
                }
                else
                {
                    switch (args[0].ToLower())
                    {
                        case Switch_Install:
                            Install();
                            break;

                        case Switch_Uninstall:
                            Uninstall();
                            break;
                    }
                }
            }
        }
    }
}
