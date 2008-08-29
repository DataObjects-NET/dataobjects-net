// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.29

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class CharacterSetComparisonResult : NodeComparisonResult,
    IComparisonResult<CharacterSet>
  {
    /// <inheritdoc/>
    public CharacterSet NewValue
    {
      get { return (CharacterSet) base.NewValue; }
    }

    /// <inheritdoc/>
    public CharacterSet OriginalValue
    {
      get { return (CharacterSet) base.OriginalValue; }
    }

    public CharacterSetComparisonResult(CharacterSet originalValue, CharacterSet newValue)
      : base(originalValue, newValue)
    {
    }
  }
}