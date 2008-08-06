// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.24

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Collection of <see cref="RecordColumn"/> items.
  /// </summary>
  [Serializable]
  public class RecordColumnCollection : 
    IEnumerable<RecordColumn>, 
    IEquatable<RecordColumnCollection>
  {
    private readonly List<RecordColumn> values;
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
          return values[index];
        return null;
      }
    }

    /// <summary>
    /// Gets <see cref="RecordColumn"/> by its <see cref="index"/>.
    /// </summary>
    /// <param name="index">Index of the <see cref="RecordColumn"/>.</param>
    public RecordColumn this[int index]
    {
      get { return values[index]; }
    }

    /// <summary>
    /// Gets count of <see cref="RecordColumn"/> elements.
    /// </summary>
    public int Count
    {
      get { return values.Count; }
    }

    private void BuildNameIndex()
    {
      for (int index = 0; index < values.Count; index++) 
        nameIndex.Add(values[index].Name, index);
    }


    #region IEnumerable<RecordColumn> Members

    IEnumerator<RecordColumn> IEnumerable<RecordColumn>.GetEnumerator()
    {
      return values.GetEnumerator();
    }


    public IEnumerator GetEnumerator()
    {
      return ((IEnumerable<RecordColumn>)this).GetEnumerator();
    }

    #endregion

    /// <summary>
    /// Compares two <see cref="RecordColumnCollection"/> instances.
    /// </summary>
    /// <param name="x">First <see cref="RecordColumnCollection"/>.</param>
    /// <param name="y">Second <see cref="RecordColumnCollection"/>.</param>
    /// <returns><see langword="false"/> if they are equal; otherwise, <see langword="true"/>.</returns>
    public static bool operator !=(RecordColumnCollection x, RecordColumnCollection y)
    {
      return !Equals(x, y);
    }

    /// <summary>
    /// Compares two <see cref="RecordColumnCollection"/> instances.
    /// </summary>
    /// <param name="x">First <see cref="RecordColumnCollection"/>.</param>
    /// <param name="y">Second <see cref="RecordColumnCollection"/>.</param>
    /// <returns><see langword="true"/> if they are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(RecordColumnCollection x, RecordColumnCollection y)
    {
      return Equals(x, y);
    }

    public bool Equals(RecordColumnCollection recordColumnCollection)
    {
      if (recordColumnCollection == null)
        return false;
      return Equals(values, recordColumnCollection.values) && Equals(nameIndex, recordColumnCollection.nameIndex);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as RecordColumnCollection);
    }

    public override int GetHashCode()
    {
      return values.GetHashCode() + 29*nameIndex.GetHashCode();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RecordColumnCollection"/> class and fills them with provided <paramref name="collection"/>.
    /// </summary>
    /// <param name="collection">Collection of items to add.</param>
    public RecordColumnCollection(IEnumerable<RecordColumn> collection)
    {
      values = new List<RecordColumn>(collection);
      BuildNameIndex();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="RecordColumnCollection"/> class and fills them with provided <paramref name="collection"/>. Also applies new <paramref name="alias"/> to all items.
    /// </summary>
    /// <param name="collection">Collection of items to add.</param>
    /// <param name="alias">Alias for the <see cref="RecordColumnCollection"/>.</param>
    public RecordColumnCollection(IEnumerable<RecordColumn> collection, string alias)
    {
      values = new List<RecordColumn>();
      foreach (RecordColumn column in collection)
        values.Add(new RecordColumn(column, alias));
      BuildNameIndex();
    }

    /// <summary>
    /// Initializes a new instance of class <see cref="RecordColumnCollection"/> and fills them with two collections of elements.
    /// </summary>
    /// <param name="collection1">First item collection.</param>
    /// <param name="collection2">Second item collection.</param>
    public RecordColumnCollection(IEnumerable<RecordColumn> collection1, IEnumerable<RecordColumn> collection2)
    {
      values = new List<RecordColumn>(collection1);
      values.AddRange(collection2);
      BuildNameIndex();
    }

  }
}