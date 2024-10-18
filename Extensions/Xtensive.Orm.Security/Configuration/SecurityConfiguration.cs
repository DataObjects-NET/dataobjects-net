// Copyright (C) 2011-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.06.10

using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Security.Configuration
{
  /// <summary>
  /// The configuration of the security system.
  /// </summary> 
  [Serializable]
  public class SecurityConfiguration : ConfigurationBase
  {
    /// <summary>
    /// Default section in configuration. Used if custom name is not provided.
    /// </summary>
    public const string DefaultSectionName = "Xtensive.Orm.Security";

    internal const string HashingServiceElementName = "HashingService";
    internal const string AuthenticationServiceElementName = "AuthenticationService";

    internal const string DefaultHashingServiceName = "plain";
    internal const string DefaultAuthenticationServiceName = "default";

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

    /// <inheritdoc />
    protected override SecurityConfiguration CreateClone() => new SecurityConfiguration();

    /// <inheritdoc />
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);

      var configuration = (SecurityConfiguration) source;
      configuration.HashingServiceName = configuration.HashingServiceName;
      configuration.AuthenticationServiceName = configuration.AuthenticationServiceName;
    }

    /// <inheritdoc />
    public override SecurityConfiguration Clone() => (SecurityConfiguration) base.Clone();

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
      if (!string.IsNullOrEmpty(hashingService)) {
        result.HashingServiceName = hashingService.ToLowerInvariant();
      }

      var authenticationService = configurationSection == null
        ? string.Empty
        : configurationSection.AuthenticationService.Name;
      if (!string.IsNullOrEmpty(authenticationService)) {
        result.AuthenticationServiceName = authenticationService.ToLowerInvariant();
      }

      return result;
    }

    /// <summary>
    /// Loads the <see cref="SecurityConfiguration"/> from given configuration section.
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> to load section from.</param>
    /// <param name="sectionName">Name of the section where configuration is stored.</param>
    /// <returns>Loaded configuration or configuration with default settings.</returns>
    public static SecurityConfiguration Load(IConfiguration configuration, string sectionName = null)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));

      if (configuration is IConfigurationRoot configurationRoot) {
        return Load(configurationRoot, sectionName);
      }
      else if (configuration is IConfigurationSection configurationSection) {
        return Load(configurationSection);
      }

      throw new NotSupportedException("Type of configuration is not supported.");
    }


    /// <summary>
    /// Loads the <see cref="SecurityConfiguration"/> from given configuration section.
    /// </summary>
    /// <param name="configurationRoot"><see cref="IConfigurationRoot"/> to load section from.</param>
    /// <param name="sectionName">Name of the section where configuration is stored.</param>
    /// <returns>Loaded configuration or configuration with default settings.</returns>
    public static SecurityConfiguration Load(IConfigurationRoot configurationRoot, string sectionName = null)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationRoot, nameof(configurationRoot));

      var configuration = new NamelessFormatSecurityConfigurationReader().Read(configurationRoot, sectionName ?? DefaultSectionName);
      if (configuration != null) {
        return configuration;
      }

      configuration = new BasedOnNamesFormatSecurityConfigurationReader().Read(configurationRoot, sectionName ?? DefaultSectionName);
      return configuration ?? new SecurityConfiguration(true);
    }

    /// <summary>
    /// Loads the <see cref="SecurityConfiguration"/> from given configuration section.
    /// </summary>
    /// <param name="configurationSection"><see cref="IConfigurationSection"/> to load from.</param>
    /// <returns>Loaded configuration or configuration with default settings.</returns>
    public static SecurityConfiguration Load(IConfigurationSection configurationSection)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationSection, nameof(configurationSection));

      var configuration = new NamelessFormatSecurityConfigurationReader().Read(configurationSection);
      if (configuration != null) {
        return configuration;
      }

      configuration = new BasedOnNamesFormatSecurityConfigurationReader().Read(configurationSection);
      return configuration ?? new SecurityConfiguration(true);
    }

    /// <summary>
    /// Creates instance of <see cref="SecurityConfiguration"/> with no properties initialized.
    /// </summary>
    public SecurityConfiguration()
    {
    }

    /// <summary>
    /// Creates instance of <see cref="SecurityConfiguration"/> with properties initialized on demand.
    /// </summary>
    internal SecurityConfiguration(bool initWithDefaults)
    {
      if (initWithDefaults) {
        HashingServiceName = DefaultHashingServiceName;
        AuthenticationServiceName = DefaultAuthenticationServiceName;
      }
    }
  }
}
