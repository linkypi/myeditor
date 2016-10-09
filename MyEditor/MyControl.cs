﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace MyEditor
{
    public partial class MyControl : UserControl
    {
        private StringBuilder _text = new StringBuilder();
        private Color _dividerColor = ColorTranslator.FromHtml("#999999");
        private int _lines = 0;
        private int _currentLine = 0;
        private int _lineHight = 2;
        private int _fontSize = 14;
        private int _dividerX = 0;
        private bool _drawCaret = false;
        private Rectangle textAreaRect;

        [Browsable(true)]
        //[Localizable(true)]
        //[SettingsBindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("Text of the control.")]
        //[Bindable(true)]
        public override string Text
        {
            get { return _text.ToString(); }
            set
            {
                this._text.Append(value);
            }
        }
        public MyControl()
        {
            InitializeComponent();
        }

        private void MyControl_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw |  
              ControlStyles.OptimizedDoubleBuffer |  
              ControlStyles.AllPaintingInWmPaint, true);  
            this.UpdateStyles(); 
        }
        [DllImport("user32.dll")]
        static extern int MapVirtualKey(uint uCode, uint uMapType);
        private static char KeyCodeToChar(Keys k)
        {
            int nonVirtualKey = MapVirtualKey((uint)k, 2);
            char mappedChar = Convert.ToChar(nonVirtualKey);
            return mappedChar;
        }
        public static string KeyCodeToStr(Keys k)
        {
            char mappedChar = KeyCodeToChar(k);
            string str = mappedChar.ToString();
            if (Char.IsWhiteSpace(mappedChar) || string.IsNullOrEmpty(str) || mappedChar == '\r' || mappedChar == '\n' || mappedChar == KeyCodeToChar(Keys.F1))
            {
                return k.ToString();
            }
            else
            {
                return str + "";
            }
        }
 
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.PageUnit = GraphicsUnit.Pixel;
            Graphics graphics = pe.Graphics;
            //BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            //BufferedGraphics myBuffer = currentContext.Allocate(pe.Graphics, pe.ClipRectangle);
            //Graphics graphics = myBuffer.Graphics;
            //graphics.PageUnit = GraphicsUnit.Pixel;
            //graphics.Clear(this.BackColor);

            //绘制行号, 右对齐
            Font font = new Font("宋体", _fontSize, GraphicsUnit.Pixel);
            StringFormat sf = StringFormat.GenericTypographic;
            sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

            string[] lineArr = string.IsNullOrEmpty(_text.ToString()) ? null : _text.ToString().Split('\n');
            _lines = lineArr != null ? lineArr.Length : 0;

            SizeF size = graphics.MeasureString("0", font, 2, sf);
            float maxLineNumberLength= _lines.ToString().Length * size.Width; 

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            //绘制左边分隔线
            _dividerX = Constants.LEFTINDENT + (int)maxLineNumberLength + 5;
            graphics.DrawLine(new Pen(_dividerColor),_dividerX , 0,  _dividerX , 81);
            graphics.DrawLine(new Pen(_dividerColor), _dividerX , 89,_dividerX , this.Height);

            //DrawLineNumbers(10, 15, 5, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(100, 105, 10, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(1000, 1005, 15, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(10000, 10005, 20, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(100000, 100005, 25, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(1000000, 1000005, 30, pe, font, maxLineNumberLength, sf);
            //DrawLineNumbers(10000000, 10000005, 35, pe, font, maxLineNumberLength sf);

            #region 绘制区块折叠符号

            DrawBlockMarker(graphics, (int)maxLineNumberLength + 1, 81, 200);

            #endregion


            #region draw text area

            textAreaRect = new Rectangle(new Point(_dividerX + 5, -1), new Size(this.Width - (int)_dividerX, this.Height));
            graphics.DrawRectangle(new Pen(Color.White),textAreaRect);

            #endregion

            //draw text
            if (lineArr != null) {
             
                SolidBrush textBrush = new SolidBrush(Color.Black);
                int index = 0;
                foreach (var item in lineArr)
                {
                    graphics.DrawString(item, font, textBrush, new PointF(textAreaRect.Left + 2, (_lineHight + font.Height) * index));
                    index++;
                }
            }

            DrawLineNumbers(0, _lines, 1, graphics, font, maxLineNumberLength, sf);

            if (_drawCaret) {
                int line =(int)Math.Ceiling((double)MousePosition.Y/font.Height);
                Point point1 = new Point(MousePosition.X,line*font.Height);
                Point point2 = new Point(MousePosition.X,(line+1)*font.Height);
                graphics.DrawLine(new Pen(Color.Black),point1,point2);
                _drawCaret = false;
            }
            //myBuffer.Render(pe.Graphics);  //呈现图像至关联的Graphics  
            //myBuffer.Dispose();
            //graphics.Dispose();  

            base.OnPaint(pe);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (textAreaRect != null && textAreaRect.Contains(MousePosition.X, MousePosition.Y))
            {
                _drawCaret = true;
                Validate();
            }
        }

        private bool IsInRegion(Point input, Point[] points)
        {
            GraphicsPath myGraphicsPath = new GraphicsPath();
            Region myRegion = new Region();
            myGraphicsPath.Reset();
            myGraphicsPath.AddPolygon(points);
            myRegion.MakeEmpty();
            myRegion.Union(myGraphicsPath);
            //返回判断点是否在多边形里
            return myRegion.IsVisible(input);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:

                    break;
                case Keys.Enter:
                    _text.Append("\n");
                    break;
                default:
                   
                    //_text.Append(KeyCodeToStr(keyData));
                    break;
            }
           
            Invalidate();
            base.OnKeyDown(e);
        }

        private void DrawBlockMarker(Graphics graphics,int x, int y,int y2)
        {
            x += Constants.LEFTINDENT;
            //draw start block 
            int half = Constants.BLOCKMARKERSIDE / 2;
            graphics.DrawRectangle(new Pen(_dividerColor), x, y, Constants.BLOCKMARKERSIDE, Constants.BLOCKMARKERSIDE);
            graphics.DrawLine(new Pen(_dividerColor), x + 2, y + half, x + 2 + half, y + half);

            //draw end block
            graphics.DrawLine(new Pen(_dividerColor), x + half, y2, x + Constants.BLOCKMARKERSIDE, y2);
        }

        private void DrawLineNumbers(int start, int end, int startline, Graphics graphics, Font font, float spiltLineX, StringFormat sf)
        {
            var fontColor = ColorTranslator.FromHtml("#009999");
            for (int i = start; i < end; i++)
            {
                int y = (i - start - 1 + startline) * (font.Height + _lineHight);
                using (var lineNumberBrush = new SolidBrush(fontColor)) //(iLine + lineNumberStartValue).
                {
                    //pe.Graphics.DrawString(i.ToString(), font, lineNumberBrush,
                    //                     new RectangleF(0, y, spiltLineX, font.Height),//LeftIndent - minLeftIndent 
                    //                     new StringFormat(StringFormatFlags.DirectionRightToLeft));
                    //pe.Graphics.DrawString(i.ToString(), font, brush, spiltLineX - width + factor * 5, (i - 1000000 - 1 + 20) * font.Height);
                   graphics.DrawString(i.ToString(), font, lineNumberBrush,
                                   new RectangleF(Constants.LEFTINDENT, y, spiltLineX, font.Height), sf);
                }
            }
        }

        private void MyControl_Click(object sender, EventArgs e)
        {
            Timer timer = new Timer();
            timer.Interval = 800;
            timer.Tick += timer_Tick;
            timer.Start();

            if (string.IsNullOrEmpty(_text.ToString())) { 
            
            }
        }

        private void MyControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            string _char = e.KeyChar.ToString();
            //if(_char!="\n")
            _text.Append(_char.ToString()); 
        }

    }
}