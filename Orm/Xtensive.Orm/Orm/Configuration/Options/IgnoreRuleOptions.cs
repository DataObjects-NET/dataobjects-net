// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class IgnoreRuleOptions : IIdentifyableOptions,
    IHasDatabaseOption,
    IValidatableOptions,
    IToNativeConvertable<IgnoreRule>
  {
    /// <inheritdoc/>
    public object Identifier =>
      (Database ?? string.Empty,
       Schema ?? string.Empty,
       Table ?? string.Empty,
       Column ?? string.Empty,
       Index ?? string.Empty);

    /// <summary>
    /// Database part of the rule
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// Schema part of the rule.
    /// </summary>
    public string Schema { get; set; }

    /// <summary>
    /// Table part of the rule.
    /// </summary>
    public string Table { get; set; }

    /// <summary>
    /// Column part of the rule.
    /// </summary>
    public string Column { get; set; }

    /// <summary>
    /// Index part of the rule.
    /// </summary>
    public string Index { get; set; }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">Configuration of properties is not valid, e.g.
    /// none of Table, Column and Index filled, or both Column and Index filled.</exception>
    public void Validate()
    {
      if (Table.IsNullOrEmpty() && Column.IsNullOrEmpty() && Index.IsNullOrEmpty()) {
        throw new ArgumentException("Ignore rule should be configured for at least column, index or table");
      }
      if (!Column.IsNullOrEmpty() && !Index.IsNullOrEmpty()) {
        throw new ArgumentException("Ignore rule can't be configured for column and index at the same time");
      }
    }

    public object GetMappedIdentifier(IDictionary<string, DatabaseOptions> databaseMap)
    {
      if (!Database.IsNullOrEmpty() && databaseMap.TryGetValue(Database, out var map) && !map.RealName.IsNullOrEmpty()) {
        return (map.RealName, Schema ?? string.Empty, Table ?? string.Empty, Column ?? string.Empty, Index ?? string.Empty);
      }
      return Identifier;
    }

    /// <inheritdoc />
    public IgnoreRule ToNative()
    {
      Validate();

      return new IgnoreRule(Database, Schema, Table, Column, Index);
    }

    
  }
}
