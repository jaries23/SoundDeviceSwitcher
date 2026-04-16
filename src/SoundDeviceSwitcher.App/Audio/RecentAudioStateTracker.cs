namespace SoundDeviceSwitcher.App.Audio;

internal enum RecentAudioUndoAvailability
{
    Available,
    NoCurrentState,
    NoPreviousState,
    Expired
}

internal enum RecentAudioStateChangeResult
{
    None,
    Recorded,
    Cleared
}

internal sealed class RecentAudioStateTracker
{
    public static readonly TimeSpan UndoWindowDuration = TimeSpan.FromSeconds(5);

    private AudioDeviceState? _currentState;
    private AudioDeviceState? _previousState;
    private DateTime _undoAvailableUntilUtc = DateTime.MinValue;

    public DateTime? UndoAvailableUntilUtc =>
        _previousState is null || _undoAvailableUntilUtc == DateTime.MinValue
            ? null
            : _undoAvailableUntilUtc;

    public void Initialize(AudioDeviceState state)
    {
        _currentState = state;
        _previousState = null;
        _undoAvailableUntilUtc = DateTime.MinValue;
    }

    public RecentAudioStateChangeResult RecordStateChange(
        AudioDeviceState state,
        DateTime utcNow,
        bool collapseIfReturningToPreviousState = false)
    {
        if (_currentState is null)
        {
            Initialize(state);
            return RecentAudioStateChangeResult.None;
        }

        if (_currentState.Matches(state))
        {
            return RecentAudioStateChangeResult.None;
        }

        if (collapseIfReturningToPreviousState &&
            _previousState is not null &&
            _previousState.Matches(state))
        {
            _currentState = state;
            InvalidateUndo();
            return RecentAudioStateChangeResult.Cleared;
        }

        _previousState = _currentState;
        _currentState = state;
        _undoAvailableUntilUtc = utcNow + UndoWindowDuration;
        return RecentAudioStateChangeResult.Recorded;
    }

    public bool TryGetUndoTarget(DateTime utcNow, out AudioDeviceState? targetState, out RecentAudioUndoAvailability availability)
    {
        targetState = null;

        if (_currentState is null)
        {
            availability = RecentAudioUndoAvailability.NoCurrentState;
            return false;
        }

        if (_previousState is null)
        {
            availability = RecentAudioUndoAvailability.NoPreviousState;
            return false;
        }

        if (utcNow > _undoAvailableUntilUtc)
        {
            availability = RecentAudioUndoAvailability.Expired;
            return false;
        }

        availability = RecentAudioUndoAvailability.Available;
        targetState = _previousState;
        return true;
    }

    public void CommitUndo(AudioDeviceState appliedState, DateTime utcNow)
    {
        if (_currentState is null || _previousState is null)
        {
            Initialize(appliedState);
            return;
        }

        var priorCurrentState = _currentState;
        _currentState = appliedState;
        _previousState = priorCurrentState;
        _undoAvailableUntilUtc = utcNow + UndoWindowDuration;
    }

    public void InvalidateUndo()
    {
        _previousState = null;
        _undoAvailableUntilUtc = DateTime.MinValue;
    }

    public bool ExpireUndoIfNeeded(DateTime utcNow)
    {
        if (_previousState is null || utcNow <= _undoAvailableUntilUtc)
        {
            return false;
        }

        InvalidateUndo();
        return true;
    }
}
