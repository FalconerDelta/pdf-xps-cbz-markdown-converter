using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DocToMd
{
    public partial class MainForm : Form
    {
        private readonly Button _selectFileButton = new Button();
        private readonly Label _fileLabel = new Label();
        private readonly ComboBox _levelComboBox = new ComboBox();
        private readonly TextBox _outputDirBox = new TextBox();
        private readonly Button _browseButton = new Button();
        private readonly TextBox _minImgSizeBox = new TextBox();
        private readonly Button _convertButton = new Button();

        private string? _selectedFilePath;
        private string? _customOutputDir;

        public MainForm()
        {
            Text = "PDF / XPS / CBZ to Markdown Converter";
            Width = 650;
            Height = 520;
            MinimumSize = new Size(500, 450);
            StartPosition = FormStartPosition.CenterScreen;
            AllowDrop = true;

            DragEnter += OnDragEnter;
            DragDrop += OnDragDrop;

            SetupControls();
        }

        /// <summary>
        /// Allows Program.cs to pass the file path and default extraction level directly.
        /// </summary>
        public void LoadDocument(string path, ExtractionLevel level = ExtractionLevel.Default)
        {
            if (File.Exists(path))
            {
                string ext = Path.GetExtension(path).ToLower();
                if (ext == ".pdf" || ext == ".xps" || ext == ".cbz")
                {
                    SetSelectedFile(path);
                    _levelComboBox.SelectedItem = level.ToString();
                }
            }
        }

        private void SetupControls()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6, // Increased to 6
                Padding = new Padding(12),
                AllowDrop = true
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f)); // Source Document
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 75f)); // Detail Level
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f)); // Output Directory
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90f)); // Min Image Size Filter
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f)); // Convert Button
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));  // Status Label Row

            mainLayout.DragEnter += OnDragEnter;
            mainLayout.DragDrop += OnDragDrop;

            // ── 1. Source Document Group ──
            var fileGroup = new GroupBox { Text = "Source Document", Dock = DockStyle.Fill, Padding = new Padding(8) };

            _selectFileButton.Text = "Select File...";
            _selectFileButton.Dock = DockStyle.Right;
            _selectFileButton.Width = 110;
            _selectFileButton.Click += SelectFileClick;

            _fileLabel.Text = "No file selected (Drag & Drop or click Select File)";
            _fileLabel.Dock = DockStyle.Fill;
            _fileLabel.TextAlign = ContentAlignment.MiddleLeft;
            _fileLabel.ForeColor = Color.Gray;

            var filePanel = new Panel { Dock = DockStyle.Fill };
            filePanel.Controls.Add(_fileLabel);
            filePanel.Controls.Add(_selectFileButton);
            fileGroup.Controls.Add(filePanel);
            mainLayout.Controls.Add(fileGroup, 0, 0);

            // ── 2. Detail Level Group ──
            var levelGroup = new GroupBox { Text = "Detail Level", Dock = DockStyle.Fill, Padding = new Padding(8) };

            _levelComboBox.Dock = DockStyle.Fill;
            _levelComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _levelComboBox.Items.AddRange(Enum.GetNames<ExtractionLevel>());
            _levelComboBox.SelectedIndex = 1; // Default

            levelGroup.Controls.Add(_levelComboBox);
            mainLayout.Controls.Add(levelGroup, 0, 1);

            // ── 3. Output Directory Group ──
            var outputGroup = new GroupBox { Text = "Output Directory", Dock = DockStyle.Fill, Padding = new Padding(8) };

            _browseButton.Text = "Browse...";
            _browseButton.Dock = DockStyle.Right;
            _browseButton.Width = 110;
            _browseButton.Click += BrowseClick;

            _outputDirBox.ReadOnly = true;
            _outputDirBox.Dock = DockStyle.Fill;
            _outputDirBox.Text = "(same as source file)";
            _outputDirBox.ForeColor = Color.Gray;

            var outputPanel = new Panel { Dock = DockStyle.Fill };
            outputPanel.Controls.Add(_outputDirBox);
            outputPanel.Controls.Add(_browseButton);
            outputGroup.Controls.Add(outputPanel);
            mainLayout.Controls.Add(outputGroup, 0, 2);

            // ── 4. Min Image Size Filter Group ──
            var filterGroup = new GroupBox { Text = "Min Image Size Filter (pixels)", Dock = DockStyle.Fill, Padding = new Padding(8) };

            _minImgSizeBox.Dock = DockStyle.Fill;
            _minImgSizeBox.Text = "50";
            _minImgSizeBox.ForeColor = Color.Black;

            var filterPanel = new Panel { Dock = DockStyle.Fill };
            filterPanel.Controls.Add(_minImgSizeBox);
            filterGroup.Controls.Add(filterPanel);
            mainLayout.Controls.Add(filterGroup, 0, 3);

            // ── 5. Convert Button ──
            var buttonPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            _convertButton.Text = "Convert to Markdown";
            _convertButton.Dock = DockStyle.Fill;
            _convertButton.Font = new Font(Font.FontFamily, 11, FontStyle.Bold);
            _convertButton.Enabled = false;
            _convertButton.Click += ConvertClick;

            buttonPanel.Controls.Add(_convertButton);
            mainLayout.Controls.Add(buttonPanel, 0, 4);

            Controls.Add(mainLayout);

            // ── 6. Status Label ──
            _statusLabel.Text = "Ready";
            _statusLabel.Dock = DockStyle.Fill;
            _statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            _statusLabel.ForeColor = Color.DimGray;
            _statusLabel.Font = new Font(Font.FontFamily, 9.5f, FontStyle.Italic);

            mainLayout.Controls.Add(_statusLabel, 0, 5);

        }

        private void SelectFileClick(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "Supported Documents (*.pdf;*.xps;*.cbz)|*.pdf;*.xps;*.cbz|All Files (*.*)|*.*",
                Title = "Select Document to Convert"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                SetSelectedFile(openFileDialog.FileName);
            }
        }

        private void BrowseClick(object? sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select Output Directory"
            };

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                _customOutputDir = folderDialog.SelectedPath;
                _outputDirBox.Text = _customOutputDir;
                _outputDirBox.ForeColor = Color.Black;
            }
        }

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    string ext = Path.GetExtension(files[0]).ToLower();
                    if (ext == ".pdf" || ext == ".xps" || ext == ".cbz")
                    {
                        e.Effect = DragDropEffects.Copy;
                        return;
                    }
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void OnDragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    SetSelectedFile(files[0]);
                }
            }
        }

        private void SetSelectedFile(string path)
        {
            _selectedFilePath = path;
            _fileLabel.Text = Path.GetFileName(path);
            _fileLabel.ForeColor = Color.Black;
            _convertButton.Enabled = true;

            if (string.IsNullOrEmpty(_customOutputDir))
            {
                _outputDirBox.Text = "(same as source file)";
                _outputDirBox.ForeColor = Color.Gray;
            }
        }

        private void ConvertClick(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                _statusLabel.Text = "Please select a valid source file first.";
                _statusLabel.ForeColor = Color.Red;
                return;
            }

            if (!Enum.TryParse<ExtractionLevel>(_levelComboBox.SelectedItem?.ToString(), out var level))
            {
                level = ExtractionLevel.Default;
            }

            if (!int.TryParse(_minImgSizeBox.Text, out int minImageSize))
            {
                minImageSize = 50;
            }

            string targetDir = _customOutputDir ?? Path.GetDirectoryName(_selectedFilePath) ?? AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                Cursor = Cursors.WaitCursor;
                _convertButton.Enabled = false;
                _statusLabel.Text = "Converting document...";
                _statusLabel.ForeColor = Color.Blue;
                Refresh(); // Force UI update

                MuPdfConverter.Convert(_selectedFilePath, targetDir, level, minImageSize);

                _statusLabel.Text = $"Successfully converted to: {Path.Combine(targetDir, Path.GetFileNameWithoutExtension(_selectedFilePath) + ".md")}";
                _statusLabel.ForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Conversion failed: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
            }
            finally
            {
                Cursor = Cursors.Default;
                _convertButton.Enabled = true;
            }
        }

        private readonly Label _statusLabel = new Label();

    }
}