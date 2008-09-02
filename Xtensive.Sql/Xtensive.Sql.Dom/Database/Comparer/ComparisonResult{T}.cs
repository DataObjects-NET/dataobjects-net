// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Base class for comparison results with original and new values inside.
  /// </summary>
  public class ComparisonResult<T> : ComparisonResult, 
    IComparisonResult<T>
  {
    private readonly T originalValue;
    private readonly T newValue;

    /// <summary>
    /// Gets new value.
    /// </summary>
    public T NewValue
    {
      get { return newValue; }
    }

    /// <summary>
    /// Gets original value.
    /// </summary>
    public T OriginalValue
    {
      get { return originalValue; }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        (originalValue as ILockable).LockSafely(recursive);
        (newValue as ILockable).LockSafely(recursive);
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ComparisonResult(T originalValue, T newValue)
      : base()
    {
      this.originalValue = originalValue;
      this.newValue = newValue;
    }

    /// <inheritdoc/>
    public override IEnumerable<IComparisonResult> NestedComparisons
    {
      get { return Enumerable.Empty<IComparisonResult>(); }
    }
  }
}