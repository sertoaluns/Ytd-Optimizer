using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace YtdOptimizer.UI
{
    public class CheckerboardControl : Control
    {
        private Image _image;
        private int _checkerSize = 16;
        private Color _color1 = Color.FromArgb(240, 240, 240);
        private Color _color2 = Color.FromArgb(200, 200, 200);

        public CheckerboardControl()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public Image Image
        {
            get => _image;
            set
            {
                _image = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Draw checkerboard
            using (var brush1 = new SolidBrush(_color1))
            using (var brush2 = new SolidBrush(_color2))
            {
                for (int y = 0; y < Height; y += _checkerSize)
                {
                    for (int x = 0; x < Width; x += _checkerSize)
                    {
                        bool isEven = ((x / _checkerSize) + (y / _checkerSize)) % 2 == 0;
                        g.FillRectangle(isEven ? brush1 : brush2, x, y, _checkerSize, _checkerSize);
                    }
                }
            }

            // Draw image centered maintaining aspect ratio
            if (_image != null)
            {
                float scale = Math.Min((float)Width / _image.Width, (float)Height / _image.Height);
                int drawW = (int)(_image.Width * scale);
                int drawH = (int)(_image.Height * scale);
                int drawX = (Width - drawW) / 2;
                int drawY = (Height - drawH) / 2;

                g.DrawImage(_image, new Rectangle(drawX, drawY, drawW, drawH));

                // Border around image
                using (var pen = new Pen(Color.FromArgb(100, 0, 0, 0), 1))
                {
                    g.DrawRectangle(pen, drawX, drawY, drawW, drawH);
                }
            }
            else
            {
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var brush = new SolidBrush(Color.Gray))
                {
                    g.DrawString("(Önizleme yok)", Font, brush, ClientRectangle, sf);
                }
            }
        }
    }
}
