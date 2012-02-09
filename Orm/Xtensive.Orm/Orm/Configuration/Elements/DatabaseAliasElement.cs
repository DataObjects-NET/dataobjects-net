// Copyright (C) 2012 Xtensive LLC.
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
  public class DatabaseAliasElement : ConfigurationCollectionElementBase
  {
    private const string NameElementName = "name";
    private const string DatabaseElementName = "database";

    /// <inheritdoc/>
    public override object Identifier { get { return Name; } }

    /// <summary>
    /// <see cref="DatabaseAlias.Name" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NameElementName, IsKey = true)]
    public string Name
    {
      get { return (string) this[NameElementName]; }
      set { this[NameElementName] = value; }
    }

    /// <summary>
    /// <see cref="DatabaseAlias.Database" copy="true"/>
    /// </summary>
    [ConfigurationProperty(DatabaseElementName)]
    public string Database
    {
      get { return (string) this[DatabaseElementName]; }
      set { this[DatabaseElementName] = value; }
    }

    /// <summary>
    /// Converts this instance to corresponding <see cref="DatabaseAlias"/>.
    /// </summary>
    /// <returns>Result of conversion.</returns>
    public DatabaseAlias ToNative()
    {
      return new DatabaseAlias(Name, Database);
    }
  }
}