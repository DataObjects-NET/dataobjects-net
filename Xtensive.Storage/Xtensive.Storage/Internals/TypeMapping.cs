// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using System.Linq;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class TypeMapping
  {
    public readonly TypeInfo Type;
    public readonly int TypeId;
    public readonly MapTransform KeyTransform;
    public readonly int[] KeyIndexes;
    public readonly MapTransform Transform;


    // Constructors

    public TypeMapping(TypeInfo type, MapTransform keyTransform, MapTransform transform)
      : this(type, keyTransform, transform, 
        keyTransform.SingleSourceMap.Where(item => item!=MapTransform.NoMapping).ToArray())
    {
    }

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