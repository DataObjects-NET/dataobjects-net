// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.07

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PostSharp.Extensibility;
using PostSharp.Sdk.AspectWeaver;
using Xtensive.Aspects;
using Xtensive.Core;
using Xtensive.Licensing;
using Xtensive.Licensing.Validator;

namespace Xtensive.Aspects.Weaver
{
  /// <summary>
  /// Creates the weavers defined by the 'Xtensive.Weaver' plug-in.
  /// </summary>
  public sealed class PlugIn : AspectWeaverPlugIn
  {
    private static readonly Regex dateExtractor = new Regex(@"\d{2}.\d{2}.\d{4}$", RegexOptions.Compiled);
    private static readonly byte[] tokenExpected = new byte[]{0x93, 0xa6, 0xc5,0x3d, 0x77, 0xa5, 0x29, 0x6c};
    // "$(ProjectDir)..\..\Xtensive.Licensing\Protection\Protect.bat" "$(TargetPath)" "$(ProjectDir)obj\$(ConfigurationName)\$(TargetFileName)" "true"
    private const string XtensiveLicensingManagerExe = "Xtensive.Licensing.Manager.exe";
    private static List<string> knownAssemblies = new List<string> {
      "Xtensive.Core",
      "Xtensive.Aspects",
      "Xtensive.Aspects.Weaver",
      "Xtensive.Indexing",
      "Xtensive.Sql",
      "Xtensive.Sql.Oracle",
      "Xtensive.Sql.PostgreSql",
      "Xtensive.Sql.SqlServer",
      "Xtensive.Sql.SqlServerCe",
      "Xtensive.Orm",
      "Xtensive.Orm.Indexing",
      "Xtensive.Orm.Indexing.Model",
      "Xtensive.Orm.Model",
      "Xtensive.Storage.Providers.Index",
      "Xtensive.Storage.Providers.Sql",
    };

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
          .Where(a => knownAssemblies.Contains(a.Name))
          .ToList();
        if (declarations.Count!=0)
          isXtensiveAssembly = declarations.All(a => IsPublicTokenConsistent(a.GetPublicKeyToken()));
        if (isXtensiveAssembly) {
          var assemblyVersions = declarations
            .Select(r => r.GetSystemAssembly())
            .Select(a => a.GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false).Single())
            .Cast<AssemblyInformationalVersionAttribute>()
            .Select(av => av.InformationalVersion)
            .ToList();
          var maxAssemblyDate = assemblyVersions
            .Select(s => dateExtractor.Match(s))
            .Select(m => m.Success ? m.Value : "01/01/2010")
            .Select(s => DateTime.Parse(s, DateTimeFormatInfo.InvariantInfo))
            .Max();
          if (licenseInfo.ExpireOn < maxAssemblyDate) {
            ErrorLog.Write(SeverityType.Fatal, "Your subscription expired {0} and is not valid for {1}.", licenseInfo.ExpireOn.ToShortDateString(), assemblyVersions.First());
            return;
          }
        }
      }
      if (!isXtensiveAssembly) {
        ErrorLog.Write(SeverityType.Fatal, "DataObjects.Net license validation failed.");
        return;
      }

      if (licenseInfo!=null)
        try {
          if (licenseInfo.IsValid && LicenseValidator.WeaverLicenseCheckIsRequired()) {
            var companyLicenseData = licenseInfo.EvaluationMode
              ? null
              : LicenseValidator.GetCompanyLicenseData(LicenseValidator.GetLicensesPath());
            var hardwareId = LicenseValidator.TrialLicense.HardwareId;
            var request = new InternetCheckRequest(
              companyLicenseData, licenseInfo.ExpireOn,
              LicenseValidator.GetProductVersion(), hardwareId);
            var result = InternetActivator.Check(request);
            if (result.IsValid==false) {
              LicenseValidator.InvalidateLicense(licenseInfo.HardwareId);
              licenseInfo.HardwareKeyIsValid = false;
            }
          }
        }
        catch { }

      RunLicensingAgent(licenseInfo);
      if (licenseInfo == null || !licenseInfo.IsValid) {
        ErrorLog.Write(SeverityType.Fatal, "DataObjects.Net license is invalid.");
      }
      else {
        BindAspectWeaver<ReplaceAutoProperty, ReplaceAutoPropertyWeaver>();
        BindAspectWeaver<ImplementConstructorEpilogue, ConstructorEpilogueWeaver>();
        BindAspectWeaver<NotSupportedAttribute, NotSupportedWeaver>();
        BindAspectWeaver<ImplementConstructor, ImplementConstructorWeaver>();
        BindAspectWeaver<ImplementFactoryMethod, ImplementFactoryMethodWeaver>();
      }
    }

    private static void RunLicensingAgent(LicenseInfo licenseInfo)
    {
      var directory = Path.GetDirectoryName(PostSharp.Hosting.Platform.Current.GetAssemblyLocation(typeof(PlugIn).Assembly));
      var path = Path.Combine(directory, XtensiveLicensingManagerExe);
      if (!File.Exists(path))
        throw new FileNotFoundException(path);
      if (!Environment.UserInteractive || Environment.OSVersion.Platform!=PlatformID.Win32NT || string.IsNullOrEmpty(path))
        return;
      var startInfo = new ProcessStartInfo(path, "wait") {
        UseShellExecute = false
      };
      Process.Start(startInfo);
    }

    
    // Constructors

    public PlugIn()
      : base(StandardPriorities.User)
    {
    }
  }
}