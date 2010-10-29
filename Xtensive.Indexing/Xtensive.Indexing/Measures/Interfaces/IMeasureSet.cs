// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.13

using Xtensive.Collections;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// A set of measures that serves the collection of <typeparamref name="TItem"/>s.
  /// </summary>
  /// <typeparam name="TItem">The type of collection item.</typeparam>
  public interface IMeasureSet<TItem> : IConfigurationSet<IMeasure<TItem>>
  {
    /// <summary>
    /// Gets measure by name.
    /// </summary>
    /// <typeparam name="TMeasure">Measure type.</typeparam>
    /// <param name="name">Measure name.</param>
    /// <returns><typeparamref name="TMeasure"/> measure.</returns>
    TMeasure GetItem<TMeasure>(string name) where TMeasure : IMeasure<TItem>;
  }
}