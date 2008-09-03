// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Helpers;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class SequenceComparisonResult : NodeComparisonResult,
    IComparisonResult<Sequence>
  {
    private ComparisonResult<SqlValueType> dataType;
    private SequenceDescriptorComparisonResult sequenceDescriptor;

    /// <inheritdoc/>
    public new Sequence NewValue
    {
      get { return (Sequence) base.NewValue; }
    }

    /// <inheritdoc/>
    public new Sequence OriginalValue
    {
      get { return (Sequence) base.OriginalValue; }
    }

    public ComparisonResult<SqlValueType> DataType
    {
      get { return dataType; }
      set
      {
        this.EnsureNotLocked();
        dataType = value;
      }
    }

    public SequenceDescriptorComparisonResult SequenceDescriptor
    {
      get { return sequenceDescriptor; }
      set
      {
        this.EnsureNotLocked();
        sequenceDescriptor = value;
      }
    }

    /// <inheritdoc/>
    public override IEnumerable<IComparisonResult> NestedComparisons
    {
      get
      {
        return base.NestedComparisons
          .AddOne(dataType)
          .AddOne(sequenceDescriptor);
      }
    }

    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        sequenceDescriptor.LockSafely(recursive);
        dataType.LockSafely(recursive);
      }
    }
  }
}