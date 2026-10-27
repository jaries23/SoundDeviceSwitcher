using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SoundDeviceSwitcher.App.Audio;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Diagnostics;
using SoundDeviceSwitcher.App.Hotkeys;
using SoundDeviceSwitcher.App.Localization;
using SoundDeviceSwitcher.App.Profiles;
using SoundDeviceSwitcher.App.Shell;
using SoundDeviceSwitcher.App.Theming;
using SoundDeviceSwitcher.App.UI.Controls;
using SoundDeviceSwitcher.App.Updates;

namespace SoundDeviceSwitcher.App.UI;

internal sealed partial class MainShellForm
{
    private void RefreshMainProgramToggleList(bool refreshDeviceChoices, string? selectedRuleId = null, string? selectedProgramKey = null)
    {
        if (_mainProgramToggleProgramsListBox is null)
        {
            return;
        }

        var selectedItem = GetSelectedMainProgramToggleItem();
        var ruleIdToSelect = selectedRuleId ?? selectedItem?.Rule.Id;
        var programKeyToSelect = selectedProgramKey ?? selectedItem?.ProgramKey;
        var items = _editableToggleRules
            .SelectMany(rule => rule.Programs.Select(program => new MainProgramToggleListItem(
                rule,
                program,
                BuildMainProgramToggleListLabel(rule, program))))
            .ToList();
        var selectedIndex = -1;

        RunWithoutAutoSave(() =>
        {
            _mainProgramToggleProgramsListBox.BeginUpdate();
            _suppressMainProgramToggleListSelectionEvents = true;

            try
            {
                _mainProgramToggleProgramsListBox.Items.Clear();
                foreach (var item in items)
                {
                    _mainProgramToggleProgramsListBox.Items.Add(item);
                }

                if (_mainProgramToggleProgramsListBox.Items.Count > 0)
                {
                    selectedIndex = 0;
                    for (var index = 0; index < _mainProgramToggleProgramsListBox.Items.Count; index++)
                    {
                        if (_mainProgramToggleProgramsListBox.Items[index] is MainProgramToggleListItem item &&
                            string.Equals(item.Rule.Id, ruleIdToSelect, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(item.ProgramKey, programKeyToSelect, StringComparison.OrdinalIgnoreCase))
                        {
                            selectedIndex = index;
                            break;
                        }
                    }

                    if (selectedIndex == 0 && !string.IsNullOrWhiteSpace(ruleIdToSelect))
                    {
                        for (var index = 0; index < _mainProgramToggleProgramsListBox.Items.Count; index++)
                        {
                            if (_mainProgramToggleProgramsListBox.Items[index] is MainProgramToggleListItem item &&
                                string.Equals(item.Rule.Id, ruleIdToSelect, StringComparison.OrdinalIgnoreCase))
                            {
                                selectedIndex = index;
                                break;
                            }
                        }
                    }
                }

                _mainProgramToggleProgramsListBox.SelectedIndex = selectedIndex;
            }
            finally
            {
                _suppressMainProgramToggleListSelectionEvents = false;
                _mainProgramToggleProgramsListBox.EndUpdate();
            }
        });

        UpdateSelectedMainProgramToggleEditor(refreshDeviceChoices);
    }

    private string BuildMainProgramToggleListLabel(ProgramToggleRule rule, ProfileProgramTarget program)
    {
        var programLabel = BuildProgramLabel(program);
        var ruleLabel = BuildToggleRuleListLabel(rule);
        return string.IsNullOrWhiteSpace(ruleLabel)
            ? programLabel
            : $"{programLabel} - {ruleLabel}";
    }

    private MainProgramToggleListItem? GetSelectedMainProgramToggleItem()
    {
        return _mainProgramToggleProgramsListBox?.SelectedItem as MainProgramToggleListItem;
    }

    private void UpdateSelectedMainProgramToggleEditor(bool refreshDeviceChoices = true)
    {
        var item = GetSelectedMainProgramToggleItem();
        _suppressMainProgramToggleEditorEvents = true;

        try
        {
            if (item is null)
            {
                SetComboBoxItems(_mainProgramTogglePrimaryDeviceComboBox, BuildToggleDeviceChoices([], new DeviceSelection()));
                SetComboBoxItems(_mainProgramToggleSecondaryDeviceComboBox, BuildToggleDeviceChoices([], new DeviceSelection()));
                SelectDeviceChoice(_mainProgramTogglePrimaryDeviceComboBox, null);
                SelectDeviceChoice(_mainProgramToggleSecondaryDeviceComboBox, null);
                _mainProgramToggleVolumeMixerCheckBox.Checked = false;
                SetMainProgramToggleEditorEnabled(false);
                return;
            }

            SetMainProgramToggleEditorEnabled(true);
            _mainProgramToggleVolumeMixerCheckBox.Checked = item.Rule.Target == ProfileToggleTarget.ApplicationAudioDevice;

            if (refreshDeviceChoices)
            {
                RefreshSelectedMainProgramToggleDeviceChoices(item.Rule);
            }
            else
            {
                SelectDeviceChoice(_mainProgramTogglePrimaryDeviceComboBox, item.Rule.PrimaryDevice.Id);
                SelectDeviceChoice(_mainProgramToggleSecondaryDeviceComboBox, item.Rule.SecondaryDevice.Id);
            }
        }
        finally
        {
            _suppressMainProgramToggleEditorEvents = false;
        }
    }

    private void SetMainProgramToggleEditorEnabled(bool enabled)
    {
        _mainProgramTogglePrimaryDeviceComboBox.Enabled = enabled;
        _mainProgramToggleSecondaryDeviceComboBox.Enabled = enabled;
        _mainProgramToggleVolumeMixerCheckBox.Enabled = enabled;
    }

    private void RefreshSelectedMainProgramToggleDeviceChoices(ProgramToggleRule rule)
    {
        _ = TryRepairToggleRuleDeviceSelections(
            rule,
            persistChanges: !_isLoading && !_suppressAutoSave);

        var selectablePlaybackDevices = _services.AudioDeviceService.GetSelectablePlaybackDevices();

        SetComboBoxItems(
            _mainProgramTogglePrimaryDeviceComboBox,
            BuildToggleDeviceChoices(
                selectablePlaybackDevices,
                rule.PrimaryDevice));
        SetComboBoxItems(
            _mainProgramToggleSecondaryDeviceComboBox,
            BuildToggleDeviceChoices(
                selectablePlaybackDevices,
                rule.SecondaryDevice));

        SelectDeviceChoice(_mainProgramTogglePrimaryDeviceComboBox, rule.PrimaryDevice.Id);
        SelectDeviceChoice(_mainProgramToggleSecondaryDeviceComboBox, rule.SecondaryDevice.Id);
    }

    private void HandleMainProgramToggleSelectionChanged()
    {
        if (_suppressMainProgramToggleListSelectionEvents)
        {
            return;
        }

        UpdateSelectedMainProgramToggleEditor();
    }

    private void HandleSelectedMainProgramToggleChanged()
    {
        if (_suppressMainProgramToggleEditorEvents)
        {
            return;
        }

        var item = GetSelectedMainProgramToggleItem();
        if (item is null)
        {
            return;
        }

        var rule = item.Rule;
        rule.Target = _mainProgramToggleVolumeMixerCheckBox.Checked
            ? ProfileToggleTarget.ApplicationAudioDevice
            : ProfileToggleTarget.DefaultDevice;

        if (_mainProgramTogglePrimaryDeviceComboBox.SelectedItem is DeviceChoice primaryChoice)
        {
            rule.PrimaryDevice.Id = primaryChoice.Id;
            rule.PrimaryDevice.Name = primaryChoice.DeviceName;
        }

        if (_mainProgramToggleSecondaryDeviceComboBox.SelectedItem is DeviceChoice secondaryChoice)
        {
            rule.SecondaryDevice.Id = secondaryChoice.Id;
            rule.SecondaryDevice.Name = secondaryChoice.DeviceName;
        }

        RefreshMainProgramToggleList(
            refreshDeviceChoices: false,
            selectedRuleId: rule.Id,
            selectedProgramKey: item.ProgramKey);
        RefreshToggleRuleLists(refreshDeviceChoices: false, selectedRuleId: rule.Id);
        AutoSaveCurrentSelections();
    }

    private void RefreshToggleRuleLists(bool refreshDeviceChoices, string? selectedRuleId = null)
    {
        if (_toggleRulesListBox is null)
        {
            return;
        }

        var ruleIdToSelect = selectedRuleId ?? GetSelectedToggleRule()?.Id;
        var items = _editableToggleRules
            .Select(rule => new ToggleRuleListItem(rule, BuildToggleRuleListLabel(rule)))
            .ToList();
        var selectedIndex = -1;

        RunWithoutAutoSave(() =>
        {
            _toggleRulesListBox.BeginUpdate();
            _suppressToggleRuleListSelectionEvents = true;

            try
            {
                _toggleRulesListBox.Items.Clear();
                foreach (var item in items)
                {
                    _toggleRulesListBox.Items.Add(item);
                }

                if (_toggleRulesListBox.Items.Count > 0)
                {
                    selectedIndex = 0;
                    for (var index = 0; index < _toggleRulesListBox.Items.Count; index++)
                    {
                        if (_toggleRulesListBox.Items[index] is ToggleRuleListItem item &&
                            string.Equals(item.Rule.Id, ruleIdToSelect, StringComparison.OrdinalIgnoreCase))
                        {
                            selectedIndex = index;
                            break;
                        }
                    }
                }

                _toggleRulesListBox.SelectedIndex = selectedIndex;
            }
            finally
            {
                _suppressToggleRuleListSelectionEvents = false;
                _toggleRulesListBox.EndUpdate();
            }
        });

        UpdateToggleRuleProgramButtons();
        UpdateSelectedToggleRuleEditor(refreshDeviceChoices);
    }

    private string BuildToggleRuleListLabel(ProgramToggleRule rule)
    {
        return string.IsNullOrWhiteSpace(rule.Name)
            ? _services.Localizer.Get("ToggleRuleUnnamed")
            : rule.Name;
    }

    private ProgramToggleRule? GetSelectedToggleRule()
    {
        return (_toggleRulesListBox?.SelectedItem as ToggleRuleListItem)?.Rule;
    }

    private ProfileProgramTarget? GetSelectedToggleRuleProgram()
    {
        return (_toggleRuleProgramsListBox?.SelectedItem as ProgramListItem)?.Program;
    }

    private void UpdateSelectedToggleRuleEditor(bool refreshDeviceChoices = true)
    {
        if (_toggleRuleNameTextBox is null ||
            _toggleRuleOrderTextBox is null ||
            _toggleRuleProgramsListBox is null)
        {
            return;
        }

        var rule = GetSelectedToggleRule();
        _suppressToggleRuleEditorEvents = true;

        try
        {
            if (rule is null)
            {
                _toggleRuleNameTextBox.Text = string.Empty;
                _toggleRuleOrderTextBox.Text = string.Empty;
                SetComboBoxItems(_toggleRulePrimaryDeviceComboBox, BuildToggleDeviceChoices([], new DeviceSelection()));
                SetComboBoxItems(_toggleRuleSecondaryDeviceComboBox, BuildToggleDeviceChoices([], new DeviceSelection()));
                SelectDeviceChoice(_toggleRulePrimaryDeviceComboBox, null);
                SelectDeviceChoice(_toggleRuleSecondaryDeviceComboBox, null);
                _toggleRuleVolumeMixerCheckBox.Checked = false;
                _toggleRuleProgramsListBox.Items.Clear();
                SetToggleRuleEditorEnabled(false);
                return;
            }

            SetToggleRuleEditorEnabled(true);
            _toggleRuleNameTextBox.Text = rule.Name;
            var ruleOrder = _editableToggleRules.FindIndex(item => string.Equals(item.Id, rule.Id, StringComparison.OrdinalIgnoreCase)) + 1;
            _toggleRuleOrderTextBox.Text = ruleOrder > 0 ? ruleOrder.ToString() : string.Empty;
            _toggleRuleVolumeMixerCheckBox.Checked = rule.Target == ProfileToggleTarget.ApplicationAudioDevice;

            if (refreshDeviceChoices)
            {
                RefreshSelectedToggleRuleDeviceChoices(rule);
            }
            else
            {
                SelectDeviceChoice(_toggleRulePrimaryDeviceComboBox, rule.PrimaryDevice.Id);
                SelectDeviceChoice(_toggleRuleSecondaryDeviceComboBox, rule.SecondaryDevice.Id);
            }

            RefreshToggleRuleProgramList(rule);
        }
        finally
        {
            _suppressToggleRuleEditorEvents = false;
            UpdateToggleRuleProgramButtons();
        }
    }

    private void SetToggleRuleEditorEnabled(bool enabled)
    {
        if (_toggleRuleNameTextBox is null || _toggleRuleOrderTextBox is null || _toggleRuleProgramsListBox is null)
        {
            return;
        }

        _toggleRuleNameTextBox.Enabled = enabled;
        _toggleRuleOrderTextBox.Enabled = enabled;
        _toggleRulePrimaryDeviceComboBox.Enabled = enabled;
        _toggleRuleSecondaryDeviceComboBox.Enabled = enabled;
        _toggleRuleVolumeMixerCheckBox.Enabled = enabled;
        _toggleRuleProgramsListBox.Enabled = enabled;
        _removeToggleRuleButton.Enabled = enabled;
        var selectedIndex = _toggleRulesListBox?.SelectedIndex ?? -1;
        _moveToggleRuleUpButton.Enabled = enabled && selectedIndex > 0;
        _moveToggleRuleDownButton.Enabled = enabled && selectedIndex >= 0 && selectedIndex < _editableToggleRules.Count - 1;
        _addToggleRuleProgramButton.Enabled = enabled;
        _removeToggleRuleProgramButton.Enabled = enabled && _toggleRuleProgramsListBox.SelectedItem is not null;
    }

    private void RefreshSelectedToggleRuleDeviceChoices(ProgramToggleRule rule)
    {
        _ = TryRepairToggleRuleDeviceSelections(
            rule,
            persistChanges: !_isLoading && !_suppressAutoSave);

        var selectablePlaybackDevices = _services.AudioDeviceService.GetSelectablePlaybackDevices();

        SetComboBoxItems(
            _toggleRulePrimaryDeviceComboBox,
            BuildToggleDeviceChoices(
                selectablePlaybackDevices,
                rule.PrimaryDevice));
        SetComboBoxItems(
            _toggleRuleSecondaryDeviceComboBox,
            BuildToggleDeviceChoices(
                selectablePlaybackDevices,
                rule.SecondaryDevice));

        SelectDeviceChoice(_toggleRulePrimaryDeviceComboBox, rule.PrimaryDevice.Id);
        SelectDeviceChoice(_toggleRuleSecondaryDeviceComboBox, rule.SecondaryDevice.Id);
    }

    private bool TryRepairToggleRuleDeviceSelections(ProgramToggleRule rule, bool persistChanges)
    {
        var repairedSelections = false;
        _ = ResolveToggleRuleDeviceSelection(rule, isPrimaryDevice: true, out var primarySelectionRepaired);
        repairedSelections |= primarySelectionRepaired;
        _ = ResolveToggleRuleDeviceSelection(rule, isPrimaryDevice: false, out var secondarySelectionRepaired);
        repairedSelections |= secondarySelectionRepaired;

        if (repairedSelections && persistChanges)
        {
            PersistCachedConfigurationRepairs();
        }

        return repairedSelections;
    }

    private AudioDeviceInfo? ResolveToggleRuleDeviceSelection(
        ProgramToggleRule rule,
        bool isPrimaryDevice,
        out bool repairedSelection)
    {
        repairedSelection = false;

        var selection = isPrimaryDevice ? rule.PrimaryDevice : rule.SecondaryDevice;
        if (string.IsNullOrWhiteSpace(selection.Id))
        {
            return null;
        }

        var resolvedDevice = _services.AudioDeviceService.ResolveSelectablePlaybackDevice(selection, out var resolvedByName);
        if (!resolvedByName || resolvedDevice is null)
        {
            return resolvedDevice;
        }

        repairedSelection = UpdateToggleRuleDeviceSelection(rule, isPrimaryDevice, resolvedDevice);
        return resolvedDevice;
    }

    private bool UpdateToggleRuleDeviceSelection(
        ProgramToggleRule rule,
        bool isPrimaryDevice,
        AudioDeviceInfo resolvedDevice)
    {
        var changed = UpdateDeviceSelection(
            isPrimaryDevice ? rule.PrimaryDevice : rule.SecondaryDevice,
            resolvedDevice);

        var editableRule = _editableToggleRules.FirstOrDefault(
            item => string.Equals(item.Id, rule.Id, StringComparison.OrdinalIgnoreCase));
        if (editableRule is not null && !ReferenceEquals(editableRule, rule))
        {
            changed |= UpdateDeviceSelection(
                isPrimaryDevice ? editableRule.PrimaryDevice : editableRule.SecondaryDevice,
                resolvedDevice);
        }

        var cachedRule = _cachedConfig?.ToggleRules.FirstOrDefault(
            item => string.Equals(item.Id, rule.Id, StringComparison.OrdinalIgnoreCase));
        if (cachedRule is not null &&
            !ReferenceEquals(cachedRule, rule) &&
            !ReferenceEquals(cachedRule, editableRule))
        {
            changed |= UpdateDeviceSelection(
                isPrimaryDevice ? cachedRule.PrimaryDevice : cachedRule.SecondaryDevice,
                resolvedDevice);
        }

        return changed;
    }

    private void RefreshToggleRuleProgramList(ProgramToggleRule rule)
    {
        if (_toggleRuleProgramsListBox is null)
        {
            return;
        }

        _toggleRuleProgramsListBox.BeginUpdate();
        _toggleRuleProgramsListBox.Items.Clear();
        foreach (var program in rule.Programs)
        {
            _toggleRuleProgramsListBox.Items.Add(new ProgramListItem(program, BuildProgramLabel(program)));
        }

        _toggleRuleProgramsListBox.EndUpdate();
    }

    private void UpdateToggleRuleProgramButtons()
    {
        if (_toggleRuleProgramsListBox is null)
        {
            return;
        }

        _removeToggleRuleProgramButton.Enabled =
            _toggleRuleProgramsListBox.Enabled &&
            _toggleRuleProgramsListBox.SelectedItem is not null;
    }

    private void HandleToggleRuleSelectionChanged()
    {
        if (_suppressToggleRuleListSelectionEvents)
        {
            return;
        }

        UpdateSelectedToggleRuleEditor();
    }

    private void HandleSelectedToggleRuleChanged()
    {
        if (_suppressToggleRuleEditorEvents)
        {
            return;
        }

        var rule = GetSelectedToggleRule();
        if (rule is null || _toggleRuleNameTextBox is null)
        {
            return;
        }

        rule.Name = _toggleRuleNameTextBox.Text.Trim();
        rule.Target = _toggleRuleVolumeMixerCheckBox.Checked
            ? ProfileToggleTarget.ApplicationAudioDevice
            : ProfileToggleTarget.DefaultDevice;

        if (_toggleRulePrimaryDeviceComboBox.SelectedItem is DeviceChoice primaryChoice)
        {
            rule.PrimaryDevice.Id = primaryChoice.Id;
            rule.PrimaryDevice.Name = primaryChoice.DeviceName;
        }

        if (_toggleRuleSecondaryDeviceComboBox.SelectedItem is DeviceChoice secondaryChoice)
        {
            rule.SecondaryDevice.Id = secondaryChoice.Id;
            rule.SecondaryDevice.Name = secondaryChoice.DeviceName;
        }

        RefreshToggleRuleLists(refreshDeviceChoices: false, selectedRuleId: rule.Id);
        RefreshMainProgramToggleList(refreshDeviceChoices: false, selectedRuleId: rule.Id);
        AutoSaveCurrentSelections();
    }

    private void AddToggleRule()
    {
        var suffix = 1;
        string name;
        do
        {
            name = _services.Localizer.Format("ToggleRuleDefaultName", suffix++);
        }
        while (_editableToggleRules.Any(rule => string.Equals(rule.Name, name, StringComparison.OrdinalIgnoreCase)));

        var rule = new ProgramToggleRule
        {
            Name = name
        };

        _editableToggleRules.Add(rule);
        RefreshToggleRuleLists(refreshDeviceChoices: true, selectedRuleId: rule.Id);
        RefreshMainProgramToggleList(refreshDeviceChoices: true, selectedRuleId: rule.Id);
        AutoSaveCurrentSelections();
    }

    private void RemoveSelectedToggleRule()
    {
        var rule = GetSelectedToggleRule();
        if (rule is null)
        {
            return;
        }

        _editableToggleRules.Remove(rule);
        RefreshToggleRuleLists(refreshDeviceChoices: true);
        RefreshMainProgramToggleList(refreshDeviceChoices: true);
        AutoSaveCurrentSelections();
    }

    private void MoveSelectedToggleRule(int offset)
    {
        var rule = GetSelectedToggleRule();
        if (rule is null)
        {
            return;
        }

        var currentIndex = _editableToggleRules.FindIndex(item => string.Equals(item.Id, rule.Id, StringComparison.OrdinalIgnoreCase));
        if (currentIndex < 0)
        {
            return;
        }

        var targetIndex = currentIndex + offset;
        if (targetIndex < 0 || targetIndex >= _editableToggleRules.Count)
        {
            return;
        }

        _editableToggleRules.RemoveAt(currentIndex);
        _editableToggleRules.Insert(targetIndex, rule);
        RefreshToggleRuleLists(refreshDeviceChoices: false, selectedRuleId: rule.Id);
        RefreshMainProgramToggleList(refreshDeviceChoices: false, selectedRuleId: rule.Id);
        AutoSaveCurrentSelections();
    }

    private void AddProgramToSelectedToggleRule()
    {
        var rule = GetSelectedToggleRule();
        if (rule is null)
        {
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = "Applications (*.exe)|*.exe",
            Multiselect = true,
            Title = _services.Localizer.Get("SelectProgramsDialogTitle")
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        foreach (var fileName in dialog.FileNames)
        {
            var executableName = Path.GetFileName(fileName);
            if (rule.Programs.Any(program =>
                    string.Equals(program.ExecutablePath, fileName, StringComparison.OrdinalIgnoreCase) ||
                    (string.IsNullOrWhiteSpace(program.ExecutablePath) &&
                     string.Equals(program.ExecutableName, executableName, StringComparison.OrdinalIgnoreCase))))
            {
                continue;
            }

            rule.Programs.Add(new ProfileProgramTarget
            {
                DisplayName = Path.GetFileNameWithoutExtension(fileName),
                ExecutableName = executableName,
                ExecutablePath = fileName
            });
        }

        RefreshToggleRuleProgramList(rule);
        UpdateToggleRuleProgramButtons();
        RefreshMainProgramToggleList(refreshDeviceChoices: true, selectedRuleId: rule.Id);
        AutoSaveCurrentSelections();
    }

    private void RemoveSelectedProgramFromToggleRule()
    {
        var rule = GetSelectedToggleRule();
        var program = GetSelectedToggleRuleProgram();
        if (rule is null || program is null)
        {
            return;
        }

        rule.Programs.Remove(program);
        RefreshToggleRuleProgramList(rule);
        UpdateToggleRuleProgramButtons();
        RefreshMainProgramToggleList(refreshDeviceChoices: true, selectedRuleId: rule.Id);
        AutoSaveCurrentSelections();
    }

}
