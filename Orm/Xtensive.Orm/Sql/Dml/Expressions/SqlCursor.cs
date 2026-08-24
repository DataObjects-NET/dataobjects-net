// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlCursor: SqlExpression
  {
    private readonly SqlColumnCollection columns = new SqlColumnCollection();
    private ISqlQueryExpression query;

    /// <summary>
    /// Gets the name of the cursor.
    /// </summary>
    /// <value>The name of the cursor.</value>
    public string Name { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="SqlCursor"/>
    /// makes a temporary copy of the data to be used.
    /// </summary>
    /// <value><see langword="true"/> if this <see cref="SqlCursor"/> makes a temporary
    /// copy of the data to be used; otherwise, <see langword="false"/>.</value>
    public bool Insensitive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether all fetch options are available for this cursor.
    /// </summary>
    /// <value><see langword="true"/> if all fetch options are available; otherwise, <see langword="false"/>.</value>
    public bool Scroll { get; set; }

    /// <summary>
    /// Gets or sets the select statement that defines the result set of the cursor.
    /// </summary>
    /// <value>The select statement that defines the result set of the cursor.</value>
    public ISqlQueryExpression Query
    {
      get => query;
      set => query = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Gets or sets a value indicating whether cursor is read only.
    /// </summary>
    /// <value><see langword="true"/> if cursor is read only; otherwise, <see langword="false"/>.</value>
    public bool ReadOnly { get; set; } = false;

    /// <summary>
    /// Gets updatable columns within the cursor.
    /// </summary>
    /// <value>Updatable columns within the cursor.</value>
    public SqlColumnCollection Columns => columns;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="SqlCursor"/> is holdable cursor.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance is a holdable cursor; otherwise, <see langword="false"/>.
    /// </value>
    public bool WithHold { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is result set cursor.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is result set cursor; otherwise, <see langword="false"/>.
    /// </value>
    public bool WithReturn { get; set; }

    public SqlDeclareCursor Declare()
    {
      return new SqlDeclareCursor(this);
    }

    public SqlOpenCursor Open()
    {
      return new SqlOpenCursor(this);
    }

    public SqlCloseCursor Close()
    {
      return new SqlCloseCursor(this);
    }

    public SqlFetch Fetch(SqlFetchOption option, SqlExpression rowCount, params ISqlCursorFetchTarget[] target)
    {
      if (rowCount is not null) {
        if (option != SqlFetchOption.Absolute && option != SqlFetchOption.Relative)
          throw new ArgumentException(Strings.ExInvalidUsageOfTheRowCountArgument, "rowCount");
        SqlValidator.EnsureIsArithmeticExpression(rowCount);
      }
      else if (option == SqlFetchOption.Absolute || option == SqlFetchOption.Relative)
        throw new ArgumentException(Strings.ExInvalidUsageOfTheOrientationArgument, "option");
      if (target != null)
        for (int i = 0, l = target.Length; i < l; i++)
          ArgumentValidator.EnsureArgumentNotNull(target[i], "target");
      return new SqlFetch(option, rowCount, this, target);
    }

    public SqlFetch Fetch(SqlFetchOption option, params ISqlCursorFetchTarget[] target)
    {
      return Fetch(option, null, target);
    }

    public SqlFetch Fetch(SqlFetchOption option)
    {
      return Fetch(option, null, (ISqlCursorFetchTarget[]) null);
    }

    public SqlFetch Fetch(SqlFetchOption option, SqlExpression rowCount)
    {
      return Fetch(option, rowCount, (ISqlCursorFetchTarget[]) null);
    }

    public SqlFetch Fetch(params ISqlCursorFetchTarget[] target)
    {
      return Fetch(SqlFetchOption.Next, null, target);
    }

    public SqlFetch Fetch()
    {
      return Fetch(SqlFetchOption.Next, null, (ISqlCursorFetchTarget[])null);
    }


    public override void ReplaceWith(SqlExpression expression)
    {
      throw new NotImplementedException();
    }

    internal override SqlCursor Clone(SqlNodeCloneContext context)
    {
      throw new NotImplementedException();
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCursor(string name, ISqlQueryExpression query)
      : base(SqlNodeType.Cursor)
    {
      Name = name;
      this.query = query;
    }
  }
}