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
                ToolStripSeparator separator = new ToolStripSeparator();
                ToolStripMenuItem item = new ToolStripMenuItem()
                {
                    Text = textBox1.Text.First().ToString().ToUpper() + textBox1.Text.Substring(1),
                    Tag = "menuItem"
                };

                string itemName = textBox2.Text;
                string itemData = textBox3.Text;

                int counter = -1;

                if (!File.Exists("labels.trs") && !File.Exists("data.trs"))
                {
                    AddItemsToContextMenuAndStorage(item, separator);
                    AddItemLabelsToLabelStorage(item);
                    counter++;
                }
                else if(File.Exists("labels.trs") && File.Exists("data.trs")) 
                {
                    var labels = File.ReadAllText("labels.trs");

                    if (!labels.Contains(textBox1.Text.First().ToString().ToUpper() + textBox1.Text.Substring(1)))
                    {
                        AddItemsToContextMenuAndStorage(item, separator);
                        AddItemLabelsToLabelStorage(item);
                        counter++;
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
        }

        private void item_MouseDown(object senders, MouseEventArgs a, ToolStripMenuItem item, ToolStripSeparator separator, string itemName, string itemData)
        {
            if (a.Button == MouseButtons.Left)
            {
                Clipboard.SetText(itemName);
                notifyIcon1.ShowBalloonTip(1000, string.Empty, "Copied name", ToolTipIcon.None);
            }
            else if (a.Button == MouseButtons.Right)
            {
                Clipboard.SetText(itemData);
                notifyIcon1.ShowBalloonTip(1000, string.Empty, "Copied data", ToolTipIcon.None);
            }
            else if (a.Button == MouseButtons.Middle)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to remove this field?", "Remove", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    contextMenuStrip1.Items.Remove(item);
                    contextMenuStrip1.Items.Remove(separator);
                    var data = File.ReadAllLines("data.trs").ToList();
                    var labels = File.ReadAllLines("labels.trs").ToList();

                    var dataKvp = Enumerable.Range(0, data.Count / 2).Select(i => new KeyValuePair<string, string>(data[i * 2], data[i * 2 + 1])).ToList();

                    List<string> dataList = new List<string>();
                    dataList.AddRange(data);
                    
                    //List<string> labelsList = new List<string>();
                    //labelsList.AddRange(labels);

                    if (data.Contains(itemName) && labels.Contains(item.Text))
                    {
                        labels.Remove(item.Text);
                        File.WriteAllLines("labels.trs", labels);

                        dataKvp.Remove(new KeyValuePair<string, string> (itemName, itemData));
                        List<string> printList = new List<string>();

                        foreach (var pair in dataKvp)
                        {
                            printList.Add(pair.Key);
                            printList.Add(pair.Value);
                        }

                        File.WriteAllLines("data.trs", printList);
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
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            CheckIfInputIsEnglish(e);
        }

        private void TextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            CheckIfInputIsEnglish(e);
        }

        #region Helper Methods

        private void LoadDataWhenAppIsLoaded()
        {
            if (File.Exists("data.trs") && File.Exists("labels.trs"))
            {
                var data = File.ReadAllLines("data.trs");
                var labels = File.ReadAllLines("labels.trs");

                try
                {
                    var itemNames = data.Where((c, i) => i % 2 == 0).ToList();
                    var itemData = data.Where((c, i) => i % 2 != 0).ToList();
                    var itemLabels = labels.ToList();

                    int counter = 0;

                    foreach (var item in itemLabels)
                    {
                        ToolStripMenuItem tool = new ToolStripMenuItem();
                        ToolStripSeparator separator = new ToolStripSeparator();
                        tool.Text = itemLabels[counter];
                        contextMenuStrip1.Items.Insert(0, tool);
                        contextMenuStrip1.Items.Insert(1, separator);

                        string innerName = itemNames[counter].First().ToString().ToLower() + itemNames[counter].Substring(1);
                        string innerData = itemData[counter];

                        tool.MouseDown += delegate (object senders, MouseEventArgs a) { item_MouseDown(senders, a, tool, separator, innerName, innerData); };
                        counter++;
                    }

                    //var mergedLists = itemNames.Select((k, idx) => new { k, idx })
                    //                      .GroupBy(x => x.k)
                    //                      .ToDictionary(g => g.Key.First().ToString().ToUpper() + g.Key.Substring(1), g => g.Select(c => itemData[c.idx]).Single());


                    #region semi-working method
                    //int t;
                    //for (t = 0; t < itemLabels.Count; t++)
                    //{
                    //    ToolStripMenuItem tool = new ToolStripMenuItem();
                    //    ToolStripSeparator separator = new ToolStripSeparator();
                    //    tool.Text = itemLabels[t];
                    //    contextMenuStrip1.Items.Insert(0, tool);
                    //    contextMenuStrip1.Items.Insert(1, separator);
                    //
                    //    tool.MouseDown += delegate (object senders, MouseEventArgs a) { item_MouseDown(senders, a, tool, separator, itemNames[t], itemData[t]); };
                    //}
                    //t = 0;
                    #endregion
                }
                catch (Exception)
                {
                    MessageBox.Show("Something is wrong with storage file!");
                    Application.Exit();
                }

            }
        }
        private void AddItemsToContextMenuAndStorage(ToolStripMenuItem item, ToolStripSeparator separator)
        {
            StreamWriter writerData = new StreamWriter("data.trs", true);
            writerData.WriteLine(textBox2.Text);
            writerData.WriteLine(textBox3.Text);
            writerData.Close();
            contextMenuStrip1.Items.Insert(0, item);
            contextMenuStrip1.Items.Insert(1, separator);
        }

        private void AddItemLabelsToLabelStorage(ToolStripMenuItem item)
        {
            StreamWriter writerLabel = new StreamWriter("labels.trs", true);
            writerLabel.WriteLine(item.Text.First().ToString().ToUpper() + textBox1.Text.Substring(1));
            writerLabel.Close();
            this.Hide();
        }

        bool IsEnglishCharacter(char ch)
        {
            if (ch >= 97 && ch <= 122 || ch >= 65 && ch <= 90 || ch >= 48 && ch <= 57)
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

            if (!IsEnglishCharacter(e.KeyChar))
            {
                if (e.KeyChar >= 32 && e.KeyChar <= 47 || e.KeyChar >= 58 && e.KeyChar <= 64 || e.KeyChar >= 91 && e.KeyChar <= 96 || e.KeyChar >= 123 && e.KeyChar <= 126)
                {
                    infoLabel.Text = "Input cannot contain symbols";
                    e.Handled = true;
                }
                else
                {
                    infoLabel.Text = "Input has to be in english";
                    e.Handled = true;
                }
            }
            else
            {
                infoLabel.Text = "";
            }
        }

        #endregion
    }
}
