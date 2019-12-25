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
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            MainForm._contextMenuStrip.Items.Remove(MainForm._stripMenuItem);
            MainForm._contextMenuStrip.Items.Remove(MainForm._separator);
            var Lines = File.ReadAllLines("text.txt");
            if (Lines.Contains(MainForm._stripMenuItem.Text))
            {
                var newLines = Lines.Where(line => !line.Contains(MainForm._stripMenuItem.Text) && !line.Contains(MainForm._iValues));
                File.WriteAllLines("text.txt", newLines);
                this.Hide();
            }
            else
            {
                MessageBox.Show("Not Found!");
            }
        }
    }
}
