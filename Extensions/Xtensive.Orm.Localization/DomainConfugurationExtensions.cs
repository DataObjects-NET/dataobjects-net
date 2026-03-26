// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Localization.Configuration;

namespace Xtensive.Orm.Localization
{
  /// <summary>
  /// Contains extensions for DomainConfiguration that help to configure the extension.
  /// </summary>
  public static class DomainConfugurationLocalizationExtensions
  {
    /// <summary>
    /// Loads configuration by calling <see cref="LocalizationConfiguration.Load()"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureLocalizationExtension(this DomainConfiguration domainConfiguration)
      => ConfigureLocalizationExtension(domainConfiguration, LocalizationConfiguration.Load());

    /// <summary>
    /// Loads configuration by calling <see cref="LocalizationConfiguration.Load(string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configurationSectionName">Section name.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureLocalizationExtension(this DomainConfiguration domainConfiguration,
       string configurationSectionName) =>
      ConfigureLocalizationExtension(domainConfiguration, LocalizationConfiguration.Load(configurationSectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="LocalizationConfiguration.Load(System.Configuration.Configuration)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configuration">Configuration to load from.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureLocalizationExtension(this DomainConfiguration domainConfiguration,
        System.Configuration.Configuration configuration) =>
      ConfigureLocalizationExtension(domainConfiguration, LocalizationConfiguration.Load(configuration));

    /// <summary>
    /// Loads configuration by calling <see cref="LocalizationConfiguration.Load(System.Configuration.Configuration)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configuration">Configuration to load from.</param>
    /// <param name="sectionName">Section in <paramref name="configuration"/></param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureLocalizationExtension(this DomainConfiguration domainConfiguration,
        System.Configuration.Configuration configuration, string sectionName) =>
      ConfigureLocalizationExtension(domainConfiguration, LocalizationConfiguration.Load(configuration, sectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="LocalizationConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configuration">Configuration to load from.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureLocalizationExtension(this DomainConfiguration domainConfiguration,
        IConfiguration configuration) =>
      ConfigureLocalizationExtension(domainConfiguration, LocalizationConfiguration.Load(configuration));

    /// <summary>
    /// Loads configuration by calling <see cref="LocalizationConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configuration">Configuration to load from.</param>
    /// <param name="sectionName">Section in <paramref name="configuration"/></param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureLocalizationExtension(this DomainConfiguration domainConfiguration,
        IConfiguration configuration, string sectionName = null) =>
      ConfigureLocalizationExtension(domainConfiguration, LocalizationConfiguration.Load(configuration, sectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="LocalizationConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configurationRoot">Configuration to load from.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureLocalizationExtension(this DomainConfiguration domainConfiguration,
      IConfigurationRoot configurationRoot) =>
      ConfigureLocalizationExtension(domainConfiguration, LocalizationConfiguration.Load(configurationRoot));

    /// <summary>
    /// Loads configuration by calling <see cref="LocalizationConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configurationRoot">Configuration to load from.</param>
    /// <param name="sectionName">Section in <paramref name="configurationRoot"/></param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureLocalizationExtension(this DomainConfiguration domainConfiguration,
        IConfigurationRoot configurationRoot, string sectionName) =>
      ConfigureLocalizationExtension(domainConfiguration, LocalizationConfiguration.Load(configurationRoot, sectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="LocalizationConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configurationSection">Configuration section to load from.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureLocalizationExtension(this DomainConfiguration domainConfiguration,
        IConfigurationSection configurationSection) =>
      ConfigureLocalizationExtension(domainConfiguration, LocalizationConfiguration.Load(configurationSection));

    /// <summary>
    /// Configures the extension with given localization configuration instance.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="localizationConfiguration">Localization configuration instance.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureLocalizationExtension(this DomainConfiguration domainConfiguration,
      LocalizationConfiguration localizationConfiguration)
    {
      domainConfiguration.ExtensionConfigurations.Set(localizationConfiguration);
      domainConfiguration.Types.Register(typeof(DomainConfugurationLocalizationExtensions).Assembly);
      return domainConfiguration;
    }
  }
}