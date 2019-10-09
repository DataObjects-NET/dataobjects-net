// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2012.07.06

using System;
using System.Configuration;

namespace Xtensive.Orm.Localization.Configuration
{
  /// <summary>
  /// A root element of localization configuration section within a configuration file.
  /// </summary>
  public class ConfigurationSection : System.Configuration.ConfigurationSection
  {
    /// <summary>
    /// Gets default section name for security configuration.
    /// Value is "Xtensive.Orm.Localization".
    /// </summary>
    public static readonly string DefaultSectionName = "Xtensive.Orm.Localization";

    private const string DefaultCultureElementName = "defaultCulture";

    /// <summary>
    /// Gets or sets the authentication service.
    /// </summary>
    /// <value>The authentication service.</value>
    [ConfigurationProperty(DefaultCultureElementName, IsRequired = false)]
    public DefaultCultureConfigurationElement DefaultCulture
    {
      get { return (DefaultCultureConfigurationElement) this[DefaultCultureElementName]; }
      set { this[DefaultCultureElementName] = value; }
    }
  }
}