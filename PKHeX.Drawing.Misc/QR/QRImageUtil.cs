using System;
using SkiaSharp;

namespace PKHeX.Drawing.Misc;

/// <summary>
/// Provides utility methods for composing and extending QR code images with overlays and text.
/// </summary>
public static class QRImageUtil
{
    /// <summary>
    /// Creates a new QR image with a preview image layered in the center.
    /// </summary>
    /// <param name="qr">The base QR code image.</param>
    /// <param name="preview">The preview image to overlay.</param>
    /// <returns>A new bitmap with the preview image centered on the QR code.</returns>
    public static SKBitmap GetQRImage(SKBitmap qr, SKBitmap preview)
    {
        // create a small area with the pk sprite, with a white background
        var foreground = new SKBitmap(preview.Width + 4, preview.Height + 4);
        using (SKCanvas canvas = new SKCanvas(foreground))
        {
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(0, 0, foreground.Width, foreground.Height, paint);
            int x = (foreground.Width / 2) - (preview.Width / 2);
            int y = (foreground.Height / 2) - (preview.Height / 2);
            canvas.DrawBitmap(preview, x, y);
        }

        // Layer on Preview Image
        {
            int x = (qr.Width / 2) - (foreground.Width / 2);
            int y = (qr.Height / 2) - (foreground.Height / 2);
            return ImageUtil.LayerImage(qr, foreground, x, y);
        }
    }

    /// <summary>
    /// Creates an extended QR image with additional text and formatting.
    /// </summary>
    /// <param name="typeface">The typeface to use for text.</param>
    /// <param name="qr">The base QR code image.</param>
    /// <param name="pk">The preview image to overlay.</param>
    /// <param name="width">The width of the final image.</param>
    /// <param name="height">The height of the final image.</param>
    /// <param name="lines">The lines of text to display.</param>
    /// <param name="extraText">Additional text to display.</param>
    /// <returns>A new bitmap with the preview image and extended text.</returns>
    public static SKBitmap GetQRImageExtended(SKTypeface typeface, SKBitmap qr, SKBitmap pk, int width, int height, ReadOnlySpan<string> lines, string extraText)
    {
        var pic = GetQRImage(qr, pk);
        return ExtendImage(typeface, qr, width, height, pic, lines, extraText);
    }

    /// <summary>
    /// Extends an image with additional lines of text and formatting.
    /// </summary>
    /// <param name="typeface">The typeface to use for text.</param>
    /// <param name="qr">The base QR code image.</param>
    /// <param name="width">The width of the final image.</param>
    /// <param name="height">The height of the final image.</param>
    /// <param name="pic">The image to extend.</param>
    /// <param name="lines">The lines of text to display.</param>
    /// <param name="extraText">Additional text to display.</param>
    /// <returns>A new bitmap with extended text.</returns>
    private static SKBitmap ExtendImage(SKTypeface typeface, SKBitmap qr, int width, int height, SKBitmap pic, ReadOnlySpan<string> lines, string extraText)
    {
        var newpic = new SKBitmap(width, height);
        using SKCanvas g = new SKCanvas(newpic);
        using var bgPaint = new SKPaint { Color = SKColors.White };
        g.DrawRect(0, 0, newpic.Width, newpic.Height, bgPaint);
        g.DrawBitmap(pic, 0, 0);

        using var font = new SKFont(typeface, 12);
        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        const int indent = 18;
        g.DrawText(GetLine(lines, 0), indent, qr.Height - 5, font, textPaint);
        g.DrawText(GetLine(lines, 1), indent, qr.Height + 8, font, textPaint);
        g.DrawText(GetLine2(lines), indent, qr.Height + 20, font, textPaint);
        g.DrawText(GetLine(lines, 3) + extraText, indent, qr.Height + 32, font, textPaint);
        return newpic;
    }

    /// <summary>
    /// Gets and formats the second line of text for display.
    /// </summary>
    /// <param name="lines">The lines of text.</param>
    /// <returns>The formatted second line.</returns>
    private static string GetLine2(ReadOnlySpan<string> lines) => GetLine(lines, 2)
        .Replace(Environment.NewLine, "/")
        .Replace("//", "   ")
        .Replace(":/", ": ");

    /// <summary>
    /// Gets a specific line of text or an empty string if the line does not exist.
    /// </summary>
    /// <param name="lines">The lines of text.</param>
    /// <param name="line">The line index to retrieve.</param>
    /// <returns>The requested line or an empty string.</returns>
    private static string GetLine(ReadOnlySpan<string> lines, int line) => lines.Length <= line ? string.Empty : lines[line];
}
