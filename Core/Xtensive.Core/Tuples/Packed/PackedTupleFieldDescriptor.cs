// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;

namespace Xtensive.Tuples.Packed
{
  internal sealed class PackedTupleFieldDescriptor
  {
    public bool IsObject;

    public int ValueIndex = -1;
    public int ObjectIndex = -1;

    public Func<long, object> Boxer;
    public Func<object, long> Unboxer;

    public Delegate GetValueDelegate;
    public Delegate SetValueDelegate;

    public Delegate GetNullableValueDelegate;
    public Delegate SetNullableValueDelegate;
  }
}