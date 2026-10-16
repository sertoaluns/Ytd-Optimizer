using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using YtdOptimizer.Core;

namespace YtdOptimizer.UI
{
    public class SettingsForm : Form
    {
        private readonly OptimizerConfig _config;

        private ComboBox _cmbLanguage;
        private TextBox _txtGta5Path;
        private TextBox _txtCodeWalkerPath;
        private TextBox _txtTexconvPath;

        private NumericUpDown _numSquare;
        private NumericUpDown _numWideW;
        private NumericUpDown _numWideH;
        private NumericUpDown _numTallW;
        private NumericUpDown _numTallH;
        private NumericUpDown _numLongEdge;

        private CheckBox _chkBackup;
        private CheckBox _chkSkipNormal;
        private CheckBox _chkSkipSpec;
        private CheckBox _chkConvertFormat;
        private NumericUpDown _numParallel;

        private NumericUpDown _numAlphaDiff;
        private NumericUpDown _numTransDelta;

        public SettingsForm(OptimizerConfig config)
        {
            _config = config ?? OptimizerConfig.Default();
            InitializeComponent();
            LoadConfigValues();
        }

        private void InitializeComponent()
        {
            this.Text = Loc.T("SettingsTitle");
            this.Size = new Size(640, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("Segoe UI", 9F);

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (File.Exists(iconPath))
                    this.Icon = new System.Drawing.Icon(iconPath);
                else
                    this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }

            var tabControl = new TabControl { Dock = DockStyle.Fill };

            // TAB 1: Genel & Dosya Yolları
            var tabGeneral = new TabPage(Loc.T("TabGeneral"));
            SetupGeneralTab(tabGeneral);

            // TAB 2: Boyut Kuralları
            var tabSizes = new TabPage(Loc.T("TabSizes"));
            SetupSizesTab(tabSizes);

            // TAB 3: Şeffaflık & Doğrulama
            var tabAlpha = new TabPage(Loc.T("TabAlpha"));
            SetupAlphaTab(tabAlpha);

            tabControl.TabPages.Add(tabGeneral);
            tabControl.TabPages.Add(tabSizes);
            tabControl.TabPages.Add(tabAlpha);

            // Bottom Buttons
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(10) };
            var btnSave = new Button { Text = Loc.T("Save"), DialogResult = DialogResult.OK, Width = 95, Dock = DockStyle.Right };
            var btnCancel = new Button { Text = Loc.T("Cancel"), DialogResult = DialogResult.Cancel, Width = 95, Dock = DockStyle.Right };
            btnSave.Click += BtnSave_Click;

            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(new Panel { Width = 10, Dock = DockStyle.Right }); // spacer
            pnlBottom.Controls.Add(btnSave);

            this.Controls.Add(tabControl);
            this.Controls.Add(pnlBottom);
        }

        private void SetupGeneralTab(TabPage tab)
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(15) };
            int y = 15;

            // Language Selector
            var lblLang = new Label { Text = Loc.T("Language"), Location = new Point(15, y + 3), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            _cmbLanguage = new ComboBox
            {
                Location = new Point(160, y),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbLanguage.Items.AddRange(new object[] { "🇹🇷 Türkçe", "🇬🇧 English", "🇩🇪 Deutsch" });
            pnl.Controls.AddRange(new Control[] { lblLang, _cmbLanguage });
            y += 40;

            // GTA 5
            var lblGta = new Label { Text = Loc.T("Gta5Folder"), Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            y += 22;
            _txtGta5Path = new TextBox { Location = new Point(15, y), Width = 460 };
            var btnBrowseGta = new Button { Text = Loc.T("Browse"), Location = new Point(485, y - 2), Width = 80 };
            btnBrowseGta.Click += (s, e) => BrowseFolder("GTA 5 Folder", _txtGta5Path);
            pnl.Controls.AddRange(new Control[] { lblGta, _txtGta5Path, btnBrowseGta });
            y += 35;

            // CodeWalker
            var lblCw = new Label { Text = Loc.T("CodeWalkerFolder"), Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            y += 22;
            _txtCodeWalkerPath = new TextBox { Location = new Point(15, y), Width = 460 };
            var btnBrowseCw = new Button { Text = Loc.T("Browse"), Location = new Point(485, y - 2), Width = 80 };
            btnBrowseCw.Click += (s, e) => BrowseFolder("CodeWalker Folder", _txtCodeWalkerPath);
            pnl.Controls.AddRange(new Control[] { lblCw, _txtCodeWalkerPath, btnBrowseCw });
            y += 35;

            // texconv.exe
            var lblTex = new Label { Text = Loc.T("TexconvPath"), Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            y += 22;
            _txtTexconvPath = new TextBox { Location = new Point(15, y), Width = 460 };
            var btnBrowseTex = new Button { Text = Loc.T("Browse"), Location = new Point(485, y - 2), Width = 80 };
            btnBrowseTex.Click += (s, e) => BrowseFile("texconv.exe", "texconv.exe (*.exe)|*.exe", _txtTexconvPath);
            pnl.Controls.AddRange(new Control[] { lblTex, _txtTexconvPath, btnBrowseTex });
            y += 45;

            // Yedekleme
            _chkBackup = new CheckBox
            {
                Text = Loc.T("BackupOriginal"),
                Location = new Point(15, y),
                AutoSize = true
            };
            pnl.Controls.Add(_chkBackup);
            y += 30;

            // Normal ve Spec haritaları
            _chkSkipNormal = new CheckBox
            {
                Text = Loc.T("SkipNormal"),
                Location = new Point(15, y),
                AutoSize = true
            };
            pnl.Controls.Add(_chkSkipNormal);
            y += 25;

            _chkSkipSpec = new CheckBox
            {
                Text = Loc.T("SkipSpecular"),
                Location = new Point(15, y),
                AutoSize = true
            };
            pnl.Controls.Add(_chkSkipSpec);
            y += 25;

            _chkConvertFormat = new CheckBox
            {
                Text = Loc.T("ConvertIfNotDxt5"),
                Location = new Point(15, y),
                AutoSize = true
            };
            pnl.Controls.Add(_chkConvertFormat);
            y += 35;

            // Paralel işlem sayısı
            var lblParallel = new Label { Text = Loc.T("ParallelThreads"), Location = new Point(15, y + 2), AutoSize = true };
            _numParallel = new NumericUpDown { Location = new Point(240, y), Width = 70, Minimum = 1, Maximum = 16, Value = 4 };
            pnl.Controls.AddRange(new Control[] { lblParallel, _numParallel });
            y += 35;

            var lblFmtInfo = new Label
            {
                Text = "* Format: Microsoft DirectXTex texconv -> DDS DXT5 (BC3_UNORM).",
                ForeColor = Color.DimGray,
                Location = new Point(15, y),
                AutoSize = true
            };
            pnl.Controls.Add(lblFmtInfo);

            tab.Controls.Add(pnl);
        }

        private void SetupSizesTab(TabPage tab)
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(15) };
            int y = 15;

            var lblDesc = new Label
            {
                Text = "Hedef boyutlar: Oran her zaman korunur ve 2'nin kuvvetine yuvarlanır.\nHedeften küçük dokular asla büyütülmez.",
                Location = new Point(15, y),
                AutoSize = true,
                ForeColor = Color.DarkSlateBlue
            };
            pnl.Controls.Add(lblDesc);
            y += 40;

            // Kare dokular
            var lblSquare = new Label { Text = Loc.T("SquareRule"), Location = new Point(15, y + 2), AutoSize = true };
            _numSquare = new NumericUpDown { Location = new Point(340, y), Width = 90, Minimum = 64, Maximum = 4096, Increment = 64 };
            pnl.Controls.AddRange(new Control[] { lblSquare, _numSquare });
            y += 35;

            // 2:1 Yatay dokular
            var lblWide = new Label { Text = Loc.T("WideRule"), Location = new Point(15, y + 2), AutoSize = true };
            _numWideW = new NumericUpDown { Location = new Point(340, y), Width = 75, Minimum = 64, Maximum = 4096, Increment = 64 };
            var lblX1 = new Label { Text = "x", Location = new Point(420, y + 2), AutoSize = true };
            _numWideH = new NumericUpDown { Location = new Point(435, y), Width = 75, Minimum = 64, Maximum = 4096, Increment = 64 };
            pnl.Controls.AddRange(new Control[] { lblWide, _numWideW, lblX1, _numWideH });
            y += 35;

            // 1:2 Dikey dokular
            var lblTall = new Label { Text = Loc.T("TallRule"), Location = new Point(15, y + 2), AutoSize = true };
            _numTallW = new NumericUpDown { Location = new Point(340, y), Width = 75, Minimum = 64, Maximum = 4096, Increment = 64 };
            var lblX2 = new Label { Text = "x", Location = new Point(420, y + 2), AutoSize = true };
            _numTallH = new NumericUpDown { Location = new Point(435, y), Width = 75, Minimum = 64, Maximum = 4096, Increment = 64 };
            pnl.Controls.AddRange(new Control[] { lblTall, _numTallW, lblX2, _numTallH });
            y += 35;

            // Diğer oranlar
            var lblOther = new Label { Text = Loc.T("GenericRule"), Location = new Point(15, y + 2), AutoSize = true };
            _numLongEdge = new NumericUpDown { Location = new Point(340, y), Width = 90, Minimum = 64, Maximum = 4096, Increment = 64 };
            pnl.Controls.AddRange(new Control[] { lblOther, _numLongEdge });

            tab.Controls.Add(pnl);
        }

        private void SetupAlphaTab(TabPage tab)
        {
            var pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(15) };
            int y = 15;

            var lblDesc = new Label
            {
                Text = Loc.T("AlphaInfoText"),
                Location = new Point(15, y),
                AutoSize = true,
                ForeColor = Color.DarkSlateBlue
            };
            pnl.Controls.Add(lblDesc);
            y += 125;

            var lblDiff = new Label { Text = Loc.T("AlphaDiffThreshold"), Location = new Point(15, y + 2), AutoSize = true };
            _numAlphaDiff = new NumericUpDown { Location = new Point(350, y), Width = 80, DecimalPlaces = 1, Minimum = 0.5M, Maximum = 20.0M, Increment = 0.5M };
            pnl.Controls.AddRange(new Control[] { lblDiff, _numAlphaDiff });
            y += 35;

            var lblRatio = new Label { Text = Loc.T("TransparentRatioThreshold"), Location = new Point(15, y + 2), AutoSize = true };
            _numTransDelta = new NumericUpDown { Location = new Point(350, y), Width = 80, DecimalPlaces = 1, Minimum = 0.5M, Maximum = 20.0M, Increment = 0.5M };
            pnl.Controls.AddRange(new Control[] { lblRatio, _numTransDelta });
            y += 45;

            var lblNotice = new Label
            {
                Text = "* Note: Textures failing alpha verification are NEVER modified and keep their original data.",
                ForeColor = Color.DarkRed,
                Location = new Point(15, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic)
            };
            pnl.Controls.Add(lblNotice);

            tab.Controls.Add(pnl);
        }

        private void LoadConfigValues()
        {
            string lang = _config.Language ?? "tr";
            if (lang == "en") _cmbLanguage.SelectedIndex = 1;
            else if (lang == "de") _cmbLanguage.SelectedIndex = 2;
            else _cmbLanguage.SelectedIndex = 0;

            _txtGta5Path.Text = _config.Gta5Folder;
            _txtCodeWalkerPath.Text = _config.CodeWalkerFolder;
            _txtTexconvPath.Text = _config.TexconvPath;

            _chkBackup.Checked = _config.BackupOriginal;
            _chkSkipNormal.Checked = _config.SkipNormalMaps;
            _chkSkipSpec.Checked = _config.SkipSpecularMaps;
            _chkConvertFormat.Checked = _config.ConvertFormatIfNotDxt5;
            _numParallel.Value = Math.Max(1, Math.Min(16, _config.ParallelDegree));

            _numSquare.Value = _config.MaxSquareDimension;
            _numWideW.Value = _config.MaxWideWidth;
            _numWideH.Value = _config.MaxWideHeight;
            _numTallW.Value = _config.MaxTallWidth;
            _numTallH.Value = _config.MaxTallHeight;
            _numLongEdge.Value = _config.MaxGenericLongEdge;

            _numAlphaDiff.Value = (decimal)_config.AlphaDiffThresholdPercent;
            _numTransDelta.Value = (decimal)_config.TransparentRatioThresholdPercent;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (_cmbLanguage.SelectedIndex == 1) _config.Language = "en";
            else if (_cmbLanguage.SelectedIndex == 2) _config.Language = "de";
            else _config.Language = "tr";

            Loc.SetLanguage(_config.Language);

            _config.Gta5Folder = _txtGta5Path.Text.Trim();
            _config.CodeWalkerFolder = _txtCodeWalkerPath.Text.Trim();
            _config.TexconvPath = _txtTexconvPath.Text.Trim();

            _config.BackupOriginal = _chkBackup.Checked;
            _config.SkipNormalMaps = _chkSkipNormal.Checked;
            _config.SkipSpecularMaps = _chkSkipSpec.Checked;
            _config.ConvertFormatIfNotDxt5 = _chkConvertFormat.Checked;
            _config.ParallelDegree = (int)_numParallel.Value;

            _config.MaxSquareDimension = (int)_numSquare.Value;
            _config.MaxWideWidth = (int)_numWideW.Value;
            _config.MaxWideHeight = (int)_numWideH.Value;
            _config.MaxTallWidth = (int)_numTallW.Value;
            _config.MaxTallHeight = (int)_numTallH.Value;
            _config.MaxGenericLongEdge = (int)_numLongEdge.Value;

            _config.AlphaDiffThresholdPercent = (double)_numAlphaDiff.Value;
            _config.TransparentRatioThresholdPercent = (double)_numTransDelta.Value;

            ConfigService.Save(_config);
        }

        private void BrowseFolder(string title, TextBox target)
        {
            using (var fbd = new FolderBrowserDialog { Description = title, ShowNewFolderButton = false })
            {
                if (!string.IsNullOrEmpty(target.Text) && Directory.Exists(target.Text))
                {
                    fbd.SelectedPath = target.Text;
                }
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    target.Text = fbd.SelectedPath;
                }
            }
        }

        private void BrowseFile(string title, string filter, TextBox target)
        {
            using (var ofd = new OpenFileDialog { Title = title, Filter = filter })
            {
                if (!string.IsNullOrEmpty(target.Text) && File.Exists(target.Text))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(target.Text);
                }
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    target.Text = ofd.FileName;
                }
            }
        }
    }
}
