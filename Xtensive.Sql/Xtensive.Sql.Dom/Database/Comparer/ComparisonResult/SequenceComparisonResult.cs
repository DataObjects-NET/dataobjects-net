// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.21

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class SequenceComparisonResult : NodeComparisonResult,
    IComparisonResult<Sequence>
  {
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SequenceComparisonResult(Sequence originalValue, Sequence newValue)
      : base(originalValue, newValue)
    {
    }
  }
}