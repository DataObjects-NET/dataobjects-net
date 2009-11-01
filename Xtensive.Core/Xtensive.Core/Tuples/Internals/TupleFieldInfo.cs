// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.06.18

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Xtensive.Core.Tuples.Internals
{
  internal sealed class TupleFieldInfo
  {
    private const string FieldNameFormatString = "f{0}";
    private const string CompressingFieldNameFormatString = "c{0}";
    private const int CompressingFieldSize = 32;
    private readonly static Dictionary<Type, int> CompressableTypes = new Dictionary<Type, int>();

    public readonly TupleInfo TupleInfo;
    public readonly Type   Type;
    public readonly Type   ActualType;
    public readonly string Name;
    public readonly bool IsFlagsHolder;
    public readonly bool IsCompressed;
    public readonly bool IsCompressing;
    public readonly bool IsValueType;
    public readonly int ValueBitMask;
    public readonly int BitMask;
    public readonly int InversedBitMask;
    public readonly int BitShift;
    public readonly TupleFieldInfo FlagsField;
    public readonly TupleFieldInfo CompressingFieldInfo;
    public readonly TupleInterfaceInfo Interface;
    public readonly TupleInterfaceInfo NullableInterface; // !=null if IsValueType==true
    private FieldBuilder fieldBuilder;

    public FieldBuilder FieldBuilder
    {
      get { return fieldBuilder; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "fieldBuilder");
        fieldBuilder = value;
      }
    }

    // Constructors

    public TupleFieldInfo(TupleInfo tupleInfo, Type type)
      : this(tupleInfo, type, false, false)
    {
    }

    public TupleFieldInfo(TupleInfo tupleInfo, Type type, bool isCompressing, bool isFlag)
    {
      TupleInfo = tupleInfo;
      Type = type;
      IsFlagsHolder = typeof(TupleFieldState)==type;
      IsValueType = type.IsValueType;
      if (!isCompressing && CompressableTypes.ContainsKey(type)) {
        // Field is compressable
        IsCompressed = true;
        ActualType = typeof (int);
        int sizeInBits = CompressableTypes[type];
        bool allocated = false;
        while (!allocated) {
          int actualCompressingFieldCount = tupleInfo.ActualCompressingFields.Count;
          for (int fieldIndex = 0; fieldIndex < actualCompressingFieldCount; fieldIndex++) {
            TupleFieldInfo compressingField = tupleInfo.ActualCompressingFields[fieldIndex];
            int occupiedState = tupleInfo.ActualCompressingFieldOccupiedBits[fieldIndex];
            if ((occupiedState + sizeInBits) <= CompressingFieldSize) {
              // Necessary space is found in existing compressing field
              int bits = 1;
              for (int i = 1; i < sizeInBits; i++)
                bits |= bits << 1;
              BitShift = occupiedState;
              ValueBitMask = bits;
              BitMask = bits << occupiedState;
              InversedBitMask = BitMask ^ -1;
              CompressingFieldInfo = compressingField;
              tupleInfo.ActualCompressingFieldOccupiedBits[fieldIndex] = occupiedState + sizeInBits;
              allocated = true;
              break;
            }
          }
          if (!allocated)
            // Not enough space - adding one more compressing field
            new TupleFieldInfo(tupleInfo, typeof (int), true, false);
        }
      }
      else {
        // Non-compressable field
        IsCompressed = false;
        ActualType = type;
        if (!isCompressing) {
          Name = string.Format(FieldNameFormatString, tupleInfo.ActualFields.Count);
          tupleInfo.ActualFields.Add(this);
        }
        else {
          // But a compressing one
          IsCompressing = true;
          Name = string.Format(CompressingFieldNameFormatString, tupleInfo.ActualCompressingFields.Count);
          tupleInfo.ActualCompressingFields.Add(this);
          tupleInfo.ActualCompressingFieldOccupiedBits.Add(0);
        }
      }
      if (!isFlag && !isCompressing) {
        tupleInfo.Fields.Add(this);
        // Real field, so let's create flags & interfaces for it
        FlagsField = new TupleFieldInfo(tupleInfo, typeof(TupleFieldState), false, true);
        if (tupleInfo.Interfaces.ContainsKey(type)) {
          // Interfaces are already created
          Interface = tupleInfo.Interfaces[type];
          if (IsValueType)
            NullableInterface = tupleInfo.Interfaces[typeof (Nullable<>).MakeGenericType(type)];
        }
        else {
          Interface = new TupleInterfaceInfo(tupleInfo, type);
          if (IsValueType)
            NullableInterface = new TupleInterfaceInfo(tupleInfo, typeof (Nullable<>).MakeGenericType(type));
        }
      }
    }

    // Static constructor
    static TupleFieldInfo()
    {
      CompressableTypes.Add(typeof (bool), 1);
      CompressableTypes.Add(typeof (TupleFieldState), 2);
      CompressableTypes.Add(typeof (byte), 8);
      CompressableTypes.Add(typeof (sbyte), 8);
      CompressableTypes.Add(typeof (char), 16);
      CompressableTypes.Add(typeof (short), 16);
      CompressableTypes.Add(typeof (ushort), 16);
    }
  }
}
