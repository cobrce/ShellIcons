using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShellIcons
{
    public partial class Form1 : Form
    {
        RegistryKey dllFile, exeFile;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshKeys();
        }

        private void RefreshKeys()
        {
            listView1.Items.Clear();

            dllFile = Registry.ClassesRoot.OpenSubKey("dllfile\\shell", false);
            exeFile = Registry.ClassesRoot.OpenSubKey("exefile\\shell", false);

            listView1.Items.AddRange(CreateListViewItems("exe", exeFile));
            listView1.Items.AddRange(CreateListViewItems("dll", dllFile));

            foreach (ColumnHeader column in listView1.Columns)
                column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
        }
        private ListViewItem[] CreateListViewItems(string type, RegistryKey key)
        {
            string[] subkeys = key.GetSubKeyNames();

            List<ListViewItem> lvis = new List<ListViewItem>();
            foreach (string subkey in subkeys)
            {
                if (subkey.ToLower() == "open" || subkey.ToLower() == "runas") continue;
                string val = "";
                string icon = "";
                using (RegistryKey item = key.OpenSubKey(subkey))
                {
                    icon = (string)item.GetValue("Icon", "");
                    using (RegistryKey command = item.OpenSubKey("Command"))
                    {
                        val = (string)command.GetValue("");
                    }
                }
                if (val == null || val == "") continue;

                bool check = false;
                if (icon == null || icon == "")
                {
                    check = true;
                    icon = "\"" + val.Split(val.StartsWith("\"") ? '"' : '%')[val.StartsWith("\"") ? 1 : 0] + "\",0";
                }


                ListViewItem lvi = new ListViewItem(new string[] { type, subkey, val, icon });
                if (check)
                {
                    lvi.Checked = true;
                    lvi.BackColor = Color.SkyBlue;
                }
                lvis.Add(lvi);
            }
            return lvis.ToArray();
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshKeys();
        }

        private void btnSetIcons_Click(object sender, EventArgs e)
        {
            List<string> errors = new List<string>();
            foreach(ListViewItem lvi in listView1.Items)
            {
                if (!lvi.Checked) continue;
                try
                {                    
                    RegistryKey key = (lvi.SubItems[0].Text == "dll") ? dllFile : exeFile;
                    RegistryKey item = key.OpenSubKey(lvi.SubItems[1].Text, true);
                    item.SetValue("Icon", lvi.SubItems[3].Text);
                    item.Close();
                }
                catch { errors.Add(lvi.Index.ToString()); }
            }
            string message = (errors.Count == 0) ? "Done!" : "Errors found in the following index" + Environment.NewLine + string.Join(",", errors);
            MessageBox.Show(message);
        }
    }
}
