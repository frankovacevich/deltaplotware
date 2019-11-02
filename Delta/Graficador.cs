using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Delta
{
    public class Graficador
    {
        double sqrt3 = 1.7320508075689;
        double sqrt2 = 1.4142135623731;

        double R;
        int H, W;

        public string svg_context = "";

        Bitmap BMP;
        Graphics G;
        public List<PointF> ReferencePoints = new List<PointF>();

        private PointF X2Veq(PointF X)
        {
            return new PointF(
                Convert.ToSingle((H/3+R*7/15-2*X.Y/3)/R),
                Convert.ToSingle((X.X*sqrt3/3-sqrt3*W/6+X.Y/3-H/6-R*7/30)/R+0.5)
                );
        }

        private PointF X2Vrt(PointF X)
        {
            return new PointF(
                Convert.ToSingle( - (- Convert.ToDouble(H) / 2 + X.Y) / (sqrt2 * R) + 0.5),
                Convert.ToSingle((- Convert.ToDouble(W) / 2 + X.X) / (sqrt2 * R) + 0.5)
                );
        }

        private double getR(float radius_ratio = 0.85f)
        {
            return radius_ratio * 0.5 * Convert.ToDouble(((H > W) ? W : H));
        }

        public PointF GetScreenPoint(PointF X, Opciones options)
        {
            if (options.DisplayStyle == DispStyle.Equilateral) return X2Veq(X);
            if (options.DisplayStyle == DispStyle.Rectangle) return X2Vrt(X);

            return new Point(0,0);
        }

        PointF V2Xeq(PointF V) => new PointF(
            Convert.ToSingle((0.5 * sqrt3 * V.X + sqrt3 * V.Y - sqrt3 * 0.5) * R + Convert.ToDouble(W) * 0.5),
            Convert.ToSingle(-0.5 * (3 * V.X - 1) * R + Convert.ToDouble(H) * 0.5 + 0.2 * R));

        PointF V2Xrt(PointF V) => new PointF(
            Convert.ToSingle((sqrt2 * V.Y - 0.5 * sqrt2) * R + 0.5 * Convert.ToDouble(W)),
            Convert.ToSingle(-(sqrt2 * V.X - 0.5 * sqrt2) * R + 0.5 * Convert.ToDouble(H)));


        ///DRAW
        public Bitmap Draw(List<Serie> Series, Opciones Options, int w, int h, float radius_ratio = 0.85f)
        {
            H = h;
            W = w;
            R = getR(radius_ratio);

            Func<PointF, PointF> convert = null;
            if (Options.DisplayStyle == DispStyle.Equilateral)
                convert = V2Xeq;

            if (Options.DisplayStyle == DispStyle.Rectangle)
                convert = V2Xrt;

            BMP = new Bitmap(W, H);
            G = Graphics.FromImage(BMP);

            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            svg_context = "";

            DrawBackground(convert, Options);
            DrawWaterMark();
            DrawWhiteMargins(convert, Options);
            DrawGrid(convert, Options);
            DrawTriangle(convert, Options);
            DrawTicks(convert, Options);

            foreach (Serie S in Series)
            {
                DrawSerie(convert, S, Options);
            }

            int j = 0;
            foreach (Serie S in Series)
            { 
                if (Options.LegendVisible)
                    if (S.displayInLeyend)
                         DrawLegend(S,convert,Options, j++);
            }

            
            return BMP;            

        }

        private void DrawTriangle(Func<PointF,PointF> convert, Opciones Options)
        {
            PointF A = convert(new PointF(1, 0));
            PointF B = convert(new PointF(0, 1));
            PointF C = convert(new PointF(0, 0));

            svg_context += LineSVG(A, B, Color.Black, 2);
            svg_context += LineSVG(B, C, Color.Black, 2);
            svg_context += LineSVG(C, A, Color.Black, 2);

            Pen BlackPen = new Pen(Color.Black, 2);
            G.DrawLine(BlackPen, A, B);
            G.DrawLine(BlackPen, B, C);
            G.DrawLine(BlackPen, C, A);

            StringFormat format = new StringFormat();
            Brush defaultBrush = new SolidBrush(Color.Black);
            Font optionsFont = new Font(Options.FontFamily, Options.FontSize);
            float stringHeight = G.MeasureString("DUMMY", optionsFont).Height;

            if (Options.CompoundTextLocation == CompoundTxtLocation.Hidden) return;
            else if (Options.CompoundTextLocation == CompoundTxtLocation.Top)
            {
                format.Alignment = StringAlignment.Center;
                G.DrawString(Options.CompoundA, optionsFont, defaultBrush, new PointF(A.X, A.Y - 10 - stringHeight), format);
                svg_context += TextSVG(Options.CompoundA, new PointF(A.X, A.Y - 10 - stringHeight*0.25f), optionsFont, Color.Black, 0);

                format.Alignment = StringAlignment.Near;
                G.DrawString(Options.CompoundB, optionsFont, defaultBrush, new PointF(B.X + 10, B.Y + 5), format);
                svg_context += TextSVG(Options.CompoundB, new PointF(B.X + 10, B.Y + 5 + stringHeight * 0.75f), optionsFont, Color.Black, -1);

                format.Alignment = StringAlignment.Far;
                G.DrawString(Options.CompoundC, optionsFont, defaultBrush, new PointF(C.X - 10, C.Y + 5), format);
                svg_context += TextSVG(Options.CompoundC, new PointF(C.X - 15, C.Y + 5 + stringHeight * 0.75f), optionsFont, Color.Black, 1);

            }
            else
            {
                int TickEcart = (Options.TickVisible == true) ? ((int) (stringHeight*1.4f)) : ((int)(stringHeight * 0.1f));

                if (Options.DisplayStyle == DispStyle.Equilateral)
                {
                    format.Alignment = StringAlignment.Center;
                    System.Drawing.Drawing2D.GraphicsState GS = G.Save();
                    G.RotateTransform(60);
                    G.TranslateTransform((A.X + B.X) * 0.5f + 35 + TickEcart, (A.Y + B.Y) * 0.5f - TickEcart, System.Drawing.Drawing2D.MatrixOrder.Append);
                    G.DrawString(Options.CompoundA, optionsFont, defaultBrush, new PointF(0, 0), format);
                    G.Restore(GS);
                    svg_context += TextSVG(Options.CompoundA, new PointF((A.X + B.X) * 0.5f + 25 + TickEcart, (A.Y + B.Y) * 0.5f - 10 - TickEcart + stringHeight), optionsFont, Color.Black,0,60);

                    GS = G.Save();
                    G.RotateTransform(-60);
                    G.TranslateTransform((A.X + C.X) * 0.5f - 35 - TickEcart, (A.Y + C.Y) * 0.5f - TickEcart, System.Drawing.Drawing2D.MatrixOrder.Append);
                    G.DrawString(Options.CompoundC, optionsFont, defaultBrush, new PointF(0, 0), format);
                    G.Restore(GS);
                    svg_context += TextSVG(Options.CompoundC, new PointF((A.X + C.X) * 0.5f - 25 - TickEcart, (A.Y + C.Y) * 0.5f - 10 - TickEcart + stringHeight), optionsFont, Color.Black, 0, -60);
                    
                    G.DrawString(Options.CompoundB, optionsFont, defaultBrush, new PointF((B.X + C.X) * 0.5f, C.Y + 10 + TickEcart), format);
                    svg_context += TextSVG(Options.CompoundB, new PointF((B.X + C.X) * 0.5f, C.Y + 10 + TickEcart + stringHeight*0.75f), optionsFont, Color.Black, 0);

                }
                else if(Options.DisplayStyle == DispStyle.Rectangle)
                {
                    format.Alignment = StringAlignment.Center;
                    System.Drawing.Drawing2D.GraphicsState GS = G.Save();
                    G.RotateTransform(-90);
                    G.TranslateTransform(A.X - 25 - TickEcart * 1.5f, (A.Y + C.Y) * 0.5f, System.Drawing.Drawing2D.MatrixOrder.Append);
                    G.DrawString(Options.CompoundA, optionsFont, defaultBrush, new PointF(0, 0), format);
                    G.Restore(GS);
                    svg_context += TextSVG(Options.CompoundA, new PointF(A.X - 25 - TickEcart * 1.5f + stringHeight * 0.75f, (A.Y + C.Y) * 0.5f), optionsFont, Color.Black, 0, -90);


                    /*GS = G.Save();
                    G.RotateTransform(45);
                    G.TranslateTransform((A.X + B.X) * 0.5f + 20 + TickEcart, (A.Y + B.Y) * 0.5f - 20 - TickEcart, System.Drawing.Drawing2D.MatrixOrder.Append);
                    G.DrawString(Options.CompoundC, optionsFont, defaultBrush, new PointF(0, 0), format);
                    G.Restore(GS);
                    svg_context += TextSVG(Options.CompoundC, new PointF((A.X + B.X) * 0.5f + 20 + TickEcart - stringHeight * 0.5f, (A.Y + B.Y) * 0.5f - 20 - TickEcart + stringHeight * 0.75f), optionsFont, Color.Black, 0, 45);
                    */

                    G.DrawString(Options.CompoundB, optionsFont, defaultBrush, new PointF((B.X + C.X) * 0.5f, C.Y + 10 + TickEcart), format);
                    svg_context += TextSVG(Options.CompoundB, new PointF((B.X + C.X) * 0.5f, C.Y + 30 + TickEcart - stringHeight * 0.5f), optionsFont, Color.Black, 0);

                }
            }
        }

        private void DrawGrid(Func<PointF,PointF> convert, Opciones options)
        {
            Color gridColor = Color.FromArgb(options.GridColor);
            Pen GridPen = new Pen(gridColor, 1);
            if(options.GridStyle == GridStyle.Dashed) GridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            if (options.GridStyle == GridStyle.Dotted) GridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            if (options.GridStyle == GridStyle.Solid) GridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
            if (options.GridStyle == GridStyle.Hidden) return;
   
            int i;
            for (i = 1; i < options.GridSpacing; i++)
            {
                float l = Convert.ToSingle(i) / Convert.ToSingle(options.GridSpacing);

                G.DrawLine(GridPen, convert(new PointF(0, 1 - l)), convert(new PointF(l, 1 - l)));
                G.DrawLine(GridPen, convert(new PointF(l, 1 - l)), convert(new PointF(l, 0)));
                svg_context += LineSVG(convert(new PointF(0, 1 - l)), convert(new PointF(l, 1 - l)), gridColor, 1, (int) options.GridStyle);
                svg_context += LineSVG(convert(new PointF(l, 1 - l)), convert(new PointF(l, 0)), gridColor, 1, (int)options.GridStyle);


                if (options.DisplayStyle == DispStyle.Equilateral)
                {
                    G.DrawLine(GridPen, convert(new PointF(l, 0)), convert(new PointF(0, l)));
                    svg_context += LineSVG(convert(new PointF(l, 0)), convert(new PointF(0, l)), gridColor, 1, (int)options.GridStyle);
                }
                if (options.DisplayStyle == DispStyle.Rectangle)
                {
                    /*G.DrawLine(GridPen, convert(new PointF(l, 1-l)), convert( (1-2*l >=0) ? new PointF(0, 1-2*l) : new PointF(2 * l - 1,0)));
                    svg_context += LineSVG(convert(new PointF(l, 1 - l)), convert((1 - 2 * l >= 0) ? new PointF(0, 1 - 2 * l) : new PointF(2 * l - 1, 0)), gridColor, 1, (int)options.GridStyle);
                    */
                }
                
                

            }
        }

        private void DrawTicks(Func<PointF, PointF> convert, Opciones options)
        {
            Pen BlackPen = new Pen(Color.Black, 2);
            Font optionsFont = new Font(options.FontFamily, options.FontSize);
            float stringHeight = G.MeasureString("DUMMY", optionsFont).Height;
            double k = 6; //longitud del tick = 2k
            StringFormat format = new StringFormat();
            if (options.TickVisible == false) return;


            int i;
            for (i = 1; i < options.GridSpacing; i++)
            {
                float l = Convert.ToSingle(i) / Convert.ToSingle(options.GridSpacing);

                if (options.DisplayStyle == DispStyle.Equilateral) {
                    PointF P1 = convert(new PointF(1 - l, 0));
                    PointF P2 = convert(new PointF(0, l));
                    PointF P3 = convert(new PointF(l, 1 - l));

                    G.DrawLine(BlackPen, P1, new PointF(Convert.ToSingle(P1.X - k), Convert.ToSingle(P1.Y - sqrt3 * k)));
                    svg_context += LineSVG(P1, new PointF(Convert.ToSingle(P1.X - k), Convert.ToSingle(P1.Y - sqrt3 * k)), Color.Black, 2);
                    G.DrawLine(BlackPen, P2, new PointF(Convert.ToSingle(P2.X - k), Convert.ToSingle(P2.Y + sqrt3 * k)));
                    svg_context += LineSVG(P2, new PointF(Convert.ToSingle(P2.X - k), Convert.ToSingle(P2.Y + sqrt3 * k)), Color.Black, 2);
                    G.DrawLine(BlackPen, P3, new PointF(Convert.ToSingle(P3.X + 2 * k), Convert.ToSingle(P3.Y)));
                    svg_context += LineSVG(P3, new PointF(Convert.ToSingle(P3.X + 2 * k), Convert.ToSingle(P3.Y)), Color.Black, 2);
                    
                    format.Alignment = StringAlignment.Far;
                    G.DrawString(Math.Round(l, 2).ToString(), optionsFont, new SolidBrush(Color.Black), new PointF(Convert.ToSingle(P1.X - k), Convert.ToSingle(P1.Y - sqrt3 * k) - stringHeight * 0.5f), format);
                    svg_context += TextSVG(Math.Round(l, 2).ToString(), new PointF(Convert.ToSingle(P1.X - k - 2), Convert.ToSingle(P1.Y - sqrt3 * k) + stringHeight * 0.25f), optionsFont, Color.Black, 1, 0);

                    format.Alignment = StringAlignment.Center;
                    G.DrawString(Math.Round(l, 2).ToString(), optionsFont, new SolidBrush(Color.Black), new PointF(Convert.ToSingle(P2.X - k), Convert.ToSingle(P2.Y + sqrt3 * k) + 1), format);
                    svg_context += TextSVG(Math.Round(l, 2).ToString(), new PointF(Convert.ToSingle(P2.X - k), Convert.ToSingle(P2.Y + sqrt3 * k) + stringHeight * 0.75f), optionsFont, Color.Black, 0, 0);

                    format.Alignment = StringAlignment.Near;
                    G.DrawString(Math.Round(l, 2).ToString(), optionsFont, new SolidBrush(Color.Black), new PointF(Convert.ToSingle(P3.X + 2 * k) + 1, Convert.ToSingle(P3.Y) - stringHeight * 0.5f), format);
                    svg_context += TextSVG(Math.Round(l, 2).ToString(), new PointF(Convert.ToSingle(P3.X + 2 * k) + 3, Convert.ToSingle(P3.Y) + stringHeight*0.25f), optionsFont, Color.Black, -1, 0);

                }
                if (options.DisplayStyle == DispStyle.Rectangle)
                {
                    PointF P1 = convert(new PointF(1 - l, 0));
                    PointF P2 = convert(new PointF(0, l));
                    PointF P3 = convert(new PointF(l, 1 - l));

                    G.DrawLine(BlackPen, P1, new PointF(Convert.ToSingle(P1.X - 2 * k), Convert.ToSingle(P1.Y )));
                    svg_context += LineSVG(P1, new PointF(Convert.ToSingle(P1.X - 2 * k), Convert.ToSingle(P1.Y)), Color.Black, 2);
                    G.DrawLine(BlackPen, P2, new PointF(Convert.ToSingle(P2.X ), Convert.ToSingle(P2.Y + 2 * k)));
                    svg_context += LineSVG(P2, new PointF(Convert.ToSingle(P2.X), Convert.ToSingle(P2.Y + 2 * k)), Color.Black, 2);
                    /*G.DrawLine(BlackPen, P3, new PointF(Convert.ToSingle(P3.X + sqrt2 * k), Convert.ToSingle(P3.Y - sqrt2 * k)));
                    svg_context += LineSVG(P3, new PointF(Convert.ToSingle(P3.X + sqrt2 * k), Convert.ToSingle(P3.Y - sqrt2 * k)), Color.Black, 2);*/

                    format.Alignment = StringAlignment.Far;
                    G.DrawString(Math.Round(1-l, 2).ToString(), optionsFont, new SolidBrush(Color.Black), new PointF(Convert.ToSingle(P1.X - 2 * k), Convert.ToSingle(P1.Y) - 8), format);
                    svg_context += TextSVG(Math.Round(1 - l, 2).ToString(), new PointF(Convert.ToSingle(P1.X - 2 * k - 5), Convert.ToSingle(P1.Y)+4), optionsFont, Color.Black, 1, 0);

                    format.Alignment = StringAlignment.Center;
                    G.DrawString(Math.Round(l, 2).ToString(), optionsFont, new SolidBrush(Color.Black), new PointF(Convert.ToSingle(P2.X), Convert.ToSingle(P2.Y + 2 * k)+1), format);
                    svg_context += TextSVG(Math.Round(l, 2).ToString(), new PointF(Convert.ToSingle(P2.X), Convert.ToSingle(P2.Y + 2 * k) + 12), optionsFont, Color.Black, 0, 0);

                    /*format.Alignment = StringAlignment.Near;
                    G.DrawString(Math.Round(l, 2).ToString(), optionsFont, new SolidBrush(Color.Black), new PointF(Convert.ToSingle(P3.X + sqrt2 * k) + 1, Convert.ToSingle(P3.Y - sqrt2 * k) - 15), format);
                    svg_context += TextSVG(Math.Round(l, 2).ToString(), new PointF(Convert.ToSingle(P3.X + sqrt2 * k) + 1, Convert.ToSingle(P3.Y - sqrt2 * k) - 3), optionsFont, Color.Black, -1, 0);
                    */
                }
            }
        }

        private void DrawWaterMark()
        {
            if (Properties.Settings.Default.FullMode == true) return;
            Image img = Properties.Resources.text7599;
            G.DrawImage(img, new Point((W-img.Width)/2, (H - img.Height) / 2));

        }

        private void DrawLegend(Serie S, Func<PointF, PointF> convert, Opciones Options, int j)
        {
            try
            {
                Pen defaultPen = new Pen(Color.FromArgb(S.color), S.size);
                Font optionsFont = new Font(Options.FontFamily, Options.FontSize);
                float stringHeight = G.MeasureString("DUMMY", optionsFont).Height;

                PointF A = convert(new PointF(1, 0));
                float lengedYlocation = A.Y - 10 - stringHeight;

                int ancho_punto = (int)(stringHeight * 1.5f);
                int alto_punto = (int)(stringHeight);
                int X = Options.LegendLocation.X;
                int Y = (int)(lengedYlocation + stringHeight * j + stringHeight * 0.2f);

                if (S.type == Types.point)
                {
                    if (S.style != Style.Text)
                        DrawPoint(S, new Point((int)(X + 0.5 * ancho_punto), (int)(Y + 0.5 * alto_punto)), Options, 5);
                    else
                    {
                        StringFormat format = new StringFormat();
                        format.Alignment = StringAlignment.Center;
                        G.DrawString("Tx", optionsFont, new SolidBrush(Color.FromArgb(S.color)), new PointF(X + ancho_punto * 0.5f, Y), format);
                        svg_context += TextSVG("Tx", new PointF(X + ancho_punto * 0.5f, Y + stringHeight * 0.75f), optionsFont, Color.FromArgb(S.color), 0);
                    }
                }
                else if (S.type == Types.spline || S.type == Types.polyline || S.type == Types.segments)
                {
                    if (S.style == Style.Line_dash) defaultPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    if (S.style == Style.Line_dot) defaultPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    if (S.style == Style.Line_dashdot) defaultPen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                    G.DrawLine(defaultPen, X, (int)(Y + 0.5 * alto_punto), X + ancho_punto, (int)(Y + 0.5 * alto_punto));
                    string arrayOfPoints = X.ToString() + "," + (Y + 0.5 * alto_punto).ToString() + " " + (X + ancho_punto).ToString() + "," + (Y + 0.5 * alto_punto).ToString();
                    svg_context += "<polyline points=\"" + arrayOfPoints + "\" " + SVGDashArray[(int)(S.style - 20)] + " fill =\"none\" stroke=" + ColorSVG(Color.FromArgb(S.color)) + " stroke-width=\"" + S.size.ToString() + "\"/>\n";
                }
                else
                {
                    G.FillRectangle(getBrushForArea(S), X, Y, ancho_punto, alto_punto * 0.9f);
                    string FillingSVG = getFillForAreaSVG(S);
                    string arrayOfPoints = X.ToString() + "," + Y.ToString() + " " + (X + ancho_punto).ToString() + "," + Y.ToString() + " " + (X + ancho_punto).ToString() + "," + (Y + alto_punto).ToString() + " " + X.ToString() + "," + (Y + alto_punto).ToString();
                    svg_context += "<polygon points=\"" + arrayOfPoints + "\" stroke=\"none\" " + FillingSVG + "/>\n";
                }

                G.DrawString(S.name, optionsFont, new SolidBrush(Color.Black), X + ancho_punto + 15, Y);
                svg_context += TextSVG(S.Name, new PointF(X + ancho_punto + 17, Y + stringHeight * 0.75f), optionsFont, Color.Black, -1);

            } catch(Exception ex)
            {

            }
        }

        private void DrawSerie(Func<PointF, PointF> convert, Serie serie, Opciones options)
        {
            try
            {
                if (serie.visible == false) return;
                if (serie.Points.Count == 0) return;

                Pen defaultPen = new Pen(Color.FromArgb(serie.color), serie.size);
                if (serie.style == Style.Line_dash) defaultPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                if (serie.style == Style.Line_dot) defaultPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                if (serie.style == Style.Line_dashdot) defaultPen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;

                PointF[] ArrayOfPoints = new PointF[serie.Points.Count];
                string SVGArrayOfPoints = "";
                if (serie.type != Types.point)
                {
                    for (int i = 0; i < serie.Points.Count; i++)
                    {
                        PointF PF = convert(serie.Points[i]);
                        ArrayOfPoints[i] = PF;
                        SVGArrayOfPoints += PF.X + "," + PF.Y + " ";

                    }
                }


                //POINT
                if (serie.type == Types.point)
                {
                    foreach (PointF P in serie.Points)
                    {
                        DrawPoint(serie, convert(P), options);
                    }
                }

                //LINE OR CURVE
                else if (serie.type == Types.polyline)
                {
                    if (serie.Points.Count < 2) return;
                    G.DrawLines(defaultPen, ArrayOfPoints);
                    svg_context += "<polyline points=\"" + SVGArrayOfPoints + "\" " + SVGDashArray[(int)(serie.style - 20)] + " fill =\"none\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"" + serie.size.ToString() + "\"/>\n";
                }
                else if (serie.type == Types.spline)
                {
                    if (serie.Points.Count < 2) return;
                    G.DrawCurve(defaultPen, ArrayOfPoints);

                    string[] svgcode = { "" };
                    serie.CalculatePolynomialSpline(serie.Points, convert, svgcode);

                    svg_context += "<path d=\"" + svgcode[0] + "\" " + SVGDashArray[(int)(serie.style - 20)] + " fill =\"none\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"" + serie.size.ToString() + "\"/>\n";
                }
                else if (serie.type == Types.segments)
                {
                    if (serie.Points.Count < 2) return;
                    for (int i = 0; i < serie.Points.Count - 1; i += 2)
                    {
                        G.DrawLine(defaultPen, convert(serie.Points[i]), convert(serie.Points[i + 1]));
                        svg_context += LineSVG(convert(serie.Points[i]), convert(serie.Points[i + 1]), Color.FromArgb(serie.color), serie.size, (int)(serie.style - 20));
                    }
                }

                //AREA
                else if (serie.type == Types.area || serie.type == Types.polygon)
                {
                    if (serie.Points.Count < 3) return;
                    Brush defaultBrush = getBrushForArea(serie);
                    string FillingSVG = getFillForAreaSVG(serie);

                    if (serie.type == Types.polygon)
                    {
                        G.FillPolygon(defaultBrush, ArrayOfPoints);
                        svg_context += "<polygon points=\"" + SVGArrayOfPoints + "\" stroke=\"none\" " + FillingSVG + "/>\n";
                    }
                    if (serie.type == Types.area)
                    {
                        G.FillClosedCurve(defaultBrush, ArrayOfPoints);

                        string[] svgcode = { "" };
                        List<PointF> auxList = new List<PointF>();
                        auxList.AddRange(serie.Points);
                        auxList.Add(serie.Points[0]);
                        serie.CalculatePolynomialSpline(auxList, convert, svgcode, true);

                        svg_context += "<path d=\"" + svgcode[0] + " z\" stroke=\"none\" " + FillingSVG + "/>\n";
                    }
                    defaultBrush.Dispose();
                }

                defaultPen.Dispose();

                if (serie.type != Types.point)
                {
                    DrawWhiteMargins(convert, options);
                    DrawTriangle(convert, options);
                    DrawTicks(convert, options);
                }
            } catch (Exception ex)
            {

            }
        }

        private void DrawWhiteMargins(Func<PointF, PointF> convert, Opciones Options)
        {
            //Fills the whole page with white, except for the triangle
            //To do that it draws a polygon inside the points A, (0,0), (W,0), (W,H), B -> Right
            //                                                A, (0,0), (0,H), C        -> Left
            //                                                C, (0,H), (W,H), B        -> Bottom

            PointF A = convert(new PointF(1, 0));
            PointF B = convert(new PointF(0, 1));
            PointF C = convert(new PointF(0, 0));

            PointF[] PolygonRight = { A, new PointF(0, 0), new PointF(W, 0), new PointF(W, H), B, A };
            PointF[] PolygonLeft = { A, new PointF(0, 0), new PointF(0, H), C , A };
            PointF[] PolygonBottom = { C, new PointF(0, H), new PointF(W, H), B, C };
            string SVGArrayOfPointsRight = "";
            string SVGArrayOfPointsLeft = "";
            string SVGArrayOfPointsBottom = "";

            foreach (PointF PF in PolygonRight) SVGArrayOfPointsRight += PF.X + "," + PF.Y + " ";
            foreach (PointF PF in PolygonLeft) SVGArrayOfPointsLeft += PF.X + "," + PF.Y + " ";
            foreach (PointF PF in PolygonBottom) SVGArrayOfPointsBottom += PF.X + "," + PF.Y + " ";
        
            String FillingSVG = " fill=" + ColorSVG(Color.FromArgb(Options.BackgroundColor));
            svg_context += "<polygon points=\"" + SVGArrayOfPointsRight + "\" stroke=\"none\" " + FillingSVG + "/>\n";
            svg_context += "<polygon points=\"" + SVGArrayOfPointsLeft + "\" stroke=\"none\" " + FillingSVG + "/>\n";
            svg_context += "<polygon points=\"" + SVGArrayOfPointsBottom + "\" stroke=\"none\" " + FillingSVG + "/>\n";

            Brush WhiteBrush = new SolidBrush(Color.FromArgb(Options.BackgroundColor));
            G.FillPolygon(WhiteBrush, PolygonRight);
            G.FillPolygon(WhiteBrush, PolygonLeft);
            G.FillPolygon(WhiteBrush, PolygonBottom);
        }

        private void DrawBackground(Func<PointF, PointF> convert, Opciones Options)
        {
            G.FillRectangle(new SolidBrush(Color.FromArgb(Options.BackgroundColor)), 0, 0, W, H);
            string arrayOfPoints = "0,0 " + W.ToString() + ",0 " + W.ToString() + "," + H.ToString() + " 0," + H.ToString();
            svg_context += "<polygon points=\"" + arrayOfPoints + "\" stroke=\"none\" fill=" + ColorSVG(Color.FromArgb(Options.BackgroundColor)) + " />\n";

            PointF A = convert(new PointF(1, 0));
            PointF B = convert(new PointF(0, 1));
            PointF C = convert(new PointF(0, 0));

            if (Options.BackgroundPicture != null)
            {
                if (File.Exists(Options.BackgroundPicture))
                {
                    Image image = Image.FromFile(Options.BackgroundPicture);
                    G.DrawImage(image, new RectangleF(C.X, A.Y, B.X - C.X, C.Y - A.Y));
                    svg_context += "<image xlink:href=\"" + AppDomain.CurrentDomain.BaseDirectory + "/" + Options.BackgroundPicture + "\" x =\"" + C.X.ToString() + "\" y=\"" + A.Y.ToString() +"\" height=\"" + (C.Y - A.Y).ToString() + "\" width=\"" + (B.X - C.X).ToString() + "\"/>\n";
                    DrawWhiteMargins(convert, Options);
                }
            }
        }

        private Brush getBrushForArea(Serie serie)
        {
            if (serie.style == Style.Area_Solid)
            {
                return new SolidBrush(Color.FromArgb((int)(255 * (serie.size - 1) / 9), Color.FromArgb(serie.color).R, Color.FromArgb(serie.color).G, Color.FromArgb(serie.color).B));
            }
            else if (serie.style == Style.Area_Hatch1)
            {
                return new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.BackwardDiagonal, Color.FromArgb(serie.color), Color.FromArgb((int)(255 * (serie.size - 1) / 9), 255, 255, 255));
            }
            else if (serie.style == Style.Area_Hatch2)
            {
                return new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.ForwardDiagonal, Color.FromArgb(serie.color), Color.FromArgb((int)(255 * (serie.size-1) / 9), 255, 255, 255));
            }
            else if (serie.style == Style.Area_Hatch3)
            {
                return new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Cross, Color.FromArgb(serie.color), Color.FromArgb((int)(255 * (serie.size - 1) / 9), 255, 255, 255));
            }
            else if (serie.style == Style.Area_Hatch4)
            {
                return new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.DiagonalCross, Color.FromArgb(serie.color), Color.FromArgb((int)(255 * (serie.size - 1) / 9), 255, 255, 255));
            }
            else if (serie.style == Style.Area_Hatch5)
            {
                return new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Vertical, Color.FromArgb(serie.color), Color.FromArgb((int)(255 * (serie.size - 1) / 9), 255, 255, 255));
            }
            else if (serie.style == Style.Area_Hatch6)
            {
                return new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.Horizontal, Color.FromArgb(serie.color), Color.FromArgb((int)(255 * (serie.size - 1) / 9), 255, 255, 255));
            }

            return null;
        }

        private string getFillForAreaSVG(Serie serie)
        {
            if (serie.style == Style.Area_Solid)
            {
                return " fill = " + ColorSVG(Color.FromArgb(serie.color)) + " fill-opacity=\"" + ((float) serie.size - 1) /9 + "\" ";
            }
            else if (serie.style == Style.Area_Hatch1) 
            {
                svg_context += "<pattern id =\"Hatch" + serie.Id + "\" width=\"6\" height=\"6\" patternTransform=\"rotate(45 0 0)\" patternUnits=\"userSpaceOnUse\"><rect x=\"0\" y=\"0\" width=\"6\" height=\"6\" fill=\"white\" fill-opacity=\"" + ((float)serie.size - 1) / 9 + "\" stroke=\"none\"/><line x1=\"0\" y1=\"0\" x2=\"0\" y2=\"6\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"2\" fill=\"black\"/> </pattern>\n";
            }
            else if (serie.style == Style.Area_Hatch2)
            {
                svg_context += "<pattern id =\"Hatch" + serie.Id + "\" width=\"6\" height=\"6\" patternTransform=\"rotate(-45 0 0)\" patternUnits=\"userSpaceOnUse\"><rect x=\"0\" y=\"0\" width=\"6\" height=\"6\" fill=\"white\"  fill-opacity=\"" + ((float)serie.size - 1) / 9 + "\"  stroke=\"none\"/><line x1=\"0\" y1=\"0\" x2=\"0\" y2=\"6\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"2\" fill=\"black\"/> </pattern>\n";
            }
            else if (serie.style == Style.Area_Hatch3)
            {
                svg_context += "<pattern id =\"Hatch" + serie.Id + "\" width=\"7.5\" height=\"7.5\" patternUnits=\"userSpaceOnUse\"><rect x=\"0\" y=\"0\" width=\"7.5\" height=\"7.5\" fill=\"white\"  fill-opacity=\"" + ((float)serie.size - 1) / 9 + "\"  stroke=\"none\"/><line x1=\"0\" y1=\"3.75\" x2=\"7.5\" y2=\"3.75\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"2\" fill=\"black\"/><line x1=\"3.75\" y1=\"0\" x2=\"3.75\" y2=\"7.5\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"2\" fill=\"black\"/></pattern>\n";
            }
            else if (serie.style == Style.Area_Hatch4)
            {
                svg_context += "<pattern id =\"Hatch" + serie.Id + "\" width=\"8\" height=\"8\" patternUnits=\"userSpaceOnUse\"><rect x=\"0\" y=\"0\" width=\"8\" height=\"8\" fill=\"white\"  fill-opacity=\"" + ((float)serie.size - 1) / 9 + "\"  stroke=\"none\"/><line x1=\"0\" y1=\"0\" x2=\"8\" y2=\"8\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"2\" fill=\"black\"/><line x1=\"0\" y1=\"8\" x2=\"8\" y2=\"0\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"2\" fill=\"black\"/> </pattern>\n";
            }
            else if (serie.style == Style.Area_Hatch5)
            {
                svg_context += "<pattern id =\"Hatch" + serie.Id + "\" width=\"8\" height=\"8\" patternTransform=\"rotate(0 0 0)\" patternUnits=\"userSpaceOnUse\"><rect x=\"0\" y=\"0\" width=\"8\" height=\"8\" fill=\"white\"  fill-opacity=\"" + ((float)serie.size - 1) / 9 + "\" stroke=\"none\"/><line x1=\"0\" y1=\"0\" x2=\"0\" y2=\"8\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"2\" fill=\"black\"/> </pattern>\n";
            }
            else if (serie.style == Style.Area_Hatch6)
            {
                svg_context += "<pattern id =\"Hatch" + serie.Id + "\" width=\"8\" height=\"8\" patternTransform=\"rotate(90 0 0)\" patternUnits=\"userSpaceOnUse\"><rect x=\"0\" y=\"0\" width=\"8\" height=\"8\" fill=\"white\"  fill-opacity=\"" + ((float)serie.size - 1) / 9 + "\" stroke=\"none\"/><line x1=\"0\" y1=\"0\" x2=\"0\" y2=\"8\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"2\" fill=\"black\"/></pattern>\n";
            }
            return "fill = \"url(#Hatch" + serie.Id + ")\"";
        }

        private void DrawPoint(Serie serie, PointF point, Opciones options, int size = 0)
        {
            //In the X,Y absolute reference frame

            if (size == 0) size = serie.size;

            Font optionsFont = new Font(options.FontFamily, options.FontSize);
            Pen pointPen = new Pen(Color.FromArgb(serie.color), size / 4);
            SolidBrush pointBrush = new SolidBrush(Color.FromArgb(serie.color));
            StringFormat format = new StringFormat();
            Rectangle pointRect = Rectangle.FromLTRB((int)point.X - size, (int)point.Y - size, (int)point.X + size, (int)point.Y + size);

            if (serie.style == Style.circulo)
            {
                G.DrawEllipse(pointPen, pointRect);
                svg_context += "<circle cx=\"" + point.X + "\" cy=\"" + point.Y + "\" r=\"" + size + "\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"" + ((size / 4 > 0) ? size / 4 : 1) + "\" fill=\"" + "none" + "\"/>\n";
            } else if (serie.style == Style.circulo_fill)
            {
                G.FillEllipse(pointBrush, pointRect);
                svg_context += "<circle cx=\"" + point.X + "\" cy=\"" + point.Y + "\" r=\"" + size + "\" stroke=\"" + "none" + "\" stroke-width=\"" + 0 + "\" fill=" + ColorSVG(Color.FromArgb(serie.color)) + "/>\n";
            } else if (serie.style == Style.cuadrado)
            {
                G.DrawRectangle(pointPen, pointRect);
                svg_context += "<polygon points=\"" + ((int) pointRect.X) + "," + ((int) pointRect.Y) + " " + ((int)pointRect.X + 2*size) + "," + ((int)pointRect.Y) + " " + ((int)pointRect.X + 2*size) + "," + ((int)pointRect.Y +2* size) + " " + ((int)pointRect.X) + "," + ((int)pointRect.Y + 2*size) + "\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"" + ((size / 4 > 0) ? size / 4 : 1) + "\" fill=\"" + "none" + "\"/>\n";
            } else if (serie.style == Style.cuadrado_fill)
            {
                G.FillRectangle(pointBrush, pointRect);
                svg_context += "<polygon points=\"" + ((int)pointRect.X) + "," + ((int)pointRect.Y) + " " + ((int)pointRect.X + 2 * size) + "," + ((int)pointRect.Y) + " " + ((int)pointRect.X + 2 * size) + "," + ((int)pointRect.Y + 2 * size) + " " + ((int)pointRect.X) + "," + ((int)pointRect.Y + 2 * size) + "\" stroke=\"" + "none" + "\" stroke-width=\"" + 0 + "\" fill=" + ColorSVG(Color.FromArgb(serie.color)) + "/>\n";
            } else if(serie.style == Style.triangulo_up)
            {
                PointF[] corners = { new PointF(point.X, point.Y - size), new PointF(point.X + (float)(size * sqrt3 * 0.5), point.Y + (float)0.5 * size), new PointF(point.X - (float)(size * sqrt3 * 0.5), point.Y + (float)0.5 * size) };
                G.DrawPolygon(pointPen, corners);
                svg_context += "<polygon points=\"" + corners[0].X + "," + corners[0].Y + " " + corners[1].X + "," + corners[1].Y + " " + corners[2].X + "," + corners[2].Y + "\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"" + ((size / 4 > 0) ? size / 4 : 1) + "\" fill=\"" + "none" + "\"/>\n";
            } else if(serie.style == Style.triangulo_up_fill)
            {
                PointF[] corners = { new PointF(point.X, point.Y - size), new PointF(point.X + (float)(size * sqrt3 * 0.5), point.Y + (float)0.5 * size), new PointF(point.X - (float)(size * sqrt3 * 0.5), point.Y + (float)0.5 * size) };
                G.FillPolygon(pointBrush, corners);
                svg_context += "<polygon points=\"" + corners[0].X + "," + corners[0].Y + " " + corners[1].X + "," + corners[1].Y + " " + corners[2].X + "," + corners[2].Y + "\" stroke=\"" + "none" + "\" stroke-width=\"" + 0 + "\" fill=" + ColorSVG(Color.FromArgb(serie.color)) + "/>\n";
            } else if(serie.style == Style.triangulo_down)
            {
                PointF[] corners = { new PointF(point.X, point.Y + size), new PointF(point.X + (float)(size * sqrt3 * 0.5), point.Y - (float)0.5 * size), new PointF(point.X - (float)(size * sqrt3 * 0.5), point.Y - (float)0.5 * size) };
                G.DrawPolygon(pointPen, corners);
                svg_context += "<polygon points=\"" + corners[0].X + "," + corners[0].Y + " " + corners[1].X + "," + corners[1].Y + " " + corners[2].X + "," + corners[2].Y + "\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"" + ((size / 4 > 0) ? size / 4 : 1) + "\" fill=\"" + "none" + "\"/>\n";
            } else if(serie.style == Style.triangulo_down_fill)
            {
                PointF[] corners = { new PointF(point.X, point.Y + size), new PointF(point.X + (float)(size * sqrt3 * 0.5), point.Y - (float)0.5 * size), new PointF(point.X - (float)(size * sqrt3 * 0.5), point.Y - (float)0.5 * size) };
                G.FillPolygon(pointBrush, corners);
                svg_context += "<polygon points=\"" + corners[0].X + "," + corners[0].Y + " " + corners[1].X + "," + corners[1].Y + " " + corners[2].X + "," + corners[2].Y + "\" stroke=\"" + "none" + "\" stroke-width=\"" + 0 + "\" fill=" + ColorSVG(Color.FromArgb(serie.color)) + "/>\n";
            } else if(serie.style == Style.rombo)
            {
                PointF[] corners = { new PointF(point.X, point.Y + size), new PointF(point.X + size, point.Y), new PointF(point.X, point.Y - size), new PointF(point.X - size, point.Y) };
                G.DrawPolygon(pointPen, corners);
                svg_context += "<polygon points=\"" + corners[0].X + "," + corners[0].Y + " " + corners[1].X + "," + corners[1].Y + " " + corners[2].X + "," + corners[2].Y + " " + corners[3].X + "," + corners[3].Y + "\" stroke=" + ColorSVG(Color.FromArgb(serie.color)) + " stroke-width=\"" + ((size / 4 > 0) ? size / 4 : 1) + "\" fill=\"" + "none" + "\"/>\n";
            }
            else if(serie.style == Style.rombo_fill)
            {
                PointF[] corners = { new PointF(point.X, point.Y + size), new PointF(point.X + size, point.Y), new PointF(point.X, point.Y - size), new PointF(point.X - size, point.Y) };
                G.FillPolygon(pointBrush, corners);
                svg_context += "<polygon points=\"" + corners[0].X + "," + corners[0].Y + " " + corners[1].X + "," + corners[1].Y + " " + corners[2].X + "," + corners[2].Y + " " + corners[3].X + "," + corners[3].Y  + "\" stroke=\"" + "none" + "\" stroke-width=\"" + 0 + "\" fill=" + ColorSVG(Color.FromArgb(serie.color)) + "/>\n";
            }
            else if(serie.style == Style.plus)
            {
                pointPen.Width = size;
                G.DrawLine(pointPen, new PointF(point.X, point.Y + size), new PointF(point.X, point.Y - size));
                G.DrawLine(pointPen, new PointF(point.X + size, point.Y), new PointF(point.X - size, point.Y));
                svg_context += LineSVG(new PointF(point.X, point.Y + size), new PointF(point.X, point.Y - size), Color.FromArgb(serie.color), size);
                svg_context += LineSVG(new PointF(point.X, point.Y + size), new PointF(point.X, point.Y - size), Color.FromArgb(serie.color), size);
            } else if(serie.style == Style.cross)
            {
                pointPen.Width = size;
                G.DrawLine(pointPen, new PointF(pointRect.Left, pointRect.Top), new PointF(pointRect.Right, pointRect.Bottom));
                G.DrawLine(pointPen, new PointF(pointRect.Left, pointRect.Bottom), new PointF(pointRect.Right, pointRect.Top));
                svg_context += LineSVG(new PointF(pointRect.Left, pointRect.Top), new PointF(pointRect.Right, pointRect.Bottom), Color.FromArgb(serie.color), size);
                svg_context += LineSVG(new PointF(pointRect.Left, pointRect.Bottom), new PointF(pointRect.Right, pointRect.Top), Color.FromArgb(serie.color), size);
            } else if(serie.style == Style.Text)
            {
                format.Alignment = StringAlignment.Center;
                G.DrawString(serie.name, optionsFont, pointBrush, point, format);
                svg_context += TextSVG(serie.name, point, optionsFont, Color.FromArgb(serie.color));
            }  

            pointPen.Dispose();
            pointBrush.Dispose();
        }


        ///REFERENCE POINTS
        public void CalculateReferencePoints(Archivo file)
        {
            ReferencePoints.Clear();
            for(double s = 0; s <= 1; s += 0.01)
            {
                ReferencePoints.Add(new PointF(
                    (float)(1-s),
                    (float)(s)));

                ReferencePoints.Add(new PointF(
                    (float)(0),
                    (float)(1-s)));

                ReferencePoints.Add(new PointF(
                    (float)(s),
                    (float)(0)));
            }

            if (file.Options.GridStyle != GridStyle.Hidden)
            {
                for (int i = 1; i < file.Options.GridSpacing; i++)
                {
                    float l = Convert.ToSingle(i) / Convert.ToSingle(file.Options.GridSpacing);

                    for (double s = 0; s <= 1; s += 0.01)
                    {
                        ReferencePoints.Add(new PointF((float)(l * s), 1 - l));
                        ReferencePoints.Add(new PointF(l, (float)((1 - l) * (1 - s))));

                        if (file.Options.DisplayStyle == DispStyle.Equilateral)
                        {
                            ReferencePoints.Add(new PointF((float)((1 - s) * l), (float)(s * l)));
                        }
                        if (file.Options.DisplayStyle == DispStyle.Rectangle)
                        {
                            ReferencePoints.Add(new PointF(
                                (float)((1 - s) * l + s * ((1 - 2 * l >= 0) ? 0 : 2 * l - 1)),
                                (float)((1 - s) * (1 - l) + s * ((1 - 2 * l >= 0) ? (1 - 2 * l) : 0))
                                ));
                        }
                    }
                }
            }

            foreach (Serie S in file.Series)
            {
                ReferencePoints.AddRange(S.ReferencePoints);
            }

        }

        public PointF GetClosestPoint(PointF point, Opciones Options, int h, int w, List<PointF> ListOfPoints = null, bool returnComposition = false)
        {
            // toma un punto en el sistema Va, Vb y devuelve uno en el sistema X, Y absoluto

            if (ListOfPoints == null) ListOfPoints = ReferencePoints;

            W = h;
            W = w;
            R = getR();

            Func<PointF, PointF> convert = null;
            if (Options.DisplayStyle == DispStyle.Equilateral)
                convert = V2Xeq;

            if (Options.DisplayStyle == DispStyle.Rectangle)
                convert = V2Xrt;

            ListOfPoints.Sort((PointF a, PointF b) =>
            ((point.X - a.X) * (point.X - a.X) + (point.Y - a.Y) * (point.Y - a.Y)).CompareTo((point.X - b.X) * (point.X - b.X) + (point.Y - b.Y) * (point.Y - b.Y)));

            if (ListOfPoints.Count == 0) return new PointF(float.NaN, float.NaN);

            if (returnComposition) return ListOfPoints[0];

            return convert(ListOfPoints[0]);
        }

        ///SVG
        ///
        public string GetSVG(Opciones Options)
        {
            string svg = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n";
            svg += "<svg xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:cc=\"http://creativecommons.org/ns#\" xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\" xmlns:svg=\"http://www.w3.org/2000/svg\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:sodipodi=\"http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd\" xmlns:inkscape=\"http://www.inkscape.org/namespaces/inkscape\" id=\"svg7052\" inkscape:version=\"0.92.3 (2405546, 2018-03-11)\" version=\"1.1\"\n";
            svg += "width =\"" + W.ToString() + "px\" height=\"" + H.ToString() + "px\" viewBox=\"0 0 " + W.ToString() + " " + H.ToString() + "\">\n";
           
            svg += svg_context;

            Func<PointF, PointF> convert = null;
            if (Options.DisplayStyle == DispStyle.Equilateral)
                convert = V2Xeq;

            if (Options.DisplayStyle == DispStyle.Rectangle)
                convert = V2Xrt;

            svg += "</svg>";
            return svg;
        }

        string[] SVGDashArray = {"", "stroke-dasharray=\"2,2\" ", "stroke-dasharray=\"1,1\" ", "stroke-dasharray=\"2,1,1,1\" " }; // Continuous, Dash, Dot, DashDot
        private string LineSVG(PointF A, PointF B, Color color, int stroke, int dasharray = 0)
        {

            return "<line " + SVGDashArray[dasharray] + "stroke=\"rgb(" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + ")\" stroke-width=\"" + stroke.ToString() + "\" x1=\"" + A.X + "\" y1=\"" + A.Y.ToString() + "\" x2=\"" + B.X.ToString() + "\" y2=\"" + B.Y.ToString() + "\"/>\n";
        }

        private string TextSVG(string text, PointF point, Font font, Color color, int format = 0, int AngleRotation = 0)
        {
            //Format = 0 -> Middle, Fromat = 1 -> Right, Format = -1 -> Left
            string textAnchor = "middle";
            if (format == 1) textAnchor = "end";
            if (format == -1) textAnchor = "start";
            return "<text x=\"" + point.X + "\" y=\"" + point.Y + "\" fill=" + ColorSVG(color) + " font-size=\"" + font.Size.ToString() + "pt\" font-family=\"" + font.Name + "\" text-anchor=\"" + textAnchor + "\" transform=\"rotate(" + AngleRotation.ToString() + "," + point.X + "," + point.Y + ")\">" + text + "</text>\n";
        }

        private string ColorSVG(Color color)
        {
            return "\"rgb(" + color.R.ToString() + "," + color.G.ToString() + "," + color.B.ToString() + ")\"";
        }

 

       /* private string GetSerieSVG(Serie S, Func<PointF, PointF> convert, int H)
        {
            string svg = "";
            string head = "";
            string tail = "";

            if (S.type == Types.point)
            {
                if (S.style == Style.cuadrado)
                    foreach (PointF point in S.Points)
                    {
                        PointF P = convert(point);
                        svg = head + " cx=\"" + (P.X).ToString() + "\" cy=\"" + (H - P.Y).ToString() + "\" " + tail;
                    }
            }
            else if (S.type == Types.polyline)
            {

            }
            else if (S.type == Types.segments)
            {

            }
            else if (S.type == Types.spline)
            {
                Serie.SplinePolynomial SP = new Serie.SplinePolynomial();
            }
            else if (S.type == Types.area)
            {

            }
            else if (S.type == Types.polygon)
            {

            }

            return svg;
        }*/
    }
    
}
