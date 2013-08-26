// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.16

using System.Text;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Ignore rules for presistent types
  /// </summary>
  public sealed class IgnoreRule : LockableBase
  {
    private string database;
    private string schema;
    private string table;
    private string column;


    /// <summary>
    /// Gets database that is assigned to ignored type when this rule is applied
    /// If this property is set to null or empty value <see cref="DomainConfiguration.DefaultDatabase"/>
    /// is used instead.
    /// </summary>
    public string Database
    {
      get { return database; }
      set
      {
        this.EnsureNotLocked();
        database = value;
      }
    }

    /// <summary>
    /// Get schema that is assigned to ignored type when this rule is applied
    /// If this property is set to null or empty value <see cref="DomainConfiguration.DefaultSchema"/>
    /// is used instead.
    /// </summary>
    public string Schema
    {
      get { return schema; }
      set
      {
        this.EnsureNotLocked();
        schema = value;
      }
    }


    /// <summary>
    /// Gets table condition.
    /// When type is declared in the specified table, this rule is applied.
    /// If this property is set to null value, any table matches this rule.
    /// </summary>
    public string Table
    {
      get { return table; }
      set
      {
        this.EnsureNotLocked();
        table = value;
      }
    }

    /// <summary>
    /// Gets column condition
    /// When type is declared in the specified column, this rule is applied.
    /// If this property is set to null value, any culumn matches this rule.
    /// </summary>
    public string Column
    {
      get { return column; }
      set
      {
        this.EnsureNotLocked();
        column = value;
      }
    }

    /// <inheritdoc />
    public override string ToString()
    {
      var separator = ", ";
      StringBuilder sb = new StringBuilder();
      
      var databasePart = !string.IsNullOrEmpty(database) ? string.Format("Database = {0}",database) : "<default database>";
      var schemaPart = !string.IsNullOrEmpty(schema) ? string.Format("Schema = {0}", schema) : "<default schema>";
      var tablePart = !string.IsNullOrEmpty(table) ? string.Format("Table = {0}", table) : "<any table>";
      var columnPart = !string.IsNullOrEmpty(column) ? string.Format("Column = {0}", column) : "<any column>";
      sb.Append(databasePart)
        .Append(separator)
        .Append(schemaPart)
        .Append(separator)
        .Append(tablePart)
        .Append(separator)
        .Append(columnPart);

      return sb.ToString();
    }

    /// <summary>
    /// Create clone of this instance
    /// </summary>
    /// <returns>Cloned instance</returns>
    public IgnoreRule Clone()
    {
      return new IgnoreRule(database, schema, table, column);
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
    public IgnoreRule (string database, string schema, string table, string column)
    {
      Database = database;
      Schema = schema;
      Table = table;
      Column = column;
    }
  }
}
