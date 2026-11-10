// Power Mode Toggle Setup - per-user installer and uninstaller.
//
// Built by build.ps1 together with PowerModeToggle.cs (/define:SETUP, /main:PowerModeToggle.SetupProgram),
// so it shares the version, icons and dialog style. PowerModeToggle.exe is embedded as a resource.
//
//   PowerModeToggle-Setup.exe               install or update for the current user (no admin rights)
//   Uninstall.exe --uninstall               uninstall (copied into the install folder, used by Settings > Apps)

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace PowerModeToggle
{
    static class Installer
    {
        const string ExeName = "PowerModeToggle.exe", UninstallerName = "Uninstall.exe";
        const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\PowerModeToggle";
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run", RunValue = "PowerModeToggle";

        public static string InstallDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Power Mode Toggle"); }
        }
        static string AppPath { get { return Path.Combine(InstallDir, ExeName); } }
        static string UninstallerPath { get { return Path.Combine(InstallDir, UninstallerName); } }

        public static string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(3); } }

        public static bool IsInstalled
        {
            get { using (var k = Registry.CurrentUser.OpenSubKey(UninstallKey)) return k != null; }
        }

        public static bool AutostartEnabled
        {
            get { using (var k = Registry.CurrentUser.OpenSubKey(RunKey)) return k != null && k.GetValue(RunValue) != null; }
        }

        // True when this process is the Uninstall.exe inside the install folder.
        public static bool RunningFromInstallDir
        {
            get { return string.Equals(Path.GetDirectoryName(Application.ExecutablePath), InstallDir, StringComparison.OrdinalIgnoreCase); }
        }

        static byte[] AppBytes()
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(ExeName))
            using (var ms = new MemoryStream()) { s.CopyTo(ms); return ms.ToArray(); }
        }

        // The app has no window to close, so stop it (the tray icon disappears when it exits).
        static void StopApp()
        {
            foreach (var p in Process.GetProcessesByName("PowerModeToggle"))
                try { p.Kill(); p.WaitForExit(5000); } catch { }
        }

        public static void Install(bool autostart)
        {
            StopApp();
            TryDeleteDir(UninstallCopyDir);
            Directory.CreateDirectory(InstallDir);
            var app = AppBytes();
            File.WriteAllBytes(AppPath, app);
            if (!RunningFromInstallDir) File.Copy(Application.ExecutablePath, UninstallerPath, true);

            using (var k = Registry.CurrentUser.CreateSubKey(UninstallKey))
            {
                k.SetValue("DisplayName", "Power Mode Toggle");
                k.SetValue("DisplayVersion", Version);
                k.SetValue("DisplayIcon", AppPath);
                k.SetValue("InstallLocation", InstallDir);
                k.SetValue("UninstallString", "\"" + UninstallerPath + "\" --uninstall");
                k.SetValue("URLInfoAbout", AboutForm.RepoUrl);
                k.SetValue("EstimatedSize", (app.Length + new FileInfo(UninstallerPath).Length) / 1024, RegistryValueKind.DWord);
                k.SetValue("NoModify", 1, RegistryValueKind.DWord);
                k.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            }
            using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
            {
                if (autostart) k.SetValue(RunValue, "\"" + AppPath + "\"");
                else k.DeleteValue(RunValue, false);
            }

            // The app creates its Start menu shortcut (needed for notifications) when it starts.
            Process.Start(AppPath);
        }

        public static void Uninstall(bool deleteSettings)
        {
            StopApp();
            using (var k = Registry.CurrentUser.OpenSubKey(RunKey, true)) if (k != null) k.DeleteValue(RunValue, false);
            Registry.CurrentUser.DeleteSubKeyTree(UninstallKey, false);
            TryDelete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Power Mode Toggle.lnk"));
            TryDeleteDir(Path.Combine(Path.GetTempPath(), "PowerModeToggle"));
            if (deleteSettings) TryDeleteDir(Path.GetDirectoryName(Settings.FilePath));
            TryDelete(AppPath);
            DeleteInstallDir();
        }

        // Right after Uninstall.exe in the folder exits, Windows or an antivirus scan can still hold it
        // for a moment, so retry for a few seconds.
        static void DeleteInstallDir()
        {
            for (int i = 0; i < 20 && Directory.Exists(InstallDir); i++)
            {
                try { Directory.Delete(InstallDir, true); }
                catch { System.Threading.Thread.Sleep(250); }
            }
        }

        // Uninstall.exe can't delete itself while it runs. Settings > Apps starts it from the install folder,
        // so it runs a copy of itself from %TEMP% instead (as common installers do), which can then
        // remove the whole install folder. The copy is cleaned up by the next install.
        static string UninstallCopyDir { get { return Path.Combine(Path.GetTempPath(), "PowerModeToggle-Uninstall"); } }

        public static void RestartFromTemp()
        {
            Directory.CreateDirectory(UninstallCopyDir);
            var copy = Path.Combine(UninstallCopyDir, UninstallerName);
            File.Copy(Application.ExecutablePath, copy, true);
            // Not the inherited working directory: that is often the install folder, which then can't be deleted.
            Process.Start(new ProcessStartInfo(copy, "--uninstall") { WorkingDirectory = UninstallCopyDir });
        }

        static void TryDelete(string file) { try { if (File.Exists(file)) File.Delete(file); } catch { } }
        static void TryDeleteDir(string dir) { try { if (Directory.Exists(dir)) Directory.Delete(dir, true); } catch { } }
    }

    class SetupForm : ThemedForm
    {
        readonly bool uninstall;
        readonly Label status, detail; // detail: the install folder (install only)
        readonly CheckBox option;
        readonly Button primary, cancel;

        public SetupForm(bool uninstall)
            : base(uninstall ? "Uninstall Power Mode Toggle" : "Power Mode Toggle Setup")
        {
            this.uninstall = uninstall;
            Info.Controls.Add(Line("Power Mode Toggle", TitleFont, TextColor, 0));
            Info.Controls.Add(Line("Version " + Installer.Version, Font, DimColor, S(14)));

            if (uninstall)
            {
                status = Line("Removes the app, its Start menu shortcut and its startup entry.", Font, TextColor, S(14));
                option = MakeCheckBox("Also delete my settings", false);
            }
            else
            {
                status = Line((Installer.IsInstalled ? "Updates" : "Installs") + " Power Mode Toggle for your user account in:", Font, TextColor, 0);
                detail = Line(Installer.InstallDir, Font, DimColor, S(14));
                option = MakeCheckBox("Start with Windows", Installer.AutostartEnabled || !Installer.IsInstalled);
            }
            Info.Controls.Add(status);
            if (detail != null) Info.Controls.Add(detail);
            Info.Controls.Add(option);

            primary = AddButton(uninstall ? "Uninstall" : (Installer.IsInstalled ? "Update" : "Install"), delegate { Run(); });
            cancel = AddButton("Cancel", delegate { Close(); });
            AcceptButton = primary;
            CancelButton = cancel;
        }

        void Run()
        {
            if (primary.Text == "Close") { Close(); return; }
            UseWaitCursor = true;
            try
            {
                if (uninstall)
                {
                    Installer.Uninstall(option.Checked);
                    status.Text = "Power Mode Toggle was removed.";
                }
                else
                {
                    Installer.Install(option.Checked);
                    status.Text = "Power Mode Toggle is installed and running in the notification area.";
                }
                if (detail != null) detail.Visible = false;
                option.Visible = false;
                cancel.Visible = false;
                primary.Text = "Close";
            }
            catch (Exception ex)
            {
                status.Text = (uninstall ? "Uninstall" : "Installation") + " failed: " + ex.Message;
            }
            finally { UseWaitCursor = false; }
        }
    }

    static class SetupProgram
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool uninstall = Array.Exists(args, a => a.Equals("--uninstall", StringComparison.OrdinalIgnoreCase)
                                                  || a.Equals("/uninstall", StringComparison.OrdinalIgnoreCase));
            if (uninstall && Installer.RunningFromInstallDir)
            {
                try { Installer.RestartFromTemp(); return; }
                catch { } // fall back to uninstalling from here; only Uninstall.exe stays behind
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var form = new SetupForm(uninstall);
            Application.Run(form);
        }
    }
}
