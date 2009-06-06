// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.29

using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq.Materialization
{
  internal struct EntityMaterializationData
  {
    public readonly MapTransform Transform;
    public readonly MapTransform KeyTransform;
    public readonly TypeInfo EntityType;


    // Constructor

    public EntityMaterializationData(MapTransform transform, MapTransform keyTransform, TypeInfo entityType)
    {
      Transform = transform;
      KeyTransform = keyTransform;
      EntityType = entityType;
    }
  }
}