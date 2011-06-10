// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.07

using System;
using System.Configuration;

namespace Xtensive.Practices.Security.Configuration
{
  /// <summary>
  /// A root element of security configuration section within a configuration file.
  /// </summary>
  public class ConfigurationSection : System.Configuration.ConfigurationSection
  {
    /// <summary>
    /// Gets default section name for security configuration.
    /// Value is "Xtensive.Security".
    /// </summary>
    public static readonly string DefaultSectionName = "Xtensive.Security";

    private const string HashingServiceElementName = "hashingService";
    private const string ValidationServiceElementName = "validationService";

    [ConfigurationProperty(HashingServiceElementName, IsRequired = false)]
    public HashingServiceConfigurationElement HashingService
    {
      get { return (HashingServiceConfigurationElement) this[HashingServiceElementName]; }
      set { this[HashingServiceElementName] = value; }
    }

    [ConfigurationProperty(ValidationServiceElementName, IsRequired = false)]
    public ValidationServiceConfigurationElement ValidationService
    {
      get { return (ValidationServiceConfigurationElement) this[ValidationServiceElementName]; }
      set { this[ValidationServiceElementName] = value; }
    }
  }
}