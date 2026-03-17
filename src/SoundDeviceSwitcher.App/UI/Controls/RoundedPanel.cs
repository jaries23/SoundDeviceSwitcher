using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace SoundDeviceSwitcher.App.UI.Controls;

internal sealed class RoundedPanel : Panel
{
    private Color _borderColor = Color.Transparent;
    private Color _fillColor = SystemColors.Control;
    private int _cornerRadius = 18;
    private int _borderThickness = 1;

    public RoundedPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        Padding = new Padding(18);
        Margin = new Padding(0, 0, 0, 14);
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue(typeof(Color), "Transparent")]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            _borderColor = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue(typeof(Color), "Control")]
    public Color FillColor
    {
        get => _fillColor;
        set
        {
            _fillColor = value;
            Invalidate();
        }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue(18)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Math.Max(0, value);
            UpdateRegion();
            Invalidate();
        }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue(1)]
    public int BorderThickness
    {
        get => _borderThickness;
        set
        {
            _borderThickness = Math.Max(1, value);
            Invalidate();
        }
    }

    protected override void OnSizeChanged(EventArgs eventargs)
    {
        base.OnSizeChanged(eventargs);
        UpdateRegion();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var backgroundPath = CreatePath(ClientRectangle, CornerRadius);
        using var fillBrush = new SolidBrush(FillColor);
        e.Graphics.FillPath(fillBrush, backgroundPath);

        var borderBounds = Rectangle.Inflate(ClientRectangle, -1, -1);
        using var borderPath = CreatePath(borderBounds, Math.Max(1, CornerRadius - 1));
        using var borderPen = new Pen(BorderColor, BorderThickness);
        e.Graphics.DrawPath(borderPen, borderPath);
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0)
        {
            return;
        }

        using var path = CreatePath(ClientRectangle, CornerRadius);
        Region = new Region(path);
    }

    private static GraphicsPath CreatePath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        if (diameter <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
