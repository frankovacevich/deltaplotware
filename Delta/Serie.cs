using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Delta
{
    public enum Types { point, segments, polyline, spline, polygon, area};
    public enum Style { cuadrado = 0, circulo, triangulo_up, triangulo_down, rombo, cuadrado_fill, circulo_fill, triangulo_up_fill, triangulo_down_fill, rombo_fill, cross, plus, Text, Line_continous = 20, Line_dash, Line_dot, Line_dashdot , Area_Solid = 30, Area_Hatch1, Area_Hatch2, Area_Hatch3, Area_Hatch4, Area_Hatch5, Area_Hatch6, Area_Hatch7 }

   
    public class Serie
    {
        public int Id = -1;
        public int zIndex;

        public List<PointF> Points = new List<PointF>();
        public List<PointF> ReferencePoints = new List<PointF>();
        
        public int color = System.Drawing.Color.Magenta.ToArgb();
        public int size = 3;
        public String name = "newSerie";
        public Types type = Types.point;
        public Style style = Style.circulo;

        public bool visible = true;
        public bool displayInLeyend = true;

        public int ID
        {
            get { return Id; }
        }

        public string Name
        {
            get { return name; }
        }




        public Serie Clone(bool deep = false)
        {
            if (this == null) return null;

            Serie clonedSerie = new Serie();
            clonedSerie.color = color;
            clonedSerie.size = size;
            clonedSerie.style = style;
            clonedSerie.visible = visible;
            clonedSerie.displayInLeyend = displayInLeyend;
            clonedSerie.type = type;
            clonedSerie.name = name;
            clonedSerie.zIndex = zIndex;

            if (deep==true)
                clonedSerie.Points.AddRange(Points);

            return clonedSerie;
        }

        public void CalculateReferencePoints()
        {
            ReferencePoints.Clear();
            if (type == Types.spline)
            {
                ReferencePoints.AddRange(CalculateReferencePointsSpline(Points));
            }
            else if(type == Types.polyline || type == Types.segments)
            {
                int delta_i = (type == Types.polyline) ? 1 : 2;
                
                for(int i = 0; i < Points.Count - 1; i+=delta_i)
                {
                    for(double s = 0; s <= 1; s += 0.01)
                    {
                        ReferencePoints.Add(new PointF((float) (Points[i].X * s + Points[i+1].X*(1-s)), (float) ( Points[i].Y * s + Points[i + 1].Y * (1 - s))));
                    }
                }
            }
        }

        public struct SplinePolynomial
        {
            public double Ax, Bx, Cx, Dx;
            public double Ay, By, Cy, Dy;
        }

        private List<PointF> CalculateReferencePointsSpline(List<PointF> ListOfPoints)
        {
            List<PointF> referencePoints = new List<PointF>();

            List<SplinePolynomial> ListSP = CalculatePolynomialSpline(ListOfPoints);

            for (int i = 0; i < ListSP.Count; i++)
            {
                for (double s = 0; s <= 1; s += 0.01)
                {
                    referencePoints.Add(EvaluatePolynomial(ListSP[i], s));
                }
            }

            return referencePoints;
        }

        private PointF EvaluatePolynomial(SplinePolynomial SP, double s)
        {
            PointF Pt = new PointF();
            Pt.X = (float)(SP.Ax * s * s * s + SP.Bx * s * s + SP.Cx * s + SP.Dx);
            Pt.Y = (float)(SP.Ay * s * s * s + SP.By * s * s + SP.Cy * s + SP.Dy);
            return Pt;
        }

        public List<SplinePolynomial> CalculatePolynomialSpline(List<PointF> ListOfPoints, Func<PointF, PointF> convert = null, string[] SVGCode = null, bool isarea = false)
        {
            
            int i;
            int n = ListOfPoints.Count;
            double c = 0.19;

            List<SplinePolynomial> ListSP = new List<SplinePolynomial>();


            for (i = 0; i < n - 1; i++)
            {
                SplinePolynomial SP = new SplinePolynomial();

                // 1
                double x0 = (i == 0) ? ListOfPoints[0].X : ListOfPoints[i - 1].X;
                double x1 = ListOfPoints[i].X;
                double x2 = ListOfPoints[i + 1].X;
                double x3 = (i == n - 2) ? ListOfPoints[n - 1].X : ListOfPoints[i + 2].X;

                double y0 = (i == 0) ? ListOfPoints[0].Y : ListOfPoints[i - 1].Y;
                double y1 = ListOfPoints[i].Y;
                double y2 = ListOfPoints[i + 1].Y;
                double y3 = (i == n - 2) ? ListOfPoints[n - 1].Y : ListOfPoints[i + 2].Y;

                if (isarea)
                {
                     x0 = (i == 0) ? ListOfPoints[n-2].X : ListOfPoints[i - 1].X;
                   
                     x3 = (i == n - 2) ? ListOfPoints[1].X : ListOfPoints[i + 2].X;

                     y0 = (i == 0) ? ListOfPoints[n-2].Y : ListOfPoints[i - 1].Y;
                    
                     y3 = (i == n - 2) ? ListOfPoints[1].Y : ListOfPoints[i + 2].Y;
                }

                if (SVGCode != null)
                {
                    PointF XY0 = convert(new PointF((float)x0, (float)y0));
                    PointF XY1 = convert(new PointF((float)x1, (float)y1));
                    PointF XY2 = convert(new PointF((float)x2, (float)y2));
                    PointF XY3 = convert(new PointF((float)x3, (float)y3));
                    x0 = XY0.X;
                    x1 = XY1.X;
                    x2 = XY2.X;
                    x3 = XY3.X;
                    y0 = XY0.Y;
                    y1 = XY1.Y;
                    y2 = XY2.Y;
                    y3 = XY3.Y;
                }

                // 2

                double m1x = x1 + c * (x2 - x0);
                double m2x = x2 - c * (x3 - x1);

                double m1y = y1 + c * (y2 - y0);
                double m2y = y2 - c * (y3 - y1);

                SP.Ax = -3 * m2x + 3 * m1x + x2 - x1;
                SP.Bx = -6 * m1x + 3 * m2x + 3 * x1;
                SP.Cx = 3 * m1x - 3 * x1;
                SP.Dx = x1;
    
                SP.Ay = -3 * m2y + 3 * m1y + y2 - y1;
                SP.By = -6 * m1y + 3 * m2y + 3 * y1;
                SP.Cy = 3 * m1y - 3 * y1;
                SP.Dy = y1;

                
                if (SVGCode != null)
                {
                    if (i == 0) SVGCode[0] += "M " + x1.ToString() + " " + y1.ToString() + " ";
                    SVGCode[0] += " c "
                        + (m1x-x1).ToString() + " " + (m1y-y1).ToString() + ", "
                        + (m2x-x1).ToString() + " " + (m2y-y1).ToString() + ", "
                        + (x2- x1).ToString() + " " + (y2- y1).ToString() + " ";
                }
               
                ListSP.Add(SP);
                
            }

            return ListSP;
        }


    }
}
