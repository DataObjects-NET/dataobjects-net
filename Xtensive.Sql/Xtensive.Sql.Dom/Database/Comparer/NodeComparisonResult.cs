// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class NodeComparisonResult<T> : ComparisonResult<T>
  {
    private ComparisonResult<string> dbName;

    /// <summary>
    /// Gets comparison result of db name.
    /// </summary>
    public ComparisonResult<string> DbName
    {
      get { return dbName; }
      internal set
      {
        this.EnsureNotLocked();
        dbName = value;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive)
        dbName.LockSafely(recursive);
    }
  }
}