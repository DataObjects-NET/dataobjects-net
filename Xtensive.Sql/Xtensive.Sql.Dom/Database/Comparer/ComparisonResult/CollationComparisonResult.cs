// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.29

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class CollationComparisonResult : NodeComparisonResult,
    IComparisonResult<Collation>
  {
    public new Collation NewValue
    {
      get { return (Collation) base.NewValue; }
    }

    public new Collation OriginalValue
    {
      get { return (Collation) base.OriginalValue; }
    }
  }
}