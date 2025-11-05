// Copyright (C) 2013-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    private const string IndexElementName = "index";

    /// <inheritdoc />
    public override object Identifier => 
      new Tuple<string, string, string, string, string>(
        Database ?? string.Empty,
        Schema ?? string.Empty,
        Table ?? string.Empty,
        Column ?? string.Empty,
        Index ?? string.Empty);

    /// <summary>
    /// <see cref="IgnoreRule.Database" />
    /// </summary>
    [ConfigurationProperty(DatabaseElementName)]
    public string Database
    {
      get => (string) this[DatabaseElementName];
      set => this[DatabaseElementName] = value;
    }

    /// <summary>
    /// <see cref="IgnoreRule.Schema" />
    /// </summary>
    [ConfigurationProperty(SchemaElementName)]
    public string Schema
    {
      get => (string) this[SchemaElementName];
      set => this[SchemaElementName] = value;
    }

    /// <summary>
    /// <see cref="IgnoreRule.Table" />
    /// </summary>
    [ConfigurationProperty(TableElementName)]
    public string Table
    {
      get => (string) this[TableElementName];
      set => this[TableElementName] = value;
    }

    /// <summary>
    /// <see cref="IgnoreRule.Column"/>
    /// </summary>
    [ConfigurationProperty(ColumnElementName)]
    public string Column
    {
      get => (string) this[ColumnElementName];
      set => this[ColumnElementName] = value;
    }

    /// <summary>
    /// <see cref="IgnoreRule.Index"/>
    /// </summary>
    [ConfigurationProperty(IndexElementName)]
    public string Index
    {
      get => (string) this[IndexElementName];
      set => this[IndexElementName] = value;
    }

    /// <summary>
    /// Converts this instance to corresponding <see cref="IgnoreRule"/>.
    /// </summary>
    /// <returns>Result of conversion.</returns>
    public IgnoreRule ToNative() => new IgnoreRule(Database, Schema, Table, Column, Index);
  }
}
