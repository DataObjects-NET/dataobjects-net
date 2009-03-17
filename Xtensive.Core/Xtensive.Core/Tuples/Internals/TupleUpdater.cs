// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2007.05.30

namespace Xtensive.Core.Tuples.Internals
{
  ///<summary>
  /// Helps update tuple using the functional style syntax.
  ///</summary>
  public struct TupleUpdater
  {
    private readonly Tuple tuple;

    ///<summary>
    /// Wrapped tuple.
    ///</summary>
    public Tuple Tuple { get { return tuple; } }

    /// <summary>
    /// Updates the field of <see cref="Tuple"/> with specified index.
    /// </summary>
    /// <param name="fieldIndex"></param>
    /// <param name="value"></param>
    /// <returns>Wrapped tuple.</returns>
    public Tuple UpdateField(int fieldIndex, object value)
    {
      tuple.SetValue(fieldIndex, value);
      return tuple;
    }

    ///<summary>
    /// Constructor.
    ///</summary>
    ///<param name="tuple">The <see cref="Tuple"/> to be wrapped.</param>
    public TupleUpdater(Tuple tuple)
    {
      ArgumentValidator.EnsureArgumentNotNull(tuple, "tuple");
      this.tuple = tuple;
    }
  }
}
