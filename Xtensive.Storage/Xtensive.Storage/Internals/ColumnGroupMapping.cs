// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [DebuggerDisplay("Hierarchy = {Hierarchy.Name}, TypeIdColumnIndex = {TypeIdColumnIndex}")]
  internal sealed class ColumnGroupMapping
  {
    private readonly Dictionary<int, TypeMapping> typeMappings;

    public HierarchyInfo Hierarchy { get; private set; }

    public int TypeIdColumnIndex { get; private set; }

    public TypeMapping GetMapping(int typeId)
    {
      typeId = typeId==TypeInfo.NoTypeId ? Hierarchy.Root.TypeId : typeId;
      TypeMapping result;
      if (typeMappings.TryGetValue(typeId, out result))
        return result;
      return null;
    }


    // Constructors

    public ColumnGroupMapping(HierarchyInfo hierarchy, int typeIdColumnIndex, Dictionary<int, TypeMapping> typeMappings)
    {
      this.typeMappings = typeMappings;
      Hierarchy = hierarchy;
      TypeIdColumnIndex = typeIdColumnIndex;
    }
  }
}