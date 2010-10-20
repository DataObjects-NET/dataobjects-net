// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.14

using System;
using Xtensive.Configuration;
using Xtensive.Core;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// A configurable set of measures for a collection of <typeparamref name="TItem"/>s.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  [Serializable]
  public class MeasureSet<TItem> : ConfigurationSetBase<IMeasure<TItem>>, IMeasureSet<TItem>
  {
    /// <inheritdoc/>
    public TMeasure GetItem<TMeasure>(string name) where TMeasure : IMeasure<TItem>
    {
      ArgumentValidator.EnsureArgumentNotNull(name, "name");
      foreach (IMeasure<TItem> item in this) {
        TMeasure measure = (TMeasure)item;
        if (measure!=null && measure.Name==name)
          return measure;
      }
      return default(TMeasure);
    }

    /// <inheritdoc/>
    protected override string GetItemName(IMeasure<TItem> measure)
    {
      return measure.Name;
    }

    #region Clone implementation

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new MeasureSet<TItem>();
    }

    /// <inheritdoc/>
    protected override void CopyFrom(ConfigurationBase source)
    {
      base.CopyFrom(source);
      Clear();
      var set = (MeasureSet<TItem>)source;
      foreach (IMeasure<TItem> measure in set)
        Add(measure);
    }

    #endregion
  }
}