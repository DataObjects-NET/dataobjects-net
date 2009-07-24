// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Storage.Model;
using System.Linq;

namespace Xtensive.Storage.Internals
{
  [DebuggerDisplay("Hierarchy = {Hierarchy.Name}, TypeIdColumnIndex = {TypeIdColumnIndex}")]
  internal sealed class ColumnGroupMapping
  {
    private readonly TypeMapping singleItem;
    private readonly Dictionary<int, TypeMapping> items;

    public HierarchyInfo Hierarchy { get; private set; }

    public int TypeIdColumnIndex { get; private set; }

    public TypeMapping GetTypeMapping(int typeId)
    {
      if (singleItem!=null) {
        if (typeId==singleItem.TypeId)
          return singleItem;
      }
      else {
        TypeMapping result;
        if (items.TryGetValue(typeId, out result))
          return result;
      }
      return null;
    }


    // Constructors

    public ColumnGroupMapping(HierarchyInfo hierarchy, int typeIdColumnIndex, Dictionary<int, TypeMapping> items)
    {
      if (items.Count==1)
        singleItem = items.Values.First();
      else
        this.items = items;
      Hierarchy = hierarchy;
      TypeIdColumnIndex = typeIdColumnIndex;
    }
  }
}