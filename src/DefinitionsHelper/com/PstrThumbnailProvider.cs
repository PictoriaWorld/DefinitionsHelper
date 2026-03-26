using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using DefinitionsHelper.Extraction;
using DefinitionsHelper.Imaging;

namespace DefinitionsHelper.Com;

[GeneratedComClass]
[Guid(ComGuids.PstrThumbnailProvider)]
internal sealed partial class PstrThumbnailProvider : IThumbnailProvider, IInitializeWithStream
{
    private const int S_OK = 0;
    private const int E_FAIL = unchecked((int)0x80004005);
    private const uint WTSAT_ARGB = 2;

    private IStream? _stream;

    public int Initialize(IStream stream, uint mode)
    {
        _stream = stream;
        return S_OK;
    }

    public int GetThumbnail(uint thumbnailSize, out nint bitmapHandle, out uint alphaType)
    {
        bitmapHandle = 0;
        alphaType = 0;

        if (_stream == null) return E_FAIL;

        try
        {
            byte[] fileBytes = ReadAllBytesFromStream(_stream); // This is a different kind of stream than System.IO.Stream, so we need to read it manually.
            if (fileBytes.Length == 0) return E_FAIL;

            byte[]? pngBytes = PstrImageExtractor.Extract(fileBytes);
            if (pngBytes == null) return E_FAIL;

            nint bitmap = BitmapService.CreateBitmapFromPng(pngBytes, thumbnailSize, ModulePath.GetIconPath());
            if (bitmap == 0) return E_FAIL;

            bitmapHandle = bitmap;
            alphaType = WTSAT_ARGB;
            return S_OK;
        }
        catch
        {
            return E_FAIL;
        }
    }

    private static unsafe byte[] ReadAllBytesFromStream(IStream stream)
    {
        stream.Seek(0, 0 /* STREAM_SEEK_SET */, out _);

        using var memoryStream = new MemoryStream();
        byte[] buffer = new byte[8192];

        while (true)
        {
            uint bytesRead = 0;
            fixed (byte* bufferPointer = buffer)
            {
                int readResult = stream.Read(bufferPointer, (uint)buffer.Length, &bytesRead);
                if (readResult != S_OK && readResult != 1 /* S_FALSE */) break;
            }

            if (bytesRead == 0) break;

            memoryStream.Write(buffer, 0, (int)bytesRead);
        }

        return memoryStream.ToArray();
    }

}
