// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.27

using System;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Internals
{
  [Serializable]
  internal sealed class GenericKeyTypeInfo
  {
    public readonly Type Type;
    public readonly Func<TypeInfo, Tuple, TypeReferenceAccuracy, Key> DefaultConstructor;
    public readonly Func<TypeInfo, Tuple, TypeReferenceAccuracy, int[], Key> KeyIndexBasedConstructor;


    // Constructors

    public GenericKeyTypeInfo(Type type, Func<TypeInfo, Tuple, TypeReferenceAccuracy, Key> defaultConstructor,
      Func<TypeInfo, Tuple, TypeReferenceAccuracy, int[], Key> keyIndexBasedConstructor)
    {
      Type = type;
      DefaultConstructor = defaultConstructor;
      KeyIndexBasedConstructor = keyIndexBasedConstructor;
    }
  }
}