using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Delta
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 form1 = new Form1();
            if(args.Length == 1){
                form1.CurrentFile.load_file(args[0]);
                form1.ViewCtrl.RefreshData();
                form1.SeriesCtrl.RefreshDatagridview();
                form1.ReDraw();
                form1.Text = "Delta - " + form1.CurrentFile.FileName;
            }
            form1.SeriesCtrl.dataGridView1.Refresh();
            Application.Run(form1);

        }
    }
}
