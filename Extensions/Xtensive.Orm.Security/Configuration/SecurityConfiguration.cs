// Copyright (C) 2011-2024 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.10

using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Security.Configuration
{
  /// <summary>
  /// The configuration of the security system.
  /// </summary> 
  [Serializable]
  public class SecurityConfiguration
  {
    #region nested types
#pragma warning disable CS0661, CS0659 // Type defines operator == or operator != but does not override Object.GetHashCode()
#if !NET6_0_OR_GREATER
    /// <summary>
    /// Represents Json and XML without neither name attributes nor name as child element;
    /// </summary>
    private struct SecurityOptions
    {
      public string HashingService { get; set; }

      public string AuthenticationService { get; set; }

      public static bool operator ==(SecurityOptions r1, SecurityOptions r2)
      {
        return (r1.HashingService, r1.AuthenticationService) == (r2.HashingService, r2.AuthenticationService);
      }

      public static bool operator !=(SecurityOptions r1, SecurityOptions r2)
      {
        return (r1.HashingService, r1.AuthenticationService) != (r2.HashingService, r2.AuthenticationService);
      }

      public override bool Equals(object obj)
      {
        if (obj is null)
          return false;
        if (obj is SecurityOptions objRep)
          return (HashingService, AuthenticationService) == (objRep.HashingService, objRep.AuthenticationService);
        return false;
      }
    }

#endif
#pragma warning restore CS0661, CS0659 // Type defines operator == or operator != but does not override Object.GetHashCode()

#endregion

    /// <summary>
    /// Default SectionName value:
    /// "<see langword="Xtensive.Orm.Security" />".
    /// </summary>
    public const string DefaultSectionName = "Xtensive.Orm.Security";

    internal const string HashingServiceElementName = "HashingService";
    internal const string AuthenticationServiceElementName = "AuthenticationService";

    private const string DefaultHashingServiceName = "plain";
    private const string DefaultAuthenticationServiceName = "default";
    private const string ServiceNameAttributeName = "name";

#if NET6_0_OR_GREATER
    /// <summary>
    /// Gets or sets the name of the hashing service.
    /// </summary>
    /// <value>The name of the hashing service.</value>
    [ConfigurationKeyName(HashingServiceElementName)]
    public string HashingServiceName { get; set; }

    /// <summary>
    /// Gets or sets the name of the authentication service.
    /// </summary>
    /// <value>The name of the authentication service.</value>
    [ConfigurationKeyName(AuthenticationServiceElementName)]
    public string AuthenticationServiceName { get; set; }
#else
    /// <summary>
    /// Gets or sets the name of the hashing service.
    /// </summary>
    /// <value>The name of the hashing service.</value>
    public string HashingServiceName { get; set; }

    /// <summary>
    /// Gets or sets the name of the authentication service.
    /// </summary>
    /// <value>The name of the authentication service.</value>
    public string AuthenticationServiceName { get; set; }
#endif

    /// <summary>
    /// Loads the <see cref="SecurityConfiguration"/>
    /// from application configuration file (section with <see cref="DefaultSectionName"/>).
    /// </summary>
    /// <returns>
    /// The <see cref="SecurityConfiguration"/>.
    /// </returns>
    public static SecurityConfiguration Load()
    {
      return Load(DefaultSectionName);
    }

    /// <summary>
    /// Loads the <see cref="SecurityConfiguration"/>
    /// from application configuration file (section with <paramref name="sectionName"/>).
    /// </summary>
    /// <param name="sectionName">Name of the section.</param>
    /// <returns>
    /// The <see cref="SecurityConfiguration"/>.
    /// </returns>
    public static SecurityConfiguration Load(string sectionName)
    {
      var section = (ConfigurationSection) ConfigurationManager.GetSection(sectionName);
      return GetConfigurationFromSection(section);
    }

    /// <summary>
    /// Loads the <see cref="SecurityConfiguration"/>
    /// from given configuration (section with <see cref="DefaultSectionName"/>).
    /// </summary>
    /// <param name="configuration">Configuration to load from.</param>
    /// <returns>The <see cref="SecurityConfiguration"/>.</returns>
    public static SecurityConfiguration Load(System.Configuration.Configuration configuration)
    {
      return Load(configuration, DefaultSectionName);
    }

    /// <summary>
    /// Loads the <see cref="SecurityConfiguration"/>
    /// from given configuration (section with <paramref name="sectionName"/>).
    /// </summary>
    /// <param name="configuration">Configuration to load from.</param>
    /// <param name="sectionName">Name of the section</param>
    /// <returns>The <see cref="SecurityConfiguration"/>.</returns>
    public static SecurityConfiguration Load(System.Configuration.Configuration configuration, string sectionName)
    {
      var configurationSection = (ConfigurationSection) configuration.GetSection(sectionName);
      return GetConfigurationFromSection(configurationSection);
    }

    private static SecurityConfiguration GetConfigurationFromSection(ConfigurationSection configurationSection)
    {
      var result = new SecurityConfiguration(true);

      var hashingService = configurationSection == null
        ? string.Empty
        : configurationSection.HashingService.Name;
      if (!string.IsNullOrEmpty(hashingService))
        result.HashingServiceName = hashingService.ToLowerInvariant();

      var authenticationService = configurationSection == null
        ? string.Empty
        : configurationSection.AuthenticationService.Name;
      if (!string.IsNullOrEmpty(authenticationService))
        result.AuthenticationServiceName = authenticationService.ToLowerInvariant();

      return result;
    }

    /// <summary>
    /// Loads the <see cref="SecurityConfiguration"/> from given configuration section.
    /// </summary>
    /// <param name="configurationSection"><see cref="IConfigurationSection"/> to load from.</param>
    /// <returns>Loaded configuration or configuration with default settings.</returns>
    public static SecurityConfiguration Load(IConfigurationSection configurationSection)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationSection, nameof(configurationSection));

      // First attempt is to read modern xml or json
      if (TryReadConfigurationAsTypeInstance(configurationSection, out var fromModerntStyle))
        return fromModerntStyle;

      // if failed then try to handle unusual formats or xml with name attribute
      return TryReadUnusualOrOldFormats(configurationSection, out var fallbackConfiguration)
        ? fallbackConfiguration
        : new SecurityConfiguration(true);
    }

    /// <summary>
    /// Tries to read configuration of most relevant format where service names declared as child nodes.
    /// </summary>
    /// <param name="configuration">A configuration section that contains data to read.</param>
    /// <param name="securityConfiguration"></param>
    /// <returns><see landword="true"/> if reading is successful, otherwise <see landword="true"/></returns>
    private static bool TryReadConfigurationAsTypeInstance(IConfigurationSection configuration, out SecurityConfiguration securityConfiguration)
    {
      //  <Xtensive.Orm.Security>
      //    <hashingService>sha1</hashingService>
      //    <authenticationService>SomeServiceName</authenticationService>
      //  </Xtensive.Orm.Security>
      // or
      //  "Xtensive.Orm.Security" : {
      //    "hashingService" :"sha1",
      //    "authenticationService" : "SomeServiceName"
      //  }

#if NET6_0_OR_GREATER
      try {

        var configAsIs = configuration.Get<SecurityConfiguration>();
        if (configAsIs != null && (configAsIs.AuthenticationServiceName ?? configAsIs.HashingServiceName) != null) {
          configAsIs.HashingServiceName = string.IsNullOrEmpty(configAsIs.HashingServiceName)
            ? DefaultHashingServiceName
            : configAsIs.HashingServiceName.ToLowerInvariant();
          configAsIs.AuthenticationServiceName = string.IsNullOrEmpty(configAsIs.AuthenticationServiceName)
            ? DefaultAuthenticationServiceName
            : (configAsIs.AuthenticationServiceName?.ToLowerInvariant());
          securityConfiguration = configAsIs;
          return true;
        }
      }
      catch {
        securityConfiguration = null;
        return false;
      }
#else
      try {
        var securityOptions = configuration.Get<SecurityOptions>();
        if (securityOptions != default) {
          securityConfiguration = new SecurityConfiguration(true);
          if (!string.IsNullOrEmpty(securityOptions.HashingService))
            securityConfiguration.HashingServiceName = securityOptions.HashingService.ToLowerInvariant();
          if(!string.IsNullOrEmpty(securityOptions.AuthenticationService))
            securityConfiguration.AuthenticationServiceName = securityOptions.AuthenticationService.ToLowerInvariant(); ;

          return true;
        }
      }
      catch {
        securityConfiguration = null;
        return false;
      }
#endif
      var children = configuration.GetChildren().ToList();
      if (!children.Any()) {
        securityConfiguration = new SecurityConfiguration(true);
        return true;
      }
      else {
        securityConfiguration = null;
        return false;
      }
    }

    /// <summary>
    /// Tries to read configuration of old format that supported by
    /// old <see cref="System.Configuration.ConfigurationManager"/>
    /// or configuration where name of service is element, not attribute.
    /// </summary>
    /// <param name="configuration">A configuration section that contains data to read.</param>
    /// <param name="securityConfiguration">Read configuration or null if reading was not successful.</param>
    /// <returns><see landword="true"/> if reading is successful, otherwise <see landword="true"/>.</returns>
    private static bool TryReadUnusualOrOldFormats(IConfigurationSection configuration,
      out SecurityConfiguration securityConfiguration)
    {
      var hashingServiceSection = configuration.GetSection(HashingServiceElementName);
      var authenticationServiceSection = configuration.GetSection(AuthenticationServiceElementName);

      if (hashingServiceSection == null && authenticationServiceSection == null) {
        securityConfiguration = null;
        return false;
      }

      var hashingServiceName = hashingServiceSection.GetSection(ServiceNameAttributeName)?.Value;
      if (hashingServiceName == null) {
        var children = hashingServiceSection.GetChildren().ToList();
        if (children.Count > 0) {
          hashingServiceName = children[0].GetSection(ServiceNameAttributeName).Value;
        }
      }

      var authenticationServiceName = authenticationServiceSection.GetSection(ServiceNameAttributeName)?.Value;
      if (authenticationServiceName == null) {
        var children = authenticationServiceSection.GetChildren().ToList();
        if (children.Count > 0) {
          authenticationServiceName = children[0].GetSection(ServiceNameAttributeName).Value;
        }
      }
      if ((hashingServiceName ?? authenticationServiceName) != null) {
        securityConfiguration = new SecurityConfiguration(true);
        if (!string.IsNullOrEmpty(hashingServiceName))
          securityConfiguration.HashingServiceName = hashingServiceName.ToLowerInvariant();
        if (!string.IsNullOrEmpty(authenticationServiceName))
          securityConfiguration.AuthenticationServiceName = authenticationServiceName.ToLowerInvariant();

        return true;
      }
      securityConfiguration = null;
      return false;
    }

    /// <summary>
    /// Creates instance of <see cref="SecurityConfiguration"/> with no properties initialized.
    /// </summary>
    public SecurityConfiguration()
    {
    }

    private SecurityConfiguration(bool initWithDefaults)
    {
      if (initWithDefaults) {
        HashingServiceName = DefaultHashingServiceName;
        AuthenticationServiceName = DefaultAuthenticationServiceName;
      }
    }
  }
}
