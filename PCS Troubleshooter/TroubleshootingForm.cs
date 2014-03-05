using Microsoft.Win32;
using Org.Mentalis.Files;
using SevenZip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PCS_Troubleshooter
{
    public partial class TroubleshootingForm : Form
    {
        private string FalloutPath;

        private const string Fallout32BitRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Bethesda Softworks\Fallout3";
        private const string Fallout64BitRegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Bethesda Softworks\Fallout3";

        private const int ADD_LIST_VIEW_ITEM = 1;
        private const int ERROR_MISSING_REGISTRY_KEY = 2;
        private const int INITIALIZE_FORM = 3;
        private const int SET_PROGRESS = 4;

        public TroubleshootingForm()
        {
            InitializeComponent();

            ContextMenu contextMenu = new ContextMenu();

            contextMenu.Collapse += contextMenu_Collapse;
            contextMenu.Popup += contextMenu_Popup;

            this.lvProgress.ContextMenu = contextMenu;

            Program.SetWindowTheme(lvProgress.Handle, "explorer", null);
            Program.SendMessage(lvProgress.Handle, 0x127, 0x10001, 0);
            Program.SendMessage(lvProgress.Handle, 0x1000 + 54, 0x00010000, 0x00010000);
        }

        private void TroubleshootingForm_Load(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void btnDoneCancel_Click(object sender, EventArgs e)
        {
            if (btnDoneCancel.Text == "Cancel")
            {
                backgroundWorker1.CancelAsync();
            }
            else if (btnDoneCancel.Text == "Done")
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void lvProgress_Enter(object sender, EventArgs e)
        {
            Program.SendMessage(lvProgress.Handle, 0x127, 0x10001, 0);
        }

        private void lvProgress_SelectedIndexChanged(object sender, EventArgs e)
        {
            Program.SendMessage(lvProgress.Handle, 0x127, 0x10001, 0);
        }

        #region Consts

        private const string ERROR_MASTER_ACTIVATED = "Master plugin hasn't been activated. Right click to fix.";
        private const string ERROR_THE_PITT_ACTIVATED = "'The Pitt' plugin hasn't been activated. Right click to fix.";
        private const string ERROR_POINT_LOOKOUT_ACTIVATED = "'Point Lookout' plugin hasn't been activated. Right click to fix.";
        private const string ERROR_CRAFT_ACTIVATED = "'CRAFT' plugin hasn't been activated. Right click to fix.";
        private const string ERROR_FWE_ACTIVATED = "'FWE' plugin hasn't been activated. Right click to fix.";
        private const string ERROR_WMK_ACTIVATED = "'WeaponModKits' plugin hasn't been activated. Right click to fix.";
        private const string ERROR_FOSE_MISSING = "FOSE is missing one or more files from the latest version. Right click to fix.";
        private const string ERROR_INVALIDATE_OLDER_FILES = "'bInvalidateOlderFiles' hasn't been set to 1. Right click to fix.";
        private const string ERROR_AII_BSA_FILE = "Missing 'ArchiveInvalidationInvalidated!.bsa' file. Right click to fix.";
        private const string ERROR_AII_INI = "'ArchiveInvalidationInvalidated!.bsa' is missing from INI 'SArchiveList' key. Right click to fix.";
        private const string ERROR_TEXTURES_BSA_LOADED_FIRST = "'Fallout - Textures.bsa' loaded before 'ArchiveInvalidationInvalidated!.bsa'. Right click to fix.";
        private const string ERROR_LOAD_EGT_FILES = "'bLoadFaceGenHeadEGTFiles' not set to '1'. Right click to fix.";

        #endregion

        #region contextMenu

        private void contextMenu_Collapse(object sender, EventArgs e)
        {
            (sender as ContextMenu).MenuItems.Clear();
        }

        private void contextMenu_Popup(object sender, EventArgs e)
        {
            if (lvProgress.SelectedItems.Count == 0)
                return;

            ContextMenu contextMenu = sender as ContextMenu;
            string subItem = lvProgress.SelectedItems[0].SubItems[1].Text;

            switch (subItem)
            {
                case ERROR_MASTER_ACTIVATED:
                    contextMenu.MenuItems.Add("Activate master plugin.", activateMasterMenuItem);
                    break;
                case ERROR_THE_PITT_ACTIVATED:
                    contextMenu.MenuItems.Add("Activate The Pitt plugin.", activateThePittMenuItem);
                    break;
                case ERROR_POINT_LOOKOUT_ACTIVATED:
                    contextMenu.MenuItems.Add("Activate Point Lookout plugin.", activatePointLookoutMenuItem);
                    break;
                case ERROR_CRAFT_ACTIVATED:
                    contextMenu.MenuItems.Add("Activate CRAFT plugin.", activateCRAFTMenuItem);
                    break;
                case ERROR_FWE_ACTIVATED:
                    contextMenu.MenuItems.Add("Activate FWE plugin.", activateFWEMenuItem);
                    break;
                case ERROR_WMK_ACTIVATED:
                    contextMenu.MenuItems.Add("Activate WMK plugin.", activateWMKMenuItem);
                    break;
                case ERROR_FOSE_MISSING:
                    contextMenu.MenuItems.Add("Download && install FOSE", fixFOSEMenuItem);
                    break;
                case ERROR_INVALIDATE_OLDER_FILES:
                    contextMenu.MenuItems.Add("Fix INI", fixINIMenuItem1);
                    break;
                case ERROR_AII_BSA_FILE:
                    contextMenu.MenuItems.Add("Create missing file", createAIIBSAMenuItem);
                    break;
                case ERROR_AII_INI:
                    contextMenu.MenuItems.Add("Add BSA to 'SArchiveList'", addBSAINIMenuItem);
                    break;
                case ERROR_TEXTURES_BSA_LOADED_FIRST:
                    contextMenu.MenuItems.Add("Fix BSA load order", fixBSAOrderMenuItem);
                    break;
                case ERROR_LOAD_EGT_FILES:
                    contextMenu.MenuItems.Add("Fix INI", fixINIMenuItem2);
                    break;
            }
        }

        private void activateMasterMenuItem(object sender, EventArgs e)
        {
            try
            {
                ActivatePlugin("PortableCampStuff.esm");
                lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(this, "Couldn't find 'plugins.txt' file. Start Fallout 3 to generate new one.");
            }
        }

        private void activateThePittMenuItem(object sender, EventArgs e)
        {
            try
            {
                ActivatePlugin("ThePitt.esm");
                lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(this, "Couldn't find 'plugins.txt' file. Start Fallout 3 to generate new one.");
            }
        }

        private void activatePointLookoutMenuItem(object sender, EventArgs e)
        {
            try
            {
                ActivatePlugin("PointLookout.esm");
                lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(this, "Couldn't find 'plugins.txt' file. Start Fallout 3 to generate new one.");
            }
        }

        private void activateCRAFTMenuItem(object sender, EventArgs e)
        {
            try
            {
                ActivatePlugin("CRAFT.esm");
                lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(this, "Couldn't find 'plugins.txt' file. Start Fallout 3 to generate new one.");
            }
        }

        private void activateFWEMenuItem(object sender, EventArgs e)
        {
            try
            {
                ActivatePlugin("FO3 Wanderers Edition - Main File.esm");
                lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(this, "Couldn't find 'plugins.txt' file. Start Fallout 3 to generate new one.");
            }
        }

        private void activateWMKMenuItem(object sender, EventArgs e)
        {
            try
            {
                ActivatePlugin("WeaponModKits.esp");
                lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(this, "Couldn't find 'plugins.txt' file. Start Fallout 3 to generate new one.");
            }
        }

        private void fixFOSEMenuItem(object sender, EventArgs e)
        {
            bool includeTextFiles = false;

            DialogResult result = MessageBox.Show(this, "Do you want to include text files?",
                "Text Files", MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Cancel)
                return;

            includeTextFiles = (result == DialogResult.Yes);

            ProgressForm progressForm;

            progressForm = new ProgressForm();
            progressForm.Show(this);
            progressForm.SetMaximum(50);

            new Thread(delegate()
                {
                    string url1 = @"http://fose.silverlock.org/download/fose_v1_2_beta2.7z";
                    string url2 = @"http://fose.silverlock.org/download/fose_loader.7z";
                    string dest1 = Application.StartupPath + @"\temp\fose_v1_2_beta2.7z";
                    string dest2 = Application.StartupPath + @"\temp\fose_loader.7z";
                    string temp = Application.StartupPath + @"\temp";

                    if (Directory.Exists(temp))
                        Directory.Delete(temp, true);

                    Directory.CreateDirectory(temp);

                    using (var webClient = new WebClient())
                    {
                        progressForm.SetProgressText("Downloading: fose_v1_2_beta2.7z");

                        for (int attempts = 1; attempts <= 3; attempts++)
                        {
                            try
                            {
                                webClient.DownloadFile(url1, dest1);
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);

                                if (attempts == 3)
                                {
                                    progressForm.Close();
                                    MessageBox.Show("Failed to download FOSE. Try again later.");
                                    return;
                                }

                                Thread.Sleep(1000);
                            }
                        }

                        progressForm.SetProgress(10);

                        progressForm.SetProgressText("Downloading: fose_loader.7z");

                        for (int attempts = 1; attempts <= 3; attempts++)
                        {
                            try
                            {
                                webClient.DownloadFile(url2, dest2);
                                break;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);

                                if (attempts == 3)
                                {
                                    progressForm.Close();
                                    MessageBox.Show("Failed to download FOSE. Try again later.");
                                    return;
                                }

                                Thread.Sleep(1000);
                            }
                        }

                        progressForm.SetProgress(20);
                    }

                    progressForm.SetProgressText("Extracting: fose_v1_2_beta2.7z");
                    ExtractFOSEMain(includeTextFiles);
                    progressForm.SetProgress(30);

                    progressForm.SetProgressText("Extracting: fose_loader.7z");
                    ExtractFOSELoader();
                    progressForm.SetProgress(40);

                    progressForm.SetProgressText("Cleaning up...");

                    for (int attempts = 1; attempts <= 3; attempts++)
                    {
                        try
                        {
                            Directory.Delete(temp, true);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);

                            if (attempts == 3)
                            {
                                MessageBox.Show("Failed to delete temp directory. Try again later.");
                                break;
                            }

                            Thread.Sleep(1000);
                        }
                    }

                    progressForm.SetProgress(50);

                    progressForm.Close();

                    lvProgress.Invoke(new Action(() => { lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!"; }));
                    MessageBox.Show("Installed FOSE. Re-run troubleshooter to make sure everything works.");
                }).Start();
        }

        private void fixINIMenuItem1(object sender, EventArgs e)
        {
            string file = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Fallout3\FALLOUT.INI";

            if (!File.Exists(file))
            {
                MessageBox.Show(this, "'FALLOUT.INI' doesn't exist anymore. Run Fallout 3 to generate new.");
                return;
            }

            IniReader ini = new IniReader(file);

            ini.Write("Archive", "bInvalidateOlderFiles", 1);

            MessageBox.Show(this, "'bInvalidateOlderFiles' has been set to '1'. Re-run troubleshooter to make sure everything works.");

            lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
        }

        private void createAIIBSAMenuItem(object sender, EventArgs e)
        {
            string file = Path.Combine(this.FalloutPath, @"Data\ArchiveInvalidationInvalidated!.bsa");

            File.WriteAllBytes(file, Properties.Resources.ArchiveInvalidationInvalidated_);

            MessageBox.Show(this, "Created 'ArchiveInvalidationInvalidated!.bsa'. Re-run troubleshooter to make sure everything works.");

            lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
        }

        private void addBSAINIMenuItem(object sender, EventArgs e)
        {
            string file = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Fallout3\FALLOUT.INI";

            if (!File.Exists(file))
            {
                MessageBox.Show(this, "'FALLOUT.INI' doesn't exist anymore. Run Fallout 3 to generate new.");
                return;
            }

            IniReader ini = new IniReader(file);

            string value = ini.ReadString("Archive", "SArchiveList");

            value = "ArchiveInvalidationInvalidated!.bsa, " + value;

            ini.Write("Archive", "SArchiveList", value);

            MessageBox.Show(this, "'SArchiveList' has been fixed. Re-run troubleshooter to make sure everything works.");

            lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
        }

        private void fixBSAOrderMenuItem(object sender, EventArgs e)
        {
            string file = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Fallout3\FALLOUT.INI";

            if (!File.Exists(file))
            {
                MessageBox.Show(this, "'FALLOUT.INI' doesn't exist anymore. Run Fallout 3 to generate new.");
                return;
            }

            IniReader ini = new IniReader(file);

            string[] split = ini.ReadString("Archive", "SArchiveList").Split(',');
            List<string> values = new List<string>();

            for (int i = 0; i < split.Length; i++)
            {
                // Remove space.
                if (split[i].StartsWith(" "))
                {
                    split[i] = split[i].Substring(1);
                }

                if (split[i] == "ArchiveInvalidationInvalidated!.bsa")
                    values.Insert(0, split[i]);
                else
                    values.Add(split[i]);
            }

            StringBuilder builder = new StringBuilder();

            foreach (string s in values)
            {
                if (builder.ToString() != "")
                    builder.Append(", ");

                builder.Append(s);
            }

            ini.Write("Archive", "SArchiveList", builder.ToString());

            MessageBox.Show(this, "'SArchiveList' has been fixed. Re-run troubleshooter to make sure everything works.");

            lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
        }

        private void fixINIMenuItem2(object sender, EventArgs e)
        {
            string file = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Fallout3\FALLOUT.INI";

            if (!File.Exists(file))
            {
                MessageBox.Show(this, "'FALLOUT.INI' doesn't exist anymore. Run Fallout 3 to generate new.");
                return;
            }

            IniReader ini = new IniReader(file);

            ini.Write("General", "bLoadFaceGenHeadEGTFiles", 1);

            MessageBox.Show(this, "'bLoadFaceGenHeadEGTFiles' has been set to '1'. Re-run troubleshooter to make sure everything works.");

            lvProgress.SelectedItems[0].SubItems[1].Text = "Fixed!";
        }

        #endregion

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);

            if (backgroundWorker1.CancellationPending)
            {
                this.Invoke(new Action(() => { this.DialogResult = DialogResult.Cancel; }));
                return;
            }

            if (this.FalloutPath == null)
            {
                string registryKey = Is64Bit() ? Fallout64BitRegistryKey : Fallout32BitRegistryKey;

                this.FalloutPath = (string)Registry.GetValue(registryKey, "installed path", "");
            }

            if (this.FalloutPath == "")
            {
                backgroundWorker1.ReportProgress(ERROR_MISSING_REGISTRY_KEY);

                return;
            }

            backgroundWorker1.ReportProgress(INITIALIZE_FORM, 120);

            // Plugins
            // ================================================================
            var newItem = new ListViewItem("Checking plugins (.esp/.esm files)...");

            newItem.SubItems.Add(ValidatePlugins());

            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 10);
            // ================================================================

            // Meshes
            // ================================================================
            newItem = new ListViewItem("Checking meshes...");

            newItem.SubItems.Add(ValidateMeshes());

            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 20);
            // ================================================================

            // Textures
            // ================================================================
            newItem = new ListViewItem("Checking textures...");

            newItem.SubItems.Add(ValidateTextures());

            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 30);
            // ================================================================

            // FOSE
            // ================================================================
            newItem = new ListViewItem("Checking FOSE...");

            newItem.SubItems.Add(ValidateFOSE());

            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 40);
            // ================================================================

            // Masters
            // ================================================================
            newItem = new ListViewItem("Checking masters...");
            newItem.SubItems.Add("--");
            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 50);

            // The Pitt
            newItem = new ListViewItem(" - Checking The Pitt...");
            newItem.SubItems.Add(ValidateThePitt());
            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 60);

            // Point Lookout
            newItem = new ListViewItem(" - Checking Point Lookout");
            newItem.SubItems.Add(ValidatePointLookout());
            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 70);

            // CRAFT
            newItem = new ListViewItem(" - Checking CRAFT");
            newItem.SubItems.Add(ValidateCRAFT());
            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 80);

            // FWE
            newItem = new ListViewItem(" - Checking FWE");
            newItem.SubItems.Add(ValidateFWE());
            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 90);

            // WMK
            newItem = new ListViewItem(" - Checking WMK");
            newItem.SubItems.Add(ValidateWMK());
            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);

            backgroundWorker1.ReportProgress(SET_PROGRESS, 100);
            // ================================================================

            // INI
            // ================================================================
            newItem = new ListViewItem("Checking INI...");

            newItem.SubItems.Add(ValidateINI());

            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 110);
            // ================================================================

            // ArchiveInvalidationInvalidated!
            // ================================================================
            newItem = new ListViewItem("Checking ArchiveInvalidationInvalidated!...");

            newItem.SubItems.Add(ValidateAI());

            backgroundWorker1.ReportProgress(ADD_LIST_VIEW_ITEM, newItem);
            backgroundWorker1.ReportProgress(SET_PROGRESS, 120);
            // ================================================================
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case ADD_LIST_VIEW_ITEM:
                    lvProgress.Items.Add((ListViewItem)e.UserState);
                    break;
                case ERROR_MISSING_REGISTRY_KEY:
                    HandleMissingRegistryKey();
                    break;
                case INITIALIZE_FORM:
                    progressBar1.Maximum = (int)e.UserState;
                    break;
                case SET_PROGRESS:
                    progressBar1.Value = (int)e.UserState;
                    break;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnDoneCancel.Text = "Done";
        }

        private void ActivatePlugin(string plugin)
        {
            string file = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Fallout3\\plugins.txt";

            if (!File.Exists(file))
                throw new FileNotFoundException("Missing file.", file);

            List<string> plugins = new List<string>();

            plugins.AddRange(File.ReadAllLines(file));

            List<string> esm = new List<string>();
            List<string> esp = new List<string>();

            foreach (string p in plugins)
            {
                if (p.EndsWith("esm"))
                {
                    esm.Add(p);
                }
                else if (p.EndsWith("esp"))
                {
                    esp.Add(p);
                }
            }

            if (plugin.EndsWith("esm"))
            {
                esm.Insert(esm.Count, plugin);
            }
            else if (plugin.EndsWith("esp"))
            {
                esp.Insert(esp.Count, plugin);
            }

            plugins.Clear();
            plugins.AddRange(esm);
            plugins.AddRange(esp);

            File.WriteAllLines(file, plugins);
        }

        private void ExtractFOSEMain(bool includeTextFiles)
        {
            string archive = Path.Combine(Application.StartupPath, "temp", "fose_v1_2_beta2.7z");
            string dest = Path.Combine(Application.StartupPath, "temp", "fose_v1_2_beta2");
            string directory = Path.Combine(dest, "fose_v1_2_beta2");

            SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, "7za.dll"));

            SevenZipExtractor extractor = new SevenZipExtractor(archive);
            extractor.ExtractArchive(dest);

            foreach (string file in Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly))
            {
                switch (Path.GetExtension(file))
                {
                    case ".dll":
                    case ".exe":
                        File.Copy(file, Path.Combine(this.FalloutPath, Path.GetFileName(file)), true);
                        break;
                    case ".html":
                    case ".txt":
                        if (includeTextFiles)
                        {
                            File.Copy(file, Path.Combine(this.FalloutPath, Path.GetFileName(file)), true);
                        }
                        break;
                }
            }
        }

        private void ExtractFOSELoader()
        {
            string archive = Path.Combine(Application.StartupPath, "temp", "fose_loader.7z");
            string dest = Path.Combine(Application.StartupPath, "temp", "fose_loader");

            SevenZipExtractor.SetLibraryPath(Path.Combine(Application.StartupPath, "7za.dll"));

            SevenZipExtractor extractor = new SevenZipExtractor(archive);
            extractor.ExtractArchive(dest);

            File.Copy(Path.Combine(dest, "fose_loader.exe"), Path.Combine(this.FalloutPath, "fose_loader.exe"), true);
        }

        private void HandleMissingRegistryKey()
        {
            var result = MessageBox.Show(this, "Missing Fallout 3 registry key." +
                Environment.NewLine +
                "Do you want to select the directory yourself?", "Missing Registry Key", MessageBoxButtons.YesNo);

            if (result == DialogResult.No)
                this.Close();
            else
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();

                if (fbd.ShowDialog(this) == DialogResult.Cancel)
                    this.Close();
                else
                {
                    this.FalloutPath = fbd.SelectedPath;
                    backgroundWorker1.RunWorkerAsync();
                }
            }
        }

        private bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!Program.IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }

        private bool Is64Bit()
        {
            bool is64BitProcess = (IntPtr.Size == 8);
            bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();

            return is64BitOperatingSystem;
        }

        private bool IsPluginActive(string plugin)
        {
            string file = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Fallout3\\plugins.txt";

            if (!File.Exists(file))
                throw new FileNotFoundException("Missing file.", file);

            using (var reader = new StreamReader(file))
            {
                string line = "";

                while ((line = reader.ReadLine()) != null)
                {
                    if (line == plugin)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private string ValidatePlugins()
        {
            string file = Path.Combine(this.FalloutPath, "Data", "PortableCampStuff.esm");

            if (!File.Exists(file))
                return "Missing PortableCampStuff.esm. Try running your mod manager as administrator and reinstall.";

            try
            {
                if (IsPluginActive("PortableCampStuff.esm"))
                {
                    return "Correct!";
                }
                else
                {
                    return ERROR_MASTER_ACTIVATED;
                }
            }
            catch (FileNotFoundException)
            {
                return "Missing 'plugins.txt' file. Running Fallout 3 might generate new one.";
            }
        }

        private string ValidateMeshes()
        {
            string directory = Path.Combine(this.FalloutPath, "Data\\Meshes\\_Alexx378\\Portable Camp Stuff");

            bool hasFiles;

            hasFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Length > 0;

            return Directory.Exists(directory) && hasFiles ? "Correct!" : "PortableCampStuff's Meshes folder is non-existent or empty.";
        }

        private string ValidateTextures()
        {
            string directory = Path.Combine(this.FalloutPath, "Data\\Textures\\_Alexx378\\Portable Camp Stuff");

            bool hasFiles;

            hasFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Length > 0;

            return Directory.Exists(directory) && hasFiles ? "Correct!" : "PortableCampStuff's Meshes folder is non-existent or empty.";
        }

        private string ValidateFOSE()
        {
            string[] files = new string[]
            {
                Path.Combine(this.FalloutPath, "fose_1_0.dll"),
                Path.Combine(this.FalloutPath, "fose_1_1.dll"),
                Path.Combine(this.FalloutPath, "fose_1_4.dll"),
                Path.Combine(this.FalloutPath, "fose_1_4b.dll"),
                Path.Combine(this.FalloutPath, "fose_1_5.dll"),
                Path.Combine(this.FalloutPath, "fose_1_6.dll"),
                Path.Combine(this.FalloutPath, "fose_1_7.dll"),
                Path.Combine(this.FalloutPath, "fose_1_7ng.dll"),
                Path.Combine(this.FalloutPath, "fose_editor_1_1.dll"),
                Path.Combine(this.FalloutPath, "fose_editor_1_5.dll"),
                Path.Combine(this.FalloutPath, "fose_loader.exe")
            };

            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    return ERROR_FOSE_MISSING;
                }
            }

            return "Correct!";
        }

        private string ValidateThePitt()
        {
            string file = Path.Combine(this.FalloutPath, "Data\\PortableCampStuff - The Pitt.esp");

            if (!File.Exists(file))
                return "Not installed.";

            string master = Path.Combine(this.FalloutPath, "Data\\ThePitt.esm");

            if (!File.Exists(master))
                return "Missing 'ThePitt.esm' master.";

            try
            {
                if (!IsPluginActive("ThePitt.esm"))
                {
                    return ERROR_THE_PITT_ACTIVATED;
                }
            }
            catch (FileNotFoundException)
            {
                return "Missing 'plugins.txt' file. Running Fallout 3 might generate new one.";
            }

            return "Correct!";
        }

        private string ValidatePointLookout()
        {
            string file = Path.Combine(this.FalloutPath, "Data\\PortableCampStuff - Point Lookout.esp");

            if (!File.Exists(file))
                return "Not installed.";

            string master = Path.Combine(this.FalloutPath, "Data\\PointLookout.esm");

            if (!File.Exists(master))
                return "Missing 'PointLookout.esm' master.";

            try
            {
                if (!IsPluginActive("PointLookout.esm"))
                {
                    return ERROR_POINT_LOOKOUT_ACTIVATED;
                }
            }
            catch (FileNotFoundException)
            {
                return "Missing 'plugins.txt' file. Running Fallout 3 might generate new one.";
            }

            return "Correct!";
        }

        private string ValidateCRAFT()
        {
            string file = Path.Combine(this.FalloutPath, "Data\\PortableCampStuff - CRAFT.esp");

            if (!File.Exists(file))
                return "Not installed.";

            string master = Path.Combine(this.FalloutPath, "Data\\CRAFT.esm");

            if (!File.Exists(master))
                return "Missing 'CRAFT.esm' master.";

            try
            {
                if (!IsPluginActive("CRAFT.esm"))
                {
                    return ERROR_CRAFT_ACTIVATED;
                }
            }
            catch (FileNotFoundException)
            {
                return "Missing 'plugins.txt' file. Running Fallout 3 might generate new one.";
            }

            return "Correct!";
        }

        private string ValidateFWE()
        {
            string file = Path.Combine(this.FalloutPath, "Data\\PortableCampStuff - FWE.esp");

            if (!File.Exists(file))
                return "Not installed.";

            string master = Path.Combine(this.FalloutPath, "Data\\FO3 Wanderers Edition - Main File.esm");

            if (!File.Exists(master))
                return "Missing 'FO3 Wanderers Edition - Main File.esm' master.";

            try
            {
                if (!IsPluginActive("FO3 Wanderers Edition - Main File.esm"))
                {
                    return ERROR_FWE_ACTIVATED;
                }
            }
            catch (FileNotFoundException)
            {
                return "Missing 'plugins.txt' file. Running Fallout 3 might generate new one.";
            }

            return "Correct!";
        }

        private string ValidateWMK()
        {
            string file = Path.Combine(this.FalloutPath, "Data\\PortableCampStuff - WMK.esp");

            if (!File.Exists(file))
                return "Not installed.";

            string master = Path.Combine(this.FalloutPath, "Data\\WeaponModKits.esp");

            if (!File.Exists(master))
                return "Missing 'WeaponModKits.esp' master.";

            try
            {
                if (!IsPluginActive("WeaponModKits.esp"))
                {
                    return ERROR_WMK_ACTIVATED;
                }
            }
            catch (FileNotFoundException)
            {
                return "Missing 'plugins.txt' file. Running Fallout 3 might generate new one.";
            }

            return "Correct!";
        }

        private string ValidateINI()
        {
            string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games\\Fallout3\\FALLOUT.INI");

            if (!File.Exists(file))
                return "Missing 'FALLOUT.INI' file. Run Fallout 3 once to generate new one, then come back.";

            using (var reader = new StreamReader(file))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("bInvalidateOlderFiles"))
                    {
                        return (line.EndsWith("1") ? "Correct!" : ERROR_INVALIDATE_OLDER_FILES);
                    }
                }
            }

            // 'bInvalidateOlderFiles' wasn't found in INI, but can be fixed automatically.
            return ERROR_INVALIDATE_OLDER_FILES;
        }

        private string ValidateAI()
        {
            string file = Path.Combine(this.FalloutPath, "Data\\ArchiveInvalidationInvalidated!.bsa");

            if (!File.Exists(file))
                return ERROR_AII_BSA_FILE;

            string ini = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "My Games\\Fallout3\\FALLOUT.INI");

            if (!File.Exists(ini))
                return "Missing 'FALLOUT.INI' file. Run Fallout 3 once to generate new one, then come back.";

            using (var reader = new StreamReader(ini))
            {
                string line = "";

                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("SArchiveList"))
                    {
                        string[] split = line.Split('=')[1].Split(',');

                        int iAI = -1;
                        int iTex = -1;

                        for (int i = 0; i < split.Length; i++)
                        {
                            switch (split[i])
                            {
                                case "ArchiveInvalidationInvalidated!.bsa":
                                case " ArchiveInvalidationInvalidated!.bsa":
                                    iAI = i;
                                    break;
                                case "Fallout - Textures.bsa":
                                case " Fallout - Textures.bsa":
                                    iTex = i;
                                    break;
                            }
                        }

                        if (iAI == -1)
                        {
                            return ERROR_AII_INI;
                        }

                        if (iAI > iTex)
                        {
                            return ERROR_TEXTURES_BSA_LOADED_FIRST;
                        }
                    }
                    else if (line.StartsWith("bLoadFaceGenHeadEGTFiles"))
                    {
                        if (!line.EndsWith("1"))
                        {
                            return ERROR_LOAD_EGT_FILES;
                        }
                    }
                }
            }

            return "Correct!";
        }
    }
}