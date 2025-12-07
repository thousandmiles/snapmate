namespace SnapMate.Models;

/// <summary>
/// Base class for all annotation types that can be drawn on screenshots.
/// </summary>
public abstract class Annotation
{
    /// <summary>
    /// Gets or sets the unique identifier for this annotation.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the type of annotation.
    /// </summary>
    public AnnotationType Type { get; set; }

    /// <summary>
    /// Gets or sets the color of the annotation in hex format (e.g., "#FF0000" for red).
    /// </summary>
    public string Color { get; set; } = "#FF0000";

    /// <summary>
    /// Gets or sets the stroke width for drawn elements in pixels.
    /// </summary>
    public int StrokeWidth { get; set; } = 2;
}

/// <summary>
/// Represents an arrow annotation pointing from one point to another.
/// </summary>
public class ArrowAnnotation : Annotation
{
    /// <summary>
    /// Gets or sets the X coordinate of the arrow's starting point.
    /// </summary>
    public double X1 { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the arrow's starting point.
    /// </summary>
    public double Y1 { get; set; }

    /// <summary>
    /// Gets or sets the X coordinate of the arrow's ending point.
    /// </summary>
    public double X2 { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the arrow's ending point.
    /// </summary>
    public double Y2 { get; set; }
}

/// <summary>
/// Represents a rectangular annotation for highlighting areas.
/// </summary>
public class RectangleAnnotation : Annotation
{
    /// <summary>
    /// Gets or sets the X coordinate of the rectangle's top-left corner.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the rectangle's top-left corner.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the rectangle.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the rectangle.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Gets or sets whether the rectangle should be filled or just outlined.
    /// </summary>
    public bool IsFilled { get; set; }
}

/// <summary>
/// Represents a text annotation with customizable font size.
/// </summary>
public class TextAnnotation : Annotation
{
    /// <summary>
    /// Gets or sets the X coordinate of the text's starting position.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the text's starting position.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the text content to display.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the font size in points.
    /// </summary>
    public int FontSize { get; set; } = 16;
}

/// <summary>
/// Represents a mosaic (pixelation/blur) effect applied to a specific region.
/// </summary>
public class MosaicAnnotation : Annotation
{
    /// <summary>
    /// Gets or sets the X coordinate of the mosaic region's top-left corner.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the mosaic region's top-left corner.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the mosaic region.
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the mosaic region.
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Gets or sets the size of each pixel block in the mosaic effect.
    /// </summary>
    public int PixelSize { get; set; } = 10;
}

/// <summary>
/// Defines the available types of annotations.
/// </summary>
public enum AnnotationType
{
    /// <summary>
    /// Arrow pointing from one location to another.
    /// </summary>
    Arrow,

    /// <summary>
    /// Rectangular shape for highlighting.
    /// </summary>
    Rectangle,

    /// <summary>
    /// Text label or comment.
    /// </summary>
    Text,

    /// <summary>
    /// Mosaic/blur effect for obscuring content.
    /// </summary>
    Mosaic
}
