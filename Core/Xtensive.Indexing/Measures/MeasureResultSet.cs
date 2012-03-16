// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.16

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// A set of measurements.
  /// </summary>
  [Serializable]
  public class MeasureResultSet<TItem> : IMeasureResultSet<TItem>
  {
    private readonly List<IMeasure<TItem>> items;

    /// <inheritdoc/>
    public IMeasure<TItem> this[int index]
    {
      [DebuggerStepThrough]
      get { return items[index]; }
    }

    /// <inheritdoc/>
    public IMeasure<TItem> this[string name]
    {
      get
      {
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
        foreach (IMeasure<TItem> item in this)
          if (item!=null && item.Name==name)
            return item;
        throw new ArgumentOutOfRangeException("name", String.Format(Strings.MeasureWithTheNameWasNotFound, name));
      }
    }

    /// <inheritdoc/>
    public bool IsConsistent {
      get {
        foreach (IMeasure<TItem> item in items)
          if (!item.HasResult)
            return false;
        return true;
      }
    }

    /// <inheritdoc/>
    public bool Add(TItem item)
    {
      bool success = true;
      int count = items.Count;
      for (int i = 0; i < count; i++)
        success &= items[i].Add(item);
      return success;
    }

    /// <inheritdoc/>
    public bool Subtract(TItem item)
    {
      bool success = true;
      int count = items.Count;
      for (int i = 0; i < count; i++)
        success &= items[i].Subtract(item);
      return success;
    }

    /// <inheritdoc/>
    public void Reset()
    {
      foreach (IMeasure<TItem> item in items)
        item.Reset();
    }

    /// <inheritdoc/>
    public IEnumerator<IMeasure<TItem>> GetEnumerator()
    {
      foreach (IMeasure<TItem> item in items)
        yield return item;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public long Count
    {
      [DebuggerStepThrough]
      get { return items.Count; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="measures">The initial content of the set.</param>
    public MeasureResultSet(IMeasureSet<TItem> measures)
    {
      ArgumentValidator.EnsureArgumentNotNull(measures, "measures");

      int measureCount = (int) measures.Count;
      items = new List<IMeasure<TItem>>(measureCount);
      for (int index = 0, count = measureCount; index < count; index++)
        items.Add(measures[index].CreateNew(measures[index].Name));
    }
  }
}
