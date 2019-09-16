// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2012.07.06

using System;
using System.Configuration;
using System.Globalization;
using System.Threading;

namespace Xtensive.Orm.Localization.Configuration
{
  /// <summary>
  /// The configuration of the localization extension.
  /// </summary> 
  [Serializable]
  public class LocalizationConfiguration
  {
    /// <summary>
    /// Default SectionName value:
    /// "<see langword="Xtensive.Orm.Localization" />".
    /// </summary>
    public const string DefaultSectionName = "Xtensive.Orm.Localization";

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

      string cultureName = configurationSection==null
        ? string.Empty 
        : configurationSection.DefaultCulture.Name;
      if (string.IsNullOrEmpty(cultureName))
        return result;

      try {
        var culture = new CultureInfo(cultureName);
        result.DefaultCulture = culture;
      }
      catch (CultureNotFoundException)
      {
      }
      return result;
    }
  }
}