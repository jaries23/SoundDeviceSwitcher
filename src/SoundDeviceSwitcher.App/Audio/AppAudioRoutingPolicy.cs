using System.Runtime.InteropServices;

namespace SoundDeviceSwitcher.App.Audio;

internal sealed class AppAudioRoutingPolicy : IDisposable
{
    private const string PolicyConfigClassId = "Windows.Media.Internal.AudioPolicyConfig";
    private const string MmDevApiToken = @"\\?\SWD#MMDEVAPI#";
    private const string RenderDeviceInterfaceId = "#{e6327cad-dcec-4949-ae8a-991e976a79d2}";
    private const int BuildWindows10Version1803 = 17134;
    private const int BuildWindows10Version21H2 = 21390;
    private const int VtableReleaseIndex = 2;
    private const int VtableSetPersistedDefaultEndpointIndex = 25;
    private const int VtableGetPersistedDefaultEndpointIndex = 26;
    private const int SFalse = 1;
    private const int RpcEChangedMode = unchecked((int)0x80010106);
    private const int HresultNotFound = unchecked((int)0x80070490);

    private static readonly Guid PolicyConfigDownlevelInterfaceId = new("2a59116d-6c4f-45e0-a74f-707e3fef9258");
    private static readonly Guid PolicyConfig21H2InterfaceId = new("ab3d4648-e242-459f-b02f-541c70306324");

    private readonly RoInitializationScope _roInitializationScope;
    private IntPtr _factoryPointer;

    private AppAudioRoutingPolicy(RoInitializationScope roInitializationScope, IntPtr factoryPointer)
    {
        _roInitializationScope = roInitializationScope;
        _factoryPointer = factoryPointer;
    }

    public static AppAudioRoutingPolicy Activate()
    {
        if (Environment.OSVersion.Version.Build < BuildWindows10Version1803)
        {
            throw new PlatformNotSupportedException("Per-app audio routing requires Windows 10 version 1803 or newer.");
        }

        var roInitializationScope = RoInitializationScope.Enter();
        try
        {
            var interfaceId = Environment.OSVersion.Version.Build >= BuildWindows10Version21H2
                ? PolicyConfig21H2InterfaceId
                : PolicyConfigDownlevelInterfaceId;
            using var classId = HString.Create(PolicyConfigClassId);
            ThrowOnError(RoGetActivationFactory(classId.Handle, ref interfaceId, out var factoryPointer));
            return new AppAudioRoutingPolicy(roInitializationScope, factoryPointer);
        }
        catch
        {
            roInitializationScope.Dispose();
            throw;
        }
    }

    public void SetPersistedDefaultRenderEndpoint(uint processId, string? deviceId)
    {
        using var packedDeviceId = HString.Create(PackRenderDeviceId(deviceId));
        var setEndpoint = GetDelegate<SetPersistedDefaultAudioEndpointDelegate>(VtableSetPersistedDefaultEndpointIndex);
        ThrowOnError(setEndpoint(_factoryPointer, processId, EDataFlow.Render, ERole.Console, packedDeviceId.Handle));
        ThrowOnError(setEndpoint(_factoryPointer, processId, EDataFlow.Render, ERole.Multimedia, packedDeviceId.Handle));
    }

    public string? GetPersistedDefaultRenderEndpoint(uint processId)
    {
        var getEndpoint = GetDelegate<GetPersistedDefaultAudioEndpointDelegate>(VtableGetPersistedDefaultEndpointIndex);
        var hstring = IntPtr.Zero;
        var hresult = getEndpoint(_factoryPointer, processId, EDataFlow.Render, ERole.Multimedia, out hstring);
        if (hresult == HresultNotFound || hstring == IntPtr.Zero)
        {
            return null;
        }

        ThrowOnError(hresult);

        try
        {
            return UnpackRenderDeviceId(HString.ToString(hstring));
        }
        finally
        {
            _ = WindowsDeleteString(hstring);
        }
    }

    public void Dispose()
    {
        if (_factoryPointer != IntPtr.Zero)
        {
            var release = GetDelegate<ReleaseDelegate>(VtableReleaseIndex);
            _ = release(_factoryPointer);
            _factoryPointer = IntPtr.Zero;
        }

        _roInitializationScope.Dispose();
    }

    private static string? PackRenderDeviceId(string? deviceId)
    {
        return string.IsNullOrWhiteSpace(deviceId)
            ? null
            : $"{MmDevApiToken}{deviceId}{RenderDeviceInterfaceId}";
    }

    private static string? UnpackRenderDeviceId(string? policyDeviceId)
    {
        if (string.IsNullOrWhiteSpace(policyDeviceId))
        {
            return null;
        }

        var unpacked = policyDeviceId;
        if (unpacked.StartsWith(MmDevApiToken, StringComparison.OrdinalIgnoreCase))
        {
            unpacked = unpacked[MmDevApiToken.Length..];
        }

        return unpacked.EndsWith(RenderDeviceInterfaceId, StringComparison.OrdinalIgnoreCase)
            ? unpacked[..^RenderDeviceInterfaceId.Length]
            : unpacked;
    }

    private TDelegate GetDelegate<TDelegate>(int vtableIndex)
        where TDelegate : Delegate
    {
        var vtablePointer = Marshal.ReadIntPtr(_factoryPointer);
        var functionPointer = Marshal.ReadIntPtr(vtablePointer, vtableIndex * IntPtr.Size);
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(functionPointer);
    }

    private static void ThrowOnError(int hresult)
    {
        if (hresult < 0)
        {
            Marshal.ThrowExceptionForHR(hresult);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint ReleaseDelegate(IntPtr thisPointer);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int SetPersistedDefaultAudioEndpointDelegate(
        IntPtr thisPointer,
        uint processId,
        EDataFlow flow,
        ERole role,
        IntPtr deviceId);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetPersistedDefaultAudioEndpointDelegate(
        IntPtr thisPointer,
        uint processId,
        EDataFlow flow,
        ERole role,
        out IntPtr deviceId);

    private sealed class RoInitializationScope : IDisposable
    {
        private readonly bool _shouldUninitialize;

        private RoInitializationScope(bool shouldUninitialize)
        {
            _shouldUninitialize = shouldUninitialize;
        }

        public static RoInitializationScope Enter()
        {
            var hresult = RoInitialize(RoInitType.SingleThreaded);
            if (hresult == RpcEChangedMode)
            {
                return new RoInitializationScope(shouldUninitialize: false);
            }

            ThrowOnError(hresult);
            return new RoInitializationScope(shouldUninitialize: hresult is 0 or SFalse);
        }

        public void Dispose()
        {
            if (_shouldUninitialize)
            {
                RoUninitialize();
            }
        }
    }

    private sealed class HString : IDisposable
    {
        private HString(IntPtr handle)
        {
            Handle = handle;
        }

        public IntPtr Handle { get; private set; }

        public static HString Create(string? value)
        {
            if (value is null)
            {
                return new HString(IntPtr.Zero);
            }

            ThrowOnError(WindowsCreateString(value, (uint)value.Length, out var handle));
            return new HString(handle);
        }

        public static string? ToString(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return null;
            }

            var buffer = WindowsGetStringRawBuffer(handle, out var length);
            return buffer == IntPtr.Zero ? null : Marshal.PtrToStringUni(buffer, checked((int)length));
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                _ = WindowsDeleteString(Handle);
                Handle = IntPtr.Zero;
            }
        }
    }

    private enum RoInitType
    {
        MultiThreaded = 0,
        SingleThreaded = 1
    }

    [DllImport("combase.dll")]
    private static extern int RoInitialize(RoInitType initType);

    [DllImport("combase.dll")]
    private static extern void RoUninitialize();

    [DllImport("combase.dll")]
    private static extern int RoGetActivationFactory(IntPtr activatableClassId, ref Guid interfaceId, out IntPtr factory);

    [DllImport("combase.dll", CharSet = CharSet.Unicode)]
    private static extern int WindowsCreateString(string sourceString, uint length, out IntPtr hstring);

    [DllImport("combase.dll")]
    private static extern int WindowsDeleteString(IntPtr hstring);

    [DllImport("combase.dll")]
    private static extern IntPtr WindowsGetStringRawBuffer(IntPtr hstring, out uint length);
}
