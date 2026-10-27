using System.Windows.Forms;

namespace SoundDeviceSwitcher.App.UI.Controls;

/// <summary>
/// A panel that composites its entire child hierarchy off-screen (WS_EX_COMPOSITED)
/// and blits the finished frame in one pass. Used as the settings section host so that
/// swapping the visible section card never shows its child controls painting one by one
/// (the empty skeleton boxes seen for a frame before the content fills in).
/// </summary>
internal sealed class CompositedPanel : Panel
{
    private const int WsExComposited = 0x02000000;

    protected override CreateParams CreateParams
    {
        get
        {
            var createParams = base.CreateParams;
            createParams.ExStyle |= WsExComposited;
            return createParams;
        }
    }
}
