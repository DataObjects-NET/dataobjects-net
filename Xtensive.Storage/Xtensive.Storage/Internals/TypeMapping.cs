// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.08

using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  internal class TypeMapping
  {
    public TypeInfo Type { get; private set; }

    public int TypeId { get; private set; }

    public MapTransform Transform { get; private set; }


    // Constructors

    public TypeMapping(TypeInfo type, MapTransform transform)
    {
      Type = type;
      TypeId = type.TypeId;
      Transform = transform;
    }
  }
}