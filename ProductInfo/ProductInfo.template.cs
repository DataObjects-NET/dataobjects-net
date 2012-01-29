using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable 436

[assembly: AssemblyProduct(ThisAssembly.ProductName)]
[assembly: AssemblyCompany(ThisAssembly.ProductCompany)]
[assembly: AssemblyCopyright(ThisAssembly.ProductCopyright)]

[assembly: AssemblyVersion(ThisAssembly.Version)]
[assembly: AssemblyFileVersion(ThisAssembly.FileVersion)]
[assembly: AssemblyInformationalVersion(ThisAssembly.ProductVersion)]
[assembly: AssemblyConfiguration(ThisAssembly.Configuration)]

[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]

internal static class ThisAssembly
{
  public const string ProductName = "${ProductName}";
  public const string ProductCompany = "${ProductCompany}";
  public const string ProductCopyright = "© ${ProductCopyright}";
  public const string ProductVersion = "${VersionName} " + Configuration + " ${BuildStamp}";
  public const string Version = "${Version2}.0.0";
  public const string FileVersion = "${Version4}";

  #if NET40
    #if DEBUG
      public const string Configuration = "Net40-Debug";
    #else
      public const string Configuration = "Net40-Release";
    #endif
  #else
    #if DEBUG
      public const string Configuration = "Net35-Debug";
    #else
      public const string Configuration = "Net35-Release";
    #endif
  #endif
}
