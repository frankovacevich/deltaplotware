using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// This is the code for your desktop app.
// Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.

namespace Delta
{
    public partial class Form1 : Form
    {

        public SeriesControl SeriesCtrl = new SeriesControl();
        public ViewControl ViewCtrl = new ViewControl();

        public Graficador Graficador = new Graficador();
        public CommandManager CommandManager = new CommandManager();
        public Archivo CurrentFile = new Archivo();
        public Serie SelectedSerie = null;
        
        public Form1()
        {
            InitializeComponent();
            CommandManager.FormCtrl = this;
            mainPictureBox.MouseWheel += new MouseEventHandler(mainPictureBox_MouseWheel);
            SeriesCtrl.Father = this;
            ViewCtrl.Father = this;
            ViewCtrl.RefreshData();

            CurrentFile.clear_file();
        }

        /// KEYDOWN
        /// UNDO / REDO
        /// 
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Z)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    //toolStripButton2.BackColor = SystemColors.Window;
                    //quickedit = false;
                    CommandManager.Undo();
                }
            }
            else if (e.KeyCode == Keys.Y)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    //toolStripButton2.BackColor = SystemColors.Window;
                    //quickedit = false;
                    CommandManager.Redo();
                }
            }
            else if (e.KeyCode == Keys.Q) { QuickEditButtonClick(); }
            else if (e.KeyCode == Keys.R) { ShowSpreadsheetButtonClick(); }
            else if (e.KeyCode == Keys.H) {
                zoomFactor = 1;
                ReDraw();
            }
            else if (e.KeyCode == Keys.S)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    SaveFileButtonClick();
                }
            }
            else if (e.KeyCode == Keys.N)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    NewFileButtonClick();
                }
            }
            else if (e.KeyCode == Keys.O)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    LoadFileButtonClick();
                }
            }
            
         
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReDraw();
            Graficador.CalculateReferencePoints(CurrentFile);

            asSeenToolStripMenuItem.Text = "View size (" + mainPictureBox.Width.ToString() + "x" + mainPictureBox.Height.ToString() + ")";
            customToolStripMenuItem.Text = "Custom (" + Properties.Settings.Default.CustomExportWidth.ToString() + "x" + Properties.Settings.Default.CustomExportHeight.ToString() + ")";

            if (Properties.Settings.Default.FullMode == true) toolStripButton3.Visible = false;
        }


        /// DRAW
        /// 
        float zoomFactor = 1;
        public void ReDraw(int W = 0, int H = 0, float radius_ratio = 0.85f)
        {
            if (W == 0 && H == 0)
            {
                H = mainPanel.Height;
                W = mainPanel.Width;
            }
            if (W == 0 || H == 0)
            {
                return;
            }

            mainPictureBox.Width = (int) (zoomFactor * W);
            mainPictureBox.Height = (int) (zoomFactor * H);

            if (mainPictureBox.Right < W) mainPictureBox.Left = W - mainPictureBox.Width;
            if (mainPictureBox.Bottom < H) mainPictureBox.Top = H - mainPictureBox.Height;
            if (mainPictureBox.Left > 0) mainPictureBox.Left = 0;
            if (mainPictureBox.Top > 0) mainPictureBox.Top = 0;

            mainPictureBox.Image = Graficador.Draw(CurrentFile.Series, CurrentFile.Options, mainPictureBox.Width, mainPictureBox.Height, radius_ratio);
        }

        /// TOOLSTRIP
        /// 
        private void toolStripDropDownButton1_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripControlHost toolHost = new ToolStripControlHost(SeriesCtrl);
            toolHost.Size = new Size(SeriesCtrl.Width, SeriesCtrl.Height);
            toolHost.Margin = new Padding(0);

            ToolStripDropDown toolDrop = new ToolStripDropDown();
            toolDrop.Padding = new Padding(0);
            toolDrop.Items.Add(toolHost);
            toolDrop.Show(this, new Point(toolStripDropDownButton1.Bounds.Left , toolStripDropDownButton1.Bounds.Bottom -2));
            SeriesCtrl.Focus();
        }
        private void toolStripButton4_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripControlHost toolHost = new ToolStripControlHost(ViewCtrl);
            toolHost.Size = new Size(ViewCtrl.Width, ViewCtrl.Height);
            toolHost.Margin = new Padding(0);

            ToolStripDropDown toolDrop = new ToolStripDropDown();
            toolDrop.Padding = new Padding(0);
            toolDrop.Items.Add(toolHost);
            toolDrop.Show(this, new Point(toolStripButton4.Bounds.Left, toolStripButton4.Bounds.Bottom - 2));
            ViewCtrl.Focus();
        }


        /// PICTUREBOX
        /// 
        private bool isDragging = false;
        private int currentX, currentY;
        private void mainPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (quickedit == true)
            {
                if (SelectedSerie == null) return;
                if (e.Button == MouseButtons.Left)
                {
                    Comando newCommand = new Comando() { Type = CommandType.AddPoint, SerieValue = SelectedSerie ,
                        PointValue = Graficador.GetScreenPoint(new PointF(e.X, e.Y), CurrentFile.Options) };
                    CommandManager.ExecuteCommand(newCommand);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    PointF closestPoint = Graficador.GetClosestPoint(Graficador.GetScreenPoint(new PointF(e.X, e.Y), CurrentFile.Options)
                        , CurrentFile.Options, mainPictureBox.Height, mainPictureBox.Width,SelectedSerie.Points,true);
                    if (float.IsNaN(closestPoint.X)  || float.IsNaN(closestPoint.Y)) return;

                    Comando newCommand = new Comando() { Type = CommandType.RemovePoint, SerieValue = SelectedSerie, PointValue = closestPoint };
                    CommandManager.ExecuteCommand(newCommand);
                }

            } else
            {
                isDragging = true;
                currentX = e.X;
                currentY = e.Y;
            }
            
        }

        private void mainPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void mainPictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            float prev_zoomFactor = zoomFactor;
            zoomFactor += (float) e.Delta / 1200;
            if (zoomFactor < 1)
            {
                zoomFactor = 1;
                return;
            }
            if (zoomFactor > 5)
            {
                zoomFactor = 5;
                return;
            }

            int delta_left = (int) ((mainPanel.Width * (zoomFactor - prev_zoomFactor)) * ((float) (e.X + mainPictureBox.Left) / (float)mainPanel.Width));
            int delta_top = (int) ((mainPanel.Height * (zoomFactor - prev_zoomFactor)) * ((float) (e.Y + mainPictureBox.Top) / (float) mainPanel.Height));

            mainPictureBox.Visible = false;
            mainPictureBox.Left -= delta_left;
            mainPictureBox.Top -= delta_top;
            mainPictureBox.Visible = true;

            ReDraw();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            ReDraw();
            asSeenToolStripMenuItem.Text = "View size (" + mainPictureBox.Width.ToString() + "x" + mainPictureBox.Height.ToString() + ")";
        }

        int local_tipstatic = 0;
        int local_randnumber = 0;
        private string GetTip()
        {
            if (quickedit) return "Use left/right buttons to add/remove points";
            if (zoomFactor > 1) return "Press H to restore the zoom";

            string[] tips = { "Hold CTRL to snap to points on the graph", "Press H to restore the zoom", "Use the mouse wheel to zoom in or out", "Use Ctrl+Z to undo and Ctrl+Y to redo", "Send us some feedback at www.deltaplotware.com"};
            if (local_tipstatic != CommandManager.ZStack.stack.Count)
            {
                local_tipstatic = CommandManager.ZStack.stack.Count;
                Random rand = new Random();
                local_randnumber = rand.Next(0, tips.Length - 1);
            }

            return tips[local_randnumber];
        }

        private void mainPictureBox_MouseMove(object sender, MouseEventArgs e)
        {

            PointF composition = Graficador.GetScreenPoint(new PointF(e.X, e.Y), CurrentFile.Options);

            if(composition.X < 0 || composition.Y < 0 || composition.X + composition.Y > 1)
            {
                StripLabelA.Text = "";
                StripLabelB.Text = "";
                StripLabelC.Text = GetTip();
                StripLabelC.AutoSize = true;
                this.Cursor = Cursors.Default;
            } else
            {
                StripLabelA.Text = "A: " + Math.Round(composition.X * 100, 1).ToString("0.0") + "%";
                StripLabelB.Text = "B: " + Math.Round(composition.Y * 100, 1).ToString("0.0") + "%";
                StripLabelC.Text = "C: " + Math.Round(100 - composition.X * 100 - composition.Y * 100, 1).ToString("0.0") + " %";
                StripLabelC.AutoSize = false;
                this.Cursor = Cursors.Cross;
            }

            if (ModifierKeys.HasFlag(Keys.Control))
            {
                SnapToReferencePoint(composition);
                return;
            }

            if (isDragging)
            {

                StripLabelA.Text = "";
                StripLabelB.Text = "";
                StripLabelC.Text = "";

                int delta_x = (e.X - currentX);
                if (delta_x > 0 && delta_x >= -mainPictureBox.Left)
                    delta_x = -mainPictureBox.Left;
                if (delta_x < 0 && delta_x < mainPanel.Width - mainPictureBox.Right)
                    delta_x = mainPanel.Width - mainPictureBox.Right;

                mainPictureBox.Left = mainPictureBox.Left + delta_x;

                int delta_y = (e.Y - currentY);
                if (delta_y > 0 && delta_y >= -mainPictureBox.Top)
                    delta_y = -mainPictureBox.Top;
                if (delta_y < 0 && delta_y < mainPanel.Height - mainPictureBox.Bottom)
                    delta_y = mainPanel.Height - mainPictureBox.Bottom;

                mainPictureBox.Top = mainPictureBox.Top + delta_y;
            }
            
        }

        private void SnapToReferencePoint(PointF point)
        {
            if (Graficador.ReferencePoints.Count == 0) return;

            PointF closestPoint = Graficador.GetClosestPoint(point, CurrentFile.Options, mainPictureBox.Height, mainPictureBox.Width);

            this.Cursor = new Cursor(Cursor.Current.Handle);
            int a = (int)closestPoint.X;
            int b = (int)closestPoint.Y;
            int c = PointToScreen(mainPictureBox.Location).X;
            int d = PointToScreen(mainPictureBox.Location).Y;

            Cursor.Position = new Point(c + a, b + d + mainPanel.Location.Y);

        }

      

        /// TOOLBAR
        ///
        bool quickedit = false;

        private void toolStrip1_KeyDown(object sender, KeyEventArgs e)
        {
            Form1_KeyDown(sender, e);
        }

        private void ShowSpreadsheetButtonClick()
        {
            Form3 form3 = new Form3();
            form3.Show();
            form3.LoadData(this);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            ShowSpreadsheetButtonClick();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        ///
        /// FILES
        ///

        public bool ThereIsUnsavedWork = false;

        private void toolStripButton6_Click(object sender, EventArgs e) //NewButton
        {
            NewFileButtonClick();
        }

        private void toolStripButton5_Click(object sender, EventArgs e) //LoadButton
        {
            LoadFileButtonClick();
        }

        private void toolStripButton8_Click(object sender, EventArgs e) //SaveButton
        {
            SaveFileButtonClick();
        }

        private void NewFileButtonClick()
        {
            if (CurrentFile.Series.Count > 0)
            {
                if (MessageBox.Show("Do you want to create a new document?\n\nAll unsaved progress will be lost", "Delta", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    CurrentFile.clear_file();
                    this.Text = "Delta - New File";
                    ViewCtrl.RefreshData();
                    SeriesCtrl.RefreshDatagridview();
                }
            }
            else
            {
                CurrentFile.clear_file();
                this.Text = "Delta - New File";
                ViewCtrl.RefreshData();
                SeriesCtrl.RefreshDatagridview();
            }
            ReDraw();
        }

        private void LoadFileButtonClick()
        {
            if (CurrentFile.Series.Count > 0)
            {
                if (MessageBox.Show("Do you want to open a document?\n\nAll unsaved progress will be lost", "Delta", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    LoadFile();
                }
            }
            else
            {
                LoadFile();
            }
            ViewCtrl.RefreshData();
            SeriesCtrl.RefreshDatagridview();
            ReDraw();
        }

        private void LoadFile()
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Filter = "Delta Plotware File (*.dpw)|*.dpw";
            OFD.Title = "Open File";
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                CurrentFile.load_file(OFD.FileName);
                this.Text = "Delta - " + CurrentFile.FileName;
            }
            CommandManager.Reset();
        }
       

        private void SaveFileButtonClick()
        {
            if (CurrentFile.FilePath == "")
            {
                SaveFileDialog SFD = new SaveFileDialog();
                SFD.Filter = "Delta Plotware File (*.dpw)|*.dpw";
                SFD.Title = "Save File File";
                if (SFD.ShowDialog() == DialogResult.OK)
                {
                    CurrentFile.FilePath = SFD.FileName;
                    CurrentFile.FileName = CurrentFile.getFileName(CurrentFile.FilePath);
                    CurrentFile.save_file(SFD.FileName);
                    CommandManager.Reset();
                    ThereIsUnsavedWork = false;
                }
            }
            else
            {
                CurrentFile.save_file(CurrentFile.FilePath);
                CommandManager.Reset();
                ThereIsUnsavedWork = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ThereIsUnsavedWork) { 
                if (MessageBox.Show("Do you want to save your work before closing?", "Delta", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SaveFileButtonClick();
                }
            }
        }


        // EXPORTING
        //
        Size exporting_size = new Size(0, 0);
        private void exportAsPNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float aux = zoomFactor;
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Title = "Save as PNG";
            SFD.Filter = "png (*.png)|*.png";
            if(SFD.ShowDialog() == DialogResult.OK)
            {
                float radius_ratio = 0.95f;
                if (exporting_size.Height == 0) radius_ratio = 0.85f;
                else if (exporting_size.Height == 794) radius_ratio = 0.98f;
                else if (exporting_size.Height == 600) radius_ratio = 0.98f;
                zoomFactor = 1;
                ReDraw(exporting_size.Width, exporting_size.Height, radius_ratio);
                zoomFactor = aux;
                mainPictureBox.Image.Save(SFD.FileName);
                ReDraw(0, 0);
            }
        }

        private void exportAsSVGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            float aux = zoomFactor;
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Title = "Save as SVG";
            SFD.Filter = "svg (*.svg)|*.svg";
            if (SFD.ShowDialog() == DialogResult.OK)
            {
                float radius_ratio = 0.95f;
                if (exporting_size.Height == 0) radius_ratio = 0.85f;
                else if (exporting_size.Height == 794) radius_ratio = 0.98f;
                else if (exporting_size.Height == 600) radius_ratio = 0.98f;
                zoomFactor = 1;
                ReDraw(exporting_size.Width, exporting_size.Height, radius_ratio);
                zoomFactor = aux;
                System.IO.File.WriteAllText(SFD.FileName, Graficador.GetSVG(CurrentFile.Options));
                ReDraw(0, 0);
            }
        }

        private void asSeenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exporting_size = new Size(0, 0);
            uncheck_all();
            asSeenToolStripMenuItem.Checked = true;
        }

        private void largeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exporting_size = new Size(794, 794);
            uncheck_all();
            largeToolStripMenuItem.Checked = true;   
        }

        private void mediumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exporting_size = new Size(600, 600);
            uncheck_all();
            mediumToolStripMenuItem.Checked = true;
        }

        private void a4FullWidthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exporting_size = new Size(602, 373);
            uncheck_all();
            a4FullWidthToolStripMenuItem.Checked = true;
        }

        private void a4HalfWidthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exporting_size = new Size(295, 295);
            uncheck_all();
            a4HalfWidthToolStripMenuItem.Checked = true;
        }

        private void customToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogControl DC = new DialogControl(DefaultText: Properties.Settings.Default.CustomExportWidth.ToString() + "x" + Properties.Settings.Default.CustomExportHeight.ToString());
            int newint;
            if (DC.ShowDialog() == DialogResult.OK)
            {
                string[] value = DC.getResult().Replace(" ","").Split('x');
                if (value.Length != 2) return;
                if (!int.TryParse(value[0], out newint)) return;
                if (!int.TryParse(value[1], out newint)) return;
                exporting_size = new Size(int.Parse(value[0]),int.Parse(value[1]));
                uncheck_all();
                customToolStripMenuItem.Checked = true;
                customToolStripMenuItem.Text = "Custom (" + value[0] + "x" + value[1] + ")";

                Properties.Settings.Default.CustomExportWidth = exporting_size.Width;
                Properties.Settings.Default.CustomExportHeight = exporting_size.Height;
                Properties.Settings.Default.Save();
            }
        }

        private void uncheck_all()
        {
            asSeenToolStripMenuItem.Checked = false;
            largeToolStripMenuItem.Checked = false;
            mediumToolStripMenuItem.Checked = false;
            a4FullWidthToolStripMenuItem.Checked = false;
            a4HalfWidthToolStripMenuItem.Checked = false;
            customToolStripMenuItem.Checked = false;
        }

      
        //
        // QUICKEDIT BUTTON
        private void QuickEditButtonClick()
        {
            if (quickedit == true)
            {
                toolStripButton2.BackColor = SystemColors.Window;
                quickedit = false;
            }
            else
            {
                toolStripButton2.BackColor = SystemColors.Highlight;
                quickedit = true;
            }
        }

        private void StripLabelC_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            QuickEditButtonClick();
        }
    }
}
