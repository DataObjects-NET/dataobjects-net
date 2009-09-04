// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.29

using System.Collections.Generic;
using System.Linq;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal sealed class FetchTask
  {
    internal IndexInfo Index { get; private set; }
    internal int[] Columns { get; private set; }
    private readonly int hashCode;

    private bool Equals(FetchTask obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (hashCode!=obj.hashCode)
        return false;
      if (Index!=obj.Index)
        return false;
      var objColumnIndexes = obj.Columns;
      if (Columns.Length!=objColumnIndexes.Length)
        return false;
      for (int i = 0; i < Columns.Length; i++)
        if (Columns[i]!=objColumnIndexes[i])
          return false;
      return true;
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as FetchTask);
    }

    public override int GetHashCode()
    {
      return hashCode;
    }


    // Constructors

    public FetchTask(IndexInfo index, IEnumerable<int> columns)
    {
      Index = index;
      Columns = columns.ToArray();
      hashCode = Columns.Length;
      for (int i = 0; i < Columns.Length; i++)
        hashCode = unchecked (379 * hashCode + Columns[i]);
    }
  }
}