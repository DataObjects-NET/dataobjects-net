// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.24

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Collection of <see cref="RecordColumn"/> items.
  /// </summary>
  [Serializable]
  public class RecordColumnCollection : ReadOnlyList<RecordColumn>
  {    
    private readonly Dictionary<string, int> nameIndex = new Dictionary<string, int>();

    /// <summary>
    /// Gets <see cref="RecordColumn"/> by provided <paramref name="fullName"/>.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="RecordColumn"/> if it was found; otherwise <see langword="null"/>.
    /// </remarks>
    /// <param name="fullName">Full name of the <see cref="RecordColumn"/> to find.</param>
    public RecordColumn this[string fullName]
    {
      get
      {
        int index;
        if (nameIndex.TryGetValue(fullName, out index))
          return this[index];
        
        return null;
      }
    }

    private void BuildNameIndex()
    {
      for (int index = 0; index < Count; index++) 
        nameIndex.Add(this[index].Name, index);
    }

    private static IEnumerable<RecordColumn> ApplyAlias(IEnumerable<RecordColumn> collection, string alias)
    {
      foreach (RecordColumn column in collection)
        yield return new RecordColumn(column, alias);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="RecordColumnCollection"/> class and fills them with provided <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection of items to add.</param>
    public RecordColumnCollection(IEnumerable<RecordColumn> collection)
      : base (collection.ToList())
    {           
      BuildNameIndex();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RecordColumnCollection"/> class and fills them with provided <paramref name="collection"/>. Also applies new <paramref name="alias"/> to all items.
    /// </summary>
    /// <param name="collection">Collection of items to add.</param>
    /// <param name="alias">Alias for the <see cref="RecordColumnCollection"/>.</param>
    public RecordColumnCollection(IEnumerable<RecordColumn> collection, string alias)
      : base(ApplyAlias(collection, alias).ToList())
    {      
      BuildNameIndex();    
    }    

    /// <summary>
    /// Initializes a new instance of class <see cref="RecordColumnCollection"/> and fills them with two collections of elements.
    /// </summary>
    /// <param name="collection1">First item collection.</param>
    /// <param name="collection2">Second item collection.</param>
    public RecordColumnCollection(IEnumerable<RecordColumn> collection1, IEnumerable<RecordColumn> collection2)
      : base (collection1.Concat(collection2).ToList())
    {            
      BuildNameIndex();
    }
  }
}