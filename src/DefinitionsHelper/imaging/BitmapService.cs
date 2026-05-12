using System.Runtime.InteropServices;

namespace DefinitionsHelper.Imaging;

/// <summary>
/// Converts PNG bytes to a Windows bitmap handle using the GDI+ (Graphics Device Interface Plus)
/// flat API via P/Invoke. No external dependencies - uses only Windows system DLLs.
///
/// GDI+ function names (GdipXxx) and Windows handle types (HBITMAP) are fixed API names
/// and cannot be renamed - they must match the exported symbols in gdiplus.dll.
/// </summary>
internal static partial class BitmapService
{
    private const int Ok = 0;

    /// <summary>
    /// Creates a Windows bitmap handle from PNG bytes, scaled to fit within the requested size
    /// while maintaining aspect ratio. Optionally overlays a logo icon at the bottom-right corner.
    ///
    /// Produces a square thumbnail canvas with the image centered, matching Windows'
    /// sizing behavior for the sibling preview PNG files while keeping the overlay in
    /// a consistent bottom-right canvas position.
    /// Returns 0 on failure.
    /// </summary>
    public static nint CreateBitmapFromPng(byte[] pngBytes, uint maxThumbnailSize, string? overlayIconPath = null)
    {
        nint gdiplusToken = 0;
        nint sourceBitmap = 0;
        nint thumbnail = 0;
        nint thumbnailGraphics = 0;
        nint overlayImage = 0;
        nint pngStream = 0;

        try
        {
            var startupInput = new GdiplusStartupInput
            {
                GdiplusVersion = 1,
                SuppressBackgroundThread = 0,
                SuppressExternalCodecs = 0,
                DebugEventCallback = 0
            };
            int status = GdiplusStartup(out gdiplusToken, ref startupInput, out _);
            if (status != Ok) return 0;

            pngStream = SHCreateMemStream(pngBytes, pngBytes.Length);
            if (pngStream == 0) return 0;

            status = GdipCreateBitmapFromStream(pngStream, out sourceBitmap);
            if (status != Ok) return 0;

            status = GdipGetImageWidth(sourceBitmap, out uint originalWidth);
            if (status != Ok) return 0;
            status = GdipGetImageHeight(sourceBitmap, out uint originalHeight);
            if (status != Ok) return 0;

            int canvasSize = (int)maxThumbnailSize;
            var placement = CalculateCenteredImagePlacement(originalWidth, originalHeight, maxThumbnailSize);

            status = GdipCreateBitmapFromScan0(
                canvasSize,
                canvasSize,
                0,
                PixelFormat32bppARGB,
                0,
                out thumbnail);
            if (status != Ok) return 0;

            status = GdipGetImageGraphicsContext(thumbnail, out thumbnailGraphics);
            if (status != Ok) return 0;

            GdipSetInterpolationMode(thumbnailGraphics, InterpolationModeHighQualityBicubic);

            status = GdipDrawImageRectI(
                thumbnailGraphics,
                sourceBitmap,
                placement.X,
                placement.Y,
                placement.Width,
                placement.Height);
            if (status != Ok) return 0;

            // Overlay logo at bottom-right of the canvas, not the artwork.
            if (overlayIconPath != null)
            {
                status = GdipLoadImageFromFile(overlayIconPath, out overlayImage);
                if (status == Ok)
                {
                    int logoSize = Math.Max(canvasSize * 25 / 100, 16);
                    int margin = Math.Max(canvasSize * 3 / 100, 1);
                    int logoX = canvasSize - logoSize - margin;
                    int logoY = canvasSize - logoSize - margin;

                    GdipDrawImageRectI(thumbnailGraphics, overlayImage, logoX, logoY, logoSize, logoSize);
                }
            }

            status = GdipCreateHBITMAPFromBitmap(thumbnail, out nint bitmapHandle, 0x00000000);
            if (status != Ok) return 0;

            return bitmapHandle;
        }
        catch
        {
            return 0;
        }
        finally
        {
            if (thumbnailGraphics != 0) GdipDeleteGraphics(thumbnailGraphics);
            if (overlayImage != 0) GdipDisposeImage(overlayImage);
            if (thumbnail != 0) GdipDisposeImage(thumbnail);
            if (sourceBitmap != 0) GdipDisposeImage(sourceBitmap);
            if (pngStream != 0) Marshal.Release(pngStream);
            if (gdiplusToken != 0) GdiplusShutdown(gdiplusToken);
        }
    }

    internal static (int Width, int Height) CalculateThumbnailSize(
        uint originalWidth,
        uint originalHeight,
        uint maxThumbnailSize)
    {
        int maxSize = (int)maxThumbnailSize;

        // Match Explorer's normal PNG thumbnail behavior: shrink large images to the
        // requested size, keep small preview images at their native size, and avoid
        // adding transparent padding around the artwork.
        float scale = Math.Min((float)maxSize / originalWidth, (float)maxSize / originalHeight);
        if (scale > 1.0f) scale = 1.0f;

        int scaledWidth = Math.Max((int)Math.Round(originalWidth * scale), 1);
        int scaledHeight = Math.Max((int)Math.Round(originalHeight * scale), 1);

        return (scaledWidth, scaledHeight);
    }

    internal static (int X, int Y, int Width, int Height) CalculateCenteredImagePlacement(
        uint originalWidth,
        uint originalHeight,
        uint canvasSize)
    {
        var imageSize = CalculateThumbnailSize(originalWidth, originalHeight, canvasSize);
        int size = (int)canvasSize;

        return ((size - imageSize.Width) / 2, (size - imageSize.Height) / 2, imageSize.Width, imageSize.Height);
    }

    private const int PixelFormat32bppARGB = 0x0026200A;
    private const int InterpolationModeHighQualityBicubic = 7;

    [StructLayout(LayoutKind.Sequential)]
    private struct GdiplusStartupInput
    {
        public int GdiplusVersion;
        public nint DebugEventCallback;
        public int SuppressBackgroundThread;
        public int SuppressExternalCodecs;
    }

    // GDI+ flat API - function names are fixed exports from gdiplus.dll
    [LibraryImport("gdiplus.dll")]
    private static partial int GdiplusStartup(out nint token, ref GdiplusStartupInput input, out nint output);

    [LibraryImport("gdiplus.dll")]
    private static partial void GdiplusShutdown(nint token);

    [LibraryImport("gdiplus.dll")]
    private static partial int GdipCreateBitmapFromStream(nint stream, out nint bitmap);

    [LibraryImport("gdiplus.dll")]
    private static partial int GdipGetImageWidth(nint image, out uint width);

    [LibraryImport("gdiplus.dll")]
    private static partial int GdipGetImageHeight(nint image, out uint height);

    [LibraryImport("gdiplus.dll")]
    private static partial int GdipCreateBitmapFromScan0(
        int width, int height, int stride, int format, nint scan0, out nint bitmap);

    [LibraryImport("gdiplus.dll")]
    private static partial int GdipGetImageGraphicsContext(nint image, out nint graphics);

    [LibraryImport("gdiplus.dll")]
    private static partial int GdipSetInterpolationMode(nint graphics, int interpolationMode);

    [LibraryImport("gdiplus.dll")]
    private static partial int GdipDrawImageRectI(
        nint graphics, nint image, int x, int y, int width, int height);

    [LibraryImport("gdiplus.dll")]
    private static partial int GdipCreateHBITMAPFromBitmap(nint bitmap, out nint bitmapHandle, int background);

    [LibraryImport("gdiplus.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int GdipLoadImageFromFile(string filename, out nint image);

    [LibraryImport("gdiplus.dll")]
    private static partial int GdipDisposeImage(nint image);

    [LibraryImport("gdiplus.dll")]
    private static partial int GdipDeleteGraphics(nint graphics);

    [LibraryImport("shlwapi.dll")]
    private static partial nint SHCreateMemStream(byte[] pInit, int cbInit);
}
