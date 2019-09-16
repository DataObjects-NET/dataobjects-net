// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.10

using System;
using System.Configuration;

namespace Xtensive.Orm.Security.Configuration
{
  /// <summary>
  /// The configuration of the security system.
  /// </summary> 
  [Serializable]
  public class SecurityConfiguration
  {
    /// <summary>
    /// Default SectionName value:
    /// "<see langword="Xtensive.Orm.Security" />".
    /// </summary>
    public const string DefaultSectionName = "Xtensive.Orm.Security";

    /// <summary>
    /// Gets or sets the name of the hashing service.
    /// </summary>
    /// <value>The name of the hashing service.</value>
    public string HashingServiceName { get; private set; }

    /// <summary>
    /// Gets or sets the name of the authentication service.
    /// </summary>
    /// <value>The name of the authentication service.</value>
    public string AuthenticationServiceName { get; private set; }

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
      var configurationSection = (ConfigurationSection)configuration.GetSection(sectionName);
      return GetConfigurationFromSection(configurationSection);
    }

    private static SecurityConfiguration GetConfigurationFromSection(ConfigurationSection configurationSection)
    {
      var result = new SecurityConfiguration();

      string hashingService = configurationSection==null
        ? string.Empty 
        : configurationSection.HashingService.Name;
      if (string.IsNullOrEmpty(hashingService))
        hashingService = "plain";
      result.HashingServiceName = hashingService.ToLowerInvariant();

      string authenticationService = configurationSection==null 
        ? string.Empty 
        : configurationSection.AuthenticationService.Name;
      if (string.IsNullOrEmpty(authenticationService))
        authenticationService = "default";
      result.AuthenticationServiceName = authenticationService.ToLowerInvariant();

      return result;
    }
  }
}