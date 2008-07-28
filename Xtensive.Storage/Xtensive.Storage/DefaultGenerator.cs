// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage
{
  ///<summary>
  /// Default key generator.
  ///</summary>
  public abstract class DefaultGenerator : HandlerBase
  {
    /// <summary>
    /// Gets the hierarchy this instance serves.
    /// </summary>
    public HierarchyInfo Hierarchy { get; internal set; }

    /// <summary>
    /// Create the <see cref="Tuple"/> with the unique values in key sequence.
    ///  </summary>
    public abstract Tuple Next();

    /// <summary>
    /// Create an <see cref="Array"/> of <see cref="Tuple"/>s with the unique values in key sequence.
    ///  </summary>
    /// <param name="count">The number of <see cref="Tuple"/> instances to retrieve.</param>
    /// <returns>An <see cref="Array"/> of <see cref="Tuple"/>s with unique values in key sequence.</returns>
    public virtual Tuple[] Next(int count)
    {
      ArgumentValidator.EnsureArgumentIsInRange(count, 1, Int32.MaxValue, "count");
      Tuple[] result = new Tuple[count];
      for (int i = 0; i < count; i++)
        result[i] = Next();
      return result;
    }

    // TODO: AY: Fix immediately
    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public virtual void Initialize()
    {
    }
  }
}