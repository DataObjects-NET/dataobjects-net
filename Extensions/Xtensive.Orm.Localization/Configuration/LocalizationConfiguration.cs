// Copyright (C) 2012-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2012.07.06

using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Localization.Configuration
{
  /// <summary>
  /// The configuration of the localization extension.
  /// </summary> 
  [Serializable]
  public class LocalizationConfiguration : ConfigurationBase
  {
    /// <summary>
    /// Default SectionName value:
    /// "<see langword="Xtensive.Orm.Localization" />".
    /// </summary>
    public const string DefaultSectionName = "Xtensive.Orm.Localization";

    private CultureInfo culture;

    /// <summary>
    /// Gets or sets the default culture.
    /// </summary>
    /// <value>The default culture.</value>
    public CultureInfo DefaultCulture {
      get => culture;
      internal set {
        EnsureNotLocked();
        culture = value;
      }
    }

    /// <inheritdoc />
    protected override LocalizationConfiguration CreateClone() => new LocalizationConfiguration();

    /// <inheritdoc />
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);

      var nativeConfig = source as LocalizationConfiguration;
      nativeConfig.DefaultCulture = DefaultCulture;
    }

    /// <inheritdoc />
    public override LocalizationConfiguration Clone() => (LocalizationConfiguration) base.Clone();


    /// <summary>
    /// Loads the <see cref="LocalizationConfiguration"/>
    /// from application configuration file (section with <see cref="DefaultSectionName"/>).
    /// </summary>
    /// <returns>
    /// The <see cref="LocalizationConfiguration"/>.
    /// </returns>
    public static LocalizationConfiguration Load()
    {
      return Load(DefaultSectionName);
    }

    /// <summary>
    /// Loads the <see cref="LocalizationConfiguration"/>
    /// from application configuration file (section with <paramref name="sectionName"/>).
    /// </summary>
    /// <param name="sectionName">Name of the section.</param>
    /// <returns>
    /// The <see cref="LocalizationConfiguration"/>.
    /// </returns>
    public static LocalizationConfiguration Load(string sectionName)
    {
      var section = (ConfigurationSection) ConfigurationManager.GetSection(sectionName);
      return GetConfigurationFromSection(section);
    }

    /// <summary>
    /// Loads the <see cref="LocalizationConfiguration"/>
    /// from given configuration (section with <see cref="DefaultSectionName"/>).
    /// </summary>
    /// <param name="configuration">The configuration to load from.</param>
    /// <returns>
    /// The <see cref="LocalizationConfiguration"/>.
    /// </returns>
    public static LocalizationConfiguration Load(System.Configuration.Configuration configuration)
    {
      return Load(configuration, DefaultSectionName);
    }

    /// <summary>
    /// Loads the <see cref="LocalizationConfiguration"/>
    /// from application configuration file (section with <paramref name="sectionName"/>).
    /// </summary>
    /// <param name="configuration">The configuration to load from.</param>
    /// <param name="sectionName">Name of the section.</param>
    /// <returns>
    /// The <see cref="LocalizationConfiguration"/>.
    /// </returns>
    public static LocalizationConfiguration Load(System.Configuration.Configuration configuration, string sectionName)
    {
      var section = (ConfigurationSection) configuration.GetSection(sectionName);
      return GetConfigurationFromSection(section);
    }

    private static LocalizationConfiguration GetConfigurationFromSection(ConfigurationSection configurationSection)
    {
      var result = new LocalizationConfiguration();
      result.DefaultCulture = Thread.CurrentThread.CurrentCulture;

      string cultureName = configurationSection == null
        ? string.Empty
        : configurationSection.DefaultCulture.Name;
      if (string.IsNullOrEmpty(cultureName))
        return result;

      try {
        var culture = new CultureInfo(cultureName);
        result.DefaultCulture = culture;
      }
      catch (CultureNotFoundException) {
      }
      return result;
    }

    /// <summary>
    /// Loads <see cref="LocalizationConfiguration"/> from given configuration section of <paramref name="configuration"/>.
    /// If section name is not provided <see cref="LocalizationConfiguration.DefaultSectionName"/> is used.
    /// </summary>
    /// <param name="configuration"><see cref="IConfiguration"/> of sections.</param>
    /// <param name="sectionName">Custom section name to load from.</param>
    /// <returns>Loaded configuration or default configuration if loading failed for some reason.</returns>
    public static LocalizationConfiguration Load(IConfiguration configuration, string sectionName = null)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));

      if (configuration is IConfigurationRoot configurationRoot) {
        return new LocalizationConfigurationReader().Read(configurationRoot, sectionName ?? DefaultSectionName);
      }
      else if (configuration is IConfigurationSection configurationSection) {
        return sectionName.IsNullOrEmpty()
          ? new LocalizationConfigurationReader().Read(configurationSection)
          : new LocalizationConfigurationReader().Read(configurationSection.GetSection(sectionName));
      }

      throw new NotSupportedException("Type of configuration is not supported.");
    }

    /// <summary>
    /// Loads <see cref="LocalizationConfiguration"/> from given configuration section.
    /// </summary>
    /// <param name="configurationSection"><see cref="IConfigurationSection"/> to load from.</param>
    /// <returns>Loaded configuration or default configuration if loading failed for some reason.</returns>
    public static LocalizationConfiguration Load(IConfigurationSection configurationSection)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationSection, nameof(configurationSection));

      return new LocalizationConfigurationReader().Read(configurationSection);
    }

    /// <summary>
    /// Loads <see cref="LocalizationConfiguration"/> from given configuration section of <paramref name="configurationRoot"/>.
    /// If section name is not provided <see cref="LocalizationConfiguration.DefaultSectionName"/> is used.
    /// </summary>
    /// <param name="configurationRoot"><see cref="IConfigurationRoot"/> of sections.</param>
    /// <param name="sectionName">Custom section name to load from.</param>
    /// <returns>Loaded configuration or default configuration if loading failed for some reason.</returns>
    public static LocalizationConfiguration Load(IConfigurationRoot configurationRoot, string sectionName = null)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationRoot, nameof(configurationRoot));

      return new LocalizationConfigurationReader().Read(configurationRoot, sectionName ?? DefaultSectionName);
    }
  }
}