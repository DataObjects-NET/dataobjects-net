// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.21

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Configuration.TypeRegistry
{
  /// <summary>
  /// Represents a context for <see cref="Registry"/>.
  /// </summary>
  [Serializable]
  internal class Context
    : ICloneable,
      ICountable<Type>
  {
    private readonly Set<Type> index;
    private readonly List<Type> list;

    /// <summary>
    /// Determines whether this instance contains the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>
    ///   <see langword="true"/> if this instance contains the specified type; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Type type)
    {
      return index.Contains(type);
    }

    /// <summary>
    /// Registers the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    public void Register(Type type)
    {
      if (!index.Contains(type)) {
        list.Add(type);
        index.Add(type);
      }
    }

    #region ICloneable Members

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns></returns>
    public object Clone()
    {
      return new Context(this);
    }

    #endregion

    #region ICountable<Type> Members

    /// <summary>
    /// Gets the number of elements contained in a collection.
    /// </summary>
    public long Count
    {
      get { return list.Count; }
    }

    ///<summary>
    ///Returns an enumerator that iterates through the collection.
    ///</summary>
    ///
    ///<returns>
    ///A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
    ///</returns>
    public IEnumerator<Type> GetEnumerator()
    {
      return list.GetEnumerator();
    }

    ///<summary>
    ///Returns an enumerator that iterates through a collection.
    ///</summary>
    ///
    ///<returns>
    ///An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    ///</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<Type>)this).GetEnumerator();
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    public Context()
    {
      index = new Set<Type>();
      list = new List<Type>();
    }

    private Context(Context context)
    {
      index = new Set<Type>(context.index);
      list = new List<Type>(context.list);
    }
  }
}