// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.29

using System.Linq;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Materialization
{
  internal struct EntityMaterializationInfo
  {
    public readonly MapTransform Transform;
    public readonly MapTransform KeyTransform;
    public readonly TypeInfo EntityType;
    public readonly int[] KeyFields;


    // Constructors

    public EntityMaterializationInfo(MapTransform transform, MapTransform keyTransform, TypeInfo entityType)
    {
      Transform = transform;
      KeyTransform = keyTransform;
      EntityType = entityType;
      KeyFields = KeyTransform.SingleSourceMap.Where(item => item!=MapTransform.NoMapping).ToArray();
    }
  }
}