using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Delta
{
    public class CommandStack
    {
        public List<Comando> stack = new List<Comando>();
        public int maxsize = 100;

        public void Push(Comando command)
        {
            stack.Add(command);
            if (stack.Count > maxsize)
            {
                stack.RemoveAt(0);
            }
        }

        public Comando Pop()
        {
            if (stack.Count == 0) return null;
            Comando poppedCommand = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            return poppedCommand;
        }
    }



    public enum CommandType
    {
        ChangeColor, ChangeVisibility, ChangeName, ChangeDisplayLeyend, ChangeSize, ChangeType, ChangeStyle, AddPoint, RemovePoint, AddSerie, RemoveSerie, MoveSerieUp, MoveSerieDown,
        Option_DisplayStyle = 20, Option_GridColor, Option_GridStyle, Option_GridSpacing, Option_TickVisible, Option_LegendVisible, Option_LegendLocation, Option_CompoundA, Option_CompoundB,
        Option_CompoundC, Option_CompoundTextLocation, Option_Font, Option_BackgroundColor, Option_BackgroundStyle, Option_BackgroundPicture
    }

    public class CommandManager
    {
        public CommandStack ZStack = new CommandStack();
        public CommandStack YStack = new CommandStack();

        public Form1 FormCtrl;

        public void Reset()
        {
            ZStack.stack.Clear();
            YStack.stack.Clear();
        }

        public void ExecuteCommand(Comando comando, bool AddToYStack = false)
        {
            Comando InverseCommand = null;
            if (comando.Type == CommandType.ChangeColor)
            {
                InverseCommand = new Comando { Type = comando.Type, SerieValue = comando.SerieValue, IntValue = comando.SerieValue.color };
                comando.SerieValue.color = comando.IntValue;
                FormCtrl.ReDraw();
            }
            else if (comando.Type == CommandType.ChangeVisibility)
            {
                InverseCommand = new Comando { Type = comando.Type, SerieValue = comando.SerieValue, IntValue = (comando.SerieValue.visible == false) ? 0 : 1 };
                comando.SerieValue.visible = (comando.IntValue == 0) ? false : true;
                FormCtrl.ReDraw();
            }
            else if (comando.Type == CommandType.ChangeDisplayLeyend)
            {
                InverseCommand = new Comando { Type = comando.Type, SerieValue = comando.SerieValue, IntValue = (comando.SerieValue.displayInLeyend == false) ? 0 : 1 };
                comando.SerieValue.displayInLeyend = (comando.IntValue == 0) ? false : true;
                FormCtrl.ReDraw();
            }
            else if (comando.Type == CommandType.ChangeSize)
            {
                InverseCommand = new Comando { Type = comando.Type, SerieValue = comando.SerieValue, IntValue = comando.SerieValue.size };
                comando.SerieValue.size = comando.IntValue;
                FormCtrl.ReDraw();
            }
            else if (comando.Type == CommandType.ChangeStyle)
            {
                InverseCommand = new Comando { Type = comando.Type, SerieValue = comando.SerieValue, IntValue = (int)comando.SerieValue.style };
                comando.SerieValue.style = (Style)comando.IntValue;
                FormCtrl.ReDraw();
            }
            else if (comando.Type == CommandType.ChangeType)
            {
                InverseCommand = new Comando { Type = comando.Type, SerieValue = comando.SerieValue, IntValue = (int)comando.SerieValue.type };
                comando.SerieValue.type = (Types)comando.IntValue;
                FormCtrl.ReDraw();
                FormCtrl.Graficador.CalculateReferencePoints(FormCtrl.CurrentFile);
            }
            else if (comando.Type == CommandType.ChangeName)
            {
                InverseCommand = new Comando { Type = comando.Type, SerieValue = comando.SerieValue, StringValue = comando.SerieValue.name };
                comando.SerieValue.name = comando.StringValue;
                FormCtrl.ReDraw();
                FormCtrl.SeriesCtrl.RefreshDatagridview();
            }
            else if (comando.Type == CommandType.MoveSerieDown)
            {
                FormCtrl.CurrentFile.Series.Sort((Serie a, Serie b) => a.zIndex.CompareTo(b.zIndex));
                int indexA = FormCtrl.CurrentFile.Series.IndexOf(comando.SerieValue);

                if (indexA == FormCtrl.CurrentFile.Series.Count - 1) return;

                int aux = FormCtrl.CurrentFile.Series[indexA + 1].zIndex;
                FormCtrl.CurrentFile.Series[indexA + 1].zIndex = comando.SerieValue.zIndex;
                comando.SerieValue.zIndex = aux;

                FormCtrl.CurrentFile.Series.Sort((Serie a, Serie b) => a.zIndex.CompareTo(b.zIndex));

                InverseCommand = new Comando { Type = CommandType.MoveSerieUp, SerieValue = comando.SerieValue };
                FormCtrl.ReDraw();
                FormCtrl.SeriesCtrl.RefreshDatagridview();
            }
            else if (comando.Type == CommandType.MoveSerieUp)
            {
                FormCtrl.CurrentFile.Series.Sort((Serie a, Serie b) => a.zIndex.CompareTo(b.zIndex));
                int indexA = FormCtrl.CurrentFile.Series.IndexOf(comando.SerieValue);

                if (indexA == 0) return;

                int aux = FormCtrl.CurrentFile.Series[indexA - 1].zIndex;
                FormCtrl.CurrentFile.Series[indexA - 1].zIndex = comando.SerieValue.zIndex;
                comando.SerieValue.zIndex = aux;

                FormCtrl.CurrentFile.Series.Sort((Serie a, Serie b) => a.zIndex.CompareTo(b.zIndex));

                InverseCommand = new Comando { Type = CommandType.MoveSerieDown, SerieValue = comando.SerieValue };
                FormCtrl.ReDraw();
                FormCtrl.SeriesCtrl.RefreshDatagridview();
            }
            else if (comando.Type == CommandType.AddSerie)
            {
                FormCtrl.CurrentFile.Series.Add(comando.SerieValue);
                if (comando.SerieValue.Id == -1)
                {
                    comando.SerieValue.Id = FormCtrl.CurrentFile.LastId;
                    comando.SerieValue.name = "NewSerie" + comando.SerieValue.Id.ToString();
                    comando.SerieValue.zIndex = comando.SerieValue.Id;
                    FormCtrl.CurrentFile.LastId++;
                }

                comando.SerieValue.CalculateReferencePoints();
                FormCtrl.Graficador.CalculateReferencePoints(FormCtrl.CurrentFile);
                FormCtrl.ReDraw();
                FormCtrl.SeriesCtrl.RefreshDatagridview();

                InverseCommand = new Comando { Type = CommandType.RemoveSerie, SerieValue = comando.SerieValue };
            }
            else if (comando.Type == CommandType.RemoveSerie)
            {
                FormCtrl.SeriesCtrl.dataGridView1.DataSource = null;
                FormCtrl.CurrentFile.Series.Remove(comando.SerieValue);

                comando.SerieValue.CalculateReferencePoints();
                FormCtrl.Graficador.CalculateReferencePoints(FormCtrl.CurrentFile);
                FormCtrl.ReDraw();
                FormCtrl.SeriesCtrl.RefreshDatagridview();

                InverseCommand = new Comando { Type = CommandType.AddSerie, SerieValue = comando.SerieValue };
            }
            else if (comando.Type == CommandType.AddPoint)
            {
                comando.SerieValue.Points.Add(comando.PointValue);

                comando.SerieValue.CalculateReferencePoints();
                FormCtrl.Graficador.CalculateReferencePoints(FormCtrl.CurrentFile);
                FormCtrl.ReDraw();

                InverseCommand = new Comando { Type = CommandType.RemovePoint, SerieValue = comando.SerieValue, PointValue = comando.PointValue };
            }
            else if (comando.Type == CommandType.RemovePoint)
            {
                comando.SerieValue.Points.Remove(comando.PointValue);

                comando.SerieValue.CalculateReferencePoints();
                FormCtrl.Graficador.CalculateReferencePoints(FormCtrl.CurrentFile);
                FormCtrl.ReDraw();

                InverseCommand = new Comando { Type = CommandType.AddPoint, SerieValue = comando.SerieValue, PointValue = comando.PointValue };
            }
            else if (comando.Type == CommandType.Option_DisplayStyle)
            {
                InverseCommand = new Comando { Type = comando.Type, IntValue = (int)FormCtrl.CurrentFile.Options.DisplayStyle };
                FormCtrl.CurrentFile.Options.DisplayStyle = (DispStyle)comando.IntValue;
                FormCtrl.Graficador.CalculateReferencePoints(FormCtrl.CurrentFile);
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_GridColor)
            {
                InverseCommand = new Comando { Type = comando.Type, IntValue = FormCtrl.CurrentFile.Options.GridColor };
                FormCtrl.CurrentFile.Options.GridColor = comando.IntValue;
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_GridStyle)
            {
                InverseCommand = new Comando { Type = comando.Type, IntValue = (int)FormCtrl.CurrentFile.Options.GridStyle };
                FormCtrl.CurrentFile.Options.GridStyle = (GridStyle)comando.IntValue;
                FormCtrl.Graficador.CalculateReferencePoints(FormCtrl.CurrentFile);
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_GridSpacing)
            {
                InverseCommand = new Comando { Type = comando.Type, IntValue = FormCtrl.CurrentFile.Options.GridSpacing };
                FormCtrl.CurrentFile.Options.GridSpacing = comando.IntValue;
                FormCtrl.Graficador.CalculateReferencePoints(FormCtrl.CurrentFile);
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_TickVisible)
            {
                InverseCommand = new Comando { Type = comando.Type, IntValue = (FormCtrl.CurrentFile.Options.TickVisible == false) ? 0 : 1 };
                FormCtrl.CurrentFile.Options.TickVisible = (comando.IntValue == 0) ? false : true;
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_LegendVisible)
            {
                InverseCommand = new Comando { Type = comando.Type, IntValue = (FormCtrl.CurrentFile.Options.LegendVisible == false) ? 0 : 1 };
                FormCtrl.CurrentFile.Options.LegendVisible = (comando.IntValue == 0) ? false : true;
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_LegendLocation)
            {
                InverseCommand = new Comando { Type = comando.Type, PointValue = FormCtrl.CurrentFile.Options.LegendLocation };
                FormCtrl.CurrentFile.Options.LegendLocation = new Point((int)comando.PointValue.X, (int)comando.PointValue.Y);
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_CompoundA)
            {
                InverseCommand = new Comando { Type = comando.Type, StringValue = FormCtrl.CurrentFile.Options.CompoundA };
                FormCtrl.CurrentFile.Options.CompoundA = comando.StringValue;
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_CompoundB)
            {
                InverseCommand = new Comando { Type = comando.Type, StringValue = FormCtrl.CurrentFile.Options.CompoundB };
                FormCtrl.CurrentFile.Options.CompoundB = comando.StringValue;
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_CompoundC)
            {
                InverseCommand = new Comando { Type = comando.Type, StringValue = FormCtrl.CurrentFile.Options.CompoundC };
                FormCtrl.CurrentFile.Options.CompoundC = comando.StringValue;
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_CompoundTextLocation)
            {
                InverseCommand = new Comando { Type = comando.Type, IntValue = (int)FormCtrl.CurrentFile.Options.CompoundTextLocation };
                FormCtrl.CurrentFile.Options.CompoundTextLocation = (CompoundTxtLocation)comando.IntValue;
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_Font)
            {
                InverseCommand = new Comando { Type = comando.Type, IntValue = (int)FormCtrl.CurrentFile.Options.FontSize, StringValue = FormCtrl.CurrentFile.Options.FontFamily };
                FormCtrl.CurrentFile.Options.FontFamily = comando.StringValue;
                FormCtrl.CurrentFile.Options.FontSize = comando.IntValue;
                //FormCtrl.CurrentFile.Options.Font = new Font(comando.StringValue,comando.IntValue);
                FormCtrl.ReDraw();
                FormCtrl.ViewCtrl.RefreshData();
            }
            else if (comando.Type == CommandType.Option_BackgroundColor)
            {
                InverseCommand = new Comando { Type = comando.Type, IntValue = FormCtrl.CurrentFile.Options.BackgroundColor };
                FormCtrl.CurrentFile.Options.BackgroundColor = comando.IntValue;
                FormCtrl.ReDraw();
            }
            /*else if (comando.Type == CommandType.Option_BackgroundStyle)
            {
                InverseCommand = new Comando { Type = comando.Type, IntValue = (int) FormCtrl.CurrentFile.Options.BackgroundStyle };
                FormCtrl.CurrentFile.Options.BackgroundStyle = (BackgndStyle) comando.IntValue;
                FormCtrl.ReDraw();
            }*/


            if (InverseCommand == null) return;

            FormCtrl.ThereIsUnsavedWork = true;
            if (AddToYStack)
                YStack.Push(InverseCommand);
            else
                ZStack.Push(InverseCommand);

        }

        public void Undo()
        {
            Comando newCommand = ZStack.Pop();
            if (newCommand == null) return;
            ExecuteCommand(newCommand, true);
        }

        public void Redo()
        {
            Comando newCommand = YStack.Pop();
            if (newCommand == null) return;
            ExecuteCommand(newCommand);
        }
    }

    public class Comando
    {
        public CommandType Type;

        public int IntValue = 0;
        public PointF PointValue;
        public Serie SerieValue = null;
        public string StringValue = null;

    }



}
