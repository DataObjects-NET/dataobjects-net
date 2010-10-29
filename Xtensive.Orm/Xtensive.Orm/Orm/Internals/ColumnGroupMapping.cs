// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Orm.Model;
using System.Linq;

namespace Xtensive.Orm.Internals
{
  [DebuggerDisplay("Type = {Type.Name}, TypeIdColumnIndex = {TypeIdColumnIndex}")]
  internal sealed class ColumnGroupMapping
  {
    private readonly TypeMapping singleItem;
    private readonly IntDictionary<TypeMapping> items;

    public TypeInfo Type { get; private set; }

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

    public ColumnGroupMapping(TypeInfo type, int typeIdColumnIndex, IntDictionary<TypeMapping> items)
    {
      if (items.Count==1)
        singleItem = items.First().Value;
      else
        this.items = items;
      Type = type;
      TypeIdColumnIndex = typeIdColumnIndex;
    }
  }
}