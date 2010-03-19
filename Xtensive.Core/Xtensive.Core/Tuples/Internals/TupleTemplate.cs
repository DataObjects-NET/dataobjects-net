// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.18

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Xtensive.Core.Tuples.Internals
{
  [DataContract]
  [Serializable]
  public sealed class TupleTemplate<T0,T1,T2,T3> : RegularTuple
  {
    private const int count = 4;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new TupleTemplate<T0, T1, T2, T3>(descriptor);
    }

    public override Tuple Clone()
    {
      return new TupleTemplate<T0, T1, T2, T3>(this);
    }

    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      if (fieldIndex < 0 || fieldIndex >= count)
        throw new ArgumentOutOfRangeException("fieldIndex");
      var intValue = (int)((Flags >> (fieldIndex << 1)) & 3);
      return (TupleFieldState) intValue;
    }

    public override object GetValue(int fieldIndex, out TupleFieldState fieldState)
    {
      fieldState = GetFieldState(fieldIndex);
      switch(fieldIndex) {
        case 0:
          return Value0;
        case 1:
          return Value1;
        case 2:
          return Value2;
        case 3:
          return Value3;
      }
      throw new ArgumentOutOfRangeException("fieldIndex");
    }

    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (fieldIndex < 0 || fieldIndex >= count)
        throw new ArgumentOutOfRangeException("fieldIndex");

      var mask = 3L << (fieldIndex << 1);
      if (fieldValue == null) {
        Flags |= mask;
        switch (fieldIndex) {
          case 0:
            Value0 = default(T0);
            break;
          case 1:
            Value1 = default(T1);
            break;
          case 2:
            Value2 = default(T2);
            break;
          case 3:
            Value3 = default(T3);
            break;
        }
      }
      else {
        Flags = (Flags & ~mask) | (1L << (fieldIndex << 1));
        switch (fieldIndex) {
          case 0:
            Value0 = (T0)fieldValue;
            break;
          case 1:
            Value1 = (T1)fieldValue;
            break;
          case 2:
            Value2 = (T2)fieldValue;
            break;
          case 3:
            Value3 = (T3)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (TupleTemplate<T0, T1, T2, T3>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }

    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (TupleTemplate<T0, T1, T2, T3>)tuple;
      fieldState = (TupleFieldState)((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }

    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (TupleTemplate<T0, T1, T2, T3>)tuple;
      fieldState = (TupleFieldState)((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }

    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (TupleTemplate<T0, T1, T2, T3>)tuple;
      fieldState = (TupleFieldState)((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (TupleTemplate<T0, T1, T2, T3>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }

    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (TupleTemplate<T0, T1, T2, T3>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null)
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }

    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (TupleTemplate<T0, T1, T2, T3>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null)
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }

    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (TupleTemplate<T0, T1, T2, T3>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null)
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {typeof (T0), typeof (T1), typeof (T2), typeof (T3)});
    }


    // Constructors

    public TupleTemplate(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private TupleTemplate(TupleTemplate<T0, T1, T2, T3> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Flags = template.Flags;
    }
  }
}