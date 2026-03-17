using System.Runtime.InteropServices;
using SoundDeviceSwitcher.App.Configuration;
using SoundDeviceSwitcher.App.Localization;

namespace SoundDeviceSwitcher.App.Audio;

public sealed class AudioDeviceService
{
    private readonly LocalizationService _localizer;

    public AudioDeviceService(LocalizationService localizer)
    {
        _localizer = localizer;
    }

    public IReadOnlyList<AudioDeviceInfo> GetPlaybackDevices()
    {
        IMMDeviceEnumerator? enumerator = null;
        IMMDeviceCollection? collection = null;

        try
        {
            enumerator = (IMMDeviceEnumerator)(object)new MMDeviceEnumeratorComObject();
            ThrowOnError(enumerator.EnumAudioEndpoints(EDataFlow.Render, DeviceState.Active, out collection));
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
        IMMDeviceEnumerator? enumerator = null;
        IMMDevice? device = null;

        try
        {
            enumerator = (IMMDeviceEnumerator)(object)new MMDeviceEnumeratorComObject();
            ThrowOnError(enumerator.GetDefaultAudioEndpoint(EDataFlow.Render, ERole.Console, out device));
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

    private void SetDefaultPlaybackAndCommunicationDevice(string deviceId)
    {
        IPolicyConfig? policyConfig = null;

        try
        {
            policyConfig = (IPolicyConfig)(object)new PolicyConfigClientComObject();
            ThrowOnError(policyConfig.SetDefaultEndpoint(deviceId, ERole.Console));
            ThrowOnError(policyConfig.SetDefaultEndpoint(deviceId, ERole.Multimedia));
            ThrowOnError(policyConfig.SetDefaultEndpoint(deviceId, ERole.Communications));
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
}
