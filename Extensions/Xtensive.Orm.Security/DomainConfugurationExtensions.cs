// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Security.Configuration;

namespace Xtensive.Orm.Security
{
  /// <summary>
  /// Contains extensions for DomainConfiguration that help to configure the extension.
  /// </summary>
  public static class DomainConfugurationSecurityExtensions
  {
    /// <summary>
    /// Loads configuration by calling <see cref="SecurityConfiguration.Load()"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureSecurityExtension(this DomainConfiguration domainConfiguration) =>
      ConfigureSecurityExtension(domainConfiguration, SecurityConfiguration.Load());

    /// <summary>
    /// Loads configuration by calling <see cref="SecurityConfiguration.Load(string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configurationSectionName">Section name.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureSecurityExtension(this DomainConfiguration domainConfiguration,
        string configurationSectionName) =>
      ConfigureSecurityExtension(domainConfiguration, SecurityConfiguration.Load(configurationSectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="SecurityConfiguration.Load(System.Configuration.Configuration)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configuration">Configuraton to load from.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureSecurityExtension(this DomainConfiguration domainConfiguration,
        System.Configuration.Configuration configuration) =>
      ConfigureSecurityExtension(domainConfiguration, SecurityConfiguration.Load(configuration));

    /// <summary>
    /// Loads configuration by calling <see cref="SecurityConfiguration.Load(System.Configuration.Configuration)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configuration">Configuraton to load from.</param>
    /// <param name="sectionName">Section in <paramref name="configuration"/></param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureSecurityExtension(this DomainConfiguration domainConfiguration,
        System.Configuration.Configuration configuration, string sectionName) =>
      ConfigureSecurityExtension(domainConfiguration, SecurityConfiguration.Load(configuration, sectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="SecurityConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configuration">Configuraton to load from.</param>
    /// <param name="sectionName">Section in <paramref name="configuration"/></param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureSecurityExtension(this DomainConfiguration domainConfiguration,
        IConfiguration configuration, string sectionName = null) =>
      ConfigureSecurityExtension(domainConfiguration, SecurityConfiguration.Load(configuration, sectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="SecurityConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configurationRoot">Configuraton to load from.</param>
    /// <param name="sectionName">Section in <paramref name="configurationRoot"/></param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureSecurityExtension(this DomainConfiguration domainConfiguration,
        IConfigurationRoot configurationRoot, string sectionName = null) =>
      ConfigureSecurityExtension(domainConfiguration, SecurityConfiguration.Load(configurationRoot, sectionName));

    /// <summary>
    /// Loads configuration by calling <see cref="SecurityConfiguration.Load(IConfigurationRoot, string)"/>
    /// and uses it to configure the extension.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="configurationSection">Configuration section to load from.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureSecurityExtension(this DomainConfiguration domainConfiguration,
        IConfigurationSection configurationSection) =>
      ConfigureSecurityExtension(domainConfiguration, SecurityConfiguration.Load(configurationSection));

    /// <summary>
    /// Configures the extension with given security configuration instance.
    /// </summary>
    /// <param name="domainConfiguration">Domain configuration.</param>
    /// <param name="securityConfiguration">Security configuration instance.</param>
    /// <returns><paramref name="domainConfiguration"/> instance with configured extension.</returns>
    public static DomainConfiguration ConfigureSecurityExtension(this DomainConfiguration domainConfiguration,
      SecurityConfiguration securityConfiguration)
    {
      domainConfiguration.ExtensionConfigurations.Set(securityConfiguration);
      domainConfiguration.Types.Register(typeof(DomainConfugurationSecurityExtensions).Assembly);
      return domainConfiguration;
    }
  }
}