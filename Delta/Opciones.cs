using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
    
namespace Delta
{
    public enum DispStyle { Equilateral, Rectangle}
    public enum CompoundTxtLocation { Top, Side, Hidden};
    public enum GridStyle { Solid, Dashed, Dotted, Hidden};
    public enum BackgndStyle { Color, Picture}

    public class Opciones
    {
        public DispStyle DisplayStyle;
        
        public int GridColor = Color.LightGray.ToArgb();
        public GridStyle GridStyle = GridStyle.Dotted;
        public int GridSpacing = 10;
        public bool TickVisible = true;

        public bool LegendVisible = true;
        public System.Drawing.Point LegendLocation = new Point(20,50);

        public String CompoundA = "A";
        public String CompoundB = "B";
        public String CompoundC = "C";
        public CompoundTxtLocation CompoundTextLocation = CompoundTxtLocation.Top;

        public string FontFamily = "Times New Roman";
        public int FontSize = 10;

        //public System.Drawing.Font Font = new Font("Times New Roman",10);

        public int BackgroundColor = Color.White.ToArgb();
        //public BackgndStyle BackgroundStyle = BackgndStyle.Transparent;
        public string BackgroundPicture;
        
        public Font GetDefaultFont()
        {
            System.Drawing.Text.PrivateFontCollection privateFont = new System.Drawing.Text.PrivateFontCollection();
            byte[] FontBytes = Properties.Resources.cmunrm;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(FontBytes.Length);
            System.Runtime.InteropServices.Marshal.Copy(FontBytes, 0, ptr, FontBytes.Length);

            privateFont.AddMemoryFont(ptr, FontBytes.Length);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr);

            return new Font(privateFont.Families[0], 10, FontStyle.Regular);
        }

        public Opciones Clone()
        {
            Opciones clonedOptions = new Opciones();

            clonedOptions.DisplayStyle = DisplayStyle;
            clonedOptions.GridColor = GridColor;
            clonedOptions.GridStyle = GridStyle;
            clonedOptions.GridSpacing = GridSpacing;
            clonedOptions.TickVisible = TickVisible;
            clonedOptions.LegendVisible = LegendVisible;
            clonedOptions.LegendLocation = LegendLocation;
            clonedOptions.CompoundA = CompoundA;
            clonedOptions.CompoundB = CompoundB;
            clonedOptions.CompoundC = CompoundC;
            clonedOptions.CompoundTextLocation = CompoundTextLocation;
            clonedOptions.FontFamily = FontFamily;
            clonedOptions.FontSize = FontSize;
            //clonedOptions.Font = Font;
            clonedOptions.BackgroundColor = BackgroundColor;
            clonedOptions.BackgroundPicture = BackgroundPicture;

            return clonedOptions;
        }

    

    }
}
