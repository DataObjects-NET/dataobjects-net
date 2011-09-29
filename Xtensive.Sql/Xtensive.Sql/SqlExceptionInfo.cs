// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.29

using System.Text;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Sql.Resources;

namespace Xtensive.Sql
{
  /// <summary>
  /// Information about exception occured while executing SQL query.
  /// </summary>
  public class SqlExceptionInfo : LockableBase
  {
    private SqlExceptionType type;
    private string database;
    private string table;
    private string column;
    private string value;
    private string constraint;

    /// <summary>
    /// Type of error.
    /// </summary>
    public SqlExceptionType Type {
      get { return type; }
      set { this.EnsureNotLocked(); type = value; }
    }

    /// <summary>
    /// Database in which error occured (if any).
    /// </summary>
    public string Database {
      get { return database; }
      set { this.EnsureNotLocked(); database = value; }
    }

    /// <summary>
    /// Table in which error occured (if any).
    /// </summary>
    public string Table {
      get { return table; }
      set { this.EnsureNotLocked(); table = value; }
    }

    /// <summary>
    /// Column in which error occured (if any).
    /// </summary>
    public string Column {
      get { return column; }
      set { this.EnsureNotLocked(); column = value; }
    }

    /// <summary>
    /// Value that caused error (if any).
    /// </summary>
    public string Value {
      get { return value; }
      set { this.EnsureNotLocked(); this.value = value; }
    }

    /// <summary>
    /// Name of the constraint or unique index that caused error (if any).
    /// </summary>
    public string Constraint {
      get { return constraint; }
      set { this.EnsureNotLocked(); constraint = value; }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      var builder = new StringBuilder();
      builder.AppendFormat(Strings.TypeX, type);
      if (!string.IsNullOrEmpty(database))
        builder.AppendFormat(Strings.DatabaseX, database);
      if (!string.IsNullOrEmpty(table))
        builder.AppendFormat(Strings.TableX, table);
      if (!string.IsNullOrEmpty(column))
        builder.AppendFormat(Strings.ColumnX, column);
      if (!string.IsNullOrEmpty(value))
        builder.AppendFormat(Strings.ValueX, value);
      if (!string.IsNullOrEmpty(constraint))
        builder.AppendFormat(Strings.ConstraintX, constraint);
      return builder.ToString();
    }

    /// <summary>
    /// Creates locked <see cref="SqlExceptionInfo"/> with only <see cref="Type"/> specified.
    /// </summary>
    /// <param name="type">Type of exception.</param>
    /// <returns>Created <see cref="SqlExceptionInfo"/>.</returns>
    public static SqlExceptionInfo Create(SqlExceptionType type)
    {
      var result = new SqlExceptionInfo {Type = type};
      result.Lock();
      return result;
    }
  }
}