using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using YtdOptimizer.Core;

namespace YtdOptimizer.UI
{
    public class MainForm : Form
    {
        private OptimizerConfig _config;
        private EnvironmentCheckResult _envCheck;

        private readonly List<DrawableItem> _drawables = new List<DrawableItem>();
        private readonly List<YtdFileItem> _allYtdFiles = new List<YtdFileItem>();
        private readonly List<YtdFileItem> _displayedYtdFiles = new List<YtdFileItem>();

        // Status Strip
        private ToolStripStatusLabel _statusLabel;
        private ToolStripStatusLabel _envWarningLabel;
        private ToolStripProgressBar _progressBar;

        // Top Toolbar & Folder List
        private Label _lblFolderTitle;
        private ListBox _lbFolders;
        private Button _btnAddFolder;
        private Button _btnRemoveFolder;
        private Button _btnClearFolders;
        private Button _btnAnalyze;
        private Button _btnOptimizeSelected;
        private Button _btnOptimizeAll;
        private Button _btnSettings;
        private ComboBox _cmbLanguage;

        // Summary Bar
        private Label _lblSummaryTotalTextures;
        private Label _lblSummaryTexturesToChange;
        private Label _lblSummaryTotalSize;
        private Label _lblSummaryEstimatedSaving;

        // Left Panel (Drawables & YTDs)
        private Label _lblSearchTitle;
        private TextBox _txtSearch;
        private Label _lblListCounter;
        private ListView _lvDrawables;

        // Right Panel (Textures of selected drawable/YTD)
        private ListView _lvTextures;
        private ContextMenuStrip _ctxTextureMenu;
        private ToolStripMenuItem _miPreview;
        private ToolStripMenuItem _miReplace;
        private ToolStripMenuItem _miDelete;

        // Bottom log
        private Label _lblLogTitle;
        private RichTextBox _rtbLog;

        private CancellationTokenSource _cts;

        public MainForm()
        {
            _config = ConfigService.Load();
            Loc.SetLanguage(_config.Language);

            InitializeComponent();
            CheckEnvironmentOnStartup();

            ApplyLanguage();
            Loc.LanguageChanged += ApplyLanguage;
        }

        private void InitializeComponent()
        {
            this.Text = Loc.T("AppTitle");
            this.Size = new System.Drawing.Size(1300, 850);
            this.MinimumSize = new System.Drawing.Size(1020, 680);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.AllowDrop = true;
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;

            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new System.Drawing.Icon(iconPath);
                }
                else
                {
                    this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                }
            }
            catch { }

            // Main Layout Container
            var pnlTopContainer = new Panel { Dock = DockStyle.Top, Height = 135, BackColor = System.Drawing.Color.FromArgb(248, 249, 250) };
            var pnlMainSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 490 };
            var pnlBottomLog = new Panel { Dock = DockStyle.Bottom, Height = 140, BackColor = System.Drawing.Color.FromArgb(245, 245, 245) };

            // Status strip
            var statusStrip = new StatusStrip { Dock = DockStyle.Bottom };
            _statusLabel = new ToolStripStatusLabel(Loc.T("Ready")) { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _envWarningLabel = new ToolStripStatusLabel { ForeColor = System.Drawing.Color.Red, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            _progressBar = new ToolStripProgressBar { Width = 200, Visible = false };
            statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel, _envWarningLabel, _progressBar });

            // TOP TOOLBAR
            SetupTopToolbar(pnlTopContainer);

            // LEFT PANEL: Drawables / YTD list
            SetupLeftPanel(pnlMainSplit.Panel1);

            // RIGHT PANEL: Textures list
            SetupRightPanel(pnlMainSplit.Panel2);

            // BOTTOM LOG
            SetupBottomLog(pnlBottomLog);

            this.Controls.Add(pnlMainSplit);
            this.Controls.Add(pnlBottomLog);
            this.Controls.Add(pnlTopContainer);
            this.Controls.Add(statusStrip);
        }

        private void SetupTopToolbar(Panel pnl)
        {
            var pnlButtons = new Panel { Dock = DockStyle.Top, Height = 85, Padding = new Padding(10, 8, 10, 5) };

            _lblFolderTitle = new Label { Text = Loc.T("FoldersLabel"), Location = new System.Drawing.Point(10, 10), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            
            _lbFolders = new ListBox
            {
                Location = new System.Drawing.Point(75, 8),
                Width = 430,
                Height = 68,
                Font = new System.Drawing.Font("Segoe UI", 8.5F),
                SelectionMode = SelectionMode.MultiExtended,
                HorizontalScrollbar = true
            };

            // Folder management buttons
            _btnAddFolder = new Button { Text = Loc.T("AddFolder"), Location = new System.Drawing.Point(515, 8), Width = 110, Height = 28 };
            _btnAddFolder.Click += BtnAddFolder_Click;

            _btnRemoveFolder = new Button { Text = Loc.T("RemoveFolder"), Location = new System.Drawing.Point(515, 42), Width = 110, Height = 28 };
            _btnRemoveFolder.Click += BtnRemoveFolder_Click;

            _btnClearFolders = new Button { Text = Loc.T("ClearFolders"), Location = new System.Drawing.Point(632, 42), Width = 75, Height = 28 };
            _btnClearFolders.Click += BtnClearFolders_Click;

            // Action buttons
            _btnAnalyze = new Button { Text = Loc.T("Analyze"), Location = new System.Drawing.Point(632, 8), Width = 100, Height = 28, BackColor = System.Drawing.Color.FromArgb(230, 240, 255), Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Bold) };
            _btnAnalyze.Click += async (s, e) => await AnalyzeFolderAsync();

            _btnOptimizeSelected = new Button { Text = Loc.T("OptimizeSelected"), Location = new System.Drawing.Point(740, 8), Width = 160, Height = 62, BackColor = System.Drawing.Color.FromArgb(255, 243, 205), Font = new System.Drawing.Font("Segoe UI", 9.5F, FontStyle.Bold) };
            _btnOptimizeSelected.Click += async (s, e) => await OptimizeSelectedAsync();

            _btnOptimizeAll = new Button { Text = Loc.T("OptimizeAll"), Location = new System.Drawing.Point(908, 8), Width = 160, Height = 62, BackColor = System.Drawing.Color.FromArgb(212, 237, 218), Font = new System.Drawing.Font("Segoe UI", 9.5F, FontStyle.Bold) };
            _btnOptimizeAll.Click += async (s, e) => await OptimizeAllAsync();

            _btnSettings = new Button { Text = Loc.T("Settings"), Location = new System.Drawing.Point(1076, 8), Width = 85, Height = 62 };
            _btnSettings.Click += BtnSettings_Click;

            // Language ComboBox
            _cmbLanguage = new ComboBox
            {
                Location = new System.Drawing.Point(1170, 8),
                Width = 105,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new System.Drawing.Font("Segoe UI", 8.5F)
            };
            _cmbLanguage.Items.AddRange(new object[] { "🇹🇷 TR", "🇬🇧 EN", "🇩🇪 DE" });
            string curLang = _config.Language ?? "tr";
            _cmbLanguage.SelectedIndex = (curLang == "en") ? 1 : ((curLang == "de") ? 2 : 0);
            _cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;

            pnlButtons.Controls.AddRange(new Control[] {
                _lblFolderTitle, _lbFolders, _btnAddFolder, _btnRemoveFolder, _btnClearFolders,
                _btnAnalyze, _btnOptimizeSelected, _btnOptimizeAll, _btnSettings, _cmbLanguage
            });

            // SUMMARY BAR
            var pnlSummary = new Panel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(15, 5, 15, 5), BackColor = System.Drawing.Color.FromArgb(235, 238, 242) };
            var flowSummary = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };

            _lblSummaryTotalTextures = new Label { AutoSize = true, Margin = new Padding(5, 10, 25, 0), Font = new System.Drawing.Font("Segoe UI", 9.5F, FontStyle.Bold), Text = Loc.T("SummaryTotalTextures", 0) };
            _lblSummaryTexturesToChange = new Label { AutoSize = true, Margin = new Padding(5, 10, 25, 0), Font = new System.Drawing.Font("Segoe UI", 9.5F, FontStyle.Bold), Text = Loc.T("SummaryTexturesToChange", 0), ForeColor = System.Drawing.Color.Crimson };
            _lblSummaryTotalSize = new Label { AutoSize = true, Margin = new Padding(5, 10, 25, 0), Font = new System.Drawing.Font("Segoe UI", 9.5F, FontStyle.Bold), Text = Loc.T("SummaryTotalSize", 0.0), };
            _lblSummaryEstimatedSaving = new Label { AutoSize = true, Margin = new Padding(5, 10, 25, 0), Font = new System.Drawing.Font("Segoe UI", 9.5F, FontStyle.Bold), Text = Loc.T("SummaryEstimatedSavings", 0.0), ForeColor = System.Drawing.Color.DarkGreen };

            flowSummary.Controls.AddRange(new Control[] { _lblSummaryTotalTextures, _lblSummaryTexturesToChange, _lblSummaryTotalSize, _lblSummaryEstimatedSaving });
            pnlSummary.Controls.Add(flowSummary);

            pnl.Controls.Add(pnlButtons);
            pnl.Controls.Add(pnlSummary);
        }

        private void SetupLeftPanel(SplitterPanel pnl)
        {
            var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(5) };
            _lblSearchTitle = new Label { Text = Loc.T("FilterLabel"), Location = new System.Drawing.Point(5, 10), AutoSize = true };
            _txtSearch = new TextBox { Location = new System.Drawing.Point(55, 8), Width = 200 };
            _txtSearch.TextChanged += TxtSearch_TextChanged;
            _lblListCounter = new Label { Location = new System.Drawing.Point(265, 10), AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 8.5F, FontStyle.Italic) };

            pnlSearch.Controls.AddRange(new Control[] { _lblSearchTitle, _txtSearch, _lblListCounter });

            _lvDrawables = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = true
            };
            _lvDrawables.Columns.Add(Loc.T("ColDrawableFile"), 150);
            _lvDrawables.Columns.Add(Loc.T("ColYtdFile"), 140);
            _lvDrawables.Columns.Add(Loc.T("ColFolder"), 120);
            _lvDrawables.Columns.Add(Loc.T("ColSize"), 70);
            _lvDrawables.SelectedIndexChanged += LvDrawables_SelectedIndexChanged;

            pnl.Controls.Add(_lvDrawables);
            pnl.Controls.Add(pnlSearch);
        }

        private void SetupRightPanel(SplitterPanel pnl)
        {
            _lvTextures = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };
            _lvTextures.Columns.Add(Loc.T("ColTexName"), 170);
            _lvTextures.Columns.Add(Loc.T("ColTexFormat"), 90);
            _lvTextures.Columns.Add(Loc.T("ColTexOldSize"), 85);
            _lvTextures.Columns.Add(Loc.T("ColTexNewSize"), 85);
            _lvTextures.Columns.Add(Loc.T("ColTexFileSize"), 85);
            _lvTextures.Columns.Add(Loc.T("ColTexAlpha"), 80);
            _lvTextures.Columns.Add(Loc.T("ColTexStatus"), 100);

            // Context Menu for right click
            _ctxTextureMenu = new ContextMenuStrip();
            _miPreview = new ToolStripMenuItem(Loc.T("MenuPreview"));
            _miPreview.Click += MiPreview_Click;

            _miReplace = new ToolStripMenuItem(Loc.T("MenuReplace"));
            _miReplace.Click += MiReplace_Click;

            _miDelete = new ToolStripMenuItem(Loc.T("MenuDelete"));
            _miDelete.Click += MiDelete_Click;

            _ctxTextureMenu.Items.AddRange(new ToolStripItem[] { _miPreview, new ToolStripSeparator(), _miReplace, _miDelete });
            _lvTextures.ContextMenuStrip = _ctxTextureMenu;
            _lvTextures.DoubleClick += (s, e) => MiPreview_Click(s, e);

            pnl.Controls.Add(_lvTextures);
        }

        private void SetupBottomLog(Panel pnl)
        {
            _lblLogTitle = new Label { Text = "Log:", Dock = DockStyle.Top, Height = 22, Padding = new Padding(5, 3, 0, 0), Font = new System.Drawing.Font("Segoe UI", 8.5F, FontStyle.Bold) };
            _rtbLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
                ForeColor = System.Drawing.Color.Gainsboro,
                Font = new System.Drawing.Font("Consolas", 9F)
            };
            pnl.Controls.Add(_rtbLog);
            pnl.Controls.Add(_lblLogTitle);
        }

        private void ApplyLanguage()
        {
            this.Text = Loc.T("AppTitle");
            if (_lblFolderTitle != null) _lblFolderTitle.Text = Loc.T("FoldersLabel");
            if (_btnAddFolder != null) _btnAddFolder.Text = Loc.T("AddFolder");
            if (_btnRemoveFolder != null) _btnRemoveFolder.Text = Loc.T("RemoveFolder");
            if (_btnClearFolders != null) _btnClearFolders.Text = Loc.T("ClearFolders");
            if (_btnAnalyze != null) _btnAnalyze.Text = Loc.T("Analyze");
            if (_btnOptimizeSelected != null) _btnOptimizeSelected.Text = Loc.T("OptimizeSelected");
            if (_btnOptimizeAll != null) _btnOptimizeAll.Text = Loc.T("OptimizeAll");
            if (_btnSettings != null) _btnSettings.Text = Loc.T("Settings");
            if (_lblSearchTitle != null) _lblSearchTitle.Text = Loc.T("FilterLabel");
            if (_statusLabel != null && _statusLabel.Text == "Ready" || _statusLabel.Text == "Hazır" || _statusLabel.Text == "Bereit")
            {
                _statusLabel.Text = Loc.T("Ready");
            }

            if (_lvDrawables != null && _lvDrawables.Columns.Count >= 4)
            {
                _lvDrawables.Columns[0].Text = Loc.T("ColDrawableFile");
                _lvDrawables.Columns[1].Text = Loc.T("ColYtdFile");
                _lvDrawables.Columns[2].Text = Loc.T("ColFolder");
                _lvDrawables.Columns[3].Text = Loc.T("ColSize");
            }

            if (_lvTextures != null && _lvTextures.Columns.Count >= 7)
            {
                _lvTextures.Columns[0].Text = Loc.T("ColTexName");
                _lvTextures.Columns[1].Text = Loc.T("ColTexFormat");
                _lvTextures.Columns[2].Text = Loc.T("ColTexOldSize");
                _lvTextures.Columns[3].Text = Loc.T("ColTexNewSize");
                _lvTextures.Columns[4].Text = Loc.T("ColTexFileSize");
                _lvTextures.Columns[5].Text = Loc.T("ColTexAlpha");
                _lvTextures.Columns[6].Text = Loc.T("ColTexStatus");
            }

            if (_miPreview != null) _miPreview.Text = Loc.T("MenuPreview");
            if (_miReplace != null) _miReplace.Text = Loc.T("MenuReplace");
            if (_miDelete != null) _miDelete.Text = Loc.T("MenuDelete");

            UpdateSummaryCounters();
            PopulateDrawablesList();
            LvDrawables_SelectedIndexChanged(null, null);
        }

        private void CmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string chosen = (_cmbLanguage.SelectedIndex == 1) ? "en" : ((_cmbLanguage.SelectedIndex == 2) ? "de" : "tr");
            if (_config.Language != chosen)
            {
                _config.Language = chosen;
                ConfigService.Save(_config);
                Loc.SetLanguage(chosen);
            }
        }

        private void Log(string message, System.Drawing.Color? color = null)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Log(message, color)));
                return;
            }

            _rtbLog.SelectionStart = _rtbLog.TextLength;
            _rtbLog.SelectionLength = 0;
            _rtbLog.SelectionColor = color ?? System.Drawing.Color.Gainsboro;
            _rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            _rtbLog.ScrollToCaret();
        }

        private void CheckEnvironmentOnStartup()
        {
            _envCheck = EnvironmentChecker.Check(_config);

            if (!_envCheck.AllValid)
            {
                var errList = new List<string>();
                if (!_envCheck.Gta5Installed) errList.Add("GTA 5 missing/not found");
                if (!_envCheck.CodeWalkerValid) errList.Add("CodeWalker.Core.dll missing");
                if (!_envCheck.TexconvValid) errList.Add("texconv.exe missing");

                _envWarningLabel.Text = $"⚠ [{string.Join(" | ", errList)}]";
                _btnOptimizeSelected.Enabled = false;
                _btnOptimizeAll.Enabled = false;
                Log($"[UYARI / WARNING] {string.Join(" - ", errList)}", System.Drawing.Color.OrangeRed);
            }
            else
            {
                _envWarningLabel.Text = string.Empty;
                Log("[BİLGİ / INFO] GTA5, CodeWalker.Core, texconv.exe OK.");
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private async void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (paths != null && paths.Length > 0)
            {
                bool addedAny = false;
                foreach (var p in paths)
                {
                    if (Directory.Exists(p))
                    {
                        AddFolder(p);
                        addedAny = true;
                    }
                    else if (File.Exists(p) && (p.EndsWith(".ytd", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".ydd", StringComparison.OrdinalIgnoreCase)))
                    {
                        string dir = Path.GetDirectoryName(p);
                        if (!string.IsNullOrEmpty(dir))
                        {
                            AddFolder(dir);
                            addedAny = true;
                        }
                    }
                }

                if (addedAny)
                {
                    await AnalyzeFolderAsync();
                }
            }
        }

        private void BtnAddFolder_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog { Description = "GTA 5 Mod / grzyClothTool Export Klasörünü Seçin", ShowNewFolderButton = false })
            {
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    AddFolder(fbd.SelectedPath);
                }
            }
        }

        private void BtnRemoveFolder_Click(object sender, EventArgs e)
        {
            var selected = _lbFolders.SelectedItems.Cast<string>().ToList();
            foreach (var item in selected)
            {
                _lbFolders.Items.Remove(item);
            }
        }

        private void BtnClearFolders_Click(object sender, EventArgs e)
        {
            _lbFolders.Items.Clear();
        }

        private void AddFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            path = Path.GetFullPath(path.Trim());
            if (Directory.Exists(path))
            {
                foreach (var item in _lbFolders.Items)
                {
                    if (string.Equals(item.ToString(), path, StringComparison.OrdinalIgnoreCase))
                        return;
                }
                _lbFolders.Items.Add(path);
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var sf = new SettingsForm(_config))
            {
                if (sf.ShowDialog(this) == DialogResult.OK)
                {
                    CheckEnvironmentOnStartup();
                    Log("[BİLGİ] Ayarlar güncellendi ve kaydedildi.");
                    ApplyLanguage();
                }
            }
        }

        private async Task AnalyzeFolderAsync()
        {
            if (_lbFolders.Items.Count == 0)
            {
                MessageBox.Show(Loc.T("NoFolderWarning"), Loc.T("WarningTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var folderList = _lbFolders.Items.Cast<string>().ToList();

            _btnAnalyze.Enabled = false;
            _btnOptimizeSelected.Enabled = false;
            _btnOptimizeAll.Enabled = false;
            _progressBar.Visible = true;
            _statusLabel.Text = Loc.T("ScanningFolders");
            Log(Loc.T("LogScanning", folderList.Count));

            _drawables.Clear();
            _allYtdFiles.Clear();
            _displayedYtdFiles.Clear();
            _lvDrawables.Items.Clear();
            _lvTextures.Items.Clear();

            await Task.Run(() =>
            {
                var allYddFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var allYtdFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var folder in folderList)
                {
                    if (Directory.Exists(folder))
                    {
                        foreach (var ydd in Directory.GetFiles(folder, "*.ydd", SearchOption.AllDirectories))
                            allYddFiles.Add(ydd);

                        foreach (var ytd in Directory.GetFiles(folder, "*.ytd", SearchOption.AllDirectories))
                            allYtdFiles.Add(ytd);
                    }
                }

                var drawableDict = new Dictionary<string, DrawableItem>(StringComparer.OrdinalIgnoreCase);

                foreach (var ydd in allYddFiles)
                {
                    string dName = Path.GetFileNameWithoutExtension(ydd);
                    if (!drawableDict.ContainsKey(dName))
                    {
                        drawableDict[dName] = new DrawableItem
                        {
                            Name = dName,
                            YddFilePath = ydd
                        };
                    }
                }

                // Analyze each YTD
                int cur = 0;
                foreach (var ytdPath in allYtdFiles)
                {
                    cur++;
                    var ytdItem = YtdBatchOptimizer.AnalyzeYtd(ytdPath, _config);
                    _allYtdFiles.Add(ytdItem);

                    // Try to match with drawables
                    string ytdName = Path.GetFileNameWithoutExtension(ytdPath);
                    bool matched = false;

                    foreach (var kvp in drawableDict)
                    {
                        if (FileMatcher.IsMatch(kvp.Key, ytdName))
                        {
                            kvp.Value.MatchedYtds.Add(ytdItem);
                            ytdItem.MatchedDrawableName = kvp.Key;
                            matched = true;
                            break;
                        }
                    }

                    if (!matched)
                    {
                        ytdItem.MatchedDrawableName = "(Eşleşmedi)";
                    }
                }

                _drawables.AddRange(drawableDict.Values);
            });

            _progressBar.Visible = false;
            _statusLabel.Text = Loc.T("AnalysisCompleted", _allYtdFiles.Count, _drawables.Count);
            Log(Loc.T("LogCompleted", _allYtdFiles.Count));

            PopulateDrawablesList();
            UpdateSummaryCounters();

            _btnAnalyze.Enabled = true;
            _btnOptimizeSelected.Enabled = _envCheck.AllValid && _allYtdFiles.Count > 0;
            _btnOptimizeAll.Enabled = _envCheck.AllValid && _allYtdFiles.Count > 0;
        }

        private void PopulateDrawablesList()
        {
            _lvDrawables.BeginUpdate();
            _lvDrawables.Items.Clear();
            _displayedYtdFiles.Clear();

            string search = _txtSearch.Text.Trim().ToLowerInvariant();

            // First add drawables and their matched YTDs
            foreach (var d in _drawables)
            {
                foreach (var ytd in d.MatchedYtds)
                {
                    if (string.IsNullOrEmpty(search) || d.Name.ToLowerInvariant().Contains(search) || ytd.FileName.ToLowerInvariant().Contains(search) || ytd.FolderName.ToLowerInvariant().Contains(search))
                    {
                        var item = new ListViewItem(d.Name);
                        item.SubItems.Add(ytd.FileName);
                        item.SubItems.Add(ytd.FolderName);
                        item.SubItems.Add($"{(ytd.FileSizeBytes / 1024.0 / 1024.0):F2} MB");
                        item.Tag = ytd;

                        if (ytd.TexturesToOptimizeCount > 0)
                        {
                            item.ForeColor = System.Drawing.Color.DarkRed;
                        }
                        else
                        {
                            item.ForeColor = System.Drawing.Color.DarkGreen;
                        }

                        _lvDrawables.Items.Add(item);
                        _displayedYtdFiles.Add(ytd);
                    }
                }
            }

            // Next add unmatched YTDs
            foreach (var ytd in _allYtdFiles.Where(y => !y.IsMatched))
            {
                if (string.IsNullOrEmpty(search) || ytd.FileName.ToLowerInvariant().Contains(search) || ytd.FolderName.ToLowerInvariant().Contains(search))
                {
                    var item = new ListViewItem(Loc.T("Unmatched"));
                    item.SubItems.Add(ytd.FileName);
                    item.SubItems.Add(ytd.FolderName);
                    item.SubItems.Add($"{(ytd.FileSizeBytes / 1024.0 / 1024.0):F2} MB");
                    item.Tag = ytd;
                    item.ForeColor = ytd.TexturesToOptimizeCount > 0 ? System.Drawing.Color.OrangeRed : System.Drawing.Color.DimGray;

                    _lvDrawables.Items.Add(item);
                    _displayedYtdFiles.Add(ytd);
                }
            }

            _lvDrawables.EndUpdate();
            _lblListCounter.Text = Loc.T("ShowingCounter", _lvDrawables.Items.Count, _allYtdFiles.Count);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            PopulateDrawablesList();
        }

        private void LvDrawables_SelectedIndexChanged(object sender, EventArgs e)
        {
            _lvTextures.BeginUpdate();
            _lvTextures.Items.Clear();

            if (_lvDrawables.SelectedItems.Count > 0)
            {
                var ytd = _lvDrawables.SelectedItems[0].Tag as YtdFileItem;
                if (ytd != null)
                {
                    foreach (var t in ytd.Textures)
                    {
                        var lvi = new ListViewItem(t.Name);
                        lvi.SubItems.Add(t.FormatName);
                        lvi.SubItems.Add(t.SizeDisplay);
                        lvi.SubItems.Add(t.TargetSizeDisplay);
                        lvi.SubItems.Add(t.FileSizeMbDisplay);
                        lvi.SubItems.Add(t.AlphaTag);
                        lvi.SubItems.Add(t.StatusDisplay);
                        lvi.Tag = new Tuple<YtdFileItem, TextureItem>(ytd, t);

                        // Kırmızı/Yeşil renklendirme
                        if (t.NeedsResize || t.NeedsFormatConversion)
                        {
                            lvi.ForeColor = System.Drawing.Color.Crimson;
                        }
                        else
                        {
                            lvi.ForeColor = System.Drawing.Color.ForestGreen;
                        }

                        _lvTextures.Items.Add(lvi);
                    }
                }
            }

            _lvTextures.EndUpdate();
        }

        private void UpdateSummaryCounters()
        {
            int totalTextures = 0;
            int texturesToChange = 0;
            long totalBytes = 0;
            long estimatedSavingBytes = 0;

            foreach (var ytd in _allYtdFiles)
            {
                totalBytes += ytd.FileSizeBytes;
                foreach (var t in ytd.Textures)
                {
                    totalTextures++;
                    if (t.NeedsOptimization)
                    {
                        texturesToChange++;
                        if (t.EstimatedOriginalBytes > t.EstimatedTargetBytes)
                        {
                            estimatedSavingBytes += (t.EstimatedOriginalBytes - t.EstimatedTargetBytes);
                        }
                    }
                }
            }

            if (_lblSummaryTotalTextures != null) _lblSummaryTotalTextures.Text = Loc.T("SummaryTotalTextures", totalTextures);
            if (_lblSummaryTexturesToChange != null) _lblSummaryTexturesToChange.Text = Loc.T("SummaryTexturesToChange", texturesToChange);
            if (_lblSummaryTotalSize != null) _lblSummaryTotalSize.Text = Loc.T("SummaryTotalSize", (totalBytes / 1024.0 / 1024.0));
            if (_lblSummaryEstimatedSaving != null) _lblSummaryEstimatedSaving.Text = Loc.T("SummaryEstimatedSavings", (estimatedSavingBytes / 1024.0 / 1024.0));
        }

        private async Task OptimizeSelectedAsync()
        {
            if (_lvDrawables.SelectedItems.Count == 0)
            {
                MessageBox.Show(Loc.T("NoYtdToOptimize"), Loc.T("WarningTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedYtds = new HashSet<YtdFileItem>();
            foreach (ListViewItem item in _lvDrawables.SelectedItems)
            {
                if (item.Tag is YtdFileItem ytd)
                {
                    selectedYtds.Add(ytd);
                }
            }

            await ExecuteOptimizationAsync(selectedYtds.ToList(), "Selected Files");
        }

        private async Task OptimizeAllAsync()
        {
            if (_allYtdFiles.Count == 0)
            {
                MessageBox.Show(Loc.T("NoFolderWarning"), Loc.T("WarningTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await ExecuteOptimizationAsync(_allYtdFiles, "All YTD Files");
        }

        private async Task ExecuteOptimizationAsync(List<YtdFileItem> targetYtds, string scopeName)
        {
            int toChange = targetYtds.Sum(y => y.TexturesToOptimizeCount);
            long estSave = 0;
            foreach (var y in targetYtds)
            {
                foreach (var t in y.Textures)
                {
                    if (t.NeedsOptimization && t.EstimatedOriginalBytes > t.EstimatedTargetBytes)
                    {
                        estSave += (t.EstimatedOriginalBytes - t.EstimatedTargetBytes);
                    }
                }
            }

            string confirmMsg = Loc.T("OptimizeConfirm", targetYtds.Count, targetYtds.Sum(y => y.TextureCount), toChange, (estSave / 1024.0 / 1024.0));

            var dr = MessageBox.Show(confirmMsg, Loc.T("ConfirmTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr != DialogResult.Yes) return;

            // Lock UI
            _btnAnalyze.Enabled = false;
            _btnOptimizeSelected.Enabled = false;
            _btnOptimizeAll.Enabled = false;
            _progressBar.Visible = true;
            _progressBar.Value = 0;
            _progressBar.Maximum = targetYtds.Count;

            _cts = new CancellationTokenSource();
            var optimizer = new YtdBatchOptimizer(_config);
            optimizer.OnProgress += args =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => UpdateProgressUI(args)));
                }
                else
                {
                    UpdateProgressUI(args);
                }
            };

            Log($"========== [{scopeName.ToUpper()} OPTIMIZATION STARTED] ==========");
            var startTime = DateTime.Now;

            try
            {
                var stats = await optimizer.OptimizeYtdFilesAsync(targetYtds, _cts.Token);
                var duration = DateTime.Now - startTime;

                Log($"========== [OPTIMIZATION COMPLETE - {duration.TotalSeconds:F1}s] ==========");
                Log($"[REPORT] Processed YTD: {stats.ProcessedYtdCount}/{stats.TotalYtdCount} | Failed YTD: {stats.FailedYtdCount}");
                Log($"[REPORT] Optimized: {stats.OptimizedTextureCount} | Skipped: {stats.SkippedTextureCount} | Failed: {stats.FailedTextureCount}");
                Log($"[REPORT] Types: {stats.AlphaTextureCount} ALPHA | {stats.OpaqueTextureCount} OPAQUE");
                Log($"[REPORT] Alpha Validation: {stats.AlphaValidationPassedCount} Passed | {stats.AlphaValidationFailedCount} Preserved");
                Log($"[REPORT] Initial: {stats.InitialMb:F2} MB | Final: {stats.FinalMb:F2} MB | Saved: {stats.SavedMb:F2} MB", System.Drawing.Color.LightGreen);

                string reportMsg = Loc.T("ReportSummary",
                    stats.ProcessedYtdCount, stats.TotalYtdCount, stats.FailedYtdCount,
                    stats.OptimizedTextureCount, stats.SkippedTextureCount, stats.FailedTextureCount,
                    stats.AlphaTextureCount, stats.OpaqueTextureCount,
                    stats.AlphaValidationPassedCount, stats.AlphaValidationFailedCount,
                    stats.InitialMb, stats.FinalMb, stats.SavedMb);

                MessageBox.Show(reportMsg, Loc.T("ReportTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Re-analyze folder
                await AnalyzeFolderAsync();
            }
            catch (Exception ex)
            {
                Log($"[ERROR] {ex.Message}", System.Drawing.Color.Red);
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _progressBar.Visible = false;
                _btnAnalyze.Enabled = true;
                _btnOptimizeSelected.Enabled = true;
                _btnOptimizeAll.Enabled = true;
            }
        }

        private void UpdateProgressUI(ProgressUpdateArgs args)
        {
            _statusLabel.Text = args.CurrentStatus;
            if (args.TotalItems > 0 && args.CurrentItem <= args.TotalItems)
            {
                _progressBar.Value = Math.Min(args.CurrentItem, _progressBar.Maximum);
            }

            if (!string.IsNullOrEmpty(args.LogMessage))
            {
                System.Drawing.Color c = args.IsError ? System.Drawing.Color.Crimson : (args.IsWarning ? System.Drawing.Color.Goldenrod : System.Drawing.Color.Gainsboro);
                Log(args.LogMessage, c);
            }
        }

        private void MiPreview_Click(object sender, EventArgs e)
        {
            if (_lvTextures.SelectedItems.Count == 0) return;

            var tuple = _lvTextures.SelectedItems[0].Tag as Tuple<YtdFileItem, TextureItem>;
            if (tuple == null) return;

            var ytd = tuple.Item1;
            var texItem = tuple.Item2;

            try
            {
                var ytdFile = CodeWalkerInterop.LoadYtd(ytd.FilePath);
                Texture cwTex = null;
                if (ytdFile?.TextureDict?.Textures != null)
                {
                    for (int i = 0; i < ytdFile.TextureDict.Textures.Count; i++)
                    {
                        var t = ytdFile.TextureDict.Textures[i];
                        if (t != null && t.Name.Equals(texItem.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            cwTex = t;
                            break;
                        }
                    }
                }
                if (cwTex == null)
                {
                    MessageBox.Show("Texture not found in dictionary.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string tempDir = Path.Combine(Path.GetTempPath(), "YtdOptimizerPreview");
                Directory.CreateDirectory(tempDir);

                using (var srcImage = CodeWalkerInterop.ExtractTextureToImage(cwTex, _config.TexconvPath, tempDir))
                {
                    var targetSize = SizeRules.CalculateTargetSize(srcImage.Width, srcImage.Height, _config);
                    using (var previewForm = new PreviewForm(texItem.Name, srcImage, targetSize, _config))
                    {
                        previewForm.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Preview error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MiReplace_Click(object sender, EventArgs e)
        {
            if (_lvTextures.SelectedItems.Count == 0) return;

            var tuple = _lvTextures.SelectedItems[0].Tag as Tuple<YtdFileItem, TextureItem>;
            if (tuple == null) return;

            var ytd = tuple.Item1;
            var texItem = tuple.Item2;

            using (var ofd = new OpenFileDialog
            {
                Title = $"{Loc.T("MenuReplace")}: '{texItem.Name}'",
                Filter = "Image Files (*.png;*.dds;*.jpg;*.tga)|*.png;*.dds;*.jpg;*.tga|All Files (*.*)|*.*"
            })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    string inputPath = ofd.FileName;
                    try
                    {
                        string tempDir = Path.Combine(Path.GetTempPath(), "YtdOptimizerReplace", Guid.NewGuid().ToString("N"));
                        Directory.CreateDirectory(tempDir);

                        // Load input as ImageSharp
                        using (var srcImg = SixLabors.ImageSharp.Image.Load<Rgba32>(inputPath))
                        {
                            var targetSize = SizeRules.CalculateTargetSize(srcImg.Width, srcImg.Height, _config);
                            Image<Rgba32> processedImg;
                            if (targetSize.NeedsResize)
                            {
                                processedImg = AlphaPreservingProcessor.ResizeAlphaPreserving(srcImg, targetSize.Width, targetSize.Height);
                            }
                            else
                            {
                                processedImg = srcImg.Clone();
                                AlphaPreservingProcessor.DilateTransparentEdges(processedImg, 2);
                            }

                            string tempPng = Path.Combine(tempDir, "replace.png");
                            processedImg.SaveAsPng(tempPng);
                            processedImg.Dispose();

                            // Convert to DXT5 with texconv
                            var tcRes = TexconvRunner.ConvertToDxt5(_config.TexconvPath, tempPng, tempDir);
                            if (!tcRes.Success)
                            {
                                throw new InvalidOperationException("texconv error: " + tcRes.ErrorLog);
                            }

                            byte[] ddsBytes = File.ReadAllBytes(tcRes.OutputFilePath);
                            var newTex = CodeWalkerInterop.CreateTextureFromDds(ddsBytes, texItem.Name);

                            // Replace in YTD
                            var ytdFile = CodeWalkerInterop.LoadYtd(ytd.FilePath);
                            var list = new List<Texture>();
                            if (ytdFile?.TextureDict?.Textures != null)
                            {
                                for (int i = 0; i < ytdFile.TextureDict.Textures.Count; i++)
                                {
                                    var oldT = ytdFile.TextureDict.Textures[i];
                                    if (oldT == null) continue;
                                    if (oldT.Name.Equals(texItem.Name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        list.Add(newTex);
                                    }
                                    else
                                    {
                                        list.Add(oldT);
                                    }
                                }
                            }
                            ytdFile.TextureDict.BuildFromTextureList(list);
                            CodeWalkerInterop.SaveYtd(ytdFile, ytd.FilePath, _config.BackupOriginal);

                            Log($"[REPLACED] {ytd.FileName} -> {texItem.Name}");
                            MessageBox.Show("Texture successfully replaced in YTD.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Refresh
                            LvDrawables_SelectedIndexChanged(null, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to replace texture: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void MiDelete_Click(object sender, EventArgs e)
        {
            if (_lvTextures.SelectedItems.Count == 0) return;

            var tuple = _lvTextures.SelectedItems[0].Tag as Tuple<YtdFileItem, TextureItem>;
            if (tuple == null) return;

            var ytd = tuple.Item1;
            var texItem = tuple.Item2;

            var dr = MessageBox.Show($"Delete texture '{texItem.Name}' from '{ytd.FileName}'?", Loc.T("MenuDelete"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr == DialogResult.Yes)
            {
                try
                {
                    var ytdFile = CodeWalkerInterop.LoadYtd(ytd.FilePath);
                    var list = new List<Texture>();
                    if (ytdFile?.TextureDict?.Textures != null)
                    {
                        for (int i = 0; i < ytdFile.TextureDict.Textures.Count; i++)
                        {
                            var oldT = ytdFile.TextureDict.Textures[i];
                            if (oldT != null && !oldT.Name.Equals(texItem.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                list.Add(oldT);
                            }
                        }
                    }

                    if (list.Count == 0)
                    {
                        MessageBox.Show("Cannot delete the only texture in YTD. Dictionary cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ytdFile.TextureDict.BuildFromTextureList(list);
                    CodeWalkerInterop.SaveYtd(ytdFile, ytd.FilePath, _config.BackupOriginal);

                    Log($"[DELETED] {ytd.FileName} -> '{texItem.Name}'");
                    MessageBox.Show("Texture successfully deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh
                    ytd.Textures.Remove(texItem);
                    LvDrawables_SelectedIndexChanged(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to delete texture: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
