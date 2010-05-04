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
using Xtensive.Core.Aspects;
using Xtensive.Licensing;

namespace Xtensive.Core.Weaver
{
  /// <summary>
  /// Creates the weavers defined by the 'Xtensive.Core.Weaver' plug-in.
  /// </summary>
  public sealed class PlugIn : PostSharp.AspectWeaver.PlugIn
  {
    // $(ProjectDir)..\..\Xtensive.Licensing\Protect.bat "$(TargetPath)" "$(ProjectDir)obj\$(ConfigurationName)\$(TargetFileName)"

    #region Non-public methods

    protected override void Initialize()
    {
      base.Initialize();
      // TODO: Check license
      var properties = new Dictionary<string, string>();
      for (int i = 0; i < Status.KeyValueList.Count; i++) {
        string key = Status.KeyValueList.GetKey(i).ToString();
        string value = Status.KeyValueList.GetByIndex(i).ToString();
        properties.Add(key, value);
      }
      string licensee;
      string licenseTypeString;
      string numberOfDevelopersString;
      properties.TryGetValue(LicenseInfo.LicenseeKey, out licensee);
      properties.TryGetValue(LicenseInfo.LicenseTypeKey, out licenseTypeString);
      properties.TryGetValue(LicenseInfo.NumberOfHWLicensesKey, out numberOfDevelopersString);
      LicenseType licenseType = licenseTypeString.IsNullOrEmpty()
        ? LicenseType.Trial
        : (LicenseType) Enum.Parse(typeof (LicenseType), licenseTypeString);
      int numberOfDevelopers = numberOfDevelopersString.IsNullOrEmpty()
        ? -1
        : int.Parse(numberOfDevelopersString);
      var licenseInfo = new LicenseInfo {
        LicenseType = licenseType,
        ExpireOn = Status.Expiration_Date,
        TrialDays = Status.Evaluation_Time,
        TrialDaysCurrent = Status.Evaluation_Time_Current,
        Licensee = licensee,
        NumberOfHWLicenses = numberOfDevelopers
      };
      RunLicensingAgent(licenseInfo);
      AddAspectWeaverFactory<ReplaceAutoProperty, ReplaceAutoPropertyWeaver>();
      AddAspectWeaverFactory<ImplementConstructorEpilogue, ConstructorEpilogueWeaver>();
      AddAspectWeaverFactory<NotSupportedAttribute, NotSupportedWeaver>();
      AddAspectWeaverFactory<ImplementConstructor, ImplementConstructorWeaver>();
      AddAspectWeaverFactory<ImplementFactoryMethod, ImplementFactoryMethodWeaver>();
    }

    private static void RunLicensingAgent(LicenseInfo licenseInfo)
    {
      var dataObjectsPath = Environment.GetEnvironmentVariable("DataObjectsDotNetPath");
      if (dataObjectsPath.IsNullOrEmpty() || !Directory.Exists(dataObjectsPath))
        throw new InvalidOperationException("DataObjects.Net is not installed. Please install.");
      var path = Path.Combine(dataObjectsPath, "Bin", "Latest", "Xtensive.Licensing.Manager.exe");
      if (!File.Exists(path))
        throw new FileNotFoundException("Xtensive.Licensing.Manager.exe");
      if (!Environment.UserInteractive || Environment.OSVersion.Platform!=PlatformID.Win32NT || path.IsNullOrEmpty())
        return;
      var startInfo = new ProcessStartInfo(path) {
        UseShellExecute = false
      };
      Process.Start(startInfo);
      using (var client = new PipeClient())
        client.SendLicenseInfo(licenseInfo);
    }

    #endregion

    #region Constructors

    public PlugIn()
      : base(Priorities.User)
    {}

    #endregion
  }
}