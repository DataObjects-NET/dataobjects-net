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
  public class CharacterSetComparisonResult : NodeComparisonResult,
    IComparisonResult<CharacterSet>
  {
    /// <inheritdoc/>
    public new CharacterSet NewValue
    {
      get { return (CharacterSet) base.NewValue; }
    }

    /// <inheritdoc/>
    public new CharacterSet OriginalValue
    {
      get { return (CharacterSet) base.OriginalValue; }
    }
  }
}