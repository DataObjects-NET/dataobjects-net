// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Tuples.Internals
{
  ///<summary>
  /// Helper for tuple updating with the functional style syntax.
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
    /// Updates the field of <see cref="Tuple"/> with specified index.
    /// </summary>
    /// <param name="fieldIndex">The field index.</param>
    /// <param name="value">The field value</param>
    /// <returns><see langword="this" /></returns>
    public TupleUpdater UpdateField(int fieldIndex, object value)
    {
      tuple.SetValue(fieldIndex, value);
      return this;
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="tuple">The tuple to be wrapped.</param>
    public TupleUpdater(Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");
      this.tuple = tuple;
    }
  }
}