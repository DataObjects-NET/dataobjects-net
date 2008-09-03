// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Collections;
using Xtensive.Sql.Dom.Resources;

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

    public override System.Collections.Generic.IEnumerable<IComparisonResult> NestedComparisons
    {
      get
      {
        return base.NestedComparisons
          .AddOne(dbName);
      }
    }

    public override void Initialize(Node originalNode, Node newNode)
    {
      base.Initialize(originalNode, newNode);
      if (ReferenceEquals(originalNode, null) && ReferenceEquals(newNode, null))
        return;
      string originalName = ReferenceEquals(originalNode, null) ? null : originalNode.DbName;
      string newName = ReferenceEquals(newNode, null) ? null : newNode.DbName;
      bool hasChanges = false;
      dbName = NodeComparerBase<string>.CompareSimpleNode(originalName, newName, ref hasChanges);
      if (originalNode == null)
        ResultType = ComparisonResultType.Added;
      else if (newNode == null)
        ResultType = ComparisonResultType.Removed;
      else
        ResultType = hasChanges ? ComparisonResultType.Modified : ComparisonResultType.Unchanged;
    }
  }
}