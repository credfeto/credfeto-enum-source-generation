using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Credfeto.Enumeration.Source.Generation.Helpers;

public static class VersionInformation
{
    public static string Version()
    {
        return CommonVersion(typeof(VersionInformation));
    }

    private static string CommonVersion(Type type)
    {
        string filename = GetFileName(type.Assembly);

        if (string.IsNullOrWhiteSpace(filename))
        {
            return AssemblyVersion();
        }

        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filename);

        return fileVersionInfo.ProductVersion ?? AssemblyVersion();
    }

    private static string AssemblyVersion()
    {
        return Assembly.GetExecutingAssembly()
                       .GetName()
                       .Version?.ToString() ?? "0.0.0.1";
    }

    [SuppressMessage(category: "FunFair.CodeAnalysis", checkId: "FFS0008:Don't disable warnings with #pragma", Justification = "Needed in this case")]
    private static string GetFileName(Assembly assembly)
    {
#pragma warning disable IL3000
        return assembly.Location;
#pragma warning restore IL3000
    }
}