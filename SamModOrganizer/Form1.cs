using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SamModOrganizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckConfigFile();



        }
        private string configFilePath = "config.txt";
        string modLoadPath;
        string modFilePath;
        //private void button1_Click(object sender, EventArgs e)
        //{
        //    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        //    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        string selectedPath = folderBrowserDialog.SelectedPath;
        //        string result = SamModOrganizer.GetPath.SteamPath(selectedPath);

        //        string steamPath = Path.GetFullPath(Path.Combine(result, @"..\..\..\"));
        //        modLoadPath = Path.Combine(steamPath, @"common\Serious Sam 4\Temp\SteamWorkshop");

        //        if (result != null)
        //        {
        //            label1.Text = modLoadPath;
        //            LoadData(result);
        //        }
        //        else
        //        {
        //            MessageBox.Show("未找到目标路径。");
        //        }
        //    }
        //}


        private void CheckConfigFile()
        {
            if (File.Exists(configFilePath))
            {
                var lines = File.ReadAllLines(configFilePath);
                modFilePath = lines[0];
                modLoadPath = lines[1];
                label1.Text = modLoadPath;
                LoadData(modFilePath);

                // 加载预设名称到ListBox
                for (int i = 2; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("[") && lines[i].Contains("]"))
                    {
                        presetListBox.Items.Add(lines[i].Substring(1, lines[i].IndexOf("]") - 1));
                    }
                }

            }
            else
            {
                CreateConfigFile();
            }
        }

        private void CreateConfigFile()
        {
            MessageBox.Show("Choose your Sam game disk.", "", MessageBoxButtons.OK);
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedPath = folderBrowserDialog.SelectedPath;
                modFilePath = SamModOrganizer.GetPath.SteamPath(selectedPath);
                string steamPath = Path.GetFullPath(Path.Combine(modFilePath, @"..\..\..\"));
                modLoadPath = Path.Combine(steamPath, @"common\Serious Sam 4\Temp\SteamWorkshop");

                if (modFilePath != null)
                {
                    label1.Text = modLoadPath;
                    LoadData(modFilePath);
                    File.WriteAllLines(configFilePath, new string[] { modFilePath, modLoadPath });
                }
                else
                {
                    MessageBox.Show("Error finding game files");
                }
            }

        }



        private void LoadData(string rootPath)
        {

            dataGridView.AllowUserToAddRows = false;

            dataGridView.Columns.Add("FolderPath", "FolderPath");
            dataGridView.Columns.Add("TxtFileName", "TxtFileName");
            dataGridView.Columns.Add("JpgFileName", "JpgFileName");

            dataGridView.Columns["FolderPath"].Visible = false;
            dataGridView.Columns["TxtFileName"].Visible = false;
            dataGridView.Columns["JpgFileName"].Visible = false;

            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "";
            checkBoxColumn.Name = "CheckBox";
            dataGridView.Columns.Add(checkBoxColumn);
            dataGridView.Columns["CheckBox"].Width = 80;

            dataGridView.Columns.Add("ModName", "ModName");
            dataGridView.Columns["ModName"].ReadOnly = true;
            dataGridView.Columns["ModName"].Width = 500;


            var disabledFiles = Directory.GetFiles(modLoadPath, "*.disabled");

            foreach (var directory in Directory.GetDirectories(rootPath))
            {
                var txtFile = Directory.GetFiles(directory, "*.txt")[0];
                var jpgFile = Directory.GetFiles(directory, "*.jpg")[0];
                var modName = GetModNameFromTxt(txtFile);
                var txtFileNameWithoutExtension = Path.GetFileNameWithoutExtension(txtFile);
                var isChecked = true;

                // 与disabled文件对比
                foreach (var disabledFile in disabledFiles)
                {
                    var disabledFileName = Path.GetFileNameWithoutExtension(disabledFile);
                    if (txtFileNameWithoutExtension == ("zz" + disabledFileName))
                    {
                        isChecked = false;
                        break;
                    }
                }

                dataGridView.Rows.Add(directory, Path.GetFileName(txtFile), Path.GetFileName(jpgFile), isChecked, modName);



            }
        }

        private string GetModNameFromTxt(string txtFilePath)
        {
            try
            {
                var content = File.ReadAllText(txtFilePath);
                var parts = content.Split('#');
                if (parts.Length > 1)
                {
                    return parts[1]; // 提取mod名称
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading mod info: " + ex.Message);
            }
            return "未知";
        }



        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView.SelectedRows[0];
                var folderPath = selectedRow.Cells["FolderPath"].Value.ToString();
                var jpgFileName = selectedRow.Cells["JpgFileName"].Value.ToString();
                var jpgFilePath = Path.Combine(folderPath, jpgFileName);

                if (File.Exists(jpgFilePath))
                {
                    pictureBox.Image = Image.FromFile(jpgFilePath);
                }
                else
                {
                    MessageBox.Show("Preview picture doesn't exist");
                }
            }
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedRow = dataGridView.Rows[e.RowIndex];
                var folderPath = selectedRow.Cells["FolderPath"].Value.ToString();
                var jpgFileName = selectedRow.Cells["JpgFileName"].Value.ToString();
                var jpgFilePath = Path.Combine(folderPath, jpgFileName);

                if (File.Exists(jpgFilePath))
                {
                    pictureBox.Image = Image.FromFile(jpgFilePath);
                }
                else
                {
                    MessageBox.Show("Preview picture doesn't exist");
                }
            }
        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dataGridView.Columns["CheckBox"].Index)
            {
                var selectedRow = dataGridView.Rows[e.RowIndex];
                var txtFileName = selectedRow.Cells["TxtFileName"].Value.ToString();
                var disabledFileName = Path.GetFileNameWithoutExtension(txtFileName).Replace("zz", string.Empty) + ".disabled";

                var disabledFilePath = Path.Combine(modLoadPath, disabledFileName);
                label1.Text = disabledFileName;
                if ((bool)selectedRow.Cells["CheckBox"].Value == false) // 取消勾选时创建文件
                {
                    File.Create(disabledFilePath).Close();

                }
                else // 勾选时删除文件
                {
                    if (File.Exists(disabledFilePath))
                    {
                        File.Delete(disabledFilePath);
                    }
                }

            }
        }

        private void dataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView.IsCurrentCellDirty)
            {
                dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string presetName = Prompt.ShowDialog("Input a name", "Save");
            if (!string.IsNullOrEmpty(presetName))
            {
                using (StreamWriter sw = File.AppendText(configFilePath))
                {
                    sw.Write($"[{presetName}]");
                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        var txtFileName = row.Cells["TxtFileName"].Value.ToString();
                        var checkBoxValue = (bool)row.Cells["CheckBox"].Value;
                        sw.Write($"{txtFileName}:{checkBoxValue}#");
                    }
                    sw.WriteLine();
                }
                presetListBox.Items.Add(presetName); // 将预设名称加入ListBox
                presetListBox.SelectedItem = presetName;
                MessageBox.Show("Saved");

            }
        }

        private void presetListBox_SelectedIndexChanged(object sender, EventArgs e)
        {


            if (presetListBox.SelectedItem != null)
            {
                // 当前有选中项



                string selectedPreset = presetListBox.SelectedItem.ToString();

                var lines = File.ReadAllLines(configFilePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith($"[{selectedPreset}]"))
                    {
                        var data = line.Substring(line.IndexOf("]") + 1).Split('#');
                        foreach (var row in dataGridView.Rows)
                        {
                            var dataGridViewRow = row as DataGridViewRow;
                            var txtFileName = dataGridViewRow.Cells["TxtFileName"].Value.ToString();
                            foreach (var item in data)
                            {
                                var parts = item.Split(':');
                                if (parts[0] == txtFileName)
                                {
                                    dataGridViewRow.Cells["CheckBox"].Value = bool.Parse(parts[1]);
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            }
            else
            {
                // 当前没有选中项

            }
        }

        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                Form prompt = new Form()
                {
                    Width = 500,
                    Height = 200,
                    Text = caption
                };
                Label textLabel = new Label() { Left = 50, Top = 20, Text = text, AutoSize = true };
                System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox() { Left = 50, Top = 50, Width = 400 };
                System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "Yes", Left = 350, Width = 100, Top = 100, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                row.Cells["CheckBox"].Value = true;

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                row.Cells["CheckBox"].Value = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (presetListBox.SelectedItem != null)
            {
                var result = MessageBox.Show("Are you sure to delete this preset？", "Yes", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    string selectedPreset = presetListBox.SelectedItem.ToString();
                    var lines = new List<string>(File.ReadAllLines(configFilePath));
                    lines.RemoveAll(line => line.StartsWith($"[{selectedPreset}]"));
                    File.WriteAllLines(configFilePath, lines.ToArray());
                    presetListBox.Items.Remove(selectedPreset);
                    // MessageBox.Show("Deleted");
                }
            }
            else
            {
                MessageBox.Show("Please select a preset");

            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.Trim().ToLower();

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells["ModName"].Value != null &&
                    row.Cells["ModName"].Value.ToString().ToLower().Contains(searchText))
                {
                    row.Visible = true;
                }
                else
                {
                    row.Visible = false;
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                // 如果 TextBox 为空，显示所有行
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    row.Visible = true;
                }
            }
        }

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // 确保双击的不是表头
            if (e.RowIndex >= 0)
            {
                // 获取双击行的FolderPath列的值
                string folderPath = dataGridView.Rows[e.RowIndex].Cells["FolderPath"].Value.ToString();

                // 打开指定路径
                try
                {
                    System.Diagnostics.Process.Start(folderPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fail to open mod file: {ex.Message}");
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // 判断ListBox是否有选中项
            if (presetListBox.SelectedIndex != -1)
            {
                string selectedPreset = presetListBox.SelectedItem.ToString();

                // 读取configFilePath中的所有行
                string[] lines = File.ReadAllLines(configFilePath);
                foreach (string line in lines)
                {
                    // 找到与选中项匹配的行
                    if (line.StartsWith($"[{selectedPreset}]"))
                    {
                        // 创建新的txt文件，名称为选中项的名称
                        string newFilePath = Path.Combine(Path.GetDirectoryName(configFilePath), $"{selectedPreset}.txt");
                        File.WriteAllText(newFilePath, line);

                        MessageBox.Show($"Preset saved to {newFilePath}");
                        return;
                    }
                }
                MessageBox.Show("No available Preset data");
            }
            else
            {
                MessageBox.Show("Please select a preset");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // 设置默认路径为程序根目录
            string initialDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // 创建并配置OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = initialDirectory;
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            openFileDialog.Title = "Choose a TXT file";

            // 显示对话框并判断用户是否选择了文件
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;

                // 读取选择的TXT文件中的行内容和数据名称
                string line = null;
                string presetName = null;
                using (StreamReader sr = new StreamReader(selectedFilePath))
                {
                    line = sr.ReadLine();
                    if (line != null)
                    {
                        // 使用正则表达式提取中括号内的文本
                        var match = System.Text.RegularExpressions.Regex.Match(line, @"\[(.*?)\]");
                        if (match.Success)
                        {
                            presetName = match.Groups[1].Value;
                        }
                    }
                }

                // 确保读取到的数据名称不是空值
                if (!string.IsNullOrEmpty(presetName))
                {
                    // 将整行内容添加到configFilePath的最后一行
                    using (StreamWriter sw = File.AppendText(configFilePath))
                    {
                        sw.WriteLine(line);
                    }

                    // 将数据名称添加到ListBox中
                    presetListBox.Items.Add(presetName);

                    // MessageBox.Show($"数据已成功导入，并添加到列表：{presetName}");
                }
                else
                {
                    MessageBox.Show("No available Preset data found");
                }
            }
        }
    }
}