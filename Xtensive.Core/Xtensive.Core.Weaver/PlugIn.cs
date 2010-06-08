// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.07

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using License;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core;
using Xtensive.Licensing;
using Xtensive.Licensing.Validator;

namespace Xtensive.Core.Weaver
{
  /// <summary>
  /// Creates the weavers defined by the 'Xtensive.Core.Weaver' plug-in.
  /// </summary>
  public sealed class PlugIn : PostSharp.AspectWeaver.PlugIn
  {
    private static readonly byte[] tokenExpected = new byte[]{0x93, 0xa6, 0xc5,0x3d, 0x77, 0xa5, 0x29, 0x6c};
    // "$(ProjectDir)..\..\Xtensive.Licensing\Protection\Protect.bat" "$(TargetPath)" "$(ProjectDir)obj\$(ConfigurationName)\$(TargetFileName)" "true"
    private const string XtensiveLicensingManagerExe = "Xtensive.Licensing.Manager.exe";

    public static LicenseInfo CurrentLicense;
    public static HashSet<string> ErrorMessages = new HashSet<string>();

    // Check that public key token matches what's expected.
    private static bool IsPublicTokenConsistent(byte[] assemblyToken)
    {
      // Check that lengths match
      if (assemblyToken!= null && tokenExpected.Length == assemblyToken.Length) {
        // Check that token contents match
        return !assemblyToken.Where((t, i) => tokenExpected[i]!=t).Any();
      }
      return false;
    }

    /// <exception cref="InvalidOperationException">Something went wrong.</exception>
    protected override void Initialize()
    {
      base.Initialize();
      var licenseInfo = LicenseValidator.GetLicense();
      var isXtensiveAssembly = base.Project.Module.Assembly.IsStronglyNamed 
        && IsPublicTokenConsistent(base.Project.Module.Assembly.GetPublicKeyToken());
      if (!isXtensiveAssembly) {
        CurrentLicense = licenseInfo;
        var declarations = base.Project.Module.AssemblyRefs
          .Where(a => a.Name.StartsWith("Xtensive") && !a.Name.EndsWith("Tests") && !a.Name.Contains("Sample"))
          .ToList();
        if (declarations.Count!=0)
          isXtensiveAssembly = declarations.All(a => IsPublicTokenConsistent(a.GetPublicKeyToken()));
      }
      if (!isXtensiveAssembly) {
        ErrorLog.Write(SeverityType.Fatal, "DataObjects.Net license validation failed.");
        return;
      }
      RunLicensingAgent(licenseInfo);
      if (!licenseInfo.IsValid) {
        ErrorLog.Write(SeverityType.Fatal, "DataObjects.Net license is invalid.");
      }
      else
      {
        AddAspectWeaverFactory<ReplaceAutoProperty, ReplaceAutoPropertyWeaver>();
        AddAspectWeaverFactory<ImplementConstructorEpilogue, ConstructorEpilogueWeaver>();
        AddAspectWeaverFactory<NotSupportedAttribute, NotSupportedWeaver>();
        AddAspectWeaverFactory<ImplementConstructor, ImplementConstructorWeaver>();
        AddAspectWeaverFactory<ImplementFactoryMethod, ImplementFactoryMethodWeaver>();
      }
    }

    private static void RunLicensingAgent(LicenseInfo licenseInfo)
    {
      var directory = Path.GetDirectoryName(PostSharp.Hosting.Platform.Current.GetAssemblyLocation(typeof(PlugIn).Assembly));
      var path = Path.Combine(directory, XtensiveLicensingManagerExe);
      if (!File.Exists(path))
        throw new FileNotFoundException(path);
      if (!Environment.UserInteractive || Environment.OSVersion.Platform!=PlatformID.Win32NT || path.IsNullOrEmpty())
        return;
      var startInfo = new ProcessStartInfo(path, "wait") {
        UseShellExecute = false
      };
      Process.Start(startInfo);
    }

    
    // Constructors

    public PlugIn()
      : base(Priorities.User)
    {}
  }
}