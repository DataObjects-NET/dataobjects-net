﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.08

using System.Configuration;
using Xtensive.Configuration;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// Database alias element within a configuration file.
  /// </summary>
  public class DatabaseConfigurationElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string AliasElementName = "alias";

    /// <inheritdoc/>
    public override object Identifier { get { return Name; } }

    /// <summary>
    /// <see cref="DatabaseConfiguration.Name" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true)]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="DatabaseConfiguration.Alias" copy="true"/>
    /// </summary>
    [ConfigurationProperty(AliasElementName)]
    public string Alias
    {
      get { return (string) this[AliasElementName]; }
      set { this[AliasElementName] = value; }
    }

    /// <summary>
    /// Converts this instance to corresponding <see cref="DatabaseConfiguration"/>.
    /// </summary>
    /// <returns>Result of conversion.</returns>
    public DatabaseConfiguration ToNative()
    {
      return new DatabaseConfiguration(Name) {Alias = Alias};
    }
  }
}