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
    public partial class CreatePinForm : Form
    {
        public CreatePinForm()
        {
            InitializeComponent();
        }

        private void SetButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists("pin.tsf"))
                {
                    StreamWriter pinWriter = new StreamWriter("pin.tsf");
                    string enPin = Encryption.Encrypt(textBox1.Text);
                    pinWriter.WriteLine(enPin);
                    pinWriter.Close();
                    this.Hide();
                    MainForm form = new MainForm();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There is a problem with the pin storage file!");
                this.Hide();
                File.Delete("pin.tsf");
            }
        }

        bool IsNumeric(char ch)
        {
            if (ch >= 48 && ch <= 57)
            {
                return true;
            }

            return false;
        }

        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            Keys key = (Keys)e.KeyChar;

            if (key == Keys.Back)
            {
                return;
            }

            else if (!IsNumeric(e.KeyChar))
            {
                this.Text = "Only numbers";
                e.Handled = true;
            }
            else
            {
                this.Text = "Set PIN";
            }
        }
    }
}
