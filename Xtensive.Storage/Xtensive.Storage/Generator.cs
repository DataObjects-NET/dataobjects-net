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
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  ///<summary>
  /// Default key generator.
  ///</summary>
  public abstract class Generator : HandlerBase
  {
    /// <summary>
    /// Gets the hierarchy this instance serves.
    /// </summary>
    public HierarchyInfo Hierarchy { get; internal set; }

    /// <summary>
    /// Create the <see cref="Tuple"/> with the unique values in key sequence.
    ///  </summary>
    public Tuple Next()
    {
      Type fieldType = Hierarchy.KeyTupleDescriptor[0];
      if (fieldType != typeof(Guid)) 
        return NextNumber();
      return Tuple.Create(Hierarchy.KeyTupleDescriptor, Guid.NewGuid());
    }

    /// <summary>
    /// Create the <see cref="Tuple"/> with the unique values in key sequence where key is a number.
    ///  </summary>
    protected abstract Tuple NextNumber();

    /// <summary>
    /// Create an <see cref="Array"/> of <see cref="Tuple"/>s with the unique values in key sequence.
    ///  </summary>
    /// <param name="count">The number of <see cref="Tuple"/> instances to retrieve.</param>
    /// <returns>An <see cref="Array"/> of <see cref="Tuple"/>s with unique values in key sequence.</returns>
    public virtual Tuple[] Next(int count)
    {
      ArgumentValidator.EnsureArgumentIsInRange(count, 1, Int32.MaxValue, "count");
      var result = new Tuple[count];
      for (int i = 0; i < count; i++)
        result[i] = Next();
      return result;
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <remarks>
    /// Should call <see langword="base"/> when overrided.
    /// </remarks>
    public virtual void Initialize()
    {
    }
  }
}