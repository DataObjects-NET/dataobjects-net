// Copyright (C) 2013-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.08.16

using System;
using System.Text;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Ignore rule for items in database (tables, columns, indexes).
  /// </summary>
  public sealed class IgnoreRule : LockableBase
  {
    private string database;
    private string schema;
    private string table;
    private string column;
    private string index;

    /// <summary>
    /// Gets database that the rule should be applied to.
    /// If this property is set to null or empty value <see cref="DomainConfiguration.DefaultDatabase"/>
    /// is used instead.
    /// </summary>
    public string Database
    {
      get => database;
      set {
        EnsureNotLocked();
        database = value;
      }
    }

    /// <summary>
    /// Get schema that the rule should be applied to.
    /// If this property is set to null or empty value <see cref="DomainConfiguration.DefaultSchema"/>
    /// is used instead.
    /// </summary>
    public string Schema
    {
      get => schema;
      set {
        EnsureNotLocked();
        schema = value;
      }
    }


    /// <summary>
    /// Gets table condition (exact name, prefix or suffix by using asterisk).
    /// If this property is set to null value, any table matches this rule.
    /// </summary>
    public string Table
    {
      get => table;
      set {
        EnsureNotLocked();
        table = value;
      }
    }

    /// <summary>
    /// Gets column condition (exact name, prefix or suffix by using asterisk).
    /// If this property is set to null value, any culumn matches this rule.
    /// </summary>
    /// <remarks>Either <see cref="Column"/> or <see cref="Index"/> can be declared.</remarks>
    public string Column
    {
      get => column;
      set {
        EnsureNotLocked();
        SetColumnWithCheck(value);
      }
    }

    /// <summary>
    /// Gets index condition (exact name, prefix or suffix by using asterisk).
    /// If this property is set to null value, any index matches this rule.
    /// </summary>
    /// <remarks>Either <see cref="Index"/> or <see cref="Column"/> can be declared.</remarks>
    public string Index
    {
      get => index;
      set {
        EnsureNotLocked();
        SetIndexWithCheck(value);
      }
    }

    /// <inheritdoc />
    public override string ToString()
    {
      var separator = ", ";

      var databasePart = !string.IsNullOrEmpty(database) ? $"Database = {database}" : "<default database>";
      var schemaPart = !string.IsNullOrEmpty(schema) ? $"Schema = {schema}" : "<default schema>";
      var tablePart = !string.IsNullOrEmpty(table) ? $"Table = {table}" : "<any table>";

      var columnOrIndexPart = !string.IsNullOrEmpty(column)
        ? $"Column = {column}"
        : (!string.IsNullOrEmpty(index))
          ? $"Index = {index}"
          : "<any column> OR <any index>";

      return new StringBuilder(databasePart)
        .Append(separator)
        .Append(schemaPart)
        .Append(separator)
        .Append(tablePart)
        .Append(separator)
        .Append(columnOrIndexPart)
        .ToString();
    }

    /// <summary>
    /// Create clone of this instance
    /// </summary>
    /// <returns>Cloned instance</returns>
    public IgnoreRule Clone()
    {
      return new IgnoreRule(database, schema, table, column, index);
    }

    private void SetColumnWithCheck(in string columnToSet)
    {
      if (!string.IsNullOrEmpty(columnToSet) && !string.IsNullOrEmpty(index)) {
        throw new InvalidOperationException(string.Format(Strings.ExIgnoreRuleIsAlreadyConfiguredForX, nameof(Index)));
      }
      column = columnToSet;
    }

    private void SetIndexWithCheck(in string indexToSet)
    {
      if (!string.IsNullOrEmpty(indexToSet) && !string.IsNullOrEmpty(column)) {
        throw new InvalidOperationException(string.Format(Strings.ExIgnoreRuleIsAlreadyConfiguredForX, nameof(Column)));
      }
      index = indexToSet;
    }

    //Constructors

    /// <summary>
    /// Create new instance of this class
    /// </summary>
    public IgnoreRule()
    {
    }

    /// <summary>
    /// Create new instance of this class
    /// </summary>
    /// <param name="database">Value for <see cref="Database"/></param>
    /// <param name="schema">Value for <see cref="Schema"/></param>
    /// <param name="table">Value for <see cref="Table"/></param>
    /// <param name="column">Value for <see cref="Column"/></param>
    /// <param name="index">Value for <see cref="Index"/></param>
    internal IgnoreRule(in string database, in string schema, in string table, in string column, in string index)
    {
      Database = database;
      Schema = schema;
      Table = table;
      Column = column;
      Index = index;
    }
  }
}
