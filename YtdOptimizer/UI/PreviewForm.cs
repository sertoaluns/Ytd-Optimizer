using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using YtdOptimizer.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace YtdOptimizer.UI
{
    public class PreviewForm : Form
    {
        private CheckerboardControl _picOriginal;
        private CheckerboardControl _picOptimized;
        private Label _lblOriginalInfo;
        private Label _lblOptimizedInfo;
        private Label _lblAlphaStatus;

        public PreviewForm(string textureName, Image<Rgba32> sourceImg, TargetSize targetSize, OptimizerConfig config)
        {
            InitializeComponent();
            Text = Loc.T("PreviewTitle", textureName);
            LoadPreviews(sourceImg, targetSize, config);
        }

        private void InitializeComponent()
        {
            this.Size = new System.Drawing.Size(950, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor = System.Drawing.Color.FromArgb(245, 245, 248);

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (File.Exists(iconPath))
                    this.Icon = new System.Drawing.Icon(iconPath);
                else
                    this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 465
            };

            // Left panel: Original
            var pnlLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            _lblOriginalInfo = new Label { Dock = DockStyle.Top, Height = 40, Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold) };
            _picOriginal = new CheckerboardControl { Dock = DockStyle.Fill };
            pnlLeft.Controls.Add(_picOriginal);
            pnlLeft.Controls.Add(_lblOriginalInfo);
            split.Panel1.Controls.Add(pnlLeft);

            // Right panel: Optimized
            var pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            _lblOptimizedInfo = new Label { Dock = DockStyle.Top, Height = 40, Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold) };
            _picOptimized = new CheckerboardControl { Dock = DockStyle.Fill };
            pnlRight.Controls.Add(_picOptimized);
            pnlRight.Controls.Add(_lblOptimizedInfo);
            split.Panel2.Controls.Add(pnlRight);

            // Bottom bar
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 45, Padding = new Padding(10) };
            _lblAlphaStatus = new Label
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkGreen
            };
            var btnClose = new Button
            {
                Text = Loc.T("Close"),
                Dock = DockStyle.Right,
                Width = 100,
                DialogResult = DialogResult.OK
            };
            pnlBottom.Controls.Add(_lblAlphaStatus);
            pnlBottom.Controls.Add(btnClose);

            this.Controls.Add(split);
            this.Controls.Add(pnlBottom);
        }

        private void LoadPreviews(Image<Rgba32> sourceImg, TargetSize targetSize, OptimizerConfig config)
        {
            if (sourceImg == null) return;

            // Analyze alpha
            var alphaCheck = AlphaPreservingProcessor.CheckAlpha(sourceImg);
            string alphaText = alphaCheck.HasAlpha ? Loc.T("PreviewStatusAlpha") : Loc.T("PreviewStatusOpaque");

            _lblOriginalInfo.Text = $"{Loc.T("PreviewOriginal", $"{sourceImg.Width}x{sourceImg.Height}")}\n{alphaText}";

            // Convert ImageSharp to GDI+ Bitmap for display
            _picOriginal.Image = ToGdiBitmap(sourceImg);

            // Generate target preview
            Image<Rgba32> targetImg;
            if (targetSize.NeedsResize)
            {
                targetImg = AlphaPreservingProcessor.ResizeAlphaPreserving(sourceImg, targetSize.Width, targetSize.Height);
            }
            else
            {
                targetImg = sourceImg.Clone();
                AlphaPreservingProcessor.DilateTransparentEdges(targetImg, 2);
            }

            using (targetImg)
            {
                var valResult = AlphaPreservingProcessor.ValidateAlpha(sourceImg, targetImg, config.AlphaDiffThresholdPercent, config.TransparentRatioThresholdPercent);

                _lblOptimizedInfo.Text = $"{Loc.T("PreviewOptimized", $"{targetImg.Width}x{targetImg.Height}")}\nFormat: DDS DXT5 (BC3)";
                _picOptimized.Image = ToGdiBitmap(targetImg);

                if (valResult.Passed)
                {
                    _lblAlphaStatus.Text = Loc.T("PreviewValPassed", valResult.AverageAlphaDifferencePercent, valResult.TransparentRatioDeltaPercent);
                    _lblAlphaStatus.ForeColor = System.Drawing.Color.DarkGreen;
                }
                else
                {
                    _lblAlphaStatus.Text = Loc.T("PreviewValWarning", valResult.AverageAlphaDifferencePercent, valResult.TransparentRatioDeltaPercent) + $" ({valResult.FailureReason})";
                    _lblAlphaStatus.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private System.Drawing.Bitmap ToGdiBitmap(Image<Rgba32> image)
        {
            using (var ms = new MemoryStream())
            {
                image.SaveAsPng(ms);
                ms.Position = 0;
                return new System.Drawing.Bitmap(ms);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _picOriginal.Image?.Dispose();
            _picOptimized.Image?.Dispose();
        }
    }
}
