// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.27

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class GenericKeyTypeInfo
  {
    public readonly Type Type;
    public readonly Func<HierarchyInfo, TypeInfo, Tuple, Key> DefaultConstructor;
    public readonly Func<HierarchyInfo, TypeInfo, Tuple, int[], Key> KeyIndexBasedConstructor;


    // Constructors

    public GenericKeyTypeInfo(Type type, Func<HierarchyInfo, TypeInfo, Tuple, Key> defaultConstructor,
      Func<HierarchyInfo, TypeInfo, Tuple, int[], Key> keyIndexBasedConstructor)
    {
      Type = type;
      DefaultConstructor = defaultConstructor;
      KeyIndexBasedConstructor = keyIndexBasedConstructor;
    }
  }
}