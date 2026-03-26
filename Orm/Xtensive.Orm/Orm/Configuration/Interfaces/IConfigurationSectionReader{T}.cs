// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.


using Microsoft.Extensions.Configuration;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Reads certain type of configuration (DomainConfiguration, LoggingConfiguration, etc.) from IConfigurationSection API
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public interface IConfigurationSectionReader<TConfiguration>
  {
    /// <summary>
    /// Reads configuration from given configuration section.
    /// </summary>
    /// <param name="configurationSection">Root configuration section where specific configuration is placed (for domain configuration - where all domain configurations).</param>
    /// <returns>Instance of configuration.</returns>
    TConfiguration Read(IConfigurationSection configurationSection);

    /// <summary>
    /// Reads configuration from given configuration section with specified name (if named configuration supported e.g. doman configuration)
    /// </summary>
    /// <param name="configurationSection">Root configuration section where specific configuration is placed (for domain configuration - where all domain configurations).</param>
    /// <param name="nameOfConfiguration">Name of configuration.</param>
    /// <returns>Instance of configuration.</returns>
    TConfiguration Read(IConfigurationSection configurationSection, string nameOfConfiguration);

    /// <summary>
    /// Reads configuration (with default name if name is required) from default section (and with default name) from given configuration root.
    /// </summary>
    /// <param name="configurationRoot">Root of all configuration sections.</param>
    /// <returns>Instance of configuration.</returns>
    TConfiguration Read(IConfigurationRoot configurationRoot);

    /// <summary>
    /// Reads configuration (with default name if name is required) from given section of given configuration root.
    /// </summary>
    /// <param name="configurationRoot">Root of all configuration sections.</param>
    /// <param name="sectionName">Specific section name from which configuration should be read.</param>
    /// <returns>Instance of configuration.</returns>
    TConfiguration Read(IConfigurationRoot configurationRoot, string sectionName);

    /// <summary>
    /// Reads configuration with given name from given section of given configuration root.
    /// </summary>
    /// <param name="configurationRoot">Root of all configuration sections.</param>
    /// <param name="sectionName">Specific section name from which configuration should be read.</param>
    /// <param name="nameOfConfiguration">Name of configuration.</param>
    /// <returns>Instance of configuration.</returns>
    TConfiguration Read(IConfigurationRoot configurationRoot, string sectionName, string nameOfConfiguration);
  }
}
