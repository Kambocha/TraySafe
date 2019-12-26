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
        private ToolStripMenuItem _tsi;
        private string _itmVls;
        public MainForm(ToolStripMenuItem tsi, string itmVls)
        {
            InitializeComponent();
            this._tsi = tsi;
            this._itmVls = itmVls;
            if (File.Exists("data.txt"))
            {
                var Lines = File.ReadAllLines("data.txt");

                var itemTexts = Lines.Where((c, i) => i % 2 == 0).ToList();
                var itemValues = Lines.Where((c, i) => i % 2 != 0).ToList();

                var mergedLists = itemTexts.Select((k, idx) => new { k, idx })
                                           .GroupBy(x => x.k)
                                           .ToDictionary(g => g.Key.First().ToString().ToUpper() + g.Key.Substring(1), g => g.Select(c => itemValues[c.idx]).Single());

                foreach (var pair in mergedLists)
                {
                    ToolStripMenuItem tool = new ToolStripMenuItem();
                    ToolStripSeparator separator = new ToolStripSeparator();
                    tool.Text = pair.Key;
                    contextMenuStrip1.Items.Insert(0, tool);
                    contextMenuStrip1.Items.Insert(1, separator);
                    tool.MouseDown += delegate (object senders, MouseEventArgs a) { item_MouseDown(senders, a, tool, separator, pair.Value); };
                }
            }
        }

        private void AddToolStripMenuItem_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                textBox1.Clear();
                textBox2.Clear();
                infoLabel.Text = "";
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void ExitToolStripMenuItem_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
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
            Application.ExitThread();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox2.Text))
            {
                if (textBox1.Text == _tsi.Text && textBox2.Text == _itmVls)
                {
                    var editLines = File.ReadAllLines("data.txt");
                    if (editLines.Contains(_tsi.Text) && editLines.Contains(_itmVls))
                    {
                        var newLines = editLines.Where(line => !line.Contains(_tsi.Text) && !line.Contains(_itmVls));
                    }
                }
                else
                {
                    ToolStripSeparator separator = new ToolStripSeparator();
                    ToolStripMenuItem item = new ToolStripMenuItem()
                    {
                        Text = textBox1.Text.First().ToString().ToUpper() + textBox1.Text.Substring(1),
                        Tag = "MenuItem"
                    };

                    string itemValues = textBox2.Text;

                    if (!File.Exists("data.txt"))
                    {
                        AddItemsToContextMenu(item, separator);
                    }
                    else
                    {
                        var Lines = File.ReadAllLines("data.txt");
                        if (!Lines.Contains(item.Text) && !Lines.Contains(textBox2.Text))
                        {
                            AddItemsToContextMenu(item, separator);
                        }
                        else if (Lines.Contains(item.Text))
                        {
                            infoLabel.Text = "Field with same name exists";
                        }
                        else if (Lines.Contains(textBox2.Text))
                        {
                            infoLabel.Text = "Field with same data exists";
                        }
                        else
                        {
                            infoLabel.Text = "Unknown Error!";
                        }
                    }

                    item.MouseDown += delegate (object senders, MouseEventArgs a) { item_MouseDown(senders, a, item, separator, itemValues); };
                }
            }
        }
        private void item_MouseDown(object senders, MouseEventArgs a, ToolStripMenuItem item, ToolStripSeparator separator, string itemValues)
        {
            if (a.Button == MouseButtons.Left)
            {
                Clipboard.SetText(itemValues);
            }
            else if (a.Button == MouseButtons.Middle)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to remove this field?", "Remove", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    contextMenuStrip1.Items.Remove(item);
                    contextMenuStrip1.Items.Remove(separator);
                    var Lines = File.ReadAllLines("data.txt");
                    if (Lines.Contains(item.Text))
                    {
                        var newLines = Lines.Where(line => !line.Contains(item.Text) && !line.Contains(itemValues));
                        File.WriteAllLines("data.txt", newLines);
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
        private void AddItemsToContextMenu(ToolStripMenuItem item, ToolStripSeparator separator)
        {
            StreamWriter writer = new StreamWriter("data.txt", true);
            writer.WriteLine(textBox1.Text.First().ToString().ToUpper() + textBox1.Text.Substring(1));
            writer.WriteLine(textBox2.Text);
            writer.Close();
            contextMenuStrip1.Items.Insert(0, item);
            contextMenuStrip1.Items.Insert(1, separator);
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
