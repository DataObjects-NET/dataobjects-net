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

namespace Xtensive.Core.Weaver
{
  /// <summary>
  /// Creates the weavers defined by the 'Xtensive.Core.Weaver' plug-in.
  /// </summary>
  public sealed class PlugIn : PostSharp.AspectWeaver.PlugIn
  {
    private const string DataObjectsDotNetPath = "DataObjectsDotNetPath";
    private const string BinLatest = @"Bin\Latest";
    private const string XtensiveLicensingManagerExe = "Xtensive.Licensing.Manager.exe";

    /// <exception cref="InvalidOperationException">Something went wrong.</exception>
    protected override void Initialize()
    {
      base.Initialize();
      var installationPath = Environment.GetEnvironmentVariable(DataObjectsDotNetPath);
      if (installationPath.IsNullOrEmpty() || !Directory.Exists(installationPath))
        throw new InvalidOperationException(
          @"DataObjects.Net is not installed. Please install it or run Install\Install.bat.");
      var licensePath = Path.Combine(installationPath, BinLatest, LicenseInfo.LicenseName);
      if (File.Exists(licensePath))
        Status.LoadLicense(licensePath);
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
      var licenseType = licenseTypeString.IsNullOrEmpty()
        ? LicenseType.Trial
        : (LicenseType) Enum.Parse(typeof (LicenseType), licenseTypeString);
      int numberOfDevelopers = numberOfDevelopersString.IsNullOrEmpty()
        ? -1
        : int.Parse(numberOfDevelopersString);
      var valid = Status.Licensed;
      if (!valid) {
        valid = Status.Evaluation_Lock_Enabled && Status.Evaluation_Time >= Status.Evaluation_Time_Current;
        if (Status.Expiration_Date_Lock_Enable)
          valid &= Status.Expiration_Date.ToUniversalTime().Date >= DateTime.UtcNow.Date;
      }
      var licenseInfo = new LicenseInfo {
        IsValid =  valid,
        LicenseType = licenseType,
        ExpireOn = Status.Expiration_Date,
        TrialDays = Status.Evaluation_Time,
        TrialDaysCurrent = Status.Evaluation_Time_Current,
        Licensee = licensee,
        NumberOfHWLicenses = numberOfDevelopers
      };
      
      RunLicensingAgent(licenseInfo, installationPath);
      if (!licenseInfo.IsValid)
        ErrorLog.Write(SeverityType.Fatal, "DataObjects.Net license is invalid.");
      else {
        AddAspectWeaverFactory<ReplaceAutoProperty, ReplaceAutoPropertyWeaver>();
        AddAspectWeaverFactory<ImplementConstructorEpilogue, ConstructorEpilogueWeaver>();
        AddAspectWeaverFactory<NotSupportedAttribute, NotSupportedWeaver>();
        AddAspectWeaverFactory<ImplementConstructor, ImplementConstructorWeaver>();
        AddAspectWeaverFactory<ImplementFactoryMethod, ImplementFactoryMethodWeaver>();
      }
    }

    private static void RunLicensingAgent(LicenseInfo licenseInfo, string dataObjectsPath)
    {
      var path = Path.Combine(dataObjectsPath, BinLatest, XtensiveLicensingManagerExe);
      if (!File.Exists(path))
        throw new FileNotFoundException(XtensiveLicensingManagerExe);
      if (!Environment.UserInteractive || Environment.OSVersion.Platform!=PlatformID.Win32NT || path.IsNullOrEmpty())
        return;
      var startInfo = new ProcessStartInfo(path) {
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