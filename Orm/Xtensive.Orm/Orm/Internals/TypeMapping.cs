// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using Xtensive.Tuples.Transform;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  internal class TypeMapping
  {
    public readonly TypeInfo Type;
    public readonly int TypeId;
    public readonly MapTransform KeyTransform;
    public readonly int[] KeyIndexes;
    public readonly MapTransform Transform;


    // Constructors

    public TypeMapping(TypeInfo type, MapTransform keyTransform, MapTransform transform, int[] keyIndexes)
    {
      Type = type;
      TypeId = type.TypeId;
      KeyTransform = keyTransform;
      Transform = transform;
      KeyIndexes = keyIndexes;
    }
  }
}