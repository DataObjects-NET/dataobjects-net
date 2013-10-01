// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.16

using System;
using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// Database ignore element within a configuration file.
  /// </summary>
  public class IgnoreRuleElement : ConfigurationCollectionElementBase
  {
    private const string DatabaseElementName = "database";
    private const string SchemaElementName = "schema";
    private const string TableElementName = "table";
    private const string ColumnElementName = "column";

    /// <inheritdoc />
    public override object Identifier
    {
      get { return new Tuple<string, string, string, string>(Database ?? string.Empty, Schema ?? string.Empty, Table ?? string.Empty, Column ?? string.Empty); }
    }

    /// <summary>
    /// <see cref="IgnoreRule.Database" copy="true"/>
    /// </summary>
    [ConfigurationProperty(DatabaseElementName)]
    public string Database
    {
      get { return (string)this[DatabaseElementName]; }
      set { this[DatabaseElementName] = value; }
    }

    /// <summary>
    /// <see cref="IgnoreRule.Schema" copy="true"/>
    /// </summary>
    [ConfigurationProperty(SchemaElementName)]
    public string Schema
    {
      get { return (string) this[SchemaElementName]; }
      set { this[SchemaElementName] = value; }
    }

    /// <summary>
    /// <see cref="IgnoreRule.Table" copy="true"/>
    /// </summary>
    [ConfigurationProperty(TableElementName)]
    public string Table
    {
      get { return (string) this[TableElementName]; }
      set { this[TableElementName] = value; }
    }

    /// <summary>
    /// <see cref="IgnoreRule.Column"/>
    /// </summary>
    [ConfigurationProperty(ColumnElementName)]
    public string Column
    {
      get { return (string)this[ColumnElementName]; }
      set { this[ColumnElementName] = value; }
    }

    /// <summary>
    /// Converts this instance to corresponding <see cref="IgnoreRule"/>.
    /// </summary>
    /// <returns>Result of conversion.</returns>
    public IgnoreRule ToNative()
    {
      return new IgnoreRule(Database, Schema, Table, Column);
    }
  }
}
