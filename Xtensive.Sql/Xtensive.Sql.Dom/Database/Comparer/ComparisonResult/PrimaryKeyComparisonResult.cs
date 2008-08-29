// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class PrimaryKeyComparisonResult : UniqueConstraintComparisonResult,  
    IComparisonResult<PrimaryKey>
  {
    public PrimaryKey NewValue
    {
      get { return (PrimaryKey) base.NewValue; }
    }

    public PrimaryKey OriginalValue
    {
      get { return (PrimaryKey) base.OriginalValue; }
    }

    public PrimaryKeyComparisonResult(PrimaryKey originalValue, PrimaryKey newValue)
      : base(originalValue, newValue)
    {
    }
  }
}