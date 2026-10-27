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
    private void RefreshProfileLists(bool refreshDeviceChoices, string? selectedProfileId = null)
    {
        if (_profilesListBox is null)
        {
            return;
        }

        var profileIdToSelect = selectedProfileId ?? GetSelectedProfile()?.Id;
        var items = _editableProfiles
            .Select(profile => new ProfileListItem(profile, BuildProfileListLabel(profile)))
            .ToList();
        var selectedIndex = -1;

        RunWithoutAutoSave(() =>
        {
            _profilesListBox.BeginUpdate();
            _suppressProfileListSelectionEvents = true;

            try
            {
                _profilesListBox.Items.Clear();
                foreach (var item in items)
                {
                    _profilesListBox.Items.Add(item);
                }

                if (_profilesListBox.Items.Count > 0)
                {
                    selectedIndex = 0;
                    for (var index = 0; index < _profilesListBox.Items.Count; index++)
                    {
                        if (_profilesListBox.Items[index] is ProfileListItem item &&
                            string.Equals(item.Profile.Id, profileIdToSelect, StringComparison.OrdinalIgnoreCase))
                        {
                            selectedIndex = index;
                            break;
                        }
                    }
                }

                _profilesListBox.SelectedIndex = selectedIndex;
            }
            finally
            {
                _suppressProfileListSelectionEvents = false;
                _profilesListBox.EndUpdate();
            }
        });

        UpdateProfileProgramButtons();

        UpdateSelectedProfileEditor(refreshDeviceChoices);
    }

    private string BuildProfileListLabel(ProcessAudioProfile profile)
    {
        return string.IsNullOrWhiteSpace(profile.Name)
            ? _services.Localizer.Get("ProfileUnnamed")
            : profile.Name;
    }

    private ProcessAudioProfile? GetSelectedProfile()
    {
        return (_profilesListBox?.SelectedItem as ProfileListItem)?.Profile;
    }

    private ProfileProgramTarget? GetSelectedProfileProgram()
    {
        return (_profileProgramsListBox?.SelectedItem as ProgramListItem)?.Program;
    }

    private void UpdateSelectedProfileEditor(bool refreshDeviceChoices = true)
    {
        if (_profileNameTextBox is null ||
            _profileOrderTextBox is null ||
            _profileProgramsListBox is null)
        {
            return;
        }

        var profile = GetSelectedProfile();
        _suppressProfileEditorEvents = true;

        try
        {
            if (profile is null)
            {
                _profileNameTextBox.Text = string.Empty;
                _profileOrderTextBox.Text = string.Empty;
                _profileIconComboBox.SelectedIndex = -1;
                SetPictureBoxImage(_profileIconPreview, null);
                SetComboBoxItems(_profileOutputDeviceComboBox, BuildDeviceChoices([], new DeviceSelection()));
                SetComboBoxItems(_profileInputDeviceComboBox, BuildDeviceChoices([], new DeviceSelection()));
                SelectDeviceChoice(_profileOutputDeviceComboBox, null);
                SelectDeviceChoice(_profileInputDeviceComboBox, null);
                _profileProgramsListBox.Items.Clear();
                SetProfileEditorEnabled(false);
                return;
            }

            SetProfileEditorEnabled(true);
            _profileNameTextBox.Text = profile.Name;
            var profileOrder = _editableProfiles.FindIndex(item => string.Equals(item.Id, profile.Id, StringComparison.OrdinalIgnoreCase)) + 1;
            _profileOrderTextBox.Text = profileOrder > 0 ? profileOrder.ToString() : string.Empty;
            SelectIcon(_profileIconComboBox, profile.IconFileName);
            UpdateDeviceIconPreview(_profileIconComboBox, _profileIconPreview);

            if (refreshDeviceChoices)
            {
                RefreshSelectedProfileDeviceChoices(profile);
            }
            else
            {
                SelectDeviceChoice(_profileOutputDeviceComboBox, profile.PlaybackDevice.Id);
                SelectDeviceChoice(_profileInputDeviceComboBox, profile.RecordingDevice.Id);
            }

            RefreshProfileProgramList(profile);
        }
        finally
        {
            _suppressProfileEditorEvents = false;
            UpdateProfileProgramButtons();
        }
    }

    private void SetProfileEditorEnabled(bool enabled)
    {
        if (_profileNameTextBox is null || _profileOrderTextBox is null || _profileProgramsListBox is null)
        {
            return;
        }

        _profileNameTextBox.Enabled = enabled;
        _profileOrderTextBox.Enabled = enabled;
        _profileIconComboBox.Enabled = enabled && _profileIconComboBox.Items.Count > 0;
        _profileOutputDeviceComboBox.Enabled = enabled;
        _profileInputDeviceComboBox.Enabled = enabled;
        _profileProgramsListBox.Enabled = enabled;
        _removeProfileButton.Enabled = enabled;
        var selectedIndex = _profilesListBox?.SelectedIndex ?? -1;
        _moveProfileUpButton.Enabled = enabled && selectedIndex > 0;
        _moveProfileDownButton.Enabled = enabled && selectedIndex >= 0 && selectedIndex < _editableProfiles.Count - 1;
        _addProfileProgramButton.Enabled = enabled;
        _removeProfileProgramButton.Enabled = enabled && _profileProgramsListBox.SelectedItem is not null;
    }

    private void RefreshSelectedProfileDeviceChoices(ProcessAudioProfile profile)
    {
        _ = TryRepairProfileDeviceSelections(
            profile,
            persistChanges: !_isLoading && !_suppressAutoSave);

        var selectablePlaybackDevices = _services.AudioDeviceService.GetSelectablePlaybackDevices();
        var selectableRecordingDevices = _services.AudioDeviceService.GetSelectableRecordingDevices();

        SetComboBoxItems(
            _profileOutputDeviceComboBox,
            BuildDeviceChoices(
                selectablePlaybackDevices,
                profile.PlaybackDevice));
        SetComboBoxItems(
            _profileInputDeviceComboBox,
            BuildDeviceChoices(
                selectableRecordingDevices,
                profile.RecordingDevice));

        SelectDeviceChoice(_profileOutputDeviceComboBox, profile.PlaybackDevice.Id);
        SelectDeviceChoice(_profileInputDeviceComboBox, profile.RecordingDevice.Id);
    }

    private bool TryRepairProfileDeviceSelections(ProcessAudioProfile profile, bool persistChanges)
    {
        var repairedSelections = false;
        _ = ResolveProfileDeviceSelection(profile, isPlaybackDevice: true, out var playbackSelectionRepaired);
        repairedSelections |= playbackSelectionRepaired;
        _ = ResolveProfileDeviceSelection(profile, isPlaybackDevice: false, out var recordingSelectionRepaired);
        repairedSelections |= recordingSelectionRepaired;

        if (repairedSelections && persistChanges)
        {
            PersistCachedConfigurationRepairs();
        }

        return repairedSelections;
    }

    private AudioDeviceInfo? ResolveProfileDeviceSelection(
        ProcessAudioProfile profile,
        bool isPlaybackDevice,
        out bool repairedSelection)
    {
        repairedSelection = false;

        var selection = isPlaybackDevice ? profile.PlaybackDevice : profile.RecordingDevice;
        if (string.IsNullOrWhiteSpace(selection.Id))
        {
            return null;
        }

        AudioDeviceInfo? resolvedDevice;
        bool resolvedByName;
        if (isPlaybackDevice)
        {
            resolvedDevice = _services.AudioDeviceService.ResolveSelectablePlaybackDevice(selection, out resolvedByName);
        }
        else
        {
            resolvedDevice = _services.AudioDeviceService.ResolveSelectableRecordingDevice(selection, out resolvedByName);
        }

        if (!resolvedByName || resolvedDevice is null)
        {
            return resolvedDevice;
        }

        repairedSelection = UpdateProfileDeviceSelection(profile, isPlaybackDevice, resolvedDevice);
        return resolvedDevice;
    }

    private bool UpdateProfileDeviceSelection(
        ProcessAudioProfile profile,
        bool isPlaybackDevice,
        AudioDeviceInfo resolvedDevice)
    {
        var changed = UpdateDeviceSelection(
            isPlaybackDevice ? profile.PlaybackDevice : profile.RecordingDevice,
            resolvedDevice);

        var editableProfile = _editableProfiles.FirstOrDefault(
            item => string.Equals(item.Id, profile.Id, StringComparison.OrdinalIgnoreCase));
        if (editableProfile is not null && !ReferenceEquals(editableProfile, profile))
        {
            changed |= UpdateDeviceSelection(
                isPlaybackDevice ? editableProfile.PlaybackDevice : editableProfile.RecordingDevice,
                resolvedDevice);
        }

        var cachedProfile = _cachedConfig?.Profiles.FirstOrDefault(
            item => string.Equals(item.Id, profile.Id, StringComparison.OrdinalIgnoreCase));
        if (cachedProfile is not null &&
            !ReferenceEquals(cachedProfile, profile) &&
            !ReferenceEquals(cachedProfile, editableProfile))
        {
            changed |= UpdateDeviceSelection(
                isPlaybackDevice ? cachedProfile.PlaybackDevice : cachedProfile.RecordingDevice,
                resolvedDevice);
        }

        return changed;
    }

    private static bool UpdateDeviceSelection(DeviceSelection selection, AudioDeviceInfo resolvedDevice)
    {
        if (string.Equals(selection.Id, resolvedDevice.Id, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(selection.Name, resolvedDevice.Name, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        selection.Id = resolvedDevice.Id;
        selection.Name = resolvedDevice.Name;
        return true;
    }

    private void PersistCachedConfigurationRepairs()
    {
        if (_cachedConfig is null)
        {
            return;
        }

        try
        {
            _services.ConfigStore.Save(_cachedConfig);
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.PersistCachedConfigurationRepairs", ex);
        }
    }

    private void RefreshSelectedProfileDeviceChoicesIfSelected(string profileId)
    {
        var selectedProfile = GetSelectedProfile();
        if (selectedProfile is null ||
            !string.Equals(selectedProfile.Id, profileId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var previousSuppressProfileEditorEvents = _suppressProfileEditorEvents;
        _suppressProfileEditorEvents = true;
        try
        {
            RefreshSelectedProfileDeviceChoices(selectedProfile);
        }
        finally
        {
            _suppressProfileEditorEvents = previousSuppressProfileEditorEvents;
        }
    }

    private List<DeviceChoice> BuildDeviceChoices(
        IReadOnlyList<AudioDeviceInfo> devices,
        DeviceSelection selectedDevice)
    {
        return BuildDeviceChoices(
            devices,
            selectedDevice,
            _services.Localizer.Get("ProfileDoNotChangeOption"));
    }

    private List<DeviceChoice> BuildToggleDeviceChoices(
        IReadOnlyList<AudioDeviceInfo> devices,
        DeviceSelection selectedDevice)
    {
        return BuildDeviceChoices(
            devices,
            selectedDevice,
            _services.Localizer.Get("ProfileToggleNotSpecifiedOption"));
    }

    private static List<DeviceChoice> BuildDeviceChoices(
        IReadOnlyList<AudioDeviceInfo> devices,
        DeviceSelection selectedDevice,
        string emptyOptionLabel)
    {
        var options = new List<DeviceChoice>
        {
            new(string.Empty, emptyOptionLabel, string.Empty)
        };

        options.AddRange(devices.Select(device => new DeviceChoice(device.Id, device.DisplayName, device.Name)));

        if (!string.IsNullOrWhiteSpace(selectedDevice.Id) &&
            options.All(option => !string.Equals(option.Id, selectedDevice.Id, StringComparison.OrdinalIgnoreCase)) &&
            options.All(option => !string.Equals(option.Label, selectedDevice.Name, StringComparison.OrdinalIgnoreCase)))
        {
            options.Add(new DeviceChoice(selectedDevice.Id, selectedDevice.Name, selectedDevice.Name));
        }

        return options;
    }

    private static void SelectDeviceChoice(ComboBox comboBox, string? deviceId)
    {
        if (comboBox.Items.Count == 0)
        {
            comboBox.SelectedIndex = -1;
            return;
        }

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            comboBox.SelectedIndex = 0;
            return;
        }

        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is DeviceChoice choice &&
                string.Equals(choice.Id, deviceId, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }

        comboBox.SelectedIndex = comboBox.Items.Count > 0 ? 0 : -1;
    }

    private void RefreshProfileProgramList(ProcessAudioProfile profile)
    {
        if (_profileProgramsListBox is null)
        {
            return;
        }

        _profileProgramsListBox.BeginUpdate();
        _profileProgramsListBox.Items.Clear();
        foreach (var program in profile.Programs)
        {
            _profileProgramsListBox.Items.Add(new ProgramListItem(program, BuildProgramLabel(program)));
        }

        _profileProgramsListBox.EndUpdate();
    }

    private string BuildProgramLabel(ProfileProgramTarget program)
    {
        if (!string.IsNullOrWhiteSpace(program.DisplayName))
        {
            return program.DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(program.ExecutableName))
        {
            return program.ExecutableName;
        }

        return program.ExecutablePath;
    }

    private void UpdateProfileProgramButtons()
    {
        if (_profileProgramsListBox is null)
        {
            return;
        }

        _removeProfileProgramButton.Enabled =
            _profileProgramsListBox.Enabled &&
            _profileProgramsListBox.SelectedItem is not null;
    }

    private void HandleProfileSelectionChanged()
    {
        if (_suppressProfileListSelectionEvents)
        {
            return;
        }

        UpdateSelectedProfileEditor();
    }

    private void HandleSelectedProfileChanged()
    {
        if (_suppressProfileEditorEvents)
        {
            return;
        }

        var profile = GetSelectedProfile();
        if (profile is null || _profileNameTextBox is null)
        {
            return;
        }

        profile.Name = _profileNameTextBox.Text.Trim();
        profile.IconFileName = NotificationIconCatalog.NormalizeFileName(
            GetSelectedIconFileName(_profileIconComboBox, AppConfig.DefaultIconFileName));
        if (_profileOutputDeviceComboBox.SelectedItem is DeviceChoice playbackChoice)
        {
            profile.PlaybackDevice.Id = playbackChoice.Id;
            profile.PlaybackDevice.Name = playbackChoice.DeviceName;
        }

        if (_profileInputDeviceComboBox.SelectedItem is DeviceChoice recordingChoice)
        {
            profile.RecordingDevice.Id = recordingChoice.Id;
            profile.RecordingDevice.Name = recordingChoice.DeviceName;
        }

        RefreshProfileLists(refreshDeviceChoices: false, selectedProfileId: profile.Id);
        AutoSaveCurrentSelections();
    }

    private void AddProfile()
    {
        var suffix = 1;
        string name;
        do
        {
            name = _services.Localizer.Format("ProfileDefaultName", suffix++);
        }
        while (_editableProfiles.Any(profile => string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase)));

        var profile = new ProcessAudioProfile
        {
            Name = name
        };

        _editableProfiles.Add(profile);
        RefreshProfileLists(refreshDeviceChoices: true, selectedProfileId: profile.Id);
        AutoSaveCurrentSelections();
    }

    private void RemoveSelectedProfile()
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            return;
        }

        _editableProfiles.Remove(profile);
        RefreshProfileLists(refreshDeviceChoices: true);
        AutoSaveCurrentSelections();
    }

    private void MoveSelectedProfile(int offset)
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            return;
        }

        var currentIndex = _editableProfiles.FindIndex(item => string.Equals(item.Id, profile.Id, StringComparison.OrdinalIgnoreCase));
        if (currentIndex < 0)
        {
            return;
        }

        var targetIndex = currentIndex + offset;
        if (targetIndex < 0 || targetIndex >= _editableProfiles.Count)
        {
            return;
        }

        _editableProfiles.RemoveAt(currentIndex);
        _editableProfiles.Insert(targetIndex, profile);
        RefreshProfileLists(refreshDeviceChoices: false, selectedProfileId: profile.Id);
        AutoSaveCurrentSelections();
    }

    private void AddProgramToSelectedProfile()
    {
        var profile = GetSelectedProfile();
        if (profile is null)
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
            if (profile.Programs.Any(program =>
                    string.Equals(program.ExecutablePath, fileName, StringComparison.OrdinalIgnoreCase) ||
                    (string.IsNullOrWhiteSpace(program.ExecutablePath) &&
                     string.Equals(program.ExecutableName, executableName, StringComparison.OrdinalIgnoreCase))))
            {
                continue;
            }

            profile.Programs.Add(new ProfileProgramTarget
            {
                DisplayName = Path.GetFileNameWithoutExtension(fileName),
                ExecutableName = executableName,
                ExecutablePath = fileName
            });
        }

        RefreshProfileProgramList(profile);
        UpdateProfileProgramButtons();
        AutoSaveCurrentSelections();
    }

    private void RemoveSelectedProgramFromProfile()
    {
        var profile = GetSelectedProfile();
        var program = GetSelectedProfileProgram();
        if (profile is null || program is null)
        {
            return;
        }

        profile.Programs.Remove(program);
        RefreshProfileProgramList(profile);
        UpdateProfileProgramButtons();
        AutoSaveCurrentSelections();
    }

}
