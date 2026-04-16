using System.Runtime.InteropServices;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Localization;

namespace SoundDeviceSwitcher.App.Audio;

public sealed class AudioDeviceService
{
    private const DeviceState ProfileSelectableDeviceStates = DeviceState.Active | DeviceState.Unplugged;
    private readonly LocalizationService _localizer;

    public AudioDeviceService(LocalizationService localizer)
    {
        _localizer = localizer;
    }

    public IReadOnlyList<AudioDeviceInfo> GetPlaybackDevices()
    {
        return GetDevices(EDataFlow.Render, DeviceState.Active);
    }

    public IReadOnlyList<AudioDeviceInfo> GetRecordingDevices()
    {
        return GetDevices(EDataFlow.Capture, DeviceState.Active);
    }

    public IReadOnlyList<AudioDeviceInfo> GetSelectablePlaybackDevices()
    {
        return GetDevices(EDataFlow.Render, ProfileSelectableDeviceStates);
    }

    public IReadOnlyList<AudioDeviceInfo> GetSelectableRecordingDevices()
    {
        return GetDevices(EDataFlow.Capture, ProfileSelectableDeviceStates);
    }

    public AudioDeviceInfo? ResolveActivePlaybackDevice(DeviceSelection selection, out bool resolvedByName)
    {
        return ResolveDevice(selection, EDataFlow.Render, DeviceState.Active, out resolvedByName);
    }

    public AudioDeviceInfo? ResolveActiveRecordingDevice(DeviceSelection selection, out bool resolvedByName)
    {
        return ResolveDevice(selection, EDataFlow.Capture, DeviceState.Active, out resolvedByName);
    }

    public AudioDeviceInfo? ResolveSelectablePlaybackDevice(DeviceSelection selection, out bool resolvedByName)
    {
        return ResolveDevice(selection, EDataFlow.Render, ProfileSelectableDeviceStates, out resolvedByName);
    }

    public AudioDeviceInfo? ResolveSelectableRecordingDevice(DeviceSelection selection, out bool resolvedByName)
    {
        return ResolveDevice(selection, EDataFlow.Capture, ProfileSelectableDeviceStates, out resolvedByName);
    }

    private IReadOnlyList<AudioDeviceInfo> GetDevices(EDataFlow dataFlow, DeviceState stateMask)
    {
        IMMDeviceEnumerator? enumerator = null;
        IMMDeviceCollection? collection = null;

        try
        {
            enumerator = (IMMDeviceEnumerator)(object)new MMDeviceEnumeratorComObject();
            ThrowOnError(enumerator.EnumAudioEndpoints(dataFlow, stateMask, out collection));
            ThrowOnError(collection.GetCount(out var count));

            var devices = new List<AudioDeviceInfo>((int)count);
            for (uint index = 0; index < count; index++)
            {
                IMMDevice? device = null;
                try
                {
                    ThrowOnError(collection.Item(index, out device));
                    var id = GetDeviceId(device);
                    var name = GetDeviceFriendlyName(device);
                    devices.Add(new AudioDeviceInfo(id, name, name));
                }
                finally
                {
                    ReleaseComObject(device);
                }
            }

            return DisambiguateDuplicateNames(devices);
        }
        catch (COMException ex)
        {
            throw new InvalidOperationException(
                _localizer.Format("ErrorCouldNotReadAudioDevices", $"0x{ex.HResult:X8}", ex.Message),
                ex);
        }
        finally
        {
            ReleaseComObject(collection);
            ReleaseComObject(enumerator);
        }
    }

    public AudioDeviceInfo? GetDefaultPlaybackDevice()
    {
        return GetDefaultRenderDevice(ERole.Console);
    }

    public AudioDeviceInfo? GetDefaultCommunicationDevice()
    {
        return GetDefaultRenderDevice(ERole.Communications);
    }

    public AudioDeviceInfo? GetDefaultRecordingDevice()
    {
        return GetDefaultCaptureDevice(ERole.Console);
    }

    public AudioDeviceState CaptureCurrentState()
    {
        return new AudioDeviceState(
            CreateEndpointState(GetDefaultPlaybackDevice()),
            CreateEndpointState(GetDefaultRecordingDevice()),
            CreateEndpointState(GetDefaultCommunicationDevice()));
    }

    public bool IsPlaybackDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return false;
        }

        try
        {
            return GetPlaybackDevices().Any(device => string.Equals(device.Id, deviceId, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    public bool IsRecordingDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return false;
        }

        try
        {
            return GetRecordingDevices().Any(device => string.Equals(device.Id, deviceId, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }

    private AudioDeviceInfo? ResolveDevice(
        DeviceSelection selection,
        EDataFlow dataFlow,
        DeviceState stateMask,
        out bool resolvedByName)
    {
        resolvedByName = false;

        if (string.IsNullOrWhiteSpace(selection.Id))
        {
            return null;
        }

        var candidateDevices = GetDevices(dataFlow, stateMask);
        var idMatch = candidateDevices.FirstOrDefault(
            device => string.Equals(device.Id, selection.Id, StringComparison.OrdinalIgnoreCase));
        if (idMatch is not null)
        {
            return idMatch;
        }

        if (string.IsNullOrWhiteSpace(selection.Name))
        {
            return null;
        }

        var nameMatches = candidateDevices
            .Where(device => DeviceNamesMatch(device.Name, selection.Name))
            .ToList();

        if (nameMatches.Count == 0)
        {
            nameMatches = candidateDevices
                .Where(device => DeviceNamesMatch(device.DisplayName, selection.Name))
                .ToList();
        }

        if (nameMatches.Count != 1)
        {
            return null;
        }

        resolvedByName = true;
        return nameMatches[0];
    }

    public void SetDefaultCommunicationDevice(string deviceId)
    {
        SetDefaultRenderDevice(deviceId, ERole.Communications);
    }

    public void SetDefaultPlaybackDevice(string deviceId)
    {
        SetDefaultRenderDevice(deviceId, ERole.Console, ERole.Multimedia);
    }

    public void SetDefaultRecordingDevice(string deviceId)
    {
        SetDefaultCaptureDevice(deviceId, ERole.Console, ERole.Multimedia);
    }

    public void SetDefaultPlaybackAndCommunicationDevice(string deviceId)
    {
        SetDefaultRenderDevice(deviceId, ERole.Console, ERole.Multimedia, ERole.Communications);
    }

    public void SetDefaultRecordingAndCommunicationDevice(string deviceId)
    {
        SetDefaultCaptureDevice(deviceId, ERole.Console, ERole.Multimedia, ERole.Communications);
    }

    private AudioDeviceInfo? GetDefaultRenderDevice(ERole role)
    {
        return GetDefaultDevice(EDataFlow.Render, role);
    }

    private AudioDeviceInfo? GetDefaultCaptureDevice(ERole role)
    {
        return GetDefaultDevice(EDataFlow.Capture, role);
    }

    private AudioDeviceInfo? GetDefaultDevice(EDataFlow dataFlow, ERole role)
    {
        IMMDeviceEnumerator? enumerator = null;
        IMMDevice? device = null;

        try
        {
            enumerator = (IMMDeviceEnumerator)(object)new MMDeviceEnumeratorComObject();
            ThrowOnError(enumerator.GetDefaultAudioEndpoint(dataFlow, role, out device));
            var deviceId = GetDeviceId(device);
            var friendlyName = GetDeviceFriendlyName(device);
            return new AudioDeviceInfo(deviceId, friendlyName, friendlyName);
        }
        catch (COMException)
        {
            return null;
        }
        finally
        {
            ReleaseComObject(device);
            ReleaseComObject(enumerator);
        }
    }

    public ToggleResult Toggle(AppConfig config)
    {
        var devices = GetPlaybackDevices()
            .ToDictionary(device => device.Id, StringComparer.OrdinalIgnoreCase);

        var missingDevices = new List<string>();
        if (!devices.ContainsKey(config.PrimaryDevice.Id))
        {
            missingDevices.Add(config.PrimaryDevice.Name);
        }

        if (!devices.ContainsKey(config.SecondaryDevice.Id))
        {
            missingDevices.Add(config.SecondaryDevice.Name);
        }

        if (missingDevices.Count > 0)
        {
            return ToggleResult.Fail(
                _localizer.Format("ToggleMissingDevices", string.Join(", ", missingDevices)));
        }

        var currentDefault = GetDefaultPlaybackDevice();
        var targetDevice = currentDefault is not null &&
                           string.Equals(currentDefault.Id, config.PrimaryDevice.Id, StringComparison.OrdinalIgnoreCase)
            ? devices[config.SecondaryDevice.Id]
            : devices[config.PrimaryDevice.Id];

        try
        {
            SetDefaultPlaybackAndCommunicationDevice(targetDevice.Id);
            return ToggleResult.Ok(_localizer.Format("ToggleSwitched", targetDevice.Name), targetDevice.Id);
        }
        catch (Exception ex)
        {
            return ToggleResult.Fail(_localizer.Format("ToggleFailed", ex.Message));
        }
    }

    public AudioDeviceStateApplyResult ApplyState(AudioDeviceState state)
    {
        var missingDevices = new List<string>();
        if (state.PlaybackDevice is not null && !IsPlaybackDevice(state.PlaybackDevice.DeviceId))
        {
            missingDevices.Add(state.PlaybackDevice.DisplayName);
        }

        if (state.RecordingDevice is not null && !IsRecordingDevice(state.RecordingDevice.DeviceId))
        {
            missingDevices.Add(state.RecordingDevice.DisplayName);
        }

        if (state.CommunicationDevice is not null && !IsPlaybackDevice(state.CommunicationDevice.DeviceId))
        {
            missingDevices.Add(state.CommunicationDevice.DisplayName);
        }

        if (missingDevices.Count > 0)
        {
            return AudioDeviceStateApplyResult.Fail(
                _localizer.Format("ErrorRecentSwitchUndoMissingDevices", string.Join(", ", missingDevices)));
        }

        try
        {
            var currentState = CaptureCurrentState();
            if (!AudioDeviceState.IsSameDevice(currentState.PlaybackDevice, state.PlaybackDevice) &&
                state.PlaybackDevice is not null)
            {
                SetDefaultPlaybackDevice(state.PlaybackDevice.DeviceId);
            }

            if (!AudioDeviceState.IsSameDevice(currentState.RecordingDevice, state.RecordingDevice) &&
                state.RecordingDevice is not null)
            {
                SetDefaultRecordingDevice(state.RecordingDevice.DeviceId);
            }

            if (!AudioDeviceState.IsSameDevice(currentState.CommunicationDevice, state.CommunicationDevice) &&
                state.CommunicationDevice is not null)
            {
                SetDefaultCommunicationDevice(state.CommunicationDevice.DeviceId);
            }

            return AudioDeviceStateApplyResult.Ok(_localizer.Get("StatusRecentSwitchUndoApplied"));
        }
        catch (Exception ex)
        {
            return AudioDeviceStateApplyResult.Fail(
                _localizer.Format("ErrorRecentSwitchUndoFailed", ex.Message));
        }
    }

    private static void SetDefaultRenderDevice(string deviceId, params ERole[] roles)
    {
        SetDefaultDevice(deviceId, roles);
    }

    private static void SetDefaultCaptureDevice(string deviceId, params ERole[] roles)
    {
        SetDefaultDevice(deviceId, roles);
    }

    private static void SetDefaultDevice(string deviceId, params ERole[] roles)
    {
        IPolicyConfig? policyConfig = null;

        try
        {
            policyConfig = (IPolicyConfig)(object)new PolicyConfigClientComObject();
            foreach (var role in roles)
            {
                ThrowOnError(policyConfig.SetDefaultEndpoint(deviceId, role));
            }
        }
        finally
        {
            ReleaseComObject(policyConfig);
        }
    }

    private static IReadOnlyList<AudioDeviceInfo> DisambiguateDuplicateNames(IEnumerable<AudioDeviceInfo> devices)
    {
        var groupedDevices = devices
            .GroupBy(device => device.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

        var resolved = new List<AudioDeviceInfo>();
        foreach (var device in devices.OrderBy(device => device.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (groupedDevices[device.Name] == 1)
            {
                resolved.Add(device);
                continue;
            }

            var suffix = device.Id.Length > 8 ? device.Id[^8..] : device.Id;
            resolved.Add(new AudioDeviceInfo(device.Id, device.Name, $"{device.Name} [{suffix}]"));
        }

        return resolved;
    }

    private static bool DeviceNamesMatch(string left, string right)
    {
        return string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string GetDeviceId(IMMDevice device)
    {
        ThrowOnError(device.GetId(out var deviceId));
        return deviceId;
    }

    private static string GetDeviceFriendlyName(IMMDevice device)
    {
        IPropertyStore? propertyStore = null;
        PropVariant value = default;

        try
        {
            ThrowOnError(device.OpenPropertyStore(StorageAccessMode.Read, out propertyStore));
            var propertyKey = PropertyKeys.DeviceFriendlyName;
            ThrowOnError(propertyStore.GetValue(ref propertyKey, out value));

            var friendlyName = value.GetString();
            return string.IsNullOrWhiteSpace(friendlyName) ? "Unknown device" : friendlyName;
        }
        finally
        {
            value.Dispose();
            ReleaseComObject(propertyStore);
        }
    }

    private static void ThrowOnError(int hresult)
    {
        if (hresult < 0)
        {
            Marshal.ThrowExceptionForHR(hresult);
        }
    }

    private static void ReleaseComObject(object? comObject)
    {
        if (comObject is not null && Marshal.IsComObject(comObject))
        {
            Marshal.ReleaseComObject(comObject);
        }
    }

    private static AudioEndpointState? CreateEndpointState(AudioDeviceInfo? device)
    {
        if (device is null)
        {
            return null;
        }

        return new AudioEndpointState(device.Id, device.Name);
    }
}
