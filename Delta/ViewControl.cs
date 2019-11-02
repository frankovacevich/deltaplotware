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
    public partial class ViewControl : UserControl
    {

        public Form1 Father;
        private string[] style_line_src = { "Continuous", "Dash", "Dot", "Hidden" };
        private int[] spacing_src = { 2, 3, 4, 5, 10, 20 };
        public List<int> color_src = new List<int>();

        public ViewControl()
        {
            InitializeComponent();
            comboBox1.DataSource = style_line_src;
            comboBox3.DataSource = spacing_src;

            color_src.Add(Color.Black.ToArgb());
            color_src.Add(Color.DimGray.ToArgb());
            color_src.Add(Color.Gray.ToArgb());
            color_src.Add(Color.DarkGray.ToArgb());
            color_src.Add(Color.LightGray.ToArgb());
            color_src.Add(Color.White.ToArgb());

            comboBox2.DataSource = color_src;

            textBox1.LostFocus += new EventHandler(textBox1_LostFocus);
            textBox2.LostFocus += new EventHandler(textBox2_LostFocus);
            textBox3.LostFocus += new EventHandler(textBox3_LostFocus);
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Rectangle rectangle = new Rectangle(2, e.Bounds.Top + 2, e.Bounds.Height - 4, e.Bounds.Height - 4);

            e.Graphics.DrawImage(GetLineArt(e.Index), rectangle);
            e.DrawFocusRectangle();
        }

        private Bitmap GetLineArt(int index)
        {
            Bitmap bmp = new Bitmap(24, 24);
            Graphics G = Graphics.FromImage(bmp);
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Brush defaultBrush = new SolidBrush(Color.Black);
            Pen defaultPen = new Pen(Color.Black, 2);

            if (index == 0)
            {
                G.DrawLine(new Pen(Color.Black, 2), 0, 12, 24, 12);
            }
            else if (index == 1)
            {
                G.DrawLine(new Pen(Color.Black, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }, 0, 12, 24, 12);
            } 
            else if (index == 2)
            {
                G.DrawLine(new Pen(Color.Black, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot }, 0, 12, 24, 12);
            }
            else if (index == 3)
            {
                
            }

            return bmp;
        }

        private void ViewControl_Load(object sender, EventArgs e)
        {

        }

        public void RefreshData()
        {
            Opciones opt = Father.CurrentFile.Options;
            if (opt.DisplayStyle == DispStyle.Equilateral)
            {
                button1.BackColor = SystemColors.Highlight;
                button2.BackColor = SystemColors.Control;
            }
            else
            {
                button1.BackColor = SystemColors.Control;
                button2.BackColor = SystemColors.Highlight;
            }

            if(opt.LegendVisible == true)
                button3.BackColor = SystemColors.Highlight;
            else
                button3.BackColor = SystemColors.Control;

            if (opt.TickVisible == true)
                button7.BackColor = SystemColors.Highlight;
            else
                button7.BackColor = SystemColors.Control;

            if (opt.CompoundTextLocation == CompoundTxtLocation.Top)
            {
                button4.BackColor = SystemColors.Highlight;
                button5.BackColor = SystemColors.Control;
                button11.BackColor = SystemColors.Control;
            }
            else if (opt.CompoundTextLocation == CompoundTxtLocation.Side)
            {
                button4.BackColor = SystemColors.Control;
                button5.BackColor = SystemColors.Highlight;
                button11.BackColor = SystemColors.Control;
            } else
            {
                button4.BackColor = SystemColors.Control;
                button5.BackColor = SystemColors.Control;
                button11.BackColor = SystemColors.Highlight;
            }

            if(opt.BackgroundPicture == null)
            {
                button10.BackColor = SystemColors.Highlight;
                button9.BackColor = SystemColors.Control;
            } else
            {
                button10.BackColor = SystemColors.Control;
                button9.BackColor = SystemColors.Highlight;
            }

            /*if (opt.BackgroundStyle == BackgndStyle.Transparent)
            {
                button10.BackColor = SystemColors.Highlight;
                button8.BackColor = SystemColors.Control;
                button9.BackColor = SystemColors.Control;
            }

            else if (opt.BackgroundStyle == BackgndStyle.Color)
            {
                button10.BackColor = SystemColors.Control;
                button8.BackColor = SystemColors.Highlight;
                button9.BackColor = SystemColors.Control;
            }
            else
            {
                button10.BackColor = SystemColors.Control;
                button8.BackColor = SystemColors.Control;
                button9.BackColor = SystemColors.Highlight;
            }*/

            if (color_src.Exists(x => x == opt.GridColor))
            {
                comboBox2.SelectedIndex = color_src.FindIndex(x => x == opt.GridColor);
            }
            else
            {
                InsertColorComboBox(Color.FromArgb(opt.GridColor));
            }

            comboBox3.SelectedIndex = spacing_src.ToList().IndexOf(opt.GridSpacing);
            comboBox1.SelectedIndex = (int)opt.GridStyle;

            textBox1.Text = opt.CompoundA;
            textBox2.Text = opt.CompoundB;
            textBox3.Text = opt.CompoundC;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Comando newCommand = new Comando() { Type = CommandType.Option_DisplayStyle, IntValue = 0 };
            Father.CommandManager.ExecuteCommand(newCommand);
            button1.BackColor = SystemColors.Highlight;
            button2.BackColor = SystemColors.Control;
        }

        private void comboBox2_DropDownClosed(object sender, EventArgs e)
        {
            if (comboBox2.DataSource == null) return;
            if (comboBox2.SelectedItem.Equals(Color.White.ToArgb()))
            {
                ColorDialog CD = new ColorDialog();
                if (CD.ShowDialog() == DialogResult.OK)
                {
                    InsertColorComboBox(CD.Color);
                }
            }
            Comando newCommand = new Comando() { Type = CommandType.Option_GridColor, IntValue = color_src[comboBox2.SelectedIndex] };
            Father.CommandManager.ExecuteCommand(newCommand);
        }

        private void InsertColorComboBox(Color color)
        {
            color_src.Insert(color_src.Count - 1, color.ToArgb());
            comboBox2.DataSource = null;
            comboBox2.DataSource = color_src;
            comboBox2.SelectedIndex = color_src.Count - 2;
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

        private void comboBox3_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Rectangle rectangle = new Rectangle(2, e.Bounds.Top + 2, e.Bounds.Height - 4, e.Bounds.Height - 4);
            e.Graphics.DrawString(spacing_src[e.Index].ToString(), new Font("Segoe UI", 11), new SolidBrush(Color.Black), rectangle.X+2, rectangle.Y +1);
            e.DrawFocusRectangle();
        }

        private void comboBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Rectangle rectangle = new Rectangle(2, e.Bounds.Top + 2, e.Bounds.Height - 4, e.Bounds.Height - 4);

            e.Graphics.DrawImage(GetColorBoxBitmap(color_src[e.Index]), rectangle);

            e.DrawFocusRectangle();
        }

        private void comboBox1_DropDownClosed(object sender, EventArgs e)
        {
            Comando newCommand = new Comando() { Type = CommandType.Option_GridStyle , IntValue = comboBox1.SelectedIndex };
            Father.CommandManager.ExecuteCommand(newCommand);
        }

        private void comboBox3_DropDownClosed(object sender, EventArgs e)
        {
            Comando newCommand = new Comando() { Type = CommandType.Option_GridSpacing, IntValue = spacing_src[comboBox3.SelectedIndex] };
            Father.CommandManager.ExecuteCommand(newCommand);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Comando newCommand = new Comando() { Type = CommandType.Option_DisplayStyle, IntValue = 1 };
            Father.CommandManager.ExecuteCommand(newCommand);
            button1.BackColor = SystemColors.Control;
            button2.BackColor = SystemColors.Highlight;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.BackColor == SystemColors.Control)
            {
                Comando newCommand = new Comando() { Type = CommandType.Option_LegendVisible, IntValue = 1 };
                Father.CommandManager.ExecuteCommand(newCommand);
                button3.BackColor = SystemColors.Highlight;
            }
            else
            {
                Comando newCommand = new Comando() { Type = CommandType.Option_LegendVisible, IntValue = 0 };
                Father.CommandManager.ExecuteCommand(newCommand);
                button3.BackColor = SystemColors.Control;
            }
  
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (button7.BackColor == SystemColors.Control)
            {
                Comando newCommand = new Comando() { Type = CommandType.Option_TickVisible, IntValue = 1 };
                Father.CommandManager.ExecuteCommand(newCommand);
                button7.BackColor = SystemColors.Highlight;
            }
            else
            {
                Comando newCommand = new Comando() { Type = CommandType.Option_TickVisible, IntValue = 0 };
                Father.CommandManager.ExecuteCommand(newCommand);
                button7.BackColor = SystemColors.Control;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Comando newCommand = new Comando() { Type = CommandType.Option_CompoundTextLocation, IntValue = 0 };
            Father.CommandManager.ExecuteCommand(newCommand);
            button4.BackColor = SystemColors.Highlight;
            button5.BackColor = SystemColors.Control;
            button11.BackColor = SystemColors.Control;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Comando newCommand = new Comando() { Type = CommandType.Option_CompoundTextLocation, IntValue = 1 };
            Father.CommandManager.ExecuteCommand(newCommand);
            button4.BackColor = SystemColors.Control;
            button5.BackColor = SystemColors.Highlight;
            button11.BackColor = SystemColors.Control;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Comando newCommand = new Comando() { Type = CommandType.Option_CompoundTextLocation, IntValue = 2 };
            Father.CommandManager.ExecuteCommand(newCommand);
            button4.BackColor = SystemColors.Control;
            button5.BackColor = SystemColors.Control;
            button11.BackColor = SystemColors.Highlight;
        }

        private void button8_Click(object sender, EventArgs e)
        {        
            ColorDialog CD = new ColorDialog();
            if(CD.ShowDialog() == DialogResult.OK)
            {
                Comando newCommand = new Comando() { Type = CommandType.Option_BackgroundColor, IntValue = CD.Color.ToArgb() };
                Father.CommandManager.ExecuteCommand(newCommand);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            OFD.Title = "Select picture";
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                button10.BackColor = SystemColors.Control;
                button9.BackColor = SystemColors.Highlight;
                Image image = Image.FromFile(OFD.FileName);
                string image_file = DateTime.Now.ToBinary().ToString().Replace("-","") + "_" + getFileName(OFD.FileName).Replace(" ","_");
                image.Save(image_file);
                Father.CurrentFile.Options.BackgroundPicture = image_file;
                Father.ReDraw();
                OFD.Dispose();
            }
        }

        private string getFileName(string path)
        {
            path = path.Replace("\\", "/");
            int index1 = path.LastIndexOf("/") + 1;
            return path.Substring(index1);
        }


        private void button10_Click(object sender, EventArgs e)
        {
            button10.BackColor = SystemColors.Highlight;
            button9.BackColor = SystemColors.Control;

            if (System.IO.File.Exists(Father.CurrentFile.Options.BackgroundPicture))
            {
                System.IO.File.Delete(Father.CurrentFile.Options.BackgroundPicture);
            }

            Father.CurrentFile.Options.BackgroundPicture = null;


            Father.ReDraw();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Comando newCommand = new Comando() { Type = CommandType.Option_BackgroundStyle, IntValue = 2 };
            Father.CommandManager.ExecuteCommand(newCommand);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                Comando newCommand = new Comando() { Type = CommandType.Option_CompoundA, StringValue = textBox1.Text };
                Father.CommandManager.ExecuteCommand(newCommand);
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Comando newCommand = new Comando() { Type = CommandType.Option_CompoundB, StringValue = textBox2.Text };
                Father.CommandManager.ExecuteCommand(newCommand);
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Comando newCommand = new Comando() { Type = CommandType.Option_CompoundC, StringValue = textBox3.Text };
                Father.CommandManager.ExecuteCommand(newCommand);
            }
        }

        private void textBox1_LostFocus(object sender, EventArgs e)
        {
           
                Comando newCommand = new Comando() { Type = CommandType.Option_CompoundA, StringValue = textBox1.Text };
                Father.CommandManager.ExecuteCommand(newCommand);
            
        }

        private void textBox2_LostFocus(object sender, EventArgs e)
        {
          
                Comando newCommand = new Comando() { Type = CommandType.Option_CompoundB, StringValue = textBox2.Text };
                Father.CommandManager.ExecuteCommand(newCommand);
       
        }

        private void textBox3_LostFocus(object sender, EventArgs e)
        {
            
                Comando newCommand = new Comando() { Type = CommandType.Option_CompoundC, StringValue = textBox3.Text };
                Father.CommandManager.ExecuteCommand(newCommand);
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FontDialog FD = new FontDialog();
            FD.Font = new Font(Father.CurrentFile.Options.FontFamily, Father.CurrentFile.Options.FontSize);
            if(FD.ShowDialog() == DialogResult.OK)
            {
                Comando newCommand = new Comando() { Type = CommandType.Option_Font, IntValue = (int) FD.Font.SizeInPoints, StringValue = FD.Font.Name.ToString() };
                Father.CommandManager.ExecuteCommand(newCommand);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

    }

    

     
}
