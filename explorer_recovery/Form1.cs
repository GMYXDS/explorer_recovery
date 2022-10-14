using Microsoft.VisualBasic;
using System.Diagnostics;
using static System.Environment;
using Microsoft.Win32;

namespace explorer_recovery {
    public partial class Form1 : Form {
        private List<String> temp_list = null;
        private string configPath = System.Environment.GetFolderPath(SpecialFolder.UserProfile) + "\\explorer_recovery_config.ini";
        string? record_Elapsed = "";
        string? is_auto_record = "0";
        public Form1() {
            InitializeComponent();
            this.ListBox_ac_explorer.HorizontalScrollbar = true;
            restart_store();
            iniData();
            auto_start();
            ThreadPool.RegisterWaitForSingleObject(Program.ProgramStarted, OnProgramStarted, null, -1, false);
            this.Text = "explorer恢复器v1.1";
            //初始化配置
            init_auto_record();
        }

        private void init_auto_record() {
            is_auto_record = Dbinihelper.GetIniData("setting", "is_auto_record", configPath);
            record_Elapsed = Dbinihelper.GetIniData("setting", "record_Elapsed", configPath);
            if (is_auto_record == null || record_Elapsed == null) {
                Dbinihelper.SetIniData("setting", "is_auto_record", "0", configPath);
                Dbinihelper.SetIniData("setting", "record_Elapsed", "10", configPath);
                checkBox2.Checked = false;
                textBox1.Text = "10";
                return;
            }
            textBox1.Text = record_Elapsed;
            if (record_Elapsed == "" || is_auto_record == "0") return;
            try {
                timer1.Interval = Convert.ToInt32(record_Elapsed) * 1000;
            } catch (Exception) { return; }
            checkBox2.Checked = true;
            timer1.Start();
        }

        private void restart_store() {
            var last_save = Dbinihelper.GetIniSection("active", configPath);
            if (last_save==null) return;
            if(last_save.Count!=0)Dbinihelper.SetIniSection("上次保存", last_save, configPath);
        }
        void OnProgramStarted(object state, bool timeout) {
            this.Show();
        }
        private void auto_start() {
            string path = Application.ExecutablePath;
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rk2 = rk.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            string is_autostart =  rk2.GetValue("explorer_recovery", "").ToString();
            rk2.Close();
            rk.Close();
            if(is_autostart!="")checkBox1.Checked = true;
        }
        private void iniData() {
            update_active_box();
            //读取所有的section 设置tab
            List<String>? list = Dbinihelper.GetIniAllSectionsNames(configPath);
            if (list == null) return;
            foreach (var item in list) {
                //Console.WriteLine(item);
                Dictionary<string, string>? dict = Dbinihelper.GetIniSection(item, configPath);
                if (dict == null) return;
                if (item == "setting") continue;
                List<string> list_temp = dict.Values.ToList();
                //if (item == "active") add_tab("上次保存", list_temp);
               
                add_tab(item, list_temp);
            }
        }

        private List<string> get_active_explorers() {
            var now_list = new List<string>();
            foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindows()) {
                if (Path.GetFileNameWithoutExtension(window.FullName).ToLowerInvariant() == "explorer") {
                    if (Uri.IsWellFormedUriString(window.LocationURL, UriKind.Absolute)) {
                        string path_str = new Uri(window.LocationURL).LocalPath.ToString();
                        //Console.WriteLine(path_str);
                        now_list.Add(path_str);
                    }
                }
            }
            return now_list;
        }

        private void update_active_box() {
            this.ListBox_ac_explorer.Items.Clear();
            temp_list = get_active_explorers();
            foreach (var path in temp_list) {
                this.ListBox_ac_explorer.Items.Add(path);
            }
            this.groupBox1.Text = string.Format("自动保存：{0:yyyy-MM-dd HH:mm:ss}", System.DateTime.Now);
            if(temp_list.Count!=0) store_to_file("active", temp_list);
        }

        private void button1_Click(object sender, EventArgs e) {
            //保存当前
            update_active_box();
            //询问保存名称
            String tab_name = Interaction.InputBox("输入保存的项目名称！", "提示");
            if (tab_name == String.Empty) {
                MessageBox.Show("项目名不能为空!");
                return;
            }
            add_tab(tab_name, temp_list);
            store_to_file(tab_name, temp_list);
        }

        private void store_to_file(string tab_name, List<string> list) {
            Dictionary<string, string> temp_dict = new Dictionary<string, string>();
            int index = 0;
            foreach(var item in list) {
                temp_dict["path"+index.ToString()]=item.ToString();
                index++;
            }
            Dbinihelper.SetIniSection(tab_name, temp_dict, configPath);
        }

        private void ListBox_ac_explorer_DoubleClick(object sender, EventArgs e) {
            foreach (var path in temp_list) {
                Process.Start("explorer.exe", path);
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e) {
            //if (tabControl1.TabPages.Count == 1) {
            //    MessageBox.Show("只剩一个不能删除！","提示");
            //    return;
            //}
            if (MessageBox.Show("确定要删除吗？", "提示！", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                string del_tab_name = tabControl1.SelectedTab.Text.Trim();
                Dbinihelper.DelIniSection(del_tab_name,configPath);
                tabControl1.TabPages.Remove(tabControl1.SelectedTab);
            }
        }

        private void 添加ToolStripMenuItem_Click(object sender, EventArgs e) {
            int number = tabControl1.TabPages.Count;
            string tab_name = "New" + (number + 1);
            List<string> list = get_active_explorers();
            add_tab(tab_name,list);
            store_to_file(tab_name, list);
        }

        private void add_tab(string tab_name,List<string> list) {
            //int number = tabControl1.TabPages.Count;
            TabPage page = new TabPage();
            page.Text = tab_name;
            CheckedListBox clb = new CheckedListBox();
            clb.Width = 316;
            clb.Height = 130;
            foreach (var path in list) {
                clb.Items.Add(path);
            }
            clb.HorizontalScrollbar = true;
            page.Controls.Add(clb);
            tabControl1.TabPages.Add(page);
            this.tabControl1.SelectedTab = page;//显示当前页
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (Control C in tabControl1.SelectedTab.Controls) {
                //Console.WriteLine(C.GetType());
                if (C.GetType().Name == "CheckedListBox") {
                    CheckedListBox temp = (CheckedListBox)C;
                    //Console.WriteLine(temp.Items.Count);
                    if(temp.CheckedItems.Count > 0) {
                        foreach (var path in temp.CheckedItems) {
                            //Console.WriteLine(path);
                            _ = Process.Start("explorer.exe", (string)path);
                        }
                    } else {
                        foreach (var path in temp.Items) {
                            //Console.WriteLine(path);
                            _ = Process.Start("explorer.exe", (string)path);
                        }
                    }

                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            this.Hide();
            e.Cancel = true;
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Show();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e) {
            //this.Close();
            System.Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e) {
            this.ShowInTaskbar = false;
            this.BeginInvoke(new Action(() => {
                this.Hide();
                this.Opacity = 1;
                this.ShowInTaskbar = true;
            }));
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            this.Show();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            string path = Application.ExecutablePath;
            RegistryKey rk = Registry.CurrentUser;
            RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if (checkBox1.Checked) {
                rk2.SetValue("explorer_recovery", path);
            } else {
                rk2.DeleteValue("explorer_recovery", false);
            }
            rk2.Close();
            rk.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            if (textBox1.Text.ToString() == "") return;
            Dbinihelper.SetIniData("setting", "record_Elapsed", textBox1.Text.ToString(), configPath);
            try {
                timer1.Interval = Convert.ToInt32(textBox1.Text.ToString()) * 1000;
            } catch (Exception) { return; }
            if (is_auto_record == "1") {
                timer1.Stop();
                timer1.Start();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e) {
            if(checkBox2.Checked) {
                string get_interval = textBox1.Text;
                if (get_interval == "") return;
                is_auto_record = "1";
                Dbinihelper.SetIniData("setting", "is_auto_record","1", configPath);
                Dbinihelper.SetIniData("setting", "record_Elapsed", get_interval, configPath);
                try {
                    timer1.Interval = Convert.ToInt32(get_interval) * 1000;
                } catch (Exception) { return; }
                timer1.Start();
            } else {
                is_auto_record = "0";
                Dbinihelper.SetIniData("setting", "is_auto_record", "0", configPath);
                timer1.Stop();
            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
            update_active_box();
        }
    }
}