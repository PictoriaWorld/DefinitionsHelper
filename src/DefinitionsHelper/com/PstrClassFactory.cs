using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace DefinitionsHelper.Com;

[GeneratedComClass]
internal sealed partial class PstrClassFactory : IClassFactory
{
    private const int S_OK = 0;
    private const int CLASS_E_NOAGGREGATION = unchecked((int)0x80040110);

    private static readonly StrategyBasedComWrappers s_comWrappers = new();

    public int CreateInstance(nint outerUnknown, in Guid interfaceId, out nint objectOut)
    {
        objectOut = 0;

        if (outerUnknown != 0) return CLASS_E_NOAGGREGATION;

        var provider = new PstrThumbnailProvider();
        nint providerComPointer = s_comWrappers.GetOrCreateComInterfaceForObject(provider, CreateComInterfaceFlags.None);

        Guid queriedInterfaceId = interfaceId;
        int queryResult = Marshal.QueryInterface(providerComPointer, in queriedInterfaceId, out objectOut);
        Marshal.Release(providerComPointer);
        return queryResult;
    }

    public int LockServer(bool lockServer)
    {
        return S_OK;
    }
}
