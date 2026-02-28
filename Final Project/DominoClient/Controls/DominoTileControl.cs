using System.ComponentModel;

namespace DominoClient.Controls;

/// <summary>
/// Custom control for rendering a domino tile with proper visual styling.
/// </summary>
public class DominoTileControl : UserControl
{
    private int _leftValue;
    private int _rightValue;
    private bool _isSelected;
    private bool _isHovered;

    public event EventHandler? TileClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int LeftValue
    {
        get => _leftValue;
        set
        {
            _leftValue = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int RightValue
    {
        get => _rightValue;
        set
        {
            _rightValue = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            Invalidate();
        }
    }

    public DominoTileControl()
    {
        SetStyle(ControlStyles.UserPaint | 
                 ControlStyles.AllPaintingInWmPaint | 
                 ControlStyles.OptimizedDoubleBuffer | 
                 ControlStyles.ResizeRedraw, true);

        Size = new Size(70, 110);
        Cursor = Cursors.Hand;

        MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
        MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
        Click += (s, e) => TileClicked?.Invoke(this, e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Determine colors based on state
        Color bgColor = _isSelected ? Color.FromArgb(100, 180, 255) : Color.White;
        Color borderColor = _isSelected ? Color.FromArgb(0, 120, 215) : 
                           _isHovered ? Color.FromArgb(100, 100, 100) : Color.Black;
        int borderWidth = _isSelected ? 3 : _isHovered ? 2 : 1;

        // Draw main rectangle (domino body)
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var bgBrush = new SolidBrush(bgColor))
        {
            g.FillRectangle(bgBrush, rect);
        }

        // Draw border
        using (var borderPen = new Pen(borderColor, borderWidth))
        {
            g.DrawRectangle(borderPen, rect);
        }

        // Draw center divider line
        int midY = Height / 2;
        using (var dividerPen = new Pen(Color.Gray, 2))
        {
            g.DrawLine(dividerPen, 5, midY, Width - 5, midY);
        }

        // Draw top half (left value)
        DrawDots(g, _leftValue, new Rectangle(5, 5, Width - 10, midY - 10));

        // Draw bottom half (right value)
        DrawDots(g, _rightValue, new Rectangle(5, midY + 5, Width - 10, midY - 10));
    }

    private void DrawDots(Graphics g, int value, Rectangle area)
    {
        if (value < 0 || value > 6) return;

        const int dotSize = 8;
        using var dotBrush = new SolidBrush(Color.Black);

        // Calculate positions for dots
        int centerX = area.X + area.Width / 2;
        int centerY = area.Y + area.Height / 2;
        int offsetX = area.Width / 3;
        int offsetY = area.Height / 3;

        switch (value)
        {
            case 0:
                // No dots
                break;

            case 1:
                // Center dot
                g.FillEllipse(dotBrush, centerX - dotSize / 2, centerY - dotSize / 2, dotSize, dotSize);
                break;

            case 2:
                // Top-left and bottom-right
                g.FillEllipse(dotBrush, area.X + 5, area.Y + 5, dotSize, dotSize);
                g.FillEllipse(dotBrush, area.Right - dotSize - 5, area.Bottom - dotSize - 5, dotSize, dotSize);
                break;

            case 3:
                // Diagonal: top-left, center, bottom-right
                g.FillEllipse(dotBrush, area.X + 5, area.Y + 5, dotSize, dotSize);
                g.FillEllipse(dotBrush, centerX - dotSize / 2, centerY - dotSize / 2, dotSize, dotSize);
                g.FillEllipse(dotBrush, area.Right - dotSize - 5, area.Bottom - dotSize - 5, dotSize, dotSize);
                break;

            case 4:
                // Four corners
                g.FillEllipse(dotBrush, area.X + 5, area.Y + 5, dotSize, dotSize);
                g.FillEllipse(dotBrush, area.Right - dotSize - 5, area.Y + 5, dotSize, dotSize);
                g.FillEllipse(dotBrush, area.X + 5, area.Bottom - dotSize - 5, dotSize, dotSize);
                g.FillEllipse(dotBrush, area.Right - dotSize - 5, area.Bottom - dotSize - 5, dotSize, dotSize);
                break;

            case 5:
                // Four corners + center
                g.FillEllipse(dotBrush, area.X + 5, area.Y + 5, dotSize, dotSize);
                g.FillEllipse(dotBrush, area.Right - dotSize - 5, area.Y + 5, dotSize, dotSize);
                g.FillEllipse(dotBrush, centerX - dotSize / 2, centerY - dotSize / 2, dotSize, dotSize);
                g.FillEllipse(dotBrush, area.X + 5, area.Bottom - dotSize - 5, dotSize, dotSize);
                g.FillEllipse(dotBrush, area.Right - dotSize - 5, area.Bottom - dotSize - 5, dotSize, dotSize);
                break;

            case 6:
                // Two columns of three
                int leftX = area.X + 10;
                int rightX = area.Right - dotSize - 10;
                int topY = area.Y + 5;
                int midY = centerY - dotSize / 2;
                int bottomY = area.Bottom - dotSize - 5;

                g.FillEllipse(dotBrush, leftX, topY, dotSize, dotSize);
                g.FillEllipse(dotBrush, leftX, midY, dotSize, dotSize);
                g.FillEllipse(dotBrush, leftX, bottomY, dotSize, dotSize);
                g.FillEllipse(dotBrush, rightX, topY, dotSize, dotSize);
                g.FillEllipse(dotBrush, rightX, midY, dotSize, dotSize);
                g.FillEllipse(dotBrush, rightX, bottomY, dotSize, dotSize);
                break;
        }
    }

    public override string ToString()
    {
        return $"{_leftValue}-{_rightValue}";
    }
}
