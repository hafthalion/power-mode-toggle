// Power Mode Toggle - tray utility to switch the Windows power mode
// (Best efficiency <-> Best performance) from the notification area or a global hotkey.
//
// Build: build.ps1 (uses the csc.exe that ships with .NET Framework 4.x, so C# 5 syntax only).

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

// Windows shows the FileDescription (AssemblyTitle) as the app name on notifications.
// The installer (Setup.cs, built with /define:SETUP) shares this file and its version.
#if SETUP
[assembly: AssemblyTitle("Power Mode Toggle Setup")]
#else
[assembly: AssemblyTitle("Power Mode Toggle")]
#endif
[assembly: AssemblyProduct("Power Mode Toggle")]
[assembly: AssemblyVersion("1.6.1.0")]

namespace PowerModeToggle
{
    enum PowerMode { Efficiency, Balanced, Performance, Unknown }

    static class PowerApi
    {
        // Power mode "overlay" GUIDs used by Settings > System > Power.
        static readonly Guid Efficiency  = new Guid("961cc777-2547-4f9d-8174-7d86181b8a7a");
        static readonly Guid Balanced    = Guid.Empty;
        static readonly Guid Performance = new Guid("ded574b5-45a0-4f42-8737-46345c09c238");

        [DllImport("powrprof.dll")]
        static extern uint PowerSetActiveOverlayScheme(Guid overlaySchemeGuid);

        [DllImport("powrprof.dll")]
        static extern uint PowerGetEffectiveOverlayScheme(out Guid effectiveOverlayGuid);

        // Power modes only take effect with the Balanced power plan. With another plan, Windows
        // stores the mode but ignores it, and the effective mode always reads as Balanced.
        static readonly Guid BalancedPlan = new Guid("381b4222-f694-41f0-9685-ff5bb260df2e");

        [DllImport("powrprof.dll")]
        static extern uint PowerGetActiveScheme(IntPtr userRootPowerKey, out IntPtr activePolicyGuid);

        [DllImport("powrprof.dll")]
        static extern uint PowerSetActiveScheme(IntPtr userRootPowerKey, ref Guid schemeGuid);

        [DllImport("powrprof.dll")]
        static extern uint PowerReadFriendlyName(IntPtr rootPowerKey, ref Guid schemeGuid, IntPtr subGroupOfPowerSettingsGuid,
                                                 IntPtr powerSettingGuid, byte[] buffer, ref uint bufferSize);

        [DllImport("kernel32.dll")]
        static extern IntPtr LocalFree(IntPtr hMem);

        static Guid? ActivePlan()
        {
            IntPtr p;
            if (PowerGetActiveScheme(IntPtr.Zero, out p) != 0) return null;
            try { return (Guid)Marshal.PtrToStructure(p, typeof(Guid)); }
            finally { LocalFree(p); }
        }

        // Name of the active power plan if it isn't Balanced, otherwise null.
        public static string OtherPlanName()
        {
            var plan = ActivePlan();
            if (plan == null || plan.Value == BalancedPlan) return null;
            var g = plan.Value;
            uint size = 0;
            PowerReadFriendlyName(IntPtr.Zero, ref g, IntPtr.Zero, IntPtr.Zero, null, ref size);
            var buf = new byte[size];
            if (size == 0 || PowerReadFriendlyName(IntPtr.Zero, ref g, IntPtr.Zero, IntPtr.Zero, buf, ref size) != 0)
                return "another";
            return System.Text.Encoding.Unicode.GetString(buf).TrimEnd('\0');
        }

        public static PowerMode Get()
        {
            var plan = ActivePlan();
            if (plan != null && plan.Value != BalancedPlan) return PowerMode.Unknown;
            Guid g;
            if (PowerGetEffectiveOverlayScheme(out g) != 0) return PowerMode.Unknown;
            if (g == Efficiency) return PowerMode.Efficiency;
            if (g == Performance) return PowerMode.Performance;
            if (g == Balanced) return PowerMode.Balanced;
            return PowerMode.Unknown;
        }

        // Switches to the Balanced power plan first if another plan is active.
        public static uint Set(PowerMode mode)
        {
            var plan = ActivePlan();
            if (plan == null || plan.Value != BalancedPlan)
            {
                var balanced = BalancedPlan;
                uint err = PowerSetActiveScheme(IntPtr.Zero, ref balanced);
                if (err != 0) return err;
            }
            Guid g = mode == PowerMode.Efficiency ? Efficiency
                   : mode == PowerMode.Performance ? Performance
                   : Balanced;
            return PowerSetActiveOverlayScheme(g);
        }
    }

    class Settings
    {
        // Punctuation means the key at that position on a US keyboard (see HotkeyWindow.Punctuation).
        public string ToggleHotkey = "Ctrl+Alt+,";
        public string EfficiencyHotkey = "Ctrl+Alt+/";
        public string PerformanceHotkey = "Ctrl+Alt+'";
        public bool ShowNotifications = true;

        const string OldDefaultHotkey = "Ctrl+Alt+P"; // the single "Hotkey" (toggle) setting before 1.4.0

        // Settings live in the user profile (%APPDATA%\PowerModeToggle), so the exe can be anywhere.
        static string Dir { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerModeToggle"); } }
        public static string FilePath { get { return Path.Combine(Dir, "PowerModeToggle.ini"); } }

        // Before 1.5.3 the settings were next to the exe, named settings.ini before 1.3.1.
        static IEnumerable<string> OldFilePaths
        {
            get
            {
                var exeDir = Path.GetDirectoryName(Application.ExecutablePath);
                yield return Path.Combine(exeDir, "PowerModeToggle.ini");
                yield return Path.Combine(exeDir, "settings.ini");
            }
        }

        // Moves settings from an older location; copies if the old folder isn't writable.
        static void MoveOldFile()
        {
            if (File.Exists(FilePath)) return;
            foreach (var old in OldFilePaths)
            {
                if (!File.Exists(old)) continue;
                Directory.CreateDirectory(Dir);
                try { File.Move(old, FilePath); }
                catch { File.Copy(old, FilePath); }
                return;
            }
        }

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                MoveOldFile();
                if (!File.Exists(FilePath)) { s.Save(); return s; }

                var seen = new HashSet<string>();
                string oldHotkey = null;
                foreach (var raw in File.ReadAllLines(FilePath))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#")) continue;
                    int eq = line.IndexOf('=');
                    if (eq < 0) continue;
                    var key = line.Substring(0, eq).Trim().ToLowerInvariant();
                    var val = line.Substring(eq + 1).Trim();
                    seen.Add(key);
                    if (key == "togglehotkey") s.ToggleHotkey = val;
                    else if (key == "efficiencyhotkey") s.EfficiencyHotkey = val;
                    else if (key == "performancehotkey") s.PerformanceHotkey = val;
                    else if (key == "shownotifications") s.ShowNotifications = !(val == "0" || val.Equals("false", StringComparison.OrdinalIgnoreCase));
                    else if (key == "hotkey") oldHotkey = val;
                }

                // Keep a customized toggle hotkey from older versions; the old default becomes Ctrl+Alt+T.
                if (!seen.Contains("togglehotkey") && oldHotkey != null && !oldHotkey.Equals(OldDefaultHotkey, StringComparison.OrdinalIgnoreCase))
                    s.ToggleHotkey = oldHotkey;
                // Rewrite files from older versions so they list every setting.
                if (!(seen.Contains("togglehotkey") && seen.Contains("efficiencyhotkey") && seen.Contains("performancehotkey") && seen.Contains("shownotifications")))
                    s.Save();
            }
            catch { }
            return s;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                File.WriteAllLines(FilePath, new[]
                {
                    "; Power Mode Toggle settings. Restart the app after editing.",
                    "; Hotkeys: any combination of Ctrl, Alt, Shift, Win plus a key name or punctuation, e.g. Ctrl+Alt+P, Win+Shift+F9, Ctrl+Alt+/.",
                    "; Punctuation means the key at that position on a US keyboard.",
                    "; Leave a hotkey empty to turn it off.",
                    "ToggleHotkey=" + ToggleHotkey,
                    "EfficiencyHotkey=" + EfficiencyHotkey,
                    "PerformanceHotkey=" + PerformanceHotkey,
                    "ShowNotifications=" + (ShowNotifications ? "true" : "false"),
                });
            }
            catch { }
        }
    }

    static class Autostart
    {
        const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string Name = "PowerModeToggle";

        static string Command { get { return "\"" + Application.ExecutablePath + "\""; } }

        static string Value
        {
            get { using (var k = Registry.CurrentUser.OpenSubKey(RunKey)) return k == null ? null : k.GetValue(Name) as string; }
        }

        // Enabled = our Run entry exists, wherever it points (see UpdatePath).
        public static bool IsEnabled
        {
            get { return Value != null; }
            set
            {
                using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (value) k.SetValue(Name, Command);
                    else k.DeleteValue(Name, false);
                }
            }
        }

        // Call at startup: if autostart is on but points elsewhere (e.g. the exe was moved), point it here.
        public static void UpdatePath()
        {
            try
            {
                var v = Value;
                if (v != null && !v.Equals(Command, StringComparison.OrdinalIgnoreCase)) IsEnabled = true;
            }
            catch { }
        }
    }

    // Hidden message-only window that receives WM_HOTKEY.
    class HotkeyWindow : NativeWindow, IDisposable
    {
        const int WM_HOTKEY = 0x0312;
        const uint MOD_ALT = 0x1, MOD_CONTROL = 0x2, MOD_SHIFT = 0x4, MOD_WIN = 0x8, MOD_NOREPEAT = 0x4000;
        static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        readonly Dictionary<int, Action> actions = new Dictionary<int, Action>(); // by hotkey id

        // Punctuation in hotkeys, as the key at that position on a US keyboard. Hotkeys are bound to keys,
        // not characters, so on other layouts the key may type something else (e.g. '/' is # on German).
        static readonly Dictionary<char, Keys> Punctuation = new Dictionary<char, Keys>
        {
            { ',', Keys.Oemcomma }, { '.', Keys.OemPeriod }, { '/', Keys.OemQuestion }, { ';', Keys.OemSemicolon },
            { '\'', Keys.OemQuotes }, { '[', Keys.OemOpenBrackets }, { ']', Keys.OemCloseBrackets }, { '\\', Keys.OemPipe },
            { '-', Keys.OemMinus }, { '=', Keys.Oemplus }, { '`', Keys.Oemtilde },
        };

        public HotkeyWindow()
        {
            CreateHandle(new CreateParams { Parent = HWND_MESSAGE });
        }

        // Returns null on success or for an empty spec (hotkey turned off), otherwise an error message.
        public string Register(string spec, Action action)
        {
            if (string.IsNullOrWhiteSpace(spec)) return null;
            uint mods = MOD_NOREPEAT;
            Keys key = Keys.None;
            foreach (var part in spec.Split('+'))
            {
                var p = part.Trim();
                switch (p.ToLowerInvariant())
                {
                    case "ctrl": case "control": mods |= MOD_CONTROL; break;
                    case "alt": mods |= MOD_ALT; break;
                    case "shift": mods |= MOD_SHIFT; break;
                    case "win": case "windows": mods |= MOD_WIN; break;
                    default:
                        if (p.Length == 1 && Punctuation.TryGetValue(p[0], out key)) break;
                        if (p.Length == 1 && char.IsDigit(p[0])) p = "D" + p;
                        try { key = (Keys)Enum.Parse(typeof(Keys), p, true); }
                        catch { return "Unknown key \"" + part.Trim() + "\" in hotkey \"" + spec + "\"."; }
                        break;
                }
            }
            if (key == Keys.None) return "Hotkey \"" + spec + "\" has no key.";
            int id = actions.Count + 1;
            if (!RegisterHotKey(Handle, id, mods, (uint)key)) return "Hotkey " + spec + " is already in use by another program.";
            actions[id] = action;
            return null;
        }

        protected override void WndProc(ref Message m)
        {
            Action action;
            if (m.Msg == WM_HOTKEY && actions.TryGetValue(m.WParam.ToInt32(), out action)) action();
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            foreach (var id in actions.Keys) UnregisterHotKey(Handle, id);
            DestroyHandle();
        }
    }

    // Start menu shortcut carrying our AppUserModelID. Windows only shows toast pop-ups from an
    // unpackaged desktop app if such a shortcut exists; its name and icon become the toast header.
    static class StartMenuShortcut
    {
        [ComImport, Guid("00021401-0000-0000-C000-000000000046")]
        class CShellLink { }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
        interface IShellLinkW
        {
            void GetPath([MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder file, int cch, IntPtr fd, uint flags);
            void GetIDList(out IntPtr pidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder name, int cch);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string name);
            void GetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder dir, int cch);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string dir);
            void GetArguments([MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder args, int cch);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string args);
            void GetHotkey(out short hotkey);
            void SetHotkey(short hotkey);
            void GetShowCmd(out int showCmd);
            void SetShowCmd(int showCmd);
            void GetIconLocation([MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder path, int cch, out int index);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string path, int index);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string path, uint reserved);
            void Resolve(IntPtr hwnd, uint flags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string file);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct PROPERTYKEY { public Guid fmtid; public uint pid; }

        [StructLayout(LayoutKind.Sequential)]
        struct PROPVARIANT { public ushort vt, r1, r2, r3; public IntPtr p, p2; }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
        interface IPropertyStore
        {
            void GetCount(out uint count);
            void GetAt(uint index, out PROPERTYKEY key);
            void GetValue(ref PROPERTYKEY key, out PROPVARIANT value);
            void SetValue(ref PROPERTYKEY key, ref PROPVARIANT value);
            void Commit();
        }

        const ushort VT_LPWSTR = 31;
        static readonly PROPERTYKEY PKEY_AppUserModel_ID = new PROPERTYKEY { fmtid = new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), pid = 5 };

        // Points at this exe, so it also uses the exe's embedded icon (app.ico).
        public static void CreateOrUpdate(string name, string aumid)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), name + ".lnk");
            var link = (IShellLinkW)new CShellLink();
            link.SetPath(Application.ExecutablePath);
            link.SetWorkingDirectory(Path.GetDirectoryName(Application.ExecutablePath));
            link.SetDescription("Switch the Windows power mode from the notification area");

            var value = new PROPVARIANT { vt = VT_LPWSTR, p = Marshal.StringToCoTaskMemUni(aumid) };
            try
            {
                var key = PKEY_AppUserModel_ID;
                var store = (IPropertyStore)link;
                store.SetValue(ref key, ref value);
                store.Commit();
            }
            finally { Marshal.FreeCoTaskMem(value.p); }

            ((System.Runtime.InteropServices.ComTypes.IPersistFile)link).Save(path, true);
            Marshal.ReleaseComObject(link);
        }
    }

    // Modern Windows notifications (WinRT toasts). Classic tray balloons are converted to toasts
    // by Windows 10/11 but lose their custom icon, so we send toasts directly.
    static class Toast
    {
        const string Aumid = "PowerModeToggle";
        const string AppName = "Power Mode Toggle";
        static readonly string ImageDir = Path.Combine(Path.GetTempPath(), "PowerModeToggle");
        static readonly HashSet<PowerMode> written = new HashSet<PowerMode>(); // images written this run

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SetCurrentProcessExplicitAppUserModelID(string appId);

        static Type WinRT(string type, string assembly)
        {
            return Type.GetType(type + ", " + assembly + ", ContentType=WindowsRuntime", true);
        }

        static string Image(PowerMode mode)
        {
            var path = Path.Combine(ImageDir, mode.ToString().ToLowerInvariant() + ".png");
            // Rewrite once per run, so images left by an older version with other icons get replaced.
            if (!written.Contains(mode) || !File.Exists(path))
            {
                written.Add(mode);
                Directory.CreateDirectory(ImageDir);
                // Windows shows the logo at a fixed size; transparent padding makes the icon look smaller.
                const int Canvas = 128, IconSize = 88;
                using (var bmp = new Bitmap(Canvas, Canvas))
                using (var icon = Icons.Draw(mode, IconSize))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(icon, (Canvas - IconSize) / 2, (Canvas - IconSize) / 2, IconSize, IconSize);
                    }
                    bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            return path;
        }

        // Call once at startup. Re-creating the shortcut each time keeps it valid if the exe moves.
        public static void Register()
        {
            try { SetCurrentProcessExplicitAppUserModelID(Aumid); } catch { }
            try { StartMenuShortcut.CreateOrUpdate(AppName, Aumid); } catch { }
        }

        // Mode toasts (with a logo) are silent and replace each other; messages use the default sound.
        // Returns false if toasts are unavailable, so the caller can fall back to a balloon.
        public static bool Show(string title, string text, PowerMode? logo)
        {
            try
            {
                var esc = new Func<string, string>(System.Security.SecurityElement.Escape);
                var xml = "<toast duration=\"short\"><visual><binding template=\"ToastGeneric\">"
                        + (logo.HasValue ? "<image placement=\"appLogoOverride\" src=\"" + esc(new Uri(Image(logo.Value)).AbsoluteUri) + "\"/>" : "")
                        + (string.IsNullOrEmpty(title) ? "" : "<text>" + esc(title) + "</text>")
                        + "<text>" + esc(text) + "</text>"
                        + "</binding></visual>" + (logo.HasValue ? "<audio silent=\"true\"/>" : "") + "</toast>";

                var docType = WinRT("Windows.Data.Xml.Dom.XmlDocument", "Windows.Data.Xml.Dom");
                var doc = Activator.CreateInstance(docType);
                docType.GetMethod("LoadXml", new[] { typeof(string) }).Invoke(doc, new object[] { xml });

                var toastType = WinRT("Windows.UI.Notifications.ToastNotification", "Windows.UI.Notifications");
                var toast = Activator.CreateInstance(toastType, doc);
                // Same tag/group => a new toast replaces the previous one instead of stacking up.
                toastType.GetProperty("Tag").SetValue(toast, logo.HasValue ? "mode" : "message");
                toastType.GetProperty("Group").SetValue(toast, Aumid);
                toastType.GetProperty("ExpirationTime").SetValue(toast, (DateTimeOffset?)DateTimeOffset.Now.AddSeconds(15));

                var managerType = WinRT("Windows.UI.Notifications.ToastNotificationManager", "Windows.UI.Notifications");
                var notifier = managerType.GetMethod("CreateToastNotifier", new[] { typeof(string) }).Invoke(null, new object[] { Aumid });
                WinRT("Windows.UI.Notifications.ToastNotifier", "Windows.UI.Notifications")
                    .GetMethod("Show").Invoke(notifier, new[] { toast });
                return true;
            }
            catch { return false; }
        }
    }

    // Mode icons, drawn at runtime: tray icon and notification logo. All are the app's lightning
    // bolt; the color shows the mode. (build.ps1 writes app.ico, the exe icon, from the Balanced one.)
    static class Icons
    {
        public static Icon Make(PowerMode mode)
        {
            using (var bmp = Draw(mode, 32)) return Icon.FromHandle(bmp.GetHicon());
        }

        static Color ColorOf(PowerMode mode)
        {
            switch (mode)
            {
                case PowerMode.Efficiency: return Color.FromArgb(46, 160, 67);   // green
                case PowerMode.Balanced: return Color.FromArgb(0, 120, 212);     // blue
                case PowerMode.Performance: return Color.FromArgb(232, 96, 28);  // orange
                default: return Color.Gray;                                      // other power plan
            }
        }

        public static Bitmap Draw(PowerMode mode, int size)
        {
            const int S = 32; // shapes are designed on a 32x32 grid and scaled
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            using (var circle = new SolidBrush(ColorOf(mode)))
            using (var bolt = new GraphicsPath())
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                g.ScaleTransform(size / (float)S, size / (float)S);
                g.FillEllipse(circle, 0, 0, S - 1, S - 1);
                bolt.AddPolygon(new[] {
                    new PointF(18, 3), new PointF(7, 18), new PointF(15, 18),
                    new PointF(13, 29), new PointF(25, 13), new PointF(17, 13), new PointF(18, 3) });
                g.FillPath(Brushes.White, bolt);
            }
            return bmp;
        }
    }

    // Light/dark settings from Settings > Personalization > Colors.
    static class Theme
    {
        static bool IsLight(string valueName)
        {
            using (var k = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                var v = k == null ? null : k.GetValue(valueName);
                return !(v is int) || (int)v != 0;
            }
        }

        // "Windows mode": taskbar, Start, tray menus.
        public static bool SystemIsDark { get { return !IsLight("SystemUsesLightTheme"); } }
        // "App mode": app windows such as the About dialog.
        public static bool AppsAreDark { get { return !IsLight("AppsUseLightTheme"); } }
    }

    // Native menus are light unless the process opts into dark mode. uxtheme exports for this are
    // undocumented (by ordinal) but stable since Windows 10 1903; they're used by e.g. Notepad++.
    static class MenuTheme
    {
        const int ForceDark = 2, ForceLight = 3;

        [DllImport("uxtheme.dll", EntryPoint = "#135")]
        static extern int SetPreferredAppMode(int mode);

        [DllImport("uxtheme.dll", EntryPoint = "#136")]
        static extern void FlushMenuThemes();

        // Call right before the menu opens so it picks up theme changes.
        public static void Apply()
        {
            try
            {
                SetPreferredAppMode(Theme.SystemIsDark ? ForceDark : ForceLight);
                FlushMenuThemes();
            }
            catch { } // older Windows: menus stay light
        }
    }

    // About window in the Windows 11 style; follows the light/dark app mode.
    // Small dialog in the Windows 11 style that follows the light/dark app mode: app icon on the
    // left, content in Info, buttons bottom right. Used by the About window and the installer.
    class ThemedForm : Form
    {
        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
        const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19; // 19 before Windows 10 20H1
        const int DWMWA_CLOAK = 13;

        [DllImport("user32.dll")]
        static extern bool RedrawWindow(IntPtr hwnd, IntPtr rect, IntPtr region, uint flags);
        const uint RDW_INVALIDATE = 0x1, RDW_ERASE = 0x4, RDW_ALLCHILDREN = 0x80, RDW_UPDATENOW = 0x100;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern uint PrivateExtractIcons(string file, int index, int cx, int cy, IntPtr[] icons, int[] ids, uint count, uint flags);
        [DllImport("user32.dll")]
        static extern bool DestroyIcon(IntPtr icon);

        protected readonly bool dark = Theme.AppsAreDark;
        readonly float scale;
        protected readonly Color TextColor, DimColor, LinkColor;
        protected readonly Font TitleFont = new Font("Segoe UI Semibold", 14f);
        protected readonly FlowLayoutPanel Info, Buttons;

        protected int S(int v) { return (int)Math.Round(v * scale); } // 96-DPI pixels to device pixels

        protected ThemedForm(string title)
        {
            using (var g = CreateGraphics()) scale = g.DpiX / 96f;

            Color back = dark ? Color.FromArgb(32, 32, 32) : Color.White;
            TextColor = dark ? Color.White : Color.Black;
            DimColor  = dark ? Color.FromArgb(170, 170, 170) : Color.FromArgb(96, 96, 96);
            LinkColor = dark ? Color.FromArgb(96, 205, 255) : Color.FromArgb(0, 95, 184);

            Text = title;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            ShowIcon = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.Manual; // centered in OnLoad, once AutoSize has set the final size
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = back;
            ForeColor = TextColor;
            Font = new Font("Segoe UI", 9f);
            Padding = new Padding(S(24), S(20), S(24), S(16));

            var layout = new TableLayoutPanel { ColumnCount = 2, RowCount = 2, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };

            var logo = AppIcon(S(48));
            if (logo != null)
                layout.Controls.Add(new PictureBox { Image = logo, SizeMode = PictureBoxSizeMode.AutoSize, Margin = new Padding(0, S(4), S(16), 0) }, 0, 0);

            Info = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Margin = Padding.Empty };
            layout.Controls.Add(Info, 1, 0);

            Buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Anchor = AnchorStyles.Right, Margin = new Padding(0, S(20), 0, 0) };
            layout.Controls.Add(Buttons, 0, 1);
            layout.SetColumnSpan(Buttons, 2);

            // AutoSize adds Padding only right/bottom of the content, so offset it by the left/top padding.
            layout.Location = new Point(Padding.Left, Padding.Top);
            Controls.Add(layout);
        }

        protected Label Line(string text, Font font, Color color, int gapBelow)
        {
            return new Label
            {
                Text = text, Font = font, ForeColor = color, AutoSize = true,
                MaximumSize = new Size(S(440), 0), Margin = new Padding(0, 0, 0, gapBelow)
            };
        }

        protected LinkLabel Link(string url)
        {
            var link = new LinkLabel
            {
                Text = url.Replace("https://", ""), AutoSize = true, Margin = Padding.Empty,
                LinkColor = LinkColor, ActiveLinkColor = LinkColor, VisitedLinkColor = LinkColor, LinkBehavior = LinkBehavior.HoverUnderline
            };
            link.LinkClicked += delegate { try { System.Diagnostics.Process.Start(url); } catch { } };
            return link;
        }

        // Adds a button to the bottom-right row, in order from left to right.
        protected Button AddButton(string text, EventHandler click)
        {
            var b = new Button { Text = text, MinimumSize = new Size(S(96), S(32)), AutoSize = true, Margin = new Padding(S(8), 0, 0, 0) };
            if (Buttons.Controls.Count == 0) b.Margin = Padding.Empty;
            if (dark)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.BackColor = Color.FromArgb(55, 55, 55);
                b.FlatAppearance.BorderColor = Color.FromArgb(85, 85, 85);
                b.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 65, 65);
                b.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 45, 45);
            }
            else b.FlatStyle = FlatStyle.System;
            b.Click += click;
            Buttons.Controls.Add(b);
            return b;
        }

        protected CheckBox MakeCheckBox(string text, bool isChecked)
        {
            // WinForms' flat checkbox draws a gray box on dark backgrounds, so dark mode draws its own.
            var c = dark ? new DarkCheckBox(LinkColor) : new CheckBox();
            c.Text = text;
            c.Checked = isChecked;
            c.AutoSize = true;
            c.Margin = Padding.Empty;
            return c;
        }

        // Windows 11 dark-mode checkbox: rounded gray outline, accent fill with a dark check when checked.
        class DarkCheckBox : CheckBox
        {
            readonly Color accent;
            public DarkCheckBox(Color accent) { this.accent = accent; SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.Clear(Parent != null ? Parent.BackColor : BackColor);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                float k = g.DpiX / 96f, box = 16 * k;
                var r = new RectangleF(0.5f * k, (Height - box) / 2, box, box);
                using (var path = Rounded(r, 3.5f * k))
                {
                    if (Checked) using (var b = new SolidBrush(accent)) g.FillPath(b, path);
                    else using (var p = new Pen(Color.FromArgb(160, 160, 160), 1f * k)) g.DrawPath(p, path);
                }
                if (Checked)
                    using (var p = new Pen(Color.Black, 1.6f * k) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                        g.DrawLines(p, new[] { new PointF(r.Left + 4 * k, r.Top + 8.5f * k), new PointF(r.Left + 7 * k, r.Top + 11.5f * k), new PointF(r.Left + 12 * k, r.Top + 5 * k) });
                var text = new Rectangle((int)(r.Right + 8 * k), 0, Width - (int)(r.Right + 8 * k), Height);
                TextRenderer.DrawText(g, Text, Font, text, ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix);
                if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, new Rectangle(text.Left - 2, 1, Math.Min(text.Width, TextRenderer.MeasureText(Text, Font).Width + 4), Height - 2), ForeColor, Parent.BackColor);
            }

            public override Size GetPreferredSize(Size proposed)
            {
                float k; using (var g = CreateGraphics()) k = g.DpiX / 96f;
                var t = TextRenderer.MeasureText(Text, Font);
                return new Size((int)(16 * k + 8 * k) + t.Width + 2, Math.Max(t.Height, (int)(16 * k)) + (int)(4 * k));
            }

            static GraphicsPath Rounded(RectangleF r, float radius)
            {
                float d = radius * 2;
                var p = new GraphicsPath();
                p.AddArc(r.Left, r.Top, d, d, 180, 90);
                p.AddArc(r.Right - d, r.Top, d, d, 270, 90);
                p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                p.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
                p.CloseFigure();
                return p;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            PerformLayout();
            var area = Screen.FromPoint(Cursor.Position).WorkingArea; // the screen with the tray icon that was clicked
            Location = new Point(area.Left + (area.Width - Width) / 2, area.Top + (area.Height - Height) / 2);
            base.OnLoad(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            int on = 1;
            // Keep the window invisible until it has painted (see OnShown); otherwise it flashes white first.
            DwmSetWindowAttribute(Handle, DWMWA_CLOAK, ref on, 4);
            if (dark && DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref on, 4) != 0)
                DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_OLD, ref on, 4);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_ERASE | RDW_ALLCHILDREN | RDW_UPDATENOW);
            int off = 0;
            DwmSetWindowAttribute(Handle, DWMWA_CLOAK, ref off, 4);
        }

        // The exe's embedded icon (app.ico) at an exact size, so it stays sharp at high DPI.
        static Bitmap AppIcon(int size)
        {
            var handles = new IntPtr[1];
            if (PrivateExtractIcons(Application.ExecutablePath, 0, size, size, handles, new int[1], 1, 0) != 1 || handles[0] == IntPtr.Zero)
                return null;
            try { using (var icon = Icon.FromHandle(handles[0])) return icon.ToBitmap(); }
            finally { DestroyIcon(handles[0]); }
        }
    }

    class AboutForm : ThemedForm
    {
        public const string RepoUrl = "https://github.com/hafthalion/power-mode-toggle";

        public AboutForm(string hotkeys) // one "name:  key" per line
            : base("About Power Mode Toggle")
        {
            Info.Controls.Add(Line("Power Mode Toggle", TitleFont, TextColor, 0));
            Info.Controls.Add(Line("Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3), Font, DimColor, S(14)));
            Info.Controls.Add(Line("Switches the Windows power mode between Best efficiency and Best performance.", Font, TextColor, S(14)));
            Info.Controls.Add(Line("Hotkeys:", Font, TextColor, 0));
            Info.Controls.Add(Line(hotkeys, Font, DimColor, S(10)));
            Info.Controls.Add(Line("Settings file:", Font, TextColor, 0));
            Info.Controls.Add(Line(Settings.FilePath, Font, DimColor, S(14)));
            Info.Controls.Add(Link(RepoUrl));
            AcceptButton = CancelButton = AddButton("OK", delegate { Close(); });
        }
    }

    class TrayApp : ApplicationContext
    {
        readonly Settings settings = Settings.Load();
        readonly NotifyIcon tray = new NotifyIcon();
        readonly Dictionary<PowerMode, Icon> icons = new Dictionary<PowerMode, Icon>();
        readonly HotkeyWindow hotkeys = new HotkeyWindow();
        readonly System.Windows.Forms.Timer poll = new System.Windows.Forms.Timer { Interval = 2000 };
        readonly MenuItem miEfficiency, miBalanced, miPerformance, miAutostart, miNotify;
        readonly string toggleHint, aboutHotkeys; // hotkey texts for the tooltip and the About window
        PowerMode current = PowerMode.Unknown;

        static string Label(PowerMode m)
        {
            switch (m)
            {
                case PowerMode.Efficiency: return "Best efficiency";
                case PowerMode.Balanced: return "Balanced";
                case PowerMode.Performance: return "Best performance";
                default: return "Unknown";
            }
        }

        public TrayApp()
        {
            foreach (PowerMode m in Enum.GetValues(typeof(PowerMode))) icons[m] = Icons.Make(m);

            // Hotkeys notify on use: there's no other feedback for them.
            var hotkeyErrors = new List<string>();
            Func<string, Action, string> register = (spec, action) =>
            {
                var error = hotkeys.Register(spec, action);
                if (error == null) return spec.Trim();
                hotkeyErrors.Add(error);
                return "(unavailable)";
            };
            string toggleKey      = register(settings.ToggleHotkey,      () => Toggle(true));
            string efficiencyKey  = register(settings.EfficiencyHotkey,  () => Apply(PowerMode.Efficiency, true));
            string performanceKey = register(settings.PerformanceHotkey, () => Apply(PowerMode.Performance, true));
            toggleHint = toggleKey;
            aboutHotkeys = "Toggle:  " + Or(toggleKey, "off") + "\n" +
                           "Best efficiency:  " + Or(efficiencyKey, "off") + "\n" +
                           "Best performance:  " + Or(performanceKey, "off");

            // Native Win32 menu (not ContextMenuStrip): gets the Windows 11 look and follows the dark theme.
            var title = new MenuItem("Power mode") { Enabled = false };
            miEfficiency  = new MenuItem(WithShortcut(Label(PowerMode.Efficiency), efficiencyKey),   delegate { Apply(PowerMode.Efficiency, false); })  { RadioCheck = true };
            miBalanced    = new MenuItem(Label(PowerMode.Balanced),                                  delegate { Apply(PowerMode.Balanced, false); })    { RadioCheck = true };
            miPerformance = new MenuItem(WithShortcut(Label(PowerMode.Performance), performanceKey), delegate { Apply(PowerMode.Performance, false); }) { RadioCheck = true };
            var miToggle  = new MenuItem(WithShortcut("Toggle efficiency / performance", toggleKey),
                                         delegate { Toggle(false); }) { DefaultItem = true }; // bold = left-click action
            miAutostart = new MenuItem("Start with Windows", delegate
            {
                try { Autostart.IsEnabled = !Autostart.IsEnabled; }
                catch (Exception ex) { Notify("Could not change autostart", ex.Message, ToolTipIcon.Error); }
            });
            miNotify = new MenuItem("Show notifications", delegate
            {
                settings.ShowNotifications = !settings.ShowNotifications;
                settings.Save();
            });
            var miSettings = new MenuItem("Edit settings file...", delegate
            {
                try { System.Diagnostics.Process.Start("notepad.exe", "\"" + Settings.FilePath + "\""); } catch { }
            });
            var miAbout = new MenuItem("About Power Mode Toggle", delegate { ShowAbout(); });
            var miExit = new MenuItem("Exit", delegate { ExitThread(); });

            var menu = new ContextMenu(new[] {
                title, miEfficiency, miBalanced, miPerformance, new MenuItem("-"),
                miToggle, new MenuItem("-"),
                miAutostart, miNotify, miSettings, new MenuItem("-"), miAbout, miExit });
            menu.Popup += delegate
            {
                MenuTheme.Apply();
                Refresh();
                miAutostart.Checked = Autostart.IsEnabled;
                miNotify.Checked = settings.ShowNotifications;
            };

            tray.ContextMenu = menu;
            tray.MouseClick += (s, e) => { if (e.Button == MouseButtons.Left) Toggle(false); };
            tray.Visible = true;

            poll.Tick += delegate { Refresh(); };
            poll.Start();
            Refresh();

            if (hotkeyErrors.Count > 0)
                Notify(hotkeyErrors.Count == 1 ? "Hotkey unavailable" : "Hotkeys unavailable",
                       string.Join(" ", hotkeyErrors) + " Edit the settings file to choose another.", ToolTipIcon.Warning);
        }

        static string Or(string value, string fallback) { return string.IsNullOrEmpty(value) ? fallback : value; }

        // Native menus show text after a tab right-aligned, like a shortcut.
        static string WithShortcut(string text, string shortcut) { return string.IsNullOrEmpty(shortcut) ? text : text + "\t" + shortcut; }

        void Toggle(bool notify)
        {
            var target = PowerApi.Get() == PowerMode.Performance ? PowerMode.Efficiency : PowerMode.Performance;
            Apply(target, notify);
        }

        void Apply(PowerMode mode, bool notify)
        {
            uint err = PowerApi.Set(mode);
            Refresh();
            if (err != 0 || current != mode)
                Notify("Could not switch to " + Label(mode),
                       "Error " + err + ". Power modes need the Balanced power plan, which may be missing on this PC.",
                       ToolTipIcon.Error);
            else if (notify && settings.ShowNotifications)
                Notify(null, Label(mode), ToolTipIcon.None, mode);
        }

        AboutForm about;

        void ShowAbout()
        {
            if (about == null)
            {
                about = new AboutForm(aboutHotkeys);
                about.FormClosed += delegate { about = null; };
                about.Show();
            }
            about.Activate(); // if it's already open, bring it to the front
        }

        void Refresh()
        {
            var mode = PowerApi.Get();
            miEfficiency.Checked = mode == PowerMode.Efficiency;
            miBalanced.Checked = mode == PowerMode.Balanced;
            miPerformance.Checked = mode == PowerMode.Performance;
            var otherPlan = mode == PowerMode.Unknown ? PowerApi.OtherPlanName() : null;
            if (mode == current && otherPlan == currentOtherPlan) return;
            current = mode;
            currentOtherPlan = otherPlan;
            tray.Icon = icons[mode];
            var hint = string.IsNullOrEmpty(toggleHint) ? "" : " (" + toggleHint + ")";
            if (otherPlan == null)
            {
                SetTooltip("Power mode: " + Label(mode) + "\nClick to toggle" + hint);
                return;
            }
            var rest = "\nPower modes only work with the Balanced plan.\nClick to switch to it and toggle" + hint;
            var head = "Power plan: " + otherPlan;
            int room = TooltipMax - rest.Length; // shorten a long plan name, keep the explanation
            if (head.Length > room) head = head.Substring(0, Math.Max(0, room - 3)) + "...";
            SetTooltip(head + rest);
        }

        string currentOtherPlan; // name of the active power plan when it isn't Balanced

        // Windows allows 127 tooltip characters, but NotifyIcon.Text rejects more than 63.
        // Set its private field and refresh the icon instead; fall back to the shortened text.
        const int TooltipMax = 127;

        void SetTooltip(string tip)
        {
            if (tip.Length > TooltipMax) tip = tip.Substring(0, TooltipMax);
            if (tip.Length > 63)
            {
                try
                {
                    const BindingFlags f = BindingFlags.Instance | BindingFlags.NonPublic;
                    var t = typeof(NotifyIcon);
                    t.GetField("text", f).SetValue(tray, tip);
                    if ((bool)t.GetField("added", f).GetValue(tray))
                        t.GetMethod("UpdateIcon", f).Invoke(tray, new object[] { true });
                    return;
                }
                catch { tip = tip.Substring(0, 63); }
            }
            tray.Text = tip;
        }

        void Notify(string title, string text, ToolTipIcon kind, PowerMode? logo = null)
        {
            if (!Toast.Show(title, text, logo))
                tray.ShowBalloonTip(3000, title ?? "", text, kind);
        }

        protected override void ExitThreadCore()
        {
            poll.Stop();
            tray.Visible = false;
            tray.Dispose();
            hotkeys.Dispose();
            base.ExitThreadCore();
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool created;
            using (new Mutex(true, @"Local\PowerModeToggle.SingleInstance", out created))
            {
                if (!created) return;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Toast.Register();
                Autostart.UpdatePath();
                Application.Run(new TrayApp());
            }
        }
    }
}
