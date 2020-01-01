using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraySafe
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            LoadDataWhenAppIsLoaded();
        }

        private void AddToolStripMenuItem_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                infoLabel.Text = "";
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void ExitToolStripMenuItem_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                notifyIcon1.Visible = false;
                Application.Exit();
            }
        }

        private void MainForm_Move(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.ExitThread();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox2.Text) && !string.IsNullOrWhiteSpace(textBox3.Text))
            {
                var regexTextBox1 = new Regex("^[a-zA-Z0-9]*$");
                var regexTextBox2 = new Regex("^[a-zA-Z0-9@_.-]*$");
                if (regexTextBox1.IsMatch(textBox1.Text) && regexTextBox2.IsMatch(textBox2.Text))
                {
                    ToolStripSeparator separator = new ToolStripSeparator();
                    ToolStripMenuItem item = new ToolStripMenuItem()
                    {
                        Text = textBox1.Text.First().ToString().ToUpper() + textBox1.Text.Substring(1),
                        Tag = "menuItem"
                    };

                    string itemName = textBox2.Text;
                    string itemData = textBox3.Text;

                    if (!File.Exists("labels.tsf") && !File.Exists("data.tsf"))
                    {
                        AddItemsToContextMenuAndStorage(item, separator);
                        AddItemLabelsToLabelStorage(item);
                    }
                    else if (File.Exists("labels.tsf") && File.Exists("data.tsf"))
                    {
                        var labels = File.ReadAllText("labels.tsf");

                        if (!labels.Contains(textBox1.Text.First().ToString().ToUpper() + textBox1.Text.Substring(1)))
                        {
                            AddItemsToContextMenuAndStorage(item, separator);
                            AddItemLabelsToLabelStorage(item);
                        }
                        else
                        {
                            infoLabel.Text = "Field with same label already exists";
                        }
                    }
                    else
                    {
                        MessageBox.Show("One of two storage files is missing!");
                    }

                    item.MouseDown += delegate (object senders, MouseEventArgs a) { item_MouseDown(senders, a, item, separator, itemName, itemData); };
                }
                else if (!regexTextBox1.IsMatch(textBox1.Text))
                {
                    infoLabel.Text = "Label: Only english and no symbols";
                }
                else if (!regexTextBox2.IsMatch(textBox2.Text))
                {
                    infoLabel.Text = "Name: Only english and email symbols";
                }
            }
        }

        private void item_MouseDown(object senders, MouseEventArgs a, ToolStripMenuItem item, ToolStripSeparator separator, string itemName, string itemData)
        {
            if (a.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
            {
                RemoveItemsFromContextMenuAndStorage(item, separator, itemName, itemData);
            }
            else if (a.Button == MouseButtons.Middle)
            {
                RemoveItemsFromContextMenuAndStorage(item, separator, itemName, itemData);
            }
            else if (a.Button == MouseButtons.Left)
            {
                Clipboard.SetText(itemName);
                notifyIcon1.ShowBalloonTip(1000, string.Empty, "Copied name", ToolTipIcon.None);
            }
            else if (a.Button == MouseButtons.Right)
            {
                Clipboard.SetText(itemData);
                notifyIcon1.ShowBalloonTip(1000, string.Empty, "Copied data", ToolTipIcon.None);
            }
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            CheckIfInputIsEnglish(e);
        }

        private void TextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            CheckIfInputIsEmail(e);
        }

        private void TextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            CheckIfInputIsPassword(e);
        }

        #region Helper Methods

        private void LoadDataWhenAppIsLoaded()
        {
            if (File.Exists("data.tsf") && File.Exists("labels.tsf"))
            {
                var data = File.ReadAllLines("data.tsf");
                var labels = File.ReadAllLines("labels.tsf");

                try
                {
                    var itemNamesEn = data.Where((c, i) => i % 2 == 0).ToList();
                    var itemDataEn = data.Where((c, i) => i % 2 != 0).ToList();

                    List<string> itemNamesDe = new List<string>();
                    List<string> itemDataDe = new List<string>();

                    foreach (var enName in itemNamesEn)
                    {
                        itemNamesDe.Add(Encryption.Decrypt(enName));
                    }

                    foreach (var enData in itemDataEn)
                    {
                        itemDataDe.Add(Encryption.Decrypt(enData));
                    }

                    var itemLabels = labels.ToList();

                    int counter = 0;

                    foreach (var item in itemLabels)
                    {
                        ToolStripMenuItem tool = new ToolStripMenuItem();
                        ToolStripSeparator separator = new ToolStripSeparator();
                        tool.Text = itemLabels[counter];
                        contextMenuStrip1.Items.Insert(0, tool);
                        contextMenuStrip1.Items.Insert(1, separator);

                        string innerName = itemNamesDe[counter].First().ToString().ToLower() + itemNamesDe[counter].Substring(1);
                        string innerData = itemDataDe[counter];

                        //tool.MouseHover += delegate (object senders, EventArgs a) { item_MouseHover(senders, a, tool, separator, innerName, innerData); };
                        tool.MouseDown += delegate (object senders, MouseEventArgs a) { item_MouseDown(senders, a, tool, separator, innerName, innerData); };
                        counter++;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Something is wrong with storage file!");
                    File.Delete("data.tsf");
                    File.Delete("labels.tsf");
                    Application.Exit();
                }

            }
        }
        private void AddItemsToContextMenuAndStorage(ToolStripMenuItem item, ToolStripSeparator separator)
        {
            StreamWriter writerData = new StreamWriter("data.tsf", true);
            string encryptedName = Encryption.Encrypt(textBox2.Text);
            string encryptedData = Encryption.Encrypt(textBox3.Text);
            writerData.WriteLine(encryptedName);
            writerData.WriteLine(encryptedData);
            writerData.Close();
            contextMenuStrip1.Items.Insert(0, item);
            contextMenuStrip1.Items.Insert(1, separator);
        }
        private void RemoveItemsFromContextMenuAndStorage(ToolStripMenuItem item, ToolStripSeparator separator, string itemName, string itemData)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to remove this field?", "Remove", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                contextMenuStrip1.Items.Remove(item);
                contextMenuStrip1.Items.Remove(separator);
                var dataEn = File.ReadAllLines("data.tsf").ToList();
                List<string> dataDe = new List<string>();

                foreach (var dataItem in dataEn)
                {
                    dataDe.Add(Encryption.Decrypt(dataItem));
                }

                var labels = File.ReadAllLines("labels.tsf").ToList();

                List<KeyValuePair<string, string>> dataKvp = Enumerable.Range(0, dataDe.Count / 2).Select(i => new KeyValuePair<string, string>(dataDe[i * 2], dataDe[i * 2 + 1])).ToList();

                if (dataDe.Contains(itemName) && labels.Contains(item.Text))
                {
                    int labelsIndex = labels.FindIndex(l => l.Contains(item.Text));

                    labels.Remove(item.Text);
                    File.WriteAllLines("labels.tsf", labels);

                    dataKvp.RemoveAt(labelsIndex);
                    List<string> printList = new List<string>();

                    foreach (var pair in dataKvp)
                    {
                        printList.Add(Encryption.Encrypt(pair.Key));
                        printList.Add(Encryption.Encrypt(pair.Value));
                    }

                    File.WriteAllLines("data.tsf", printList);
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Not Found!");
                }
                contextMenuStrip1.Hide();
            }
            else if (dialogResult == DialogResult.No)
            {
                contextMenuStrip1.Hide();
                return;
            }
        }
        private void AddItemLabelsToLabelStorage(ToolStripMenuItem item)
        {
            StreamWriter writerLabel = new StreamWriter("labels.tsf", true);
            writerLabel.WriteLine(item.Text.First().ToString().ToUpper() + textBox1.Text.Substring(1));
            writerLabel.Close();
            this.Hide();
        }

        #region English Check

        bool IsEnglishCharacter(char ch)
        {
            //a-z and A-Z and 0-9
            if (ch == 24 || ch == 3 || ch == 22 || ch >= 97 && ch <= 122 || ch >= 65 && ch <= 90 || ch >= 48 && ch <= 57)
            {
                return true;
            }

            return false;
        }

        private void CheckIfInputIsEnglish(KeyPressEventArgs e)
        {
            Keys key = (Keys)e.KeyChar;

            if (key == Keys.Back)
            {
                return;
            }

            else if (!IsEnglishCharacter(e.KeyChar))
            {
                infoLabel.Text = "Label: Only english and no symbols";
                e.Handled = true;
            }
            else
            {
                infoLabel.Text = "";
            }
        }
        #endregion

        #region Email Check
        bool IsEmailCharacter(char ch)
        {
            if (ch == 24 || ch == 3 || ch == 22 || ch >= 45 && ch <= 46 || ch >= 48 && ch <= 57 || ch == 64 || ch >= 65 && ch <= 90 || ch == 95 || ch >= 97 && ch <= 122)
            {
                return true;
            }

            return false;
        }

        private void CheckIfInputIsEmail(KeyPressEventArgs e)
        {
            Keys key = (Keys)e.KeyChar;

            if (key == Keys.Back)
            {
                return;
            }

            else if (!IsEmailCharacter(e.KeyChar))
            {
                infoLabel.Text = "Name: Only english and email symbols";
                e.Handled = true;
            }
            else
            {
                infoLabel.Text = "";
            }
        }
        #endregion

        #region Password Check
        bool IsPasswordCharacter(char ch)
        {
            if (ch == 24 || ch == 3 || ch == 22 || ch >= 33 && ch <= 126)
            {
                return true;
            }

            return false;
        }

        private void CheckIfInputIsPassword(KeyPressEventArgs e)
        {
            Keys key = (Keys)e.KeyChar;

            if (key == Keys.Back)
            {
                return;
            }

            else if (!IsPasswordCharacter(e.KeyChar))
            {
                infoLabel.Text = "Data: Only english";
                e.Handled = true;
            }
            else
            {
                infoLabel.Text = "";
            }
        }
        #endregion


        #endregion
    }
}
