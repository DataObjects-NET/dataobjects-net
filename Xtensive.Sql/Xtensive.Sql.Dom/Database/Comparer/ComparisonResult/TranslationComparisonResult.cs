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
  public class TranslationComparisonResult : NodeComparisonResult,
    IComparisonResult<Translation>
  {
    /// <inheritdoc/>
    public new Translation NewValue
    {
      get { return (Translation) base.NewValue; }
    }

    /// <inheritdoc/>
    public new Translation OriginalValue
    {
      get { return (Translation) base.OriginalValue; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public TranslationComparisonResult(Translation originalValue, Translation newValue)
      : base(originalValue, newValue)
    {
    }
  }
}