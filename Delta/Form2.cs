using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace Delta
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

 
        private void checkLisenceNumber()
        {
            string url = "http://www.deltaplotware.com/php/check_license.php?email=" + textBox1.Text + "&key=" + textBox2.Text + "&version=1.";

            WebBrowser browser = new WebBrowser();
            browser.Navigate(url);

            browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser_DocumentCompleted);

            void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
            {
                button10.Image = null;
                string source = browser.DocumentText;

                if (source.Contains("license_used"))
                {
                    MessageBox.Show("Ups! Seems somebody used this license key already. Was it you?. If you are having trouble trying to activate the app go to www.deltaplotware.com for help.", "Delta Plotware", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (source.Contains("wrong_version"))
                {
                    MessageBox.Show("Ups. This license corresponds to another version of the software.\n\nHaving trouble trying to activate the app? Contact us going to www.deltaplotware.com and we'll try to help you out", "Delta Plotware", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (source.Contains("no_license_found"))
                {
                    MessageBox.Show("Sorry, no license found. Please check if the email and key you are using are correct.\n\nHaving trouble trying to activate the app? Contact us going to www.deltaplotware.com and we'll try to help you out", "Delta Plotware", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (source.Contains("okay"))
                {
                    MessageBox.Show("License successfully found! Thanks for purchasing Delta Plotware.The application will restart now.\n\nPlease feel free to send us any feedback at any time through our webpage.","Delta Plotware",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    Properties.Settings.Default.FullMode = true;
                    Properties.Settings.Default.Save();
                    RestartApp();
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            button10.Image = Properties.Resources.Loading;
            checkLisenceNumber();
        }

        static void RestartApp()
        {
            Application.Restart();
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.deltaplotware.com");
            this.Close();
        }
    }
}
