using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using SkiaSharp;

namespace PKHeX.Drawing;

/// <summary>
/// Image Layering/Blending Utility
/// </summary>
public static class ImageUtil
{
    /// <summary>
    /// Converts a resource to an SKBitmap. Supports SKBitmap directly or embedded PNG resources.
    /// </summary>
    public static SKBitmap GetSKBitmap(object resource)
    {
        if (resource is SKBitmap skBitmap)
            return skBitmap;

        throw new ArgumentException($"Resource must be SKBitmap, got {resource?.GetType().Name}. Use GetSpriteResource for embedded PNG files.");
    }

    /// <summary>
    /// Loads an embedded PNG sprite resource as an SKBitmap.
    /// </summary>
    public static SKBitmap? GetSpriteResource(Assembly assembly, string resourceName)
    {
        try
        {
            var resourcePath = $"PKHeX.Drawing.PokeSprite.Resources.{resourceName}.png";
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            return stream != null ? SKBitmap.Decode(stream) : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Loads an embedded PNG resource as an SKBitmap from item resources.
    /// </summary>
    public static SKBitmap? GetItemResource(Assembly assembly, string itemName)
    {
        try
        {
            var resourcePath = $"PKHeX.Drawing.PokeSprite.Resources.img.{itemName}.png";
            using var stream = assembly.GetManifestResourceStream(resourcePath);
            return stream != null ? SKBitmap.Decode(stream) : null;
        }
        catch
        {
            return null;
        }
    }
    public static SKBitmap LayerImage(SKBitmap baseLayer, SKBitmap overLayer, int x, int y, double transparency)
    {
        overLayer = ChangeOpacity(overLayer, transparency);
        return LayerImage(baseLayer, overLayer, x, y);
    }

    public static SKBitmap LayerImage(SKBitmap baseLayer, SKBitmap overLayer, int x, int y)
    {
        SKBitmap img = new SKBitmap(baseLayer.Info);
        baseLayer.CopyTo(img);
        using var canvas = new SKCanvas(img);
        canvas.DrawBitmap(overLayer, x, y);
        return img;
    }

    public static SKBitmap ChangeOpacity(SKBitmap img, double trans)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out var data);
        SetAllTransparencyTo(data, trans);
        return bmp;
    }

    public static SKBitmap ChangeAllColorTo(SKBitmap img, SKColor c)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out var data);
        ChangeAllColorTo(data, c);
        return bmp;
    }

    public static SKBitmap ChangeTransparentTo(SKBitmap img, SKColor c, byte trans, int start = 0, int end = -1)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out var data);
        if (end == -1)
            end = data.Length - 4;
        SetAllTransparencyTo(data, c, trans, start, end);
        return bmp;
    }

    public static SKBitmap BlendTransparentTo(SKBitmap img, SKColor c, byte trans, int start = 0, int end = -1)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out var data);
        if (end == -1)
            end = data.Length - 4;
        BlendAllTransparencyTo(data, c, trans, start, end);
        return bmp;
    }

    public static SKBitmap WritePixels(SKBitmap img, SKColor c, int start, int end)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out var data);
        ChangeAllTo(data, c, start, end);
        return bmp;
    }

    public static SKBitmap ToGrayscale(SKBitmap img)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out var data);
        SetAllColorToGrayScale(data);
        return bmp;
    }

    private static void GetBitmapData(SKBitmap bmp, out Span<byte> data)
    {
        var pixmap = bmp.PeekPixels();
        data = pixmap.GetPixelSpan();
    }

    public static SKBitmap GetBitmap(ReadOnlySpan<byte> data, int width, int height, SKColorType format = SKColorType.Bgra8888)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, format));
        using var pixmap = bitmap.PeekPixels();
        data.CopyTo(pixmap.GetPixelSpan());
        return bitmap;
    }

    public static byte[] GetPixelData(SKBitmap bitmap)
    {
        var pixmap = bitmap.PeekPixels();
        return pixmap.GetPixelSpan().ToArray();
    }

    public static void SetAllUsedPixelsOpaque(Span<byte> data)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] != 0)
                data[i + 3] = 0xFF;
        }
    }

    public static void RemovePixels(Span<byte> pixels, ReadOnlySpan<byte> original)
    {
        var arr = MemoryMarshal.Cast<byte, int>(pixels);
        for (int i = original.Length - 4; i >= 0; i -= 4)
        {
            if (original[i + 3] != 0)
                arr[i >> 2] = 0;
        }
    }

    private static void SetAllTransparencyTo(Span<byte> data, double trans)
    {
        for (int i = 0; i < data.Length; i += 4)
            data[i + 3] = (byte)(data[i + 3] * trans);
    }

    public static void SetAllTransparencyTo(Span<byte> data, SKColor c, byte trans, int start, int end)
    {
        var arr = MemoryMarshal.Cast<byte, uint>(data);
        var value = (uint)new SKColor(c.Red, c.Green, c.Blue, trans);
        for (int i = end; i >= start; i -= 4)
        {
            if (data[i + 3] == 0)
                arr[i >> 2] = value;
        }
    }

    public static void BlendAllTransparencyTo(Span<byte> data, SKColor c, byte trans, int start, int end)
    {
        var arr = MemoryMarshal.Cast<byte, uint>(data);
        var value = (uint)new SKColor(c.Red, c.Green, c.Blue, trans);
        for (int i = end; i >= start; i -= 4)
        {
            var alpha = data[i + 3];
            if (alpha == 0)
                arr[i >> 2] = value;
            else if (alpha != 0xFF)
                arr[i >> 2] = (uint)BlendColor((int)arr[i >> 2], (int)value);
        }
    }

    public static uint GetAverageColor(Span<byte> data)
    {
        long r = 0, g = 0, b = 0;
        int count = 0;
        for (int i = 0; i < data.Length; i += 4)
        {
            var alpha = data[i + 3];
            if (alpha == 0)
                continue;
            r += data[i + 2];
            g += data[i + 1];
            b += data[i + 0];
            count++;
        }
        if (count == 0)
            return 0;
        byte R = (byte)(r / count);
        byte G = (byte)(g / count);
        byte B = (byte)(b / count);
        return (0xFFu << 24) | (uint)(R << 16) | (uint)(G << 8) | B;
    }

    // heavily favor second (new) color
    private static int BlendColor(int color1, int color2, double amount = 0.2)
    {
        var a1 = (color1 >> 24) & 0xFF;
        var r1 = (color1 >> 16) & 0xFF;
        var g1 = (color1 >> 8) & 0xFF;
        var b1 = color1 & 0xFF;

        var a2 = (color2 >> 24) & 0xFF;
        var r2 = (color2 >> 16) & 0xFF;
        var g2 = (color2 >> 8) & 0xFF;
        var b2 = color2 & 0xFF;

        byte a = (byte)((a1 * amount) + (a2 * (1 - amount)));
        byte r = (byte)((r1 * amount) + (r2 * (1 - amount)));
        byte g = (byte)((g1 * amount) + (g2 * (1 - amount)));
        byte b = (byte)((b1 * amount) + (b2 * (1 - amount)));

        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    public static void ChangeAllTo(Span<byte> data, SKColor c, int start, int end)
    {
        var arr = MemoryMarshal.Cast<byte, uint>(data[start..end]);
        var value = (uint)new SKColor(c.Red, c.Green, c.Blue);
        arr.Fill(value);
    }

    public static void ChangeAllColorTo(Span<byte> data, SKColor c)
    {
        byte R = c.Red;
        byte G = c.Green;
        byte B = c.Blue;
        for (int i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] == 0)
                continue;
            data[i + 0] = B;
            data[i + 1] = G;
            data[i + 2] = R;
        }
    }

    private static void SetAllColorToGrayScale(Span<byte> data)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] == 0)
                continue;
            byte greyS = (byte)((0.3 * data[i + 2]) + (0.59 * data[i + 1]) + (0.11 * data[i + 0]));
            data[i + 0] = greyS;
            data[i + 1] = greyS;
            data[i + 2] = greyS;
        }
    }

    public static void GlowEdges(Span<byte> data, byte blue, byte green, byte red, int width, int reach = 3, double amount = 0.0777)
    {
        PollutePixels(data, width, reach, amount);
        CleanPollutedPixels(data, blue, green, red);
    }

    private const int PollutePixelColorIndex = 0;

    private static void PollutePixels(Span<byte> data, int width, int reach, double amount)
    {
        int stride = width * 4;
        int height = data.Length / stride;
        for (int i = 0; i < data.Length; i += 4)
        {
            // only pollute outwards if the current pixel is fully opaque
            if (data[i + 3] == 0)
                continue;

            int x = (i % stride) / 4;
            int y = (i / stride);
            {
                int left = Math.Max(0, x - reach);
                int right = Math.Min(width - 1, x + reach);
                int top = Math.Max(0, y - reach);
                int bottom = Math.Min(height - 1, y + reach);
                for (int ix = left; ix <= right; ix++)
                {
                    for (int iy = top; iy <= bottom; iy++)
                    {
                        // update one of the color bits
                        // it is expected that a transparent pixel RGBA value is 0.
                        var c = 4 * (ix + (iy * width));
                        ref var b = ref data[c + PollutePixelColorIndex];
                        b += (byte)(amount * (0xFF - b));
                    }
                }
            }
        }
    }

    private static void CleanPollutedPixels(Span<byte> data, byte blue, byte green, byte red)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            // only clean if the current pixel isn't transparent
            if (data[i + 3] != 0)
                continue;

            // grab the transparency from the donor byte
            var transparency = data[i + PollutePixelColorIndex];
            if (transparency == 0)
                continue;

            data[i + 0] = blue;
            data[i + 1] = green;
            data[i + 2] = red;
            data[i + 3] = transparency;
        }
    }
}
