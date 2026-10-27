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
    private void EvaluateProcessProfiles(AppConfig? config = null)
    {
        if (_profileEvaluationInProgress || IsDisposed)
        {
            return;
        }

        _profileEvaluationInProgress = true;

        try
        {
            if (config is null)
            {
                config = _cachedConfig;
            }

            if (config is null && !_services.ConfigStore.TryLoad(out config, out _))
            {
                RefreshProfileEvaluationTimer(requireBackgroundMonitoring: false);
                ClearActiveProcessProfile();
                return;
            }

            if (config is null)
            {
                RefreshProfileEvaluationTimer(requireBackgroundMonitoring: false);
                ClearActiveProcessProfile();
                return;
            }

            _cachedConfig = config;
            _services.Localizer.SetLanguage(config.Language);

            if (!config.EnableProfiles)
            {
                RefreshProfileEvaluationTimer(requireBackgroundMonitoring: false);
                ClearActiveProcessProfile();
                return;
            }

            var matchResult = _processProfileMatcher.FindForegroundMatch(config.Profiles);
            RefreshProfileEvaluationTimer(matchResult.ShouldMonitorBackgroundState);

            if (matchResult.BlockingProfile is not null)
            {
                ClearActiveProcessProfile();
                return;
            }

            var matchedProfile = matchResult.MatchedProfile;
            if (matchedProfile is null)
            {
                ClearActiveProcessProfile();
                return;
            }

            var playbackDeviceId = matchedProfile.PlaybackDevice.Id;
            var recordingDeviceId = matchedProfile.RecordingDevice.Id;
            if (string.Equals(_activeProcessProfileId, matchedProfile.Id, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(_activeProcessProfilePlaybackDeviceId, playbackDeviceId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(_activeProcessProfileRecordingDeviceId, recordingDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!TryApplyProcessProfile(matchedProfile, out var message, out var isError, out var devicesChanged))
            {
                if (isError && !string.IsNullOrWhiteSpace(message))
                {
                    SetStatus(message, isError: true);
                }

                return;
            }

            _activeProcessProfileId = matchedProfile.Id;
            _activeProcessProfilePlaybackDeviceId = matchedProfile.PlaybackDevice.Id;
            _activeProcessProfileRecordingDeviceId = matchedProfile.RecordingDevice.Id;

            if (!devicesChanged || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            SetStatus(message);
            _trayNotifications.Show(
                message,
                ToolTipIcon.Info,
                2500,
                imagePath: NotificationIconCatalog.ResolvePath(AppConfig.DefaultIconFileName));
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.EvaluateProcessProfiles", ex);
        }
        finally
        {
            _profileEvaluationInProgress = false;
        }
    }

    private bool TryApplyProcessProfile(
        ProcessAudioProfile profile,
        out string? message,
        out bool isError,
        out bool devicesChanged)
    {
        message = null;
        isError = false;
        devicesChanged = false;

        var missingDevices = new List<string>();
        var repairedSelections = false;
        AudioDeviceInfo? targetPlaybackDevice = null;
        if (!string.IsNullOrWhiteSpace(profile.PlaybackDevice.Id))
        {
            targetPlaybackDevice = ResolveProfileDeviceSelection(
                profile,
                isPlaybackDevice: true,
                out var playbackSelectionRepaired);
            repairedSelections |= playbackSelectionRepaired;

            if (targetPlaybackDevice is null)
            {
                missingDevices.Add(GetProfileDeviceLabel(profile.PlaybackDevice));
            }
        }

        AudioDeviceInfo? targetRecordingDevice = null;
        if (!string.IsNullOrWhiteSpace(profile.RecordingDevice.Id))
        {
            targetRecordingDevice = ResolveProfileDeviceSelection(
                profile,
                isPlaybackDevice: false,
                out var recordingSelectionRepaired);
            repairedSelections |= recordingSelectionRepaired;

            if (targetRecordingDevice is null)
            {
                missingDevices.Add(GetProfileDeviceLabel(profile.RecordingDevice));
            }
        }

        if (missingDevices.Count > 0)
        {
            isError = true;
            message = _services.Localizer.Format(
                "ErrorProfileMissingDevices",
                GetProfileDisplayName(profile),
                string.Join(", ", missingDevices.Where(name => !string.IsNullOrWhiteSpace(name))));
            return false;
        }

        if (repairedSelections)
        {
            PersistCachedConfigurationRepairs();
            RefreshSelectedProfileDeviceChoicesIfSelected(profile.Id);
        }

        var playbackChanged = false;
        var recordingChanged = false;

        try
        {
            _ = ExecuteTrackedAudioChange(() =>
            {
                if (targetPlaybackDevice is not null)
                {
                    var currentPlaybackDevice = _services.AudioDeviceService.GetDefaultPlaybackDevice();
                    if (currentPlaybackDevice is null ||
                        !string.Equals(currentPlaybackDevice.Id, targetPlaybackDevice.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        _services.AudioDeviceService.SetDefaultPlaybackAndCommunicationDevice(targetPlaybackDevice.Id);
                        playbackChanged = true;
                    }
                }

                if (targetRecordingDevice is not null)
                {
                    var currentRecordingDevice = _services.AudioDeviceService.GetDefaultRecordingDevice();
                    if (currentRecordingDevice is null ||
                        !string.Equals(currentRecordingDevice.Id, targetRecordingDevice.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        _services.AudioDeviceService.SetDefaultRecordingAndCommunicationDevice(targetRecordingDevice.Id);
                        recordingChanged = true;
                    }
                }

                return true;
            });
        }
        catch (Exception ex)
        {
            AppLogger.LogException("MainShellForm.TryApplyProcessProfile", ex);
            isError = true;
            message = ex.Message;
            return false;
        }

        devicesChanged = playbackChanged || recordingChanged;
        message = _services.Localizer.Format("StatusProfileApplied", GetProfileDisplayName(profile));
        return true;
    }

    private static string GetProfileDeviceLabel(DeviceSelection selection)
    {
        return string.IsNullOrWhiteSpace(selection.Name)
            ? selection.Id
            : selection.Name;
    }

    private string GetProfileDisplayName(ProcessAudioProfile profile)
    {
        return string.IsNullOrWhiteSpace(profile.Name)
            ? _services.Localizer.Get("ProfileUnnamed")
            : profile.Name;
    }

    private void ClearActiveProcessProfile()
    {
        _activeProcessProfileId = null;
        _activeProcessProfilePlaybackDeviceId = null;
        _activeProcessProfileRecordingDeviceId = null;
    }

}
