using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Delta
{
    public partial class DialogControl : Form
    {

        public DialogControl(string DefaultText = "Text",string Title = "Dialog", string OKButton = "OK", string CancelButton = "Cancel")
        {
            InitializeComponent();
            this.Text = Title;
            textBox.Text = DefaultText;
            button0.Text = OKButton;
            button1.Text = CancelButton;
            this.DialogResult = DialogResult.OK;
        }

        private void DialogControl_Load(object sender, EventArgs e)
        {

        }

        public string getResult()
        {
            return textBox.Text;
        }

        private void button0_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
