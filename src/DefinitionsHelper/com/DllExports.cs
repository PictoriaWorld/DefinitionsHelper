using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace DefinitionsHelper.Com;

internal static partial class DllExports
{
    private const int S_OK = 0;
    private const int E_FAIL = unchecked((int)0x80004005);
    private const int CLASS_E_CLASSNOTAVAILABLE = unchecked((int)0x80040111);

    private const string ClsidPath = @"SOFTWARE\Classes\CLSID\{" + ComGuids.PstrThumbnailProvider + "}";
    private const string ShellexSuffix = @"\shellex\{" + ComGuids.IThumbnailProvider + "}";

    private const string PstrExtensionPath = @"SOFTWARE\Classes\.pstr";
    private const string PstrProgIdPath = @"SOFTWARE\Classes\PictoriaStructureDefinition";

    private const string PptyExtensionPath = @"SOFTWARE\Classes\.ppty";
    private const string PptyProgIdPath = @"SOFTWARE\Classes\PictoriaPropertyDefinition";

    private static readonly Guid s_providerClsid = new(ComGuids.PstrThumbnailProvider);
    private static readonly StrategyBasedComWrappers s_comWrappers = new();

    [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
    public static unsafe int DllGetClassObject(Guid* classId, Guid* interfaceId, nint* objectOut)
    {
        *objectOut = 0;

        try
        {
            if (*classId != s_providerClsid) return CLASS_E_CLASSNOTAVAILABLE;

            var factory = new PstrClassFactory();
            nint factoryComPointer = s_comWrappers.GetOrCreateComInterfaceForObject(factory, CreateComInterfaceFlags.None);

            Guid queriedInterfaceId = *interfaceId;
            int queryResult = Marshal.QueryInterface(factoryComPointer, in queriedInterfaceId, out nint queriedInterface);
            Marshal.Release(factoryComPointer);

            *objectOut = queriedInterface;
            return queryResult;
        }
        catch
        {
            return E_FAIL;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "DllCanUnloadNow")]
    public static int DllCanUnloadNow()
    {
        return S_OK;
    }

    [UnmanagedCallersOnly(EntryPoint = "DllRegisterServer")]
    public static int DllRegisterServer()
    {
        try
        {
            string dllPath = ModulePath.GetDllPath();
            string dllDirectory = dllPath.Substring(0, dllPath.LastIndexOf('\\') + 1);
            string iconPath = dllDirectory + "pictoria_structure.ico";
            string handlerClsid = "{" + ComGuids.PstrThumbnailProvider + "}";

            // COM InprocServer32 registration
            if (!RegSetString(HKEY_LOCAL_MACHINE, ClsidPath, null, "Pictoria Definition Thumbnail Provider")) return E_FAIL;
            if (!RegSetString(HKEY_LOCAL_MACHINE, ClsidPath + @"\InprocServer32", null, dllPath)) return E_FAIL;
            if (!RegSetString(HKEY_LOCAL_MACHINE, ClsidPath + @"\InprocServer32", "ThreadingModel", "Apartment")) return E_FAIL;

            // .pstr - icon + thumbnail handler
            if (!RegisterExtension(PstrExtensionPath, "PictoriaStructureDefinition",
                PstrProgIdPath, "Pictoria Structure Definition", iconPath, handlerClsid)) return E_FAIL;

            // .ppty - icon only (no thumbnail handler)
            if (!RegisterExtension(PptyExtensionPath, "PictoriaPropertyDefinition",
                PptyProgIdPath, "Pictoria Property Definition", iconPath, null)) return E_FAIL;

            SHChangeNotify(0x08000000, 0, 0, 0);

            return S_OK;
        }
        catch
        {
            return E_FAIL;
        }
    }

    [UnmanagedCallersOnly(EntryPoint = "DllUnregisterServer")]
    public static int DllUnregisterServer()
    {
        try
        {
            RegDeleteTree(HKEY_LOCAL_MACHINE, ClsidPath);

            RegDeleteTree(HKEY_LOCAL_MACHINE, PstrExtensionPath);
            RegDeleteTree(HKEY_LOCAL_MACHINE, PstrProgIdPath);

            RegDeleteTree(HKEY_LOCAL_MACHINE, PptyExtensionPath);
            RegDeleteTree(HKEY_LOCAL_MACHINE, PptyProgIdPath);

            SHChangeNotify(0x08000000, 0, 0, 0);

            return S_OK;
        }
        catch
        {
            return E_FAIL;
        }
    }

    private static bool RegisterExtension(string extensionPath, string progId,
        string progIdPath, string friendlyName, string iconPath, string? handlerClsid)
    {
        if (!RegSetString(HKEY_LOCAL_MACHINE, extensionPath, null, progId)) return false;
        if (!RegSetString(HKEY_LOCAL_MACHINE, progIdPath, null, friendlyName)) return false;
        if (!RegSetString(HKEY_LOCAL_MACHINE, progIdPath + @"\DefaultIcon", null, iconPath)) return false;

        if (handlerClsid != null)
        {
            if (!RegSetString(HKEY_LOCAL_MACHINE, extensionPath + ShellexSuffix, null, handlerClsid)) return false;
        }

        return true;
    }

    // --- Native registry helpers ---

    private static readonly nint HKEY_LOCAL_MACHINE = unchecked((nint)0x80000002);
    private const int KEY_WRITE = 0x20006;
    private const int REG_SZ = 1;

    private static bool RegSetString(nint hKeyRoot, string subKey, string? valueName, string data)
    {
        int result = RegCreateKeyExW(hKeyRoot, subKey, 0, null, 0, KEY_WRITE, 0, out nint hKey, out _);
        if (result != 0) return false;

        try
        {
            int byteCount = (data.Length + 1) * 2; // UTF-16 + null terminator
            result = RegSetValueExW(hKey, valueName, 0, REG_SZ, data, byteCount);
            return result == 0;
        }
        finally
        {
            RegCloseKey(hKey);
        }
    }

    private static void RegDeleteTree(nint hKeyRoot, string subKey)
    {
        RegDeleteTreeW(hKeyRoot, subKey);
    }

    // --- P/Invoke declarations ---

    [LibraryImport("shell32.dll")]
    private static partial void SHChangeNotify(int wEventId, uint uFlags, nint dwItem1, nint dwItem2);

    [LibraryImport("advapi32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int RegCreateKeyExW(
        nint hKey, string lpSubKey, int reserved, string? lpClass,
        int dwOptions, int samDesired, nint lpSecurityAttributes,
        out nint phkResult, out int lpdwDisposition);

    [LibraryImport("advapi32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int RegSetValueExW(
        nint hKey, string? lpValueName, int reserved, int dwType,
        string lpData, int cbData);

    [LibraryImport("advapi32.dll")]
    private static partial int RegCloseKey(nint hKey);

    [LibraryImport("advapi32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int RegDeleteTreeW(nint hKey, string lpSubKey);
}
