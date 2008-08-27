// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Tuples.Internals
{
  internal sealed class TupleInfo
  {
    private static int TupleCount;
    private const string NameFormatString = "GeneratedTuple{0}";
    public readonly string Name;
    public readonly TupleDescriptor Descriptor;
    public readonly List<TupleFieldInfo> Fields;
    public readonly List<TupleFieldInfo> ActualFields;
    public readonly List<TupleFieldInfo> ActualCompressingFields;
    public readonly List<int> ActualCompressingFieldOccupiedBits;
    public readonly Dictionary<Type, TupleInterfaceInfo> Interfaces;


    // Constructors

    public TupleInfo(TupleDescriptor tupleDescriptor)
    {
      Name = string.Format(NameFormatString, TupleCount++);
      Interfaces = new Dictionary<Type, TupleInterfaceInfo>();
      Fields = new List<TupleFieldInfo>();
      ActualFields = new List<TupleFieldInfo>();
      ActualCompressingFields = new List<TupleFieldInfo>();
      ActualCompressingFieldOccupiedBits = new List<int>();
      Descriptor = tupleDescriptor;
      foreach (Type fieldType in tupleDescriptor)
        new TupleFieldInfo(this, fieldType);
    }
  }
}