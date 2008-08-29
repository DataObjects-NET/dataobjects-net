// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.29

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  public class TranslationComparisonResult : NodeComparisonResult,
    IComparisonResult<Translation>
  {
    public Translation NewValue
    {
      get { return (Translation) base.NewValue; }
    }

    public Translation OriginalValue
    {
      get { return (Translation) base.OriginalValue; }
    }

    public TranslationComparisonResult(Translation originalValue, Translation newValue)
      : base(originalValue, newValue)
    {
    }
  }
}