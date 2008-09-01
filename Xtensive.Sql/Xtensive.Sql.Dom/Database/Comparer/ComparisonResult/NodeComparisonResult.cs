// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class NodeComparisonResult : ComparisonResult<Node>
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public NodeComparisonResult(Node originalValue, Node newValue)
      : base(originalValue, newValue)
    {
      if (ReferenceEquals(originalValue, null) && ReferenceEquals(newValue, null))
        return;
      string originalName = ReferenceEquals(originalValue, null) ? null : originalValue.DbName;
      string newName = ReferenceEquals(newValue, null) ? null : newValue.DbName;
      bool hasChanges = false;
      dbName = NodeComparerBase<string>.CompareSimpleNode(originalName, newName, ref hasChanges);
      if (originalValue==null)
        ResultType = ComparisonResultType.Added;
      else if (newValue==null)
        ResultType = ComparisonResultType.Removed;
      else
        ResultType = hasChanges ? ComparisonResultType.Modified : ComparisonResultType.Unchanged;
    }
  }
}