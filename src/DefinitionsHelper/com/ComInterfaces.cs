using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace DefinitionsHelper.Com;

[GeneratedComInterface]
[Guid(ComGuids.IThumbnailProvider)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IThumbnailProvider
{
    [PreserveSig]
    int GetThumbnail(uint thumbnailSize, out nint bitmapHandle, out uint alphaType);
}

[GeneratedComInterface]
[Guid(ComGuids.IInitializeWithStream)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IInitializeWithStream
{
    [PreserveSig]
    int Initialize(IStream stream, uint mode);
}

[GeneratedComInterface]
[Guid("0000000c-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IStream
{
    [PreserveSig]
    unsafe int Read(byte* pv, uint cb, uint* pcbRead);

    [PreserveSig]
    unsafe int Write(byte* pv, uint cb, uint* pcbWritten);

    [PreserveSig]
    int Seek(long dlibMove, uint dwOrigin, out ulong plibNewPosition);

    [PreserveSig]
    int SetSize(ulong libNewSize);

    [PreserveSig]
    int CopyTo(IStream pstm, ulong cb, out ulong pcbRead, out ulong pcbWritten);

    [PreserveSig]
    int Commit(uint grfCommitFlags);

    [PreserveSig]
    int Revert();

    [PreserveSig]
    int LockRegion(ulong libOffset, ulong cb, uint dwLockType);

    [PreserveSig]
    int UnlockRegion(ulong libOffset, ulong cb, uint dwLockType);

    [PreserveSig]
    unsafe int Stat(void* pstatstg, uint grfStatFlag);

    [PreserveSig]
    int Clone(out IStream ppstm);
}

[GeneratedComInterface]
[Guid(ComGuids.IClassFactory)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal partial interface IClassFactory
{
    [PreserveSig]
    int CreateInstance(nint outerUnknown, in Guid interfaceId, out nint objectOut);

    [PreserveSig]
    int LockServer([MarshalAs(UnmanagedType.Bool)] bool lockServer);
}
