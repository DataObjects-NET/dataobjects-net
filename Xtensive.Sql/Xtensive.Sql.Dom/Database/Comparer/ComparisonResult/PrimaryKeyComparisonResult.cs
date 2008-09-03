// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class PrimaryKeyComparisonResult : UniqueConstraintComparisonResult,  
    IComparisonResult<PrimaryKey>
  {
    /// <inheritdoc/>
    public new PrimaryKey NewValue
    {
      get { return (PrimaryKey) base.NewValue; }
    }

    /// <inheritdoc/>
    public new PrimaryKey OriginalValue
    {
      get { return (PrimaryKey) base.OriginalValue; }
    }
  }
}