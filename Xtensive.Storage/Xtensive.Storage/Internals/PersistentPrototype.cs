// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.14

using System;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal class PersistentPrototype
  {
    public TypeInfo Type;
    public Tuple Tuple;
    public MapTransform KeyInjector;

    
    // Constructors

    public PersistentPrototype(TypeInfo type, Tuple fields, MapTransform keyInjector)
    {
      Type = type;
      Tuple = fields;
      KeyInjector = keyInjector;
    }
  }
}