using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Call_of_Duty_World_at_War_Launcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            parameters();
            get_mods();

            var ConfigFile = Path.GetFullPath("main\\launcher.ini");
            var selected_mod = IniFile.ReadValue(ConfigFile, "launcher", "mod");
            comboBoxMods.SelectedItem = selected_mod;

            // Get the registry entry and format it accordingly
            RegistryKey myKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Activision\Call of Duty WAW");
            if (myKey != null)
            {
                string formatted_key = myKey.GetValue("codkey").ToString().ToUpper();
                if (formatted_key.Length == 20)
                {
                    formatted_key = Regex.Replace(formatted_key, ".{4}", "$0-");
                }
                comboBoxCDKey.Text = formatted_key.Remove(formatted_key.Length - 1, 1);
            }

            // Get the list of cdkeys from the cdkeys.txt file
            if (File.Exists("cdkeys.txt"))
            {
                string[] cdkeys = File.ReadAllLines("cdkeys.txt");
                comboBoxCDKey.Items.Clear();
                int i = 1;
                foreach (string cdkey in cdkeys)
                {

                    if (cdkey.Length != 24 && cdkey.Length != 20) continue;
                    comboBoxCDKey.Items.Add(i + " - " + cdkey);
                    i++;
                }

            }

        }

        // Click anywhere to move hack
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        private void btnSingleplayer_Click(object sender, EventArgs e)
        {
            Process.Start("CoDWaW.exe");
            Environment.Exit(0);
        }

        private void btnMultiplayer_Click(object sender, EventArgs e)
        {
            Process.Start("CoDWaWmp.exe");
            Environment.Exit(0);
        }

        private void btnOpenModFolder_Click(object sender, EventArgs e)
        {
            var LocalAppData = Environment.GetEnvironmentVariable("LocalAppData");

            Process.Start("explorer.exe", @LocalAppData+"\\Activision\\CoDWaW");
        }

        private void btnBots_Click(object sender, EventArgs e)
        {
            var selected_mod = comboBoxMods.SelectedItem.ToString();
            var check_mp = selected_mod.Substring(0, 3);

            var ConfigFile = Path.GetFullPath("main\\launcher.ini");

            IniFile.WriteValue(ConfigFile, "launcher", "mod", selected_mod);

            var p = new Process();
            if(check_mp == "mp_")
                p.StartInfo.FileName = Path.GetFullPath("CoDWaWmp.exe");
            if (check_mp != "mp_")
                p.StartInfo.FileName = Path.GetFullPath("CoDWaW.exe");
            p.StartInfo.Arguments = "+set fs_game \"mods/"+ selected_mod;

            p.Start();
            Environment.Exit(0);
        }

        
        private void parameters()
        {
            string[] args = Environment.GetCommandLineArgs();

            string command  = "";

            foreach (string arg in args)
            {
                if (command == "--cdkey")
                {
                    string cdkey = Regex.Replace(arg, "[-]", string.Empty).Trim().ToLower();
                    update_registry(cdkey);
                }

                command = arg;
            }
        }

        private void update_registry(string cdkey)
        {
            
            string formatted_cdkey = Regex.Replace(cdkey, "[-]", string.Empty).Trim().ToUpper();
            comboBoxCDKey.Text = formatted_cdkey;
            RegistryKey myKey1 = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Activision\Call of Duty WAW");

            if (myKey1 != null)
            {
                myKey1.SetValue("codkey", formatted_cdkey, RegistryValueKind.String);
                myKey1.SetValue("InstallPath", Directory.GetCurrentDirectory(), RegistryValueKind.String);
                myKey1.SetValue("InstallDrive", Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly().Location), RegistryValueKind.String);
                myKey1.SetValue("Language", "enu", RegistryValueKind.String);
                myKey1.SetValue("Version", "1.7", RegistryValueKind.String);
                myKey1.SetValue("EXEStringS", Path.GetFullPath("CoDWaW.exe"), RegistryValueKind.String);
                myKey1.SetValue("EXEStringM", Path.GetFullPath("CoDWaWmp.exe"), RegistryValueKind.String);
                myKey1.SetValue("QA", "22", RegistryValueKind.String);
                myKey1.Close();
            }
            Environment.Exit(0);
        }

        private void buttonUpdateCDKey_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Are you sure you want to update the key? This will reset your profile",
                 "Update CD Key?",
                 MessageBoxButtons.YesNo);

            if (confirmResult != DialogResult.Yes) return;

            Process proc = new Process();
            proc.StartInfo.FileName = System.AppDomain.CurrentDomain.FriendlyName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.StartInfo.Arguments = "--cdkey " + comboBoxCDKey.Text;
            proc.Start();
        }

        private void comboBoxCDKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected_item = comboBoxCDKey.SelectedItem.ToString();
            string formatted_text = selected_item.Substring(selected_item.IndexOf(" - ") + 3);

            BeginInvoke(new Action(() => comboBoxCDKey.Text = formatted_text));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void comboBoxMods_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void get_mods()
        {
            var LocalAppData = Environment.GetEnvironmentVariable("LocalAppData");
            var mods_directory = LocalAppData + "\\Activision\\CoDWaW\\mods";
            var dest = new DirectoryInfo(Path.GetFullPath(LocalAppData + "\\Activision\\CoDWaW\\mods"));

            if (Directory.Exists(mods_directory) && !IsDirectoryEmpty(mods_directory))
            {
                foreach (string modfolder in Directory.GetDirectories(mods_directory))
                {
                    comboBoxMods.Items.Add(Path.GetFileName(modfolder));
                }
            }
        }

        public bool IsDirectoryEmpty(string path)
        {
            IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
            using (IEnumerator<string> en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }

    }
}
