// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.07

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using License;
using LicenseManager;
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
    // "$(ProjectDir)..\..\Xtensive.Licensing\Protection\Protect.bat" "$(TargetPath)" "$(ProjectDir)obj\$(ConfigurationName)\$(TargetFileName)" "true"
    private const string XtensiveLicensingManagerExe = "Xtensive.Licensing.Manager.exe";

    /// <exception cref="InvalidOperationException">Something went wrong.</exception>
    protected override void Initialize()
    {
      base.Initialize();
      var licenseInfo = LicenseValidator.GetLicense();
      RunLicensingAgent(licenseInfo);
      if (!licenseInfo.CompanyLicenseIsValid) {
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
      try {
        using (var client = new PipeClient())
          client.SendLicenseInfo(licenseInfo);
      }
      catch (TimeoutException) {}
    }

    
    // Constructors

    public PlugIn()
      : base(Priorities.User)
    {}
  }
}