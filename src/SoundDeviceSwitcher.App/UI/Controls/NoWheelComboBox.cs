namespace SoundDeviceSwitcher.App.UI.Controls;

internal sealed class NoWheelComboBox : ComboBox
{
    private const int WmMouseWheel = 0x020A;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmMouseWheel && !DroppedDown)
        {
            return;
        }

        base.WndProc(ref m);
    }
}
