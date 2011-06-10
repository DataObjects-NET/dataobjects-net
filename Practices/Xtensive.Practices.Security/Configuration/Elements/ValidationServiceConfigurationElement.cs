// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.07

using System;
using System.Configuration;

namespace Xtensive.Practices.Security.Configuration
{
  public class ValidationServiceConfigurationElement : ConfigurationElement
  {
    private const string NameElementName = "name";

   /// <summary>
    /// Gets or sets the short name of the encryption service type.
    /// </summary>
    [ConfigurationProperty(NameElementName, IsRequired = false)]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }
  }
}