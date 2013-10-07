// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using System;
using Xtensive.Core;


namespace Xtensive.Tuples
{
  ///<summary>
  /// Helper type allowing to update tuples using functional style syntax.
  ///</summary>
  [Serializable]
  public struct TupleUpdater
  {
    private readonly Tuple tuple;

    /// <summary>
    /// Gets the wrapped tuple.
    /// </summary>
    /// <value>The tuple.</value>
    public Tuple Tuple { get { return tuple; } }

    /// <summary>
    /// Updates the field of <see cref="Tuple"/> with the specified index.
    /// </summary>
    /// <param name="fieldIndex">The field index.</param>
    /// <param name="value">The new field value</param>
    /// <returns><see langword="this" /></returns>
    public TupleUpdater SetValue(int fieldIndex, object value)
    {
      tuple.SetValue(fieldIndex, value);
      return this;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tuple">The tuple to wrap.</param>
    public TupleUpdater(Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");
      this.tuple = tuple;
    }
  }
}