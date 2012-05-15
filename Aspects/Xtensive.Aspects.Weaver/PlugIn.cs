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
using PostSharp;
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
    private const string XtensiveLicensingManagerExe = "Xtensive.Licensing.Manager.exe";

    private static readonly byte[] TokenExpected = new byte[] {0x93, 0xa6, 0xc5, 0x3d, 0x77, 0xa5, 0x29, 0x6c};
    private static readonly List<string> KnownAssemblies = new List<string> {
      "Xtensive.Core",
      "Xtensive.Aspects",
      "Xtensive.Aspects.Weaver",
      "Xtensive.Orm",
    };

    public static readonly HashSet<string> ErrorMessages = new HashSet<string>();
    public static LicenseInfo CurrentLicense;

    private LicenseValidator licenseValidator;

    protected override void Initialize()
    {
      base.Initialize();

      licenseValidator = new LicenseValidator(GetWeaverAssemblyLocation());

      var licenseInfo = licenseValidator.ReloadLicense();
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
          .Where(a => KnownAssemblies.Contains(a.Name))
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
      if (assemblyToken!=null && TokenExpected.Length==assemblyToken.Length) {
        // Check that token contents match
        return !assemblyToken.Where((t, i) => TokenExpected[i]!=t).Any();
      }
      return false;
    }

    private void TryCheckLicense(LicenseInfo licenseInfo)
    {
      var checkRequired = licenseInfo.IsValid && licenseValidator.WeaverLicenseCheckIsRequired();
      if (!checkRequired)
        return;

      var companyLicenseData = licenseInfo.EvaluationMode ? null : licenseValidator.GetCompanyLicenseData();
      var request = new InternetCheckRequest(
        companyLicenseData, licenseInfo.ExpireOn, licenseValidator.ProductVersion, licenseValidator.HardwareId);
      var result = InternetActivator.Check(request);
      if (result.IsValid==false && licenseInfo.RequiresHardwareLicense) {
        licenseValidator.InvalidateHardwareLicense();
        licenseInfo.HardwareKeyIsValid = false;
      }
    }

    private static void FatalLicenseError(string format, params object[] args)
    {
      RunLicenseManager();
      ErrorLog.Write(MessageLocation.Unknown, SeverityType.Fatal, format, args);
    }

    private static void RunLicenseManager()
    {
      var managerDir = Path.GetDirectoryName(GetWeaverAssemblyLocation());
      var managerExe = Path.Combine(managerDir, XtensiveLicensingManagerExe);
      var canRunManager =
        Environment.UserInteractive
        && Environment.OSVersion.Platform==PlatformID.Win32NT
        && File.Exists(managerExe);
      if (canRunManager)
        Process.Start(new ProcessStartInfo(managerExe) {UseShellExecute = false});
    }

    private static string GetWeaverAssemblyLocation()
    {
      return Platform.Current.GetAssemblyLocation(typeof (PlugIn).Assembly);
    }


    // Constructors

    public PlugIn()
      : base(StandardPriorities.User)
    {
    }
  }
}