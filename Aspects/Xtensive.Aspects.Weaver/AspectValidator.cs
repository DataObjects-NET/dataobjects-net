// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.07.06

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using PostSharp;
using PostSharp.Extensibility;
using PostSharp.Hosting;
using PostSharp.Sdk.Extensibility;
using Xtensive.Licensing;
using Xtensive.Licensing.Validator;

namespace Xtensive.Aspects.Weaver
{
  internal sealed class AspectValidator
  {
    private static readonly string[] XtensiveAssemblies = new [] {"Xtensive.Core", "Xtensive.Aspects", "Xtensive.Orm"};
    private static readonly byte[] XtensivePublicKeyToken = new byte[] {0x93, 0xa6, 0xc5, 0x3d, 0x77, 0xa5, 0x29, 0x6c};

    public static AspectValidator Current { get; private set; }

    private int numberOfAspects;

    private readonly string selfLocation;
    private readonly Project project;
    private bool enableAspectLimit;

    public bool IsValid { get; private set; }

    public void EnforceAspectLimit()
    {
      if (!enableAspectLimit)
        return;
      numberOfAspects++;
      if (numberOfAspects <= 90)
        return;
      const string errorMessage =
        "Number of persistent types in assembly exceeds the maximal available types per assembly for Community Edition. " +
        "Consider upgrading to any commercial edition of DataObjects.Net.";
      ErrorLog.Write(MessageLocation.Unknown, SeverityType.Fatal, errorMessage);
    }

    private bool Initialize()
    {
      if (IsBootstrapMode() || IsPartnershipMode())
        return true;

      var validator = new LicenseValidator(selfLocation);
      var licenseInfo = validator.ReloadLicense();

      if (!ValidateLicense(validator, licenseInfo))
        return false;
      if (!ValidateExpiration(licenseInfo))
        return false;

      enableAspectLimit = licenseInfo.LicenseType==LicenseType.Community;

      return true;
    }

    private bool ValidateExpiration(LicenseInfo licenseInfo)
    {
      var referencesToXtensiveAssemblies = project.Module.AssemblyRefs
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
      return project.Module.Assembly.IsStronglyNamed
        && CheckPublicKeyToken(project.Module.Assembly.GetPublicKeyToken(), XtensivePublicKeyToken);
    }

    private bool IsPartnershipMode()
    {
      return IsPartnerAssemblyReferenced("Werp.Models", new byte[] {0x89, 0xa4, 0x89, 0x72, 0xaa, 0xfe, 0x8a, 0xfe});
    }

    private bool IsPartnerAssemblyReferenced(string partnerAssembly, byte[] partnerToken)
    {
      return project.Module.AssemblyRefs
        .Any(r => r.Name==partnerAssembly && CheckPublicKeyToken(r.GetPublicKeyToken(), partnerToken));
    }

    private DateTime GetAssemblyBuildDate(Assembly assembly)
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

    private bool CheckPublicKeyToken(byte[] tokenToCheck, byte[] expectedToken)
    {
      return tokenToCheck!=null
        && tokenToCheck.Length==expectedToken.Length
        && tokenToCheck.SequenceEqual(expectedToken);
    }

    private bool ValidateLicense(LicenseValidator validator, LicenseInfo licenseInfo)
    {
      if (licenseInfo.IsValid && validator.WeaverLicenseCheckIsRequired())
        OnlineCheck(validator, licenseInfo);

      if (!licenseInfo.IsValid) {
        FatalLicenseError("{0} license is not valid.", ThisAssembly.ProductName);
        return false;
      }

      return true;
    }

    private void OnlineCheck(LicenseValidator validator, LicenseInfo licenseInfo)
    {
      var companyLicenseData = licenseInfo.LicenseType==LicenseType.Trial
        ? null
        : validator.GetCompanyLicenseData();
      var request = new InternetCheckRequest(
        companyLicenseData, licenseInfo.ExpireOn, validator.ProductVersion, validator.HardwareId);
      var result = InternetActivator.Check(request);
      if (result.IsValid==false && licenseInfo.RequiresHardwareLicense) {
        validator.InvalidateHardwareLicense();
        licenseInfo.HardwareKeyIsValid = false;
      }
    }

    private void FatalLicenseError(string format, params object[] args)
    {
      // ReSharper disable AssignNullToNotNullAttribute
      var executable = Path.Combine(Path.GetDirectoryName(selfLocation), "LicenseManager.exe");
      // ReSharper restore AssignNullToNotNullAttribute
      var runManager = Environment.UserInteractive
        && Environment.OSVersion.Platform==PlatformID.Win32NT
        && File.Exists(executable);
      if (runManager)
        Process.Start(new ProcessStartInfo(executable) {UseShellExecute = false});
      ErrorLog.Write(MessageLocation.Unknown, SeverityType.Fatal, format, args);
    }

    // Constructors

    public AspectValidator(Project project)
    {
      this.project = project;
      selfLocation = Platform.Current.GetAssemblyLocation(typeof (PlugIn).Assembly);
      IsValid = Initialize();
      Current = this;
    }
  }
}