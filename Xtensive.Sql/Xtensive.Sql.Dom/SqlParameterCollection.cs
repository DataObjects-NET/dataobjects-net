// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Sql.Common;

namespace Xtensive.Sql.Dom
{
  [Serializable]
  public sealed class SqlParameterCollection : ParameterCollection<SqlParameter>
  {
    /// <inheritdoc/>
    public override int Add(object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<SqlParameter>(value, "value");
      return base.Add(value);
    }

    /// <inheritdoc/>
    public override void AddRange(Array values)
    {
      ArgumentValidator.EnsureArgumentNotNull(values, "values");
      foreach (object value in values) {
        ArgumentValidator.EnsureArgumentIs<SqlParameter>(value, "value");
        items.Add(value as SqlParameter);
      }
    }

    /// <inheritdoc/>
    public override int IndexOf(object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<SqlParameter>(value, "value");
      return items.IndexOf(value as SqlParameter);
    }

    /// <inheritdoc/>
    public override void Insert(int index, object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<SqlParameter>(value, "value");
      items.Insert(index, value as SqlParameter);
    }

    /// <inheritdoc/>
    public override void Remove(object value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<SqlParameter>(value, "value");
      items.Remove(value as SqlParameter);
    }

    /// <inheritdoc/>
    public new SqlParameter this[int index]
    {
      get { return (SqlParameter)GetParameter(index); }
      set { SetParameter(index, value); }
    }

    /// <summary>
    /// Gets and sets the <see cref="SqlParameter"/> with the specified name.
    /// </summary>
    public new SqlParameter this[string parameterName]
    {
      get { return (SqlParameter)GetParameter(parameterName); }
      set { SetParameter(parameterName, value); }
    }

    /// <inheritdoc/>
    protected override void SetParameter(int index, DbParameter value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<SqlParameter>(value, "value");
      items[index] = value as SqlParameter;
    }

    /// <inheritdoc/>
    protected override void SetParameter(string parameterName, DbParameter value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentIs<SqlParameter>(value, "value");
      items[IndexOf(parameterName)] = value as SqlParameter;
    }
  }
}