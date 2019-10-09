// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2012.07.06

using System.Configuration;

namespace Xtensive.Orm.Localization.Configuration
{
  /// <summary>
  /// Default culture configuration element within a configuration file.
  /// </summary>
  public class DefaultCultureConfigurationElement : ConfigurationElement
  {
    private const string NameElementName = "name";

   /// <summary>
    /// Gets or sets the name of the default culture.
    /// </summary>
    [ConfigurationProperty(NameElementName, IsRequired = false)]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }
  }
}