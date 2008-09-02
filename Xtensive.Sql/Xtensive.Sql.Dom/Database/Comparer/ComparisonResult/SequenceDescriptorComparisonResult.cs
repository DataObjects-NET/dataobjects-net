// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.26

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class SequenceDescriptorComparisonResult : NodeComparisonResult,
    IComparisonResult<SequenceDescriptor>
  {
    private ComparisonResult<long?> startValue;
    private ComparisonResult<long?> increment;
    private ComparisonResult<long?> maxValue;
    private ComparisonResult<long?> minValue;
    private ComparisonResult<bool?> isCyclic;

    /// <inheritdoc/>
    public new SequenceDescriptor NewValue
    {
      get { return (SequenceDescriptor) base.NewValue; }
    }

    /// <inheritdoc/>
    public new SequenceDescriptor OriginalValue
    {
      get { return (SequenceDescriptor) base.OriginalValue; }
    }

    public ComparisonResult<long?> StartValue
    {
      get { return startValue; }
      internal set
      {
        this.EnsureNotLocked();
        startValue = value;
      }
    }

    public ComparisonResult<long?> Increment
    {
      get { return increment; }
      internal set
      {
        this.EnsureNotLocked();
        increment = value;
      }
    }

    public ComparisonResult<long?> MaxValue
    {
      get { return maxValue; }
      internal set
      {
        this.EnsureNotLocked();
        maxValue = value;
      }
    }

    public ComparisonResult<long?> MinValue
    {
      get { return minValue; }
      internal set
      {
        this.EnsureNotLocked();
        minValue = value;
      }
    }

    public ComparisonResult<bool?> IsCyclic
    {
      get { return isCyclic; }
      internal set
      {
        this.EnsureNotLocked();
        isCyclic = value;
      }
    }

    /// <inheritdoc/>
    public override IEnumerable<IComparisonResult> NestedComparisons
    {
      get { return base.NestedComparisons.Union(new IComparisonResult[] {startValue, increment, maxValue, minValue, isCyclic}.Where(comparisonResult => comparisonResult!=null)); }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        startValue.LockSafely(recursive);
        increment.LockSafely(recursive);
        maxValue.LockSafely(recursive);
        minValue.LockSafely(recursive);
        isCyclic.LockSafely(recursive);
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SequenceDescriptorComparisonResult(SequenceDescriptor originalValue, SequenceDescriptor newValue)
      : base(originalValue, newValue)
    {
    }
  }
}