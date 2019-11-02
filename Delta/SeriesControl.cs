using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Delta
{
    public partial class SeriesControl : UserControl
    {
        private string[] types_src = { "Points", "Segments", "Polyline", "Curve", "Polygon", "Area"};
        private string[] style_line_src = { "Continuous", "Dash", "Dot", "DashDot" };
        private string[] style_area_src = { "Solid", "Hatch1", "Hatch2", "Hatch3", "Hatch4", "Hatch5", "Hatch6" };
        private string[] style_point_src = { "Square", "Circle", "Triangle_up", "Triangle_down", "Rombus", "Square_fill", "Circle_fill", "Triangle_up_fill", "Triangle_down_fill",  "Rombus_fill", "Cross", "Plus", "Text" };
        public List<int> color_src = new List<int>();

        public Form1 Father;

    public SeriesControl()
        {
            InitializeComponent();
            comboBox4.DataSource = types_src;
            comboBox3.DataSource = style_point_src;
            comboBox2.DataSource = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            

            color_src.Add(Color.Magenta.ToArgb());
            color_src.Add(Color.IndianRed.ToArgb());
            color_src.Add(Color.Orange.ToArgb());
            color_src.Add(Color.Gold.ToArgb());
            color_src.Add(Color.ForestGreen.ToArgb());
            color_src.Add(Color.LawnGreen.ToArgb());
            color_src.Add(Color.DeepSkyBlue.ToArgb());
            color_src.Add(Color.Navy.ToArgb());
            color_src.Add(Color.Violet.ToArgb());
            color_src.Add(Color.White.ToArgb());

            comboBox1.DataSource = color_src;
            
        }

        private void SeriesControl_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void InsertColorComboBox1(Color color)
        {
            color_src.Insert(color_src.Count - 1, color.ToArgb());
            comboBox1.DataSource = null;
            comboBox1.DataSource = color_src;
            comboBox1.SelectedIndex = color_src.Count - 2;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex == 0)
                comboBox3.DataSource = style_point_src;

            if (comboBox4.SelectedIndex == 1 || comboBox4.SelectedIndex == 2 || comboBox4.SelectedIndex == 3)     
                comboBox3.DataSource = style_line_src;

            if (comboBox4.SelectedIndex == 4 || comboBox4.SelectedIndex == 5)
                comboBox3.DataSource = style_area_src;
                
            comboBox3.SelectedIndex = 0;
        }

        private void comboBox4_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle rectangle = new Rectangle(2, e.Bounds.Top + 2, e.Bounds.Height - 4, e.Bounds.Height - 4);
            e.Graphics.DrawImage(GetBitmapArt(types_src[e.Index]), rectangle);
            e.DrawFocusRectangle();
        }

        private void comboBox3_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle rectangle = new Rectangle(2, e.Bounds.Top + 2, e.Bounds.Height - 4, e.Bounds.Height - 4);

            if (comboBox4.SelectedIndex == 0)
                e.Graphics.DrawImage(GetBitmapArt(style_point_src[e.Index]), rectangle);
            if (comboBox4.SelectedIndex == 1 || comboBox4.SelectedIndex == 2 || comboBox4.SelectedIndex == 3)
                e.Graphics.DrawImage(GetBitmapArt(style_line_src[e.Index]), rectangle);
            if (comboBox4.SelectedIndex == 4 || comboBox4.SelectedIndex == 5)
                e.Graphics.DrawImage(GetBitmapArt(style_area_src[e.Index]), rectangle);


            e.DrawFocusRectangle();
        }

        private void comboBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Rectangle rectangle = new Rectangle(2, e.Bounds.Top + 2, e.Bounds.Height - 4, e.Bounds.Height - 4);
            e.Graphics.DrawString((e.Index + 1).ToString(),new Font("Times New Roman", 11), new SolidBrush(Color.Black),rectangle.X+4,rectangle.Y+4);
            e.DrawFocusRectangle();
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Rectangle rectangle = new Rectangle(2, e.Bounds.Top + 2, e.Bounds.Height - 4, e.Bounds.Height - 4);

            e.Graphics.DrawImage(GetColorBoxBitmap(color_src[e.Index]), rectangle);

            e.DrawFocusRectangle();
        }

        private Bitmap GetColorBoxBitmap(int colorArgb)
        {
            Bitmap bmp = new Bitmap(24, 24);
            Graphics G = Graphics.FromImage(bmp);
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            G.FillRectangle(new SolidBrush(Color.FromArgb(colorArgb)), 4, 4, 16, 16);
            G.DrawRectangle(new Pen(Color.Black, 1), 4, 4, 16, 16);

            return bmp;
        }

        private Bitmap GetBitmapArt(string art)
        {
            Bitmap bmp = new Bitmap(24, 24);
            Graphics G = Graphics.FromImage(bmp);
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Brush defaultBrush = new SolidBrush(Color.Black);
            Pen defaultPen = new Pen(Color.Black, 2);


            if(art == "Points")
            {
                G.FillEllipse(defaultBrush,4,4,6,6);
                G.FillEllipse(defaultBrush, 17, 6, 6, 6);
                G.FillEllipse(defaultBrush, 9, 15, 6, 6);
            } else if(art == "Segments")
            {
                G.DrawLine(defaultPen, 6, 6, 18, 9);
                G.DrawLine(defaultPen, 6, 18, 18, 16);
            } else if(art == "Polyline")
            {
                G.DrawLine(defaultPen, 17, 6, 5, 9);
                G.DrawLine(defaultPen, 5, 9, 6, 16);
                G.DrawLine(defaultPen, 6, 16, 17, 18);
            } else if(art == "Curve")
            {
                PointF[] points = { new PointF(6.0f, 20f), new PointF(5.4f, 13.1f), new PointF(14.0f, 15.6f), new PointF(19.0f, 7.9f) };
                G.DrawCurve(defaultPen, points);
            } else if(art == "Polygon")
            {
                Point[] points = { new Point(17,6), new Point(5,9), new Point(6,16), new Point(17, 18) };
                G.FillPolygon(defaultBrush, points);
            } else if(art == "Area")
            {
                Point[] points = { new Point(17,6), new Point(12, 10), new Point(5,9), new Point(6,16), new Point(11, 17),  new Point(19, 18) };
                G.FillClosedCurve(defaultBrush, points);
            }


            else if (art == "Continuous")
            {
                G.DrawLine(new Pen(Color.Black, 2), 0, 12, 24, 12);
            } else if (art == "Dash")
            {
                G.DrawLine(new Pen(Color.Black, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }, 0, 12, 24, 12);
            } else if (art == "DashDot")
            {
                G.DrawLine(new Pen(Color.Black, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot }, 0, 12, 24, 12);
            } else if (art == "Dot")
            {
                G.DrawLine(new Pen(Color.Black, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot }, 0, 12, 24, 12);
            }


            else if(art == "Square")
            {
                G.DrawRectangle(defaultPen, 6, 6, 12, 12);
            }
            else if (art == "Triangle_up")
            {
                Point[] points = { new Point(12, 6), new Point(18, 18), new Point(6, 18) };
                G.DrawPolygon(defaultPen, points);
            }
            else if (art == "Triangle_down")
            {
                Point[] points = { new Point(12, 18), new Point(18, 6), new Point(6, 6) };
                G.DrawPolygon(defaultPen, points);
            }
            else if (art == "Circle")
            {
                G.DrawEllipse(defaultPen, 6, 6, 12, 12);
            }
            else if (art == "Rombus")
            {
                Point[] points = { new Point(12, 6), new Point(18, 12), new Point(12, 18), new Point(6, 12) };
                G.DrawPolygon(defaultPen, points);
            }
            else if (art == "Square_fill")
            {
                G.FillRectangle(defaultBrush, 6, 6, 12, 12);
            }
            else if (art == "Triangle_up_fill")
            {
                Point[] points = { new Point(12, 6), new Point(18, 18), new Point(6, 18) };
                G.FillPolygon(defaultBrush, points);
            }
            else if (art == "Triangle_down_fill")
            {
                Point[] points = { new Point(12, 18), new Point(18, 6), new Point(6, 6) };
                G.FillPolygon(defaultBrush, points);
            }
            else if (art == "Circle_fill")
            {
                G.FillEllipse(defaultBrush, 6, 6, 12, 12);
            }
            else if (art == "Rombus_fill")
            {
                Point[] points = { new Point(12, 6), new Point(18, 12), new Point(12, 18), new Point(6, 12) };
                G.FillPolygon(defaultBrush, points);
            }
            else if (art == "Plus")
            {
                G.DrawLine(defaultPen, 12, 6, 12, 18);
                G.DrawLine(defaultPen, 18,12, 6, 12);
            }
            else if (art == "Cross")
            {
                G.DrawLine(defaultPen, 6, 6, 18, 18);
                G.DrawLine(defaultPen, 6, 18, 18, 6);
            }
            else if (art == "Text")
            {
                
                G.DrawString("Tx", new Font("Times New Roman", 12),defaultBrush, 0.0f, 4.0f);
            }

            else if (art == "Solid")
            {
                G.FillRectangle(new SolidBrush(Color.Black), 4, 4, 16, 16);
            }
          
            else if (art == "Hatch1")
            {
                G.FillRectangle(new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal,Color.Black,Color.White), 4, 4, 16, 16);
            }
            else if (art == "Hatch2")
            {
                G.FillRectangle(new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.ForwardDiagonal, Color.Black, Color.White), 4, 4, 16, 16);
            }
            else if (art == "Hatch3")
            {
                G.FillRectangle(new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Cross, Color.Black, Color.White), 4, 4, 16, 16);
            }
            else if (art == "Hatch4")
            {
                G.FillRectangle(new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.DiagonalCross, Color.Black, Color.White), 4, 4, 16, 16);
            }
            else if (art == "Hatch5")
            {
                G.FillRectangle(new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Vertical, Color.Black, Color.White), 4, 4, 16, 16);
            }
            else if (art == "Hatch6")
            {
                G.FillRectangle(new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Horizontal, Color.Black, Color.White), 4, 4, 16, 16);
            }

            return bmp;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        
        public void RefreshDatagridview()
        {
            try {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = Father.CurrentFile.Series;
        
            if(Father.CurrentFile.Series.Count == 0)
            {
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                comboBox3.Enabled = false;
                comboBox4.Enabled = false;
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
            } else
            {
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                checkBox1.Enabled = true;
                checkBox2.Enabled = true;
            }

            dataGridView1.Columns[0].Visible = false;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            
            }
            catch (Exception ex)
            {

            }
            dataGridView1.Refresh();
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {

            
            try {
            if (dataGridView1.SelectedRows.Count == 0) return;
            if (dataGridView1.SelectedRows[0].Index < 0) return;

            Father.SelectedSerie = (Serie)dataGridView1.SelectedRows[0].DataBoundItem;

            textBox1.Text = Father.SelectedSerie.name;
            checkBox1.Checked = Father.SelectedSerie.displayInLeyend;
            checkBox2.Checked = Father.SelectedSerie.visible;

            if(color_src.Exists(x => x == Father.SelectedSerie.color))
            {
                comboBox1.SelectedIndex = color_src.FindIndex(x => x == Father.SelectedSerie.color);
            } else
            {
                InsertColorComboBox1(Color.FromArgb(Father.SelectedSerie.color));
            }
            
            comboBox2.SelectedIndex = Father.SelectedSerie.size - 1;

            comboBox4.SelectedIndex = (int)Father.SelectedSerie.type;

            if (Father.SelectedSerie.type == Types.point)
                comboBox3.SelectedIndex = (int)Father.SelectedSerie.style;
            
            if (Father.SelectedSerie.type == Types.polyline || Father.SelectedSerie.type == Types.spline || Father.SelectedSerie.type == Types.segments)
                comboBox3.SelectedIndex = (int)Father.SelectedSerie.style - 20;

            if (Father.SelectedSerie.type == Types.polygon || Father.SelectedSerie.type == Types.area)
                comboBox3.SelectedIndex = (int)Father.SelectedSerie.style - 30;
            }
            catch (Exception ex)
            {

            }

        }

        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            try { 
            if (comboBox1.DataSource == null) return;
            if (comboBox1.SelectedItem.Equals(Color.White.ToArgb()))
            {
                ColorDialog CD = new ColorDialog();
                if (CD.ShowDialog() == DialogResult.OK)
                {
                    InsertColorComboBox1(CD.Color);
                }
            }

            Comando newCommand = new Comando() { Type = CommandType.ChangeColor, IntValue = color_src[comboBox1.SelectedIndex], SerieValue = Father.SelectedSerie };
            Father.CommandManager.ExecuteCommand(newCommand);
            }
            catch (Exception ex)
            {

            }
        }

 
        private void comboBox2_DropDownClosed(object sender, EventArgs e)
        {
            try {
            Comando newCommand = new Comando() { Type = CommandType.ChangeSize, IntValue = comboBox2.SelectedIndex + 1, SerieValue = Father.SelectedSerie };
            Father.CommandManager.ExecuteCommand(newCommand);
            }
            catch (Exception ex)
            {

            }
        }

        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            SeriesControl_KeyDown(sender, e);
        }

        private void comboBox3_DropDownClosed(object sender, EventArgs e)
        {
            try {
            if (dataGridView1.SelectedRows.Count == 0) return;

            int style = comboBox3.SelectedIndex;
            if (Father.SelectedSerie.type == Types.point) style += 0;
            if (Father.SelectedSerie.type == Types.polyline || Father.SelectedSerie.type == Types.spline || Father.SelectedSerie.type == Types.segments)
                style += 20;
            if (Father.SelectedSerie.type == Types.area || Father.SelectedSerie.type == Types.polygon)
                style += 30;

            Comando newCommand = new Comando() { Type = CommandType.ChangeStyle, IntValue = style, SerieValue = Father.SelectedSerie };
            Father.CommandManager.ExecuteCommand(newCommand);
            }
            catch (Exception ex)
            {

            }
        }

        private void comboBox4_DropDownClosed(object sender, EventArgs e)
        {
            try
            { 
            if (comboBox4.SelectedIndex == 0)
                Father.SelectedSerie.style = Style.cuadrado;

            if (comboBox4.SelectedIndex == 1 || comboBox4.SelectedIndex == 2 || comboBox4.SelectedIndex == 3)
                Father.SelectedSerie.style = Style.Line_continous;

            if (comboBox4.SelectedIndex == 4 || comboBox4.SelectedIndex == 5)
                Father.SelectedSerie.style = Style.Area_Solid;

            if (dataGridView1.SelectedRows.Count == 0) return;
            Comando newCommand = new Comando() { Type = CommandType.ChangeType, IntValue = comboBox4.SelectedIndex, SerieValue = Father.SelectedSerie };
            Father.CommandManager.ExecuteCommand(newCommand);
            }
            catch (Exception ex)
            {

            }
        }

        private void SeriesControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    Father.CommandManager.Undo();
                }
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            SeriesControl_KeyDown(sender, e);
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            SeriesControl_KeyDown(sender, e);
        }

        private void comboBox2_KeyDown(object sender, KeyEventArgs e)
        {
            SeriesControl_KeyDown(sender, e);
        }

        private void comboBox3_KeyDown(object sender, KeyEventArgs e)
        {
            SeriesControl_KeyDown(sender, e);
        }

        private void comboBox4_KeyDown(object sender, KeyEventArgs e)
        {
            SeriesControl_KeyDown(sender, e);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                try {
                if (dataGridView1.RowCount == 0) return;
                Comando newCommand = new Comando() { Type = CommandType.ChangeName, SerieValue = Father.SelectedSerie, StringValue = textBox1.Text };
                Father.CommandManager.ExecuteCommand(newCommand);
                }
                catch (Exception ex)
                {

                }
            } else
            {
                SeriesControl_KeyDown(sender, e);
            }

        }

        private void button1_KeyDown(object sender, KeyEventArgs e)
        {
            SeriesControl_KeyDown(sender, e);
        }


        private void button3_KeyDown(object sender, KeyEventArgs e)
        {
            SeriesControl_KeyDown(sender, e);
        }

        private void button4_KeyDown(object sender, KeyEventArgs e)
        {
            SeriesControl_KeyDown(sender, e);
        }

        private void button5_KeyDown(object sender, KeyEventArgs e)
        {
            SeriesControl_KeyDown(sender, e);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try { 
            if (dataGridView1.RowCount == 0) return;
            if (dataGridView1.SelectedCells[0].RowIndex == 0) return;
            int next_index = dataGridView1.SelectedCells[0].RowIndex - 1;

            Comando newCommand = new Comando() { Type = CommandType.MoveSerieUp, SerieValue = Father.SelectedSerie };
            Father.CommandManager.ExecuteCommand(newCommand);

            dataGridView1.CurrentCell = dataGridView1[1, next_index];
            }
            catch (Exception ex)
            {

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try {
            if (dataGridView1.RowCount == 0) return;
            if (dataGridView1.SelectedCells[0].RowIndex == dataGridView1.RowCount - 1) return;
            int next_index = dataGridView1.SelectedCells[0].RowIndex + 1;

            Comando newCommand = new Comando() { Type = CommandType.MoveSerieDown, SerieValue = Father.SelectedSerie };
            Father.CommandManager.ExecuteCommand(newCommand);

            dataGridView1.CurrentCell = dataGridView1[1, next_index];
            }
            catch (Exception ex)
            {

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try {
            if (dataGridView1.RowCount == 0) return;
            Comando newCommand = new Comando() { Type = CommandType.RemoveSerie, SerieValue = Father.SelectedSerie };
            Father.CommandManager.ExecuteCommand(newCommand);
            }
            catch (Exception ex)
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try { 
            if (dataGridView1.SelectedCells.Count == 0) return;
            Comando newCommand = new Comando() { Type = CommandType.AddSerie, SerieValue =  Father.SelectedSerie.Clone(true)};
            Father.CommandManager.ExecuteCommand(newCommand);
            dataGridView1.CurrentCell = dataGridView1[1, dataGridView1.RowCount - 1];
            }
            catch (Exception ex)
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Comando newCommand = new Comando() { Type = CommandType.AddSerie, SerieValue = new Serie() };
                Father.CommandManager.ExecuteCommand(newCommand);
                if (dataGridView1.RowCount > 0) { dataGridView1.CurrentCell = dataGridView1[1, dataGridView1.RowCount - 1]; }
            } catch (Exception ex)
            {

            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            try {
            Comando newCommand = new Comando() { Type = CommandType.ChangeVisibility, SerieValue = Father.SelectedSerie, IntValue = (checkBox2.Checked == false) ? 0 : 1 };
            Father.CommandManager.ExecuteCommand(newCommand);
            }
            catch (Exception ex)
            {

            }
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            try { 
            Comando newCommand = new Comando() { Type = CommandType.ChangeDisplayLeyend, SerieValue = Father.SelectedSerie, IntValue = (checkBox1.Checked == false) ? 0 : 1 };
            Father.CommandManager.ExecuteCommand(newCommand);
            }
            catch (Exception ex)
            {

            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }


}
