// Copyright (C) 2011-2024 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2012.07.06

using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Localization.Configuration
{
  /// <summary>
  /// The configuration of the localization extension.
  /// </summary> 
  [Serializable]
  public class LocalizationConfiguration
  {
    private class LocalizationOptions
    {
      public string DefaultCulture { get; set; } = null;
    }

    /// <summary>
    /// Default SectionName value:
    /// "<see langword="Xtensive.Orm.Localization" />".
    /// </summary>
    public const string DefaultSectionName = "Xtensive.Orm.Localization";

    private const string DefaultCultureElementName = "DefaultCulture";
    private const string CultureNameAttributeName = "name";

    /// <summary>
    /// Gets or sets the default culture.
    /// </summary>
    /// <value>The default culture.</value>
    public CultureInfo DefaultCulture { get; private set; }

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

    public static LocalizationConfiguration Load(IConfigurationSection configurationSection)
    {
      ArgumentValidator.EnsureArgumentNotNull(configurationSection, nameof(configurationSection));

      if (TryReadAsOptions(configurationSection, out var localizationConfiguration))
        return localizationConfiguration;

      // if failed then try to handle unusual formats or xml with name attribute
      return TryReadUnusualOrOldFormats(configurationSection, out var fallbackConfiguration)
        ? fallbackConfiguration
        : new LocalizationConfiguration {
            DefaultCulture = Thread.CurrentThread.CurrentCulture
          };
    }

    private static bool TryReadAsOptions(IConfigurationSection rootSection, out LocalizationConfiguration localizationConfiguration)
    {
      LocalizationOptions localizationOptions;
      try {
        localizationOptions = rootSection.Get<LocalizationOptions>();
      }
      catch {
        localizationConfiguration = null;
        return false;
      }

      if (localizationOptions != null) {
        if (!string.IsNullOrEmpty(localizationOptions.DefaultCulture)) {
          try {
            var culture = new CultureInfo(localizationOptions.DefaultCulture);
            localizationConfiguration = new LocalizationConfiguration() { DefaultCulture = culture };
            return true;
          }
          catch (CultureNotFoundException) {
          }
        }
      }
      localizationConfiguration = null;
      return false;
    }

    /// <summary>
    /// Tries to read configuration of old format that supported by
    /// old <see cref="System.Configuration.ConfigurationManager"/>
    /// or configuration where name of service is element, not attribute.
    /// </summary>
    /// <param name="rootSection">A configuration section that contains data to read.</param>
    /// <param name="localizationConfiguration">Read configuration or null if reading was not successful.</param>
    /// <returns><see landword="true"/> if reading is successful, otherwise <see landword="true"/>.</returns>
    private static bool TryReadUnusualOrOldFormats(IConfigurationSection rootSection,
      out LocalizationConfiguration localizationConfiguration)
    {
      var defaultCultureSection = rootSection.GetSection(DefaultCultureElementName);

      if (defaultCultureSection == null) {
        localizationConfiguration = null;
        return false;
      }

      var cultureName = defaultCultureSection.GetSection(CultureNameAttributeName)?.Value;
      if (cultureName == null) {
        var children = defaultCultureSection.GetChildren().ToList();
        if (children.Count > 0) {
          cultureName = children[0].GetSection(CultureNameAttributeName).Value;
        }
      }

      if (cultureName != null && ! string.IsNullOrEmpty(cultureName)) {
        try {
          var culture = new CultureInfo(cultureName);
          localizationConfiguration = new LocalizationConfiguration() {
            DefaultCulture = culture
          };
          return true;
        }
        catch (CultureNotFoundException) {
          localizationConfiguration = new LocalizationConfiguration() {
            DefaultCulture = Thread.CurrentThread.CurrentCulture
          };
          return true;
        }
      }
      localizationConfiguration = null;
      return false;
    }
  }
}