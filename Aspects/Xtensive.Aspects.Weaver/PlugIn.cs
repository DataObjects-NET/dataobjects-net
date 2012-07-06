// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.07

using System;
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

// ReSharper disable UnusedMember.Global

namespace Xtensive.Aspects.Weaver
{
  public sealed class PlugIn : AspectWeaverPlugIn
  {
    private static readonly string[] XtensiveAssemblies = new [] {"Xtensive.Core", "Xtensive.Aspects", "Xtensive.Orm"};
    private static readonly byte[] XtensivePublicKeyToken = new byte[] {0x93, 0xa6, 0xc5, 0x3d, 0x77, 0xa5, 0x29, 0x6c};

    private LicenseValidator licenseValidator;

    protected override void Initialize()
    {
      base.Initialize();

      licenseValidator = new LicenseValidator(GetWeaverAssemblyLocation());

      if (!IsBootstrapMode() && !IsPartnershipMode()) {
        var licenseInfo = licenseValidator.ReloadLicense();
        if (!ValidateLicense(licenseInfo))
          return;
        if (!ValidateExpiration(licenseInfo))
          return;
      }

      RegisterWeavers();
    }

    private void RegisterWeavers()
    {
      BindAspectWeaver<ReplaceAutoProperty, ReplaceAutoPropertyWeaver>();
      BindAspectWeaver<ImplementConstructorEpilogue, ConstructorEpilogueWeaver>();
      BindAspectWeaver<NotSupportedAttribute, NotSupportedWeaver>();
      BindAspectWeaver<ImplementConstructor, ImplementConstructorWeaver>();
      BindAspectWeaver<ImplementFactoryMethod, ImplementFactoryMethodWeaver>();
    }

    private bool ValidateExpiration(LicenseInfo licenseInfo)
    {
      var referencesToXtensiveAssemblies = Project.Module.AssemblyRefs
        .Where(a => XtensiveAssemblies.Contains(a.Name))
        .ToList();

      var xtensiveAssembliesValid = referencesToXtensiveAssemblies.Count > 0
        && referencesToXtensiveAssemblies.All(a => CheckPublicKeyToken(a.GetPublicKeyToken(), XtensivePublicKeyToken));

      if (!xtensiveAssembliesValid) {
        FatalLicenseError("{0} installation is invalid", ThisAssembly.ProductName);
        return false;
      }

      var maxAssemblyDate = referencesToXtensiveAssemblies.Select(r => GetAssemblyBuildDate(r.GetSystemAssembly())).Max();
      if (licenseInfo.ExpireOn < maxAssemblyDate) {
        FatalLicenseError("Your subscription expired {0} and is not valid for this release of {1}.",
          licenseInfo.ExpireOn.ToShortDateString(), ThisAssembly.ProductName);
        return false;
      }

      return true;
    }

    private bool IsBootstrapMode()
    {
      return Project.Module.Assembly.IsStronglyNamed
        && CheckPublicKeyToken(Project.Module.Assembly.GetPublicKeyToken(), XtensivePublicKeyToken);
    }

    private bool IsPartnershipMode()
    {
      return IsPartnerAssemblyReferenced("Werp.Models", new byte[] {0x89, 0xa4, 0x89, 0x72, 0xaa, 0xfe, 0x8a, 0xfe});
    }

    private bool IsPartnerAssemblyReferenced(string partnerAssembly, byte[] partnerToken)
    {
      return Project.Module.AssemblyRefs
        .Any(r => r.Name==partnerAssembly && CheckPublicKeyToken(r.GetPublicKeyToken(), partnerToken));
    }

    private static DateTime GetAssemblyBuildDate(Assembly assembly)
    {
      const string format = "yyyy-MM-dd";
      var fallback = new DateTime(2010, 01, 01, 0, 0, 0, DateTimeKind.Utc);
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

    private static bool CheckPublicKeyToken(byte[] tokenToCheck, byte[] expectedToken)
    {
      return tokenToCheck!=null
        && tokenToCheck.Length==expectedToken.Length
        && tokenToCheck.SequenceEqual(expectedToken);
    }

    private bool ValidateLicense(LicenseInfo licenseInfo)
    {
      if (licenseInfo.IsValid && licenseValidator.WeaverLicenseCheckIsRequired())
        OnlineCheck(licenseInfo);

      if (!licenseInfo.IsValid) {
        FatalLicenseError("{0} license is not valid.", ThisAssembly.ProductName);
        return false;
      }

      return true;
    }

    private void OnlineCheck(LicenseInfo licenseInfo)
    {
      var companyLicenseData = licenseInfo.LicenseType==LicenseType.Trial
        ? null
        : licenseValidator.GetCompanyLicenseData();
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
      var managerDirectory = Path.GetDirectoryName(GetWeaverAssemblyLocation());
      var managerExecutable = Path.Combine(managerDirectory, "LicenseManager.exe");
      var canRunManager =
        Environment.UserInteractive
        && Environment.OSVersion.Platform==PlatformID.Win32NT
        && File.Exists(managerExecutable);
      if (canRunManager)
        Process.Start(new ProcessStartInfo(managerExecutable) {UseShellExecute = false});
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