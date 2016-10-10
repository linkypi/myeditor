using System;
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
using MyEditor.Model;

namespace MyEditor
{
    public partial class MyControl : UserControl
    {
        private StringBuilder _text = new StringBuilder();
        private Color _dividerColor = ColorTranslator.FromHtml("#999999");
        private int _lines = 1;
        private int _currentLine = 0;
        private readonly int _lineHeight = 2;
        private int _fontSize = 14;
        private int _dividerX = 0;
        private bool _drawCaret = false;
        private Caret _caret;
        private Rectangle textAreaRect;
        private Timer _timer;
        private Point[] _points;
        private bool _firstPaint = true;
        private int _rowHeight = 0; // fontheight + lineheight
        private float _fontWidth = 0;
        private int _fontHeigth = 0;
        private Font _font = null;
        private Pen _dividerPen = null;
        StringFormat _stringFormat;
        private int _textAreaPadding = 2;

        private Point[] Points
        {
            get
            {
                if (_points == null)
                    _points = new Point[]{ textAreaRect.Location,
                            new Point(textAreaRect.Left, textAreaRect.Top + textAreaRect.Height),
                            new Point(textAreaRect.Left + textAreaRect.Width, textAreaRect.Top + textAreaRect.Height),
                            new Point(textAreaRect.Left + textAreaRect.Width, textAreaRect.Top)};

                return _points;
            }
        }

        private List<LineInfo> _lineInfos = new List<LineInfo>();

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

            _caret = new Caret(this.BackColor);
            _font = new Font("宋体", _fontSize, GraphicsUnit.Pixel);
            _dividerPen = new Pen(_dividerColor);

            _stringFormat = StringFormat.GenericTypographic;
            _stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

            _timer = new Timer();
            _timer.Interval = 300;
            _timer.Tick += timer_Tick;
            _timer.Start();

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
            //Graphics graphics = pe.Graphics;

            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            BufferedGraphics myBuffer = currentContext.Allocate(pe.Graphics, pe.ClipRectangle);
            Graphics graphics = myBuffer.Graphics;
            graphics.PageUnit = GraphicsUnit.Pixel;
            graphics.Clear(this.BackColor);

            if (_firstPaint)
            {
                _firstPaint = false;

                SizeF size = graphics.MeasureString("0", _font, 2, _stringFormat);
                _fontWidth = (int)size.Width;
                _fontHeigth = _font.Height;
                _rowHeight = _font.Height + _lineHeight;

            }
            //绘制行号, 右对齐
            string[] lineArr = string.IsNullOrEmpty(_text.ToString()) ? null : _text.ToString().Split('\r');
            _lines = lineArr != null ? lineArr.Length : 1;

            float maxLineNumberLength = _lines.ToString().Length * _fontWidth;

            //graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            //绘制左边分隔线
            _dividerX = Constants.LEFTINDENT + (int)maxLineNumberLength + 5;
            graphics.DrawLine(_dividerPen,_dividerX, 0, _dividerX, 81);
            graphics.DrawLine(_dividerPen, _dividerX, 89, _dividerX, this.Height);

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
            if (textAreaRect.Height == 0 )
            {
                textAreaRect = new Rectangle(new Point(_dividerX + 5, -1), new Size(this.Width - (int)_dividerX, this.Height));

                var point = new Point(textAreaRect.Left + _textAreaPadding, 0);
                var info = new LineInfo(string.Empty, point,_fontWidth);
                _lineInfos.Add(info);

                _caret.SetBaseValue(_fontHeigth, _fontWidth, _dividerX + _textAreaPadding);
                ReCalcCaret();
            }
            
            graphics.DrawRectangle(new Pen(Color.White), textAreaRect);

            #endregion
            
            //draw text
            if (lineArr != null)
            {
                SolidBrush textBrush = new SolidBrush(Color.Black);
                int index = 0;
                foreach (var item in lineArr)
                {
                    var point = new Point(textAreaRect.Left + _textAreaPadding, _rowHeight * index);
                    var info = new LineInfo(item, point, _fontWidth);
                    if (_lineInfos.Count == index)
                    {
                        _lineInfos.Add(info);
                    }
                    else
                    {
                        _lineInfos[index] = info;
                    }
                    graphics.DrawString(item, _font, textBrush, point);
                    index++;
                }
            }

            DrawLineNumbers(1, _lines + 1, 1, graphics, _font, maxLineNumberLength, _stringFormat);

            if (_drawCaret)//&& textAreaRect.Contains(MousePosition.X, MousePosition.Y)
            {
                graphics.DrawLine(_caret.Pen, _caret.Points[0], _caret.Points[1]);
                _drawCaret = false;
            }
            myBuffer.Render(pe.Graphics);  //呈现图像至关联的Graphics  
            myBuffer.Dispose();
            graphics.Dispose();  

           
            base.OnPaint(pe);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //if (textAreaRect != null && textAreaRect.Contains(MousePosition.X, MousePosition.Y))
            //{
                _drawCaret = true;
                Invalidate();
            //}
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

        private void DrawLineNumbers(int start, int end, int startline, Graphics graphics, Font font, float dividerX, StringFormat sf)
        {
            var fontColor = ColorTranslator.FromHtml("#009999");
            for (int i = start; i < end; i++)
            {
                int y = (i - start -1 + startline) * (font.Height + _lineHeight);
                using (var lineNumberBrush = new SolidBrush(fontColor)) //(iLine + lineNumberStartValue).
                {
                    //pe.Graphics.DrawString(i.ToString(), font, lineNumberBrush,
                    //                     new RectangleF(0, y, spiltLineX, font.Height),//LeftIndent - minLeftIndent 
                    //                     new StringFormat(StringFormatFlags.DirectionRightToLeft));
                    //pe.Graphics.DrawString(i.ToString(), font, brush, spiltLineX - width + factor * 5, (i - 1000000 - 1 + 20) * font.Height);
                   graphics.DrawString(i.ToString(), font, lineNumberBrush,
                                   new RectangleF(Constants.LEFTINDENT, y, dividerX, font.Height), sf);
                }
            }
        }

        private void MyControl_Click(object sender, EventArgs e)
        {
            var point = MousePosition;
            _caret.Position = new Point(point.X - this.Parent.Left - this.Left - _dividerX - _textAreaPadding, point.Y - this.Top - this.Parent.Top - SystemInformation.CaptionHeight);
            ReCalcCaret();
            Invalidate();
            //if (string.IsNullOrEmpty(_text.ToString())) { 
              
            //}
        }

        private void ReCalcCaret()
        {
            int line = (int)Math.Ceiling((double)_caret.Position.Y / _rowHeight);
            if (line > _lines || line == 0) line = _lines;
            //if (line <= _lineInfos.Count) break;

            var lineInfo = _lineInfos.Count >= line && _lineInfos.Count != 0 ? _lineInfos[line - 1] : null;

            int wordIndex = (int)Math.Ceiling((_caret.Position.X) * 1.0 / _fontWidth);
            wordIndex = wordIndex < 0 ? 0 : wordIndex;
            wordIndex = (lineInfo != null && wordIndex > lineInfo.Text.Length) ? lineInfo.Text.Length : wordIndex;
            //int x = lineInfo != null ? Convert.ToInt32(lineInfo.Position.X + wordIndex * _fontWidth) : (textAreaRect.Left + _textAreaPadding);
            float x = 0, y1 = 0, y2;
            if (lineInfo == null||lineInfo.Chars.Count==0)
            {
                x = textAreaRect.Left + _textAreaPadding;
                y2 = _fontHeigth;
            }
            else
            {
                x = lineInfo.Chars[wordIndex].Position.X;
                y1 = lineInfo.Chars[wordIndex].Position.Y;
                y2 = lineInfo.Chars[wordIndex].Position.Y + _fontHeigth;
            }
            //float x = lineInfo != null ?  lineInfo.Chars[wordIndex].Position.X: (textAreaRect.Left + _textAreaPadding);

            _caret.WordIndex = wordIndex;
            _caret.Points[0] = new PointF(x,y1);// (line - 1) * _fontHeigth);
            _caret.Points[1] = new PointF(x,y2);// line * _fontHeigth);
        }

        private void MyControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            string _char = e.KeyChar.ToString();
            switch (_char) { 
                case "\r":
                    _caret.Down();
                    break;
                case "\b":
                    _caret.StepBack();
                    break;
                default:
                    _caret.StepForward();
                    break;
            }
        
            _text.Append(_char.ToString());
            //_text = _text.Replace("\r","");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Right:
                    _caret.StepForward();
                    break;
                case Keys.Left:
                    _caret.StepBack();
                    break;
                case Keys.Up:
                    _caret.Up();
                    break;
                case Keys.Down:
                    _caret.Down();
                    break;
                default:

                    break;
            }
            //若要调用KeyDown,这里一定要返回false才行,否则只响应重写方法里的按键.
            //这里调用一下父类方向,相当于调用普通的KeyDown事件.//所以按空格会弹出两个对话框
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void MyControl_Leave(object sender, EventArgs e)
        {
            _timer.Stop();
        }

        public void Dispose() {
            _timer.Dispose();
        }

        private void MyControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsInRegion(e.Location, Points))
            {
                this.Cursor = Cursors.IBeam;
            }
        }

        private void MyControl_MouseLeave(object sender, EventArgs e)
        {
            //_timer.Stop();
        }

        private void MyControl_MouseHover(object sender, EventArgs e)
        {
            _timer.Start();
        }
    }
}
