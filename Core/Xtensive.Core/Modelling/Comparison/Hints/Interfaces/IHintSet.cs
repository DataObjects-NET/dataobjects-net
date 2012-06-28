// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.26

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Collections;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// <see cref="Hint"/> collection contract.
  /// </summary>
  public interface IHintSet : IEnumerable<Hint>, ILockable
  {
    /// <summary>
    /// Gets the count of contained hints.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets or sets the source model.
    /// </summary>
    IModel SourceModel { get; }

    /// <summary>
    /// Gets or sets the target model.
    /// </summary>
    IModel TargetModel { get; }

    /// <summary>
    /// Adds the specified hint to the collection.
    /// </summary>
    void Add(Hint hint);

    /// <summary>
    /// Clears the collection.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the hint of type <typeparamref name="THint"/> for the specified node.
    /// </summary>
    /// <typeparam name="THint">The type of the hint.</typeparam>
    /// <param name="node">The node hint is applicable to.</param>
    /// <returns>
    /// Found hint, if any;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    THint GetHint<THint>(Node node)
      where THint : Hint;

    /// <summary>
    /// Gets hints of type <typeparamref name="THint"/> for the specified node.
    /// </summary>
    /// <typeparam name="THint">The type of the hint.</typeparam>
    /// <param name="node">The node hints are applicable to.</param>
    /// <returns>
    /// Found hints, if any;
    /// otherwise, an empty array.
    /// </returns>
    THint[] GetHints<THint>(Node node)
      where THint : Hint;
  }
}