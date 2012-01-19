// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.07

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PostSharp.Extensibility;
using PostSharp.Hosting;
using PostSharp.Sdk.AspectWeaver;
using Xtensive.Licensing;
using Xtensive.Licensing.Validator;

namespace Xtensive.Aspects.Weaver
{
  /// <summary>
  /// Creates the weavers defined by the 'Xtensive.Weaver' plug-in.
  /// </summary>
  public sealed class PlugIn : AspectWeaverPlugIn
  {
    private static readonly byte[] tokenExpected = new byte[] {0x93, 0xa6, 0xc5, 0x3d, 0x77, 0xa5, 0x29, 0x6c};

    private const string XtensiveLicensingManagerExe = "Xtensive.Licensing.Manager.exe";
    private static readonly List<string> knownAssemblies = new List<string> {
      "Xtensive.Core",
      "Xtensive.Aspects",
      "Xtensive.Aspects.Weaver",
      "Xtensive.Orm",
    };

    public static LicenseInfo CurrentLicense;
    public static readonly HashSet<string> ErrorMessages = new HashSet<string>();

    /// <exception cref="InvalidOperationException">Something went wrong.</exception>
    protected override void Initialize()
    {
      base.Initialize();

      var licenseInfo = LicenseValidator.GetLicense();
      if (!ValidateTargetAssembly(licenseInfo))
        return;
      TryCheckLicense(licenseInfo);

      if (licenseInfo==null || !licenseInfo.IsValid) {
        FatalLicenseError("DataObjects.Net license is invalid.");
        return;
      }

      BindAspectWeaver<ReplaceAutoProperty, ReplaceAutoPropertyWeaver>();
      BindAspectWeaver<ImplementConstructorEpilogue, ConstructorEpilogueWeaver>();
      BindAspectWeaver<NotSupportedAttribute, NotSupportedWeaver>();
      BindAspectWeaver<ImplementConstructor, ImplementConstructorWeaver>();
      BindAspectWeaver<ImplementFactoryMethod, ImplementFactoryMethodWeaver>();
    }

    private bool ValidateTargetAssembly(LicenseInfo licenseInfo)
    {
      var isXtensiveAssembly = Project.Module.Assembly.IsStronglyNamed
        && IsPublicTokenConsistent(Project.Module.Assembly.GetPublicKeyToken());

      if (!isXtensiveAssembly) {
        CurrentLicense = licenseInfo;
        var declarations = Project.Module.AssemblyRefs
          .Where(a => knownAssemblies.Contains(a.Name))
          .ToList();
        if (declarations.Count!=0)
          isXtensiveAssembly = declarations.All(a => IsPublicTokenConsistent(a.GetPublicKeyToken()));
        if (isXtensiveAssembly) {
          var assemblyVersions = declarations
            .Select(r => GetAssemblyBuildDate(r.GetSystemAssembly()))
            .ToList();
          var maxAssemblyDate = assemblyVersions.Max();
          if (licenseInfo.ExpireOn < maxAssemblyDate) {
            FatalLicenseError(
              "Your subscription expired {0} and is not valid for this version of {1}.",
              licenseInfo.ExpireOn.ToShortDateString(), ThisAssembly.ProductName);
            return false;
          }
        }
      }

      if (!isXtensiveAssembly) {
        FatalLicenseError("DataObjects.Net license validation failed.");
        return false;
      }

      return true;
    }

    private static DateTime GetAssemblyBuildDate(Assembly assembly)
    {
      const string format = "yyyy-MM-dd";
      var fallback =new DateTime(2010, 01, 01);
      var attribute = assembly
        .GetCustomAttributes(typeof (AssemblyInformationalVersionAttribute), false)
        .Cast<AssemblyInformationalVersionAttribute>()
        .SingleOrDefault();
      if (attribute==null)
        return fallback;
      var versionString = attribute.InformationalVersion;
      if (versionString.Length < format.Length)
        return fallback;
      var dateString = versionString.Substring(versionString.Length - format.Length);
      DateTime result;
      var parsed = DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result);
      return parsed ? result : fallback;
    }

    // Check that public key token matches what's expected.
    private static bool IsPublicTokenConsistent(byte[] assemblyToken)
    {
      // Check that lengths match
      if (assemblyToken!=null && tokenExpected.Length==assemblyToken.Length) {
        // Check that token contents match
        return !assemblyToken.Where((t, i) => tokenExpected[i]!=t).Any();
      }
      return false;
    }

    private static void TryCheckLicense(LicenseInfo licenseInfo)
    {
      if (licenseInfo==null)
        return;
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
      catch {
      }
    }

    private static void FatalLicenseError(string format, params object[] args)
    {
      RunLicensingAgent();
      ErrorLog.Write(SeverityType.Fatal, format, args);
    }

    private static void RunLicensingAgent()
    {
      var directory = Path.GetDirectoryName(Platform.Current.GetAssemblyLocation(typeof (PlugIn).Assembly));
      var path = Path.Combine(directory, XtensiveLicensingManagerExe);
      if (!File.Exists(path))
        throw new FileNotFoundException(path);
      if (!Environment.UserInteractive || Environment.OSVersion.Platform!=PlatformID.Win32NT || string.IsNullOrEmpty(path))
        return;
      var startInfo = new ProcessStartInfo(path) {UseShellExecute = false};
      Process.Start(startInfo);
    }

    
    // Constructors

    public PlugIn()
      : base(StandardPriorities.User)
    {
    }
  }
}