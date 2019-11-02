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
    public partial class Form3 : Form
    {
        bool donotclose = false;

        Form1 father;

        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Deactivate(object sender, EventArgs e)
        {
            if(!donotclose)
                this.Close();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            
        }

        public void LoadData(Form1 form1)
        {
            father = form1;

            if (father.SelectedSerie == null)
            {
                this.Close();
                return;
            }

            dataGridView1.Columns[0].HeaderText = father.CurrentFile.Options.CompoundA;
            dataGridView1.Columns[1].HeaderText = father.CurrentFile.Options.CompoundB;
            dataGridView1.Columns[2].HeaderText = father.CurrentFile.Options.CompoundC;

            foreach(PointF p in father.SelectedSerie.Points)
            {
                dataGridView1.Rows.Add(p.X.ToString(), p.Y.ToString(), (1 - p.X - p.Y).ToString());
            }

        }

        public string SaveData()
        {
            string csvResult = "";

            father.SelectedSerie.Points.Clear();
            for (int i = 0; i < dataGridView1.RowCount - 1; i++)
            {
                object xo = dataGridView1.Rows[i].Cells[0].Value;
                object yo = dataGridView1.Rows[i].Cells[1].Value;
                object zo = dataGridView1.Rows[i].Cells[2].Value;

                string xs = xo == null ? "0" : xo.ToString(); 
                string ys = yo == null ? "0" : yo.ToString();
                string zs = zo == null ? "0" : zo.ToString();

                float newfloat;

                if (!float.TryParse(xs, out newfloat)) xs = "0";
                if (!float.TryParse(ys, out newfloat)) ys = "0";
                if (!float.TryParse(zs, out newfloat)) zs = "0";

                float x = Convert.ToSingle(xs);
                float y = Convert.ToSingle(ys);
                float z = Convert.ToSingle(zs);

                if (x == 0 && y == 0 && z == 0) continue;

                float px = Convert.ToSingle(Math.Round(x / (x + y + z),5));
                float py = Convert.ToSingle(Math.Round(y / (x + y + z),5));
                float pz = Convert.ToSingle(Math.Round(1 - px - py,5));

                csvResult += xs.Replace(",",".") + "," + ys.Replace(",", ".") + "," + zs.Replace(",", ".") + "\n"; 

                dataGridView1.Rows[i].Cells[0].Value = px.ToString();
                dataGridView1.Rows[i].Cells[1].Value = py.ToString();
                dataGridView1.Rows[i].Cells[2].Value = pz.ToString();
                
                father.SelectedSerie.Points.Add(new PointF(px, py));
            }
            father.ReDraw();
            return csvResult;

        }

        

        private void button10_Click(object sender, EventArgs e)
        {
            SaveData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            donotclose = true;
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Filter = "csv file (*.csv)|*.csv";
            SFD.Title = "Save as CSV";
            if (SFD.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(SFD.FileName, SaveData());
            }
            donotclose = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            donotclose = true;
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "csv file (*.csv)|*.csv";
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamReader SR = new System.IO.StreamReader(OFD.FileName);
                string line;
                while((line = SR.ReadLine()) != null)
                {
                    string[] s_line = line.Split(',');
                    if (s_line.Length != 3) continue;

                    string xs = s_line[0];
                    string ys = s_line[1];
                    string zs = s_line[2];
                    float newfloat;

                    if (!float.TryParse(xs, out newfloat)) xs = "0";
                    if (!float.TryParse(ys, out newfloat)) ys = "0";
                    if (!float.TryParse(zs, out newfloat)) zs = "0";

                    dataGridView1.Rows.Add(xs, ys, zs);
                }
            }
            donotclose = false;
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    Clipboard.SetDataObject(dataGridView1.GetClipboardContent());
                }
            }
            else if (e.KeyCode == Keys.V)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    string content = Clipboard.GetText().Replace("\t",",").Replace(" ",",").Replace("\r\n","\n");
                    string[] content_rows = content.Split('\n');

                    int start_row = dataGridView1.SelectedCells[0].RowIndex;
                    int start_col = dataGridView1.SelectedCells[0].ColumnIndex;

                    for(int i = 0; i < content_rows.Length; i++)
                    {
                        if (start_row + i == dataGridView1.RowCount-1) dataGridView1.Rows.Add("0", "0", "0");
                        string[] content_cols = content_rows[i].Split(',');
                        for (int j = 0; j < content_cols.Length; j++)
                        {
                            if (j + start_col > 2) break;
                            string xs = content_cols[j];
                            float newfloat; if (!float.TryParse(xs, out newfloat)) xs = "0";
                            dataGridView1[j + start_col, i + start_row].Value = xs;
                        }
                    }
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if(dataGridView1.RowCount > 1)
                {
                    int prev_row = -1;

                    for (int i = dataGridView1.SelectedCells.Count - 1; i >= 0; i--)
                    {
                        int selected_row_index = dataGridView1.SelectedCells[i].RowIndex;

                        if (selected_row_index != prev_row)
                        {
                            prev_row = selected_row_index;
                        }
                        else
                        {
                            continue;
                        }

                        if (selected_row_index == dataGridView1.RowCount - 1) continue;
                        dataGridView1.Rows.RemoveAt(selected_row_index);
                    }
                }
            }
        }


    }
}
