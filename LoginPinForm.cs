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
    public partial class LoginPinForm : Form
    {
        public LoginPinForm()
        {
            InitializeComponent();
        }

        private void TextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                var pin = File.ReadAllLines("pin.tsf");
                string dePin = Encryption.Decrypt(pin[0]);

                Keys key = (Keys)e.KeyCode;

                if (key == Keys.Back)
                {
                    return;
                }
                else if (textBox1.Text == dePin)
                {
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
    }
}
