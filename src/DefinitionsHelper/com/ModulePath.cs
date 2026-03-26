using System.Runtime.InteropServices;

namespace DefinitionsHelper.Com;

/// <summary>
/// Resolves paths relative to the loaded DLL module.
/// </summary>
internal static partial class ModulePath
{
    private static string? s_dllPath;
    private static string? s_iconPath;
    private static bool s_iconPathResolved;

    public static string GetDllPath()
    {
        return s_dllPath ??= ResolveDllPath();
    }

    /// <summary>
    /// Returns the path to pictoria_structure.ico next to the DLL, or null if not found.
    /// Cached after first resolution.
    /// </summary>
    public static string? GetIconPath()
    {
        if (!s_iconPathResolved)
        {
            string dllPath = GetDllPath();
            int lastSlash = dllPath.LastIndexOf('\\');
            if (lastSlash >= 0)
            {
                string candidate = dllPath.Substring(0, lastSlash + 1) + "pictoria_structure.ico";
                if (File.Exists(candidate)) s_iconPath = candidate;
            }
            s_iconPathResolved = true;
        }
        return s_iconPath;
    }

    private static unsafe string ResolveDllPath()
    {
        nint hModule = GetModuleHandleW("DefinitionsHelper.dll");

        char* buffer = stackalloc char[260];
        uint length = GetModuleFileNameW(hModule, buffer, 260);
        return new string(buffer, 0, (int)length);
    }

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint GetModuleHandleW(string lpModuleName);

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static unsafe partial uint GetModuleFileNameW(nint hModule, char* lpFilename, uint nSize);
}
