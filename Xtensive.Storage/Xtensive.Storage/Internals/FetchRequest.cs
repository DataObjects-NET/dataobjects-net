// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.29

using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal sealed class FetchRequest
  {
    internal readonly IndexInfo Index;
    internal readonly int[] Columns;
    private readonly int hashCode;

    private bool Equals(FetchRequest obj)
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
      return Equals(obj as FetchRequest);
    }

    public override int GetHashCode()
    {
      return hashCode;
    }


    // Constructors

    public FetchRequest(IndexInfo index, int[] columns)
    {
      this.Index = index;
      this.Columns = columns;
      hashCode = columns.Length;
      for (int i = 0; i < this.Columns.Length; i++)
        hashCode = unchecked (379 * hashCode + this.Columns[i]);
    }
  }
}