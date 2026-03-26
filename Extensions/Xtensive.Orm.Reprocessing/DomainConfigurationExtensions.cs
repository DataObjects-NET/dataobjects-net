// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Reprocessing.Configuration;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Contains extensions for DomainConfiguration that help to configure the extension.
  /// </summary>
  public static class DomainConfigurationReprocessingExtensions
  {
    /// <summary>
    /// Loads configuration by calling <see cref="ReprocessingConfiguration.Load()"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureReprocessingExtension(this DomainConfiguration domainConfiguration) =>
      ConfigureReprocessingExtension(domainConfiguration, ReprocessingConfiguration.Load());

    /// <summary>
    /// Loads configuration by calling <see cref="ReprocessingConfiguration.Load(string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configurationSectionName">Section name.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureReprocessingExtension(this DomainConfiguration domainConfiguration,
       string configurationSectionName) =>
      ConfigureReprocessingExtension(domainConfiguration, ReprocessingConfiguration.Load(configurationSectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="ReprocessingConfiguration.Load(System.Configuration.Configuration)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configuration">Configuration to load from.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureReprocessingExtension(this DomainConfiguration domainConfiguration,
        System.Configuration.Configuration configuration) =>
      ConfigureReprocessingExtension(domainConfiguration, ReprocessingConfiguration.Load(configuration));

    /// <summary>
    /// Loads configuration by calling <see cref="ReprocessingConfiguration.Load(System.Configuration.Configuration)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configuration">Configuration to load from.</param>
    /// <param name="sectionName">Section in <paramref name="configuration"/></param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureReprocessingExtension(this DomainConfiguration domainConfiguration,
        System.Configuration.Configuration configuration, string sectionName) =>
      ConfigureReprocessingExtension(domainConfiguration, ReprocessingConfiguration.Load(configuration, sectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="ReprocessingConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configuration">Configuration to load from.</param>
    /// <param name="sectionName">Section in <paramref name="configuration"/></param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureReprocessingExtension(this DomainConfiguration domainConfiguration,
        IConfiguration configuration, string sectionName = null) =>
      ConfigureReprocessingExtension(domainConfiguration, ReprocessingConfiguration.Load(configuration, sectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="ReprocessingConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configurationRoot">Configuration to load from.</param>
    /// <param name="sectionName">Section in <paramref name="configurationRoot"/></param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureReprocessingExtension(this DomainConfiguration domainConfiguration,
        IConfigurationRoot configurationRoot, string sectionName = null) =>
      ConfigureReprocessingExtension(domainConfiguration, ReprocessingConfiguration.Load(configurationRoot, sectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="ReprocessingConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configurationSection">Configuration section to load from.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureReprocessingExtension(this DomainConfiguration domainConfiguration,
        IConfigurationSection configurationSection) =>
      ConfigureReprocessingExtension(domainConfiguration, ReprocessingConfiguration.Load(configurationSection));

    /// <summary>
    /// Configures the extension with given reprocessing configuration instance.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="reprocessingConfiguration">Security configuration instance.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureReprocessingExtension(this DomainConfiguration domainConfiguration,
      ReprocessingConfiguration reprocessingConfiguration)
    {
      domainConfiguration.ExtensionConfigurations.Set(reprocessingConfiguration);
      domainConfiguration.Types.Register(typeof(DomainConfigurationReprocessingExtensions).Assembly);
      return domainConfiguration;
    }
  }
}