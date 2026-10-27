using System.Diagnostics;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.Theming;
using SoundDeviceSwitcher.App.UI.Controls;

namespace SoundDeviceSwitcher.App.UI;

internal sealed class RunningProgramPickerForm : Form
{
    private readonly LocalizationService _localizer;
    private readonly AppThemeMode _themeMode;
    private readonly Icon _windowIcon;
    private readonly RoundedPanel _surface;
    private readonly Label _hintLabel;
    private readonly CheckedListBox _programListBox;
    private readonly Label _emptyLabel;
    private readonly RoundedButton _addButton;
    private readonly RoundedButton _cancelButton;
    private readonly RoundedButton _refreshButton;

    public RunningProgramPickerForm(LocalizationService localizer, AppThemeMode themeMode)
    {
        _localizer = localizer;
        _themeMode = themeMode;

        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(460, 420);
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        Text = _localizer.Get("SelectRunningProgramDialogTitle");
        _windowIcon = NotificationIconCatalog.CreateTrayIcon();
        Icon = _windowIcon;
        ShowIcon = true;

        _surface = new RoundedPanel
        {
            Dock = DockStyle.Fill,
            CornerRadius = 0,
            BorderThickness = 1,
            Padding = new Padding(18),
            Margin = new Padding(0)
        };

        _hintLabel = new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 2),
            Text = _localizer.Get("ProgramPickerHint")
        };

        _programListBox = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            CheckOnClick = true,
            BorderStyle = BorderStyle.FixedSingle,
            IntegralHeight = false,
            Margin = new Padding(0)
        };

        _emptyLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Visible = false,
            Text = _localizer.Get("ProgramPickerEmpty")
        };

        _addButton = CreateActionButton("primary", new Size(110, 38));
        _addButton.Text = _localizer.Get("ProgramPickerAddButton");
        _addButton.Click += (_, _) => Confirm();

        _cancelButton = CreateActionButton("secondary", new Size(96, 38));
        _cancelButton.Text = _localizer.Get("ProgramPickerCancelButton");
        _cancelButton.Click += (_, _) => Close();

        _refreshButton = CreateActionButton("secondary", new Size(104, 38));
        _refreshButton.Text = _localizer.Get("ProgramPickerRefreshButton");
        _refreshButton.Click += (_, _) => LoadRunningPrograms();

        _surface.Controls.Add(BuildContent());
        Controls.Add(_surface);

        AcceptButton = _addButton;
        CancelButton = _cancelButton;

        LoadRunningPrograms();
        ThemeManager.Apply(this, _themeMode);
    }

    public IReadOnlyList<ProfileProgramTarget> SelectedPrograms { get; private set; } = [];

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _windowIcon.Dispose();
        }

        base.Dispose(disposing);
    }

    private Control BuildContent()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var listHost = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 0),
            Padding = new Padding(0)
        };
        listHost.Controls.Add(_programListBox);
        listHost.Controls.Add(_emptyLabel);
        _emptyLabel.BringToFront();

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(0, 14, 0, 0),
            BackColor = Color.Transparent
        };
        buttonRow.Controls.Add(_addButton);
        buttonRow.Controls.Add(_cancelButton);
        buttonRow.Controls.Add(_refreshButton);

        root.Controls.Add(_hintLabel, 0, 0);
        root.Controls.Add(listHost, 0, 1);
        root.Controls.Add(buttonRow, 0, 2);
        return root;
    }

    private void LoadRunningPrograms()
    {
        var programs = GetRunningProgramsWithWindows();

        _programListBox.BeginUpdate();
        try
        {
            _programListBox.Items.Clear();
            foreach (var program in programs)
            {
                _programListBox.Items.Add(program);
            }
        }
        finally
        {
            _programListBox.EndUpdate();
        }

        var hasItems = programs.Count > 0;
        _programListBox.Visible = hasItems;
        _emptyLabel.Visible = !hasItems;
        _addButton.Enabled = hasItems;
    }

    private void Confirm()
    {
        var selected = _programListBox.CheckedItems
            .OfType<RunningProgram>()
            .Select(program => new ProfileProgramTarget
            {
                DisplayName = program.DisplayName,
                ExecutableName = program.ExecutableName,
                ExecutablePath = program.ExecutablePath
            })
            .ToList();

        if (selected.Count == 0)
        {
            return;
        }

        SelectedPrograms = selected;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static List<RunningProgram> GetRunningProgramsWithWindows()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var programs = new List<RunningProgram>();

        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    if (process.MainWindowHandle == IntPtr.Zero ||
                        string.IsNullOrWhiteSpace(process.MainWindowTitle))
                    {
                        continue;
                    }

                    var processName = process.ProcessName;
                    if (string.IsNullOrWhiteSpace(processName))
                    {
                        continue;
                    }

                    var (executablePath, description) = ReadModuleInfo(process);
                    var executableName = $"{processName}.exe";
                    var key = string.IsNullOrWhiteSpace(executablePath) ? executableName : executablePath;
                    if (!seen.Add(key))
                    {
                        continue;
                    }

                    var displayName = string.IsNullOrWhiteSpace(description) ? processName : description.Trim();
                    programs.Add(new RunningProgram(displayName, executableName, executablePath ?? string.Empty));
                }
                catch
                {
                }
            }
        }

        programs.Sort((left, right) =>
            string.Compare(left.DisplayName, right.DisplayName, StringComparison.CurrentCultureIgnoreCase));
        return programs;
    }

    private static (string? ExecutablePath, string? Description) ReadModuleInfo(Process process)
    {
        try
        {
            var module = process.MainModule;
            if (module is null)
            {
                return (null, null);
            }

            var path = string.IsNullOrWhiteSpace(module.FileName) ? null : Path.GetFullPath(module.FileName);
            return (path, module.FileVersionInfo.FileDescription);
        }
        catch
        {
            return (null, null);
        }
    }

    private static RoundedButton CreateActionButton(string role, Size minimumSize)
    {
        return new RoundedButton
        {
            Tag = role,
            CornerRadius = 0,
            MinimumSize = minimumSize,
            Margin = new Padding(8, 0, 0, 0),
            Padding = new Padding(14, 8, 14, 8),
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point)
        };
    }

    private sealed record RunningProgram(string DisplayName, string ExecutableName, string ExecutablePath)
    {
        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(ExecutableName)
                ? DisplayName
                : $"{DisplayName}  ·  {ExecutableName}";
        }
    }
}
