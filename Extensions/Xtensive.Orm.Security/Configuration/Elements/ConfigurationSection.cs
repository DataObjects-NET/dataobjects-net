// Copyright (C) 2011-2024 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.07

using System;
using System.Configuration;

namespace Xtensive.Orm.Security.Configuration
{
  /// <summary>
  /// A root element of security configuration section within a configuration file.
  /// </summary>
  public class ConfigurationSection : System.Configuration.ConfigurationSection
  {
    /// <summary>
    /// Gets default section name for security configuration.
    /// Value is "Xtensive.Orm.Security".
    /// </summary>
    [Obsolete("Use SecurityConfiguration.DefaultSectionName instead")]
    public static readonly string DefaultSectionName = "Xtensive.Orm.Security";

    /// <summary>
    /// Gets or sets the hashing service.
    /// </summary>
    /// <value>The hashing service.</value>
    [ConfigurationProperty(SecurityConfiguration.HashingServiceElementName, IsRequired = false)]
    public HashingServiceConfigurationElement HashingService
    {
      get { return (HashingServiceConfigurationElement) this[SecurityConfiguration.HashingServiceElementName]; }
      set { this[SecurityConfiguration.HashingServiceElementName] = value; }
    }

    /// <summary>
    /// Gets or sets the authentication service.
    /// </summary>
    /// <value>The authentication service.</value>
    [ConfigurationProperty(SecurityConfiguration.AuthenticationServiceElementName, IsRequired = false)]
    public AuthenticationServiceConfigurationElement AuthenticationService
    {
      get { return (AuthenticationServiceConfigurationElement) this[SecurityConfiguration.AuthenticationServiceElementName]; }
      set { this[SecurityConfiguration.AuthenticationServiceElementName] = value; }
    }
  }
}