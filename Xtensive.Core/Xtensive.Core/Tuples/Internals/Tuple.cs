// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.18

using System;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Tuples.Internals
{
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0> : RegularTuple
  {
    private const int count = 1;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0>(this);
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
        }
      }
      else {
        Flags = (Flags & ~mask) | (1L << (fieldIndex << 1));
        switch (fieldIndex) {
          case 0:
            Value0 = (T0)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1> : RegularTuple
  {
    private const int count = 2;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1>(this);
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
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2> : RegularTuple
  {
    private const int count = 3;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2>(this);
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
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3> : RegularTuple
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
      return new Tuple<T0,T1,T2,T3>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3>(this);
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
      var t = (Tuple<T0,T1,T2,T3>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3>)tuple;
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
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4> : RegularTuple
  {
    private const int count = 5;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4>(this);
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
        case 4:
          return Value4;
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
          case 4:
            Value4 = default(T4);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5> : RegularTuple
  {
    private const int count = 6;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6> : RegularTuple
  {
    private const int count = 7;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7> : RegularTuple
  {
    private const int count = 8;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8> : RegularTuple
  {
    private const int count = 9;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9> : RegularTuple
  {
    private const int count = 10;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> : RegularTuple
  {
    private const int count = 11;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> : RegularTuple
  {
    private const int count = 12;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> : RegularTuple
  {
    private const int count = 13;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> : RegularTuple
  {
    private const int count = 14;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> : RegularTuple
  {
    private const int count = 15;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> : RegularTuple
  {
    private const int count = 16;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> : RegularTuple
  {
    private const int count = 17;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17> : RegularTuple
  {
    private const int count = 18;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18> : RegularTuple
  {
    private const int count = 19;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19> : RegularTuple
  {
    private const int count = 20;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20> : RegularTuple
  {
    private const int count = 21;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21> : RegularTuple
  {
    private const int count = 22;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22> : RegularTuple
  {
    private const int count = 23;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public T22 Value22;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
        case 22:
          return Value22;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
            break;
          case 22:
            Value22 = default(T22);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
          case 22:
            Value22 = (T22)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }
    public static T22 GetValue22(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (22 << 1)) & 3);
      return t.Value22;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }
    public static void SetValue22(Tuple tuple, T22 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22>)tuple;
      const long mask = 3L << (22 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (22 << 1));
      t.Value22 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21),
        typeof (T22)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Value22 = template.Value22;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23> : RegularTuple
  {
    private const int count = 24;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public T22 Value22;
    [DataMember]
    public T23 Value23;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
        case 22:
          return Value22;
        case 23:
          return Value23;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
            break;
          case 22:
            Value22 = default(T22);
            break;
          case 23:
            Value23 = default(T23);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
          case 22:
            Value22 = (T22)fieldValue;
            break;
          case 23:
            Value23 = (T23)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }
    public static T22 GetValue22(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (22 << 1)) & 3);
      return t.Value22;
    }
    public static T23 GetValue23(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (23 << 1)) & 3);
      return t.Value23;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }
    public static void SetValue22(Tuple tuple, T22 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (22 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (22 << 1));
      t.Value22 = value;
    }
    public static void SetValue23(Tuple tuple, T23 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23>)tuple;
      const long mask = 3L << (23 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (23 << 1));
      t.Value23 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21),
        typeof (T22),
        typeof (T23)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Value22 = template.Value22;
      Value23 = template.Value23;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24> : RegularTuple
  {
    private const int count = 25;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public T22 Value22;
    [DataMember]
    public T23 Value23;
    [DataMember]
    public T24 Value24;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
        case 22:
          return Value22;
        case 23:
          return Value23;
        case 24:
          return Value24;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
            break;
          case 22:
            Value22 = default(T22);
            break;
          case 23:
            Value23 = default(T23);
            break;
          case 24:
            Value24 = default(T24);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
          case 22:
            Value22 = (T22)fieldValue;
            break;
          case 23:
            Value23 = (T23)fieldValue;
            break;
          case 24:
            Value24 = (T24)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }
    public static T22 GetValue22(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (22 << 1)) & 3);
      return t.Value22;
    }
    public static T23 GetValue23(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (23 << 1)) & 3);
      return t.Value23;
    }
    public static T24 GetValue24(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (24 << 1)) & 3);
      return t.Value24;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }
    public static void SetValue22(Tuple tuple, T22 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (22 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (22 << 1));
      t.Value22 = value;
    }
    public static void SetValue23(Tuple tuple, T23 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (23 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (23 << 1));
      t.Value23 = value;
    }
    public static void SetValue24(Tuple tuple, T24 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24>)tuple;
      const long mask = 3L << (24 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (24 << 1));
      t.Value24 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21),
        typeof (T22),
        typeof (T23),
        typeof (T24)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Value22 = template.Value22;
      Value23 = template.Value23;
      Value24 = template.Value24;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25> : RegularTuple
  {
    private const int count = 26;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public T22 Value22;
    [DataMember]
    public T23 Value23;
    [DataMember]
    public T24 Value24;
    [DataMember]
    public T25 Value25;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
        case 22:
          return Value22;
        case 23:
          return Value23;
        case 24:
          return Value24;
        case 25:
          return Value25;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
            break;
          case 22:
            Value22 = default(T22);
            break;
          case 23:
            Value23 = default(T23);
            break;
          case 24:
            Value24 = default(T24);
            break;
          case 25:
            Value25 = default(T25);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
          case 22:
            Value22 = (T22)fieldValue;
            break;
          case 23:
            Value23 = (T23)fieldValue;
            break;
          case 24:
            Value24 = (T24)fieldValue;
            break;
          case 25:
            Value25 = (T25)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }
    public static T22 GetValue22(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (22 << 1)) & 3);
      return t.Value22;
    }
    public static T23 GetValue23(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (23 << 1)) & 3);
      return t.Value23;
    }
    public static T24 GetValue24(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (24 << 1)) & 3);
      return t.Value24;
    }
    public static T25 GetValue25(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (25 << 1)) & 3);
      return t.Value25;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }
    public static void SetValue22(Tuple tuple, T22 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (22 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (22 << 1));
      t.Value22 = value;
    }
    public static void SetValue23(Tuple tuple, T23 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (23 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (23 << 1));
      t.Value23 = value;
    }
    public static void SetValue24(Tuple tuple, T24 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (24 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (24 << 1));
      t.Value24 = value;
    }
    public static void SetValue25(Tuple tuple, T25 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25>)tuple;
      const long mask = 3L << (25 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (25 << 1));
      t.Value25 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21),
        typeof (T22),
        typeof (T23),
        typeof (T24),
        typeof (T25)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Value22 = template.Value22;
      Value23 = template.Value23;
      Value24 = template.Value24;
      Value25 = template.Value25;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26> : RegularTuple
  {
    private const int count = 27;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public T22 Value22;
    [DataMember]
    public T23 Value23;
    [DataMember]
    public T24 Value24;
    [DataMember]
    public T25 Value25;
    [DataMember]
    public T26 Value26;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
        case 22:
          return Value22;
        case 23:
          return Value23;
        case 24:
          return Value24;
        case 25:
          return Value25;
        case 26:
          return Value26;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
            break;
          case 22:
            Value22 = default(T22);
            break;
          case 23:
            Value23 = default(T23);
            break;
          case 24:
            Value24 = default(T24);
            break;
          case 25:
            Value25 = default(T25);
            break;
          case 26:
            Value26 = default(T26);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
          case 22:
            Value22 = (T22)fieldValue;
            break;
          case 23:
            Value23 = (T23)fieldValue;
            break;
          case 24:
            Value24 = (T24)fieldValue;
            break;
          case 25:
            Value25 = (T25)fieldValue;
            break;
          case 26:
            Value26 = (T26)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }
    public static T22 GetValue22(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (22 << 1)) & 3);
      return t.Value22;
    }
    public static T23 GetValue23(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (23 << 1)) & 3);
      return t.Value23;
    }
    public static T24 GetValue24(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (24 << 1)) & 3);
      return t.Value24;
    }
    public static T25 GetValue25(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (25 << 1)) & 3);
      return t.Value25;
    }
    public static T26 GetValue26(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (26 << 1)) & 3);
      return t.Value26;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }
    public static void SetValue22(Tuple tuple, T22 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (22 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (22 << 1));
      t.Value22 = value;
    }
    public static void SetValue23(Tuple tuple, T23 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (23 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (23 << 1));
      t.Value23 = value;
    }
    public static void SetValue24(Tuple tuple, T24 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (24 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (24 << 1));
      t.Value24 = value;
    }
    public static void SetValue25(Tuple tuple, T25 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (25 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (25 << 1));
      t.Value25 = value;
    }
    public static void SetValue26(Tuple tuple, T26 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26>)tuple;
      const long mask = 3L << (26 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (26 << 1));
      t.Value26 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21),
        typeof (T22),
        typeof (T23),
        typeof (T24),
        typeof (T25),
        typeof (T26)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Value22 = template.Value22;
      Value23 = template.Value23;
      Value24 = template.Value24;
      Value25 = template.Value25;
      Value26 = template.Value26;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27> : RegularTuple
  {
    private const int count = 28;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public T22 Value22;
    [DataMember]
    public T23 Value23;
    [DataMember]
    public T24 Value24;
    [DataMember]
    public T25 Value25;
    [DataMember]
    public T26 Value26;
    [DataMember]
    public T27 Value27;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
        case 22:
          return Value22;
        case 23:
          return Value23;
        case 24:
          return Value24;
        case 25:
          return Value25;
        case 26:
          return Value26;
        case 27:
          return Value27;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
            break;
          case 22:
            Value22 = default(T22);
            break;
          case 23:
            Value23 = default(T23);
            break;
          case 24:
            Value24 = default(T24);
            break;
          case 25:
            Value25 = default(T25);
            break;
          case 26:
            Value26 = default(T26);
            break;
          case 27:
            Value27 = default(T27);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
          case 22:
            Value22 = (T22)fieldValue;
            break;
          case 23:
            Value23 = (T23)fieldValue;
            break;
          case 24:
            Value24 = (T24)fieldValue;
            break;
          case 25:
            Value25 = (T25)fieldValue;
            break;
          case 26:
            Value26 = (T26)fieldValue;
            break;
          case 27:
            Value27 = (T27)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }
    public static T22 GetValue22(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (22 << 1)) & 3);
      return t.Value22;
    }
    public static T23 GetValue23(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (23 << 1)) & 3);
      return t.Value23;
    }
    public static T24 GetValue24(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (24 << 1)) & 3);
      return t.Value24;
    }
    public static T25 GetValue25(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (25 << 1)) & 3);
      return t.Value25;
    }
    public static T26 GetValue26(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (26 << 1)) & 3);
      return t.Value26;
    }
    public static T27 GetValue27(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (27 << 1)) & 3);
      return t.Value27;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }
    public static void SetValue22(Tuple tuple, T22 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (22 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (22 << 1));
      t.Value22 = value;
    }
    public static void SetValue23(Tuple tuple, T23 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (23 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (23 << 1));
      t.Value23 = value;
    }
    public static void SetValue24(Tuple tuple, T24 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (24 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (24 << 1));
      t.Value24 = value;
    }
    public static void SetValue25(Tuple tuple, T25 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (25 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (25 << 1));
      t.Value25 = value;
    }
    public static void SetValue26(Tuple tuple, T26 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (26 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (26 << 1));
      t.Value26 = value;
    }
    public static void SetValue27(Tuple tuple, T27 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27>)tuple;
      const long mask = 3L << (27 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (27 << 1));
      t.Value27 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21),
        typeof (T22),
        typeof (T23),
        typeof (T24),
        typeof (T25),
        typeof (T26),
        typeof (T27)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Value22 = template.Value22;
      Value23 = template.Value23;
      Value24 = template.Value24;
      Value25 = template.Value25;
      Value26 = template.Value26;
      Value27 = template.Value27;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28> : RegularTuple
  {
    private const int count = 29;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public T22 Value22;
    [DataMember]
    public T23 Value23;
    [DataMember]
    public T24 Value24;
    [DataMember]
    public T25 Value25;
    [DataMember]
    public T26 Value26;
    [DataMember]
    public T27 Value27;
    [DataMember]
    public T28 Value28;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
        case 22:
          return Value22;
        case 23:
          return Value23;
        case 24:
          return Value24;
        case 25:
          return Value25;
        case 26:
          return Value26;
        case 27:
          return Value27;
        case 28:
          return Value28;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
            break;
          case 22:
            Value22 = default(T22);
            break;
          case 23:
            Value23 = default(T23);
            break;
          case 24:
            Value24 = default(T24);
            break;
          case 25:
            Value25 = default(T25);
            break;
          case 26:
            Value26 = default(T26);
            break;
          case 27:
            Value27 = default(T27);
            break;
          case 28:
            Value28 = default(T28);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
          case 22:
            Value22 = (T22)fieldValue;
            break;
          case 23:
            Value23 = (T23)fieldValue;
            break;
          case 24:
            Value24 = (T24)fieldValue;
            break;
          case 25:
            Value25 = (T25)fieldValue;
            break;
          case 26:
            Value26 = (T26)fieldValue;
            break;
          case 27:
            Value27 = (T27)fieldValue;
            break;
          case 28:
            Value28 = (T28)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }
    public static T22 GetValue22(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (22 << 1)) & 3);
      return t.Value22;
    }
    public static T23 GetValue23(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (23 << 1)) & 3);
      return t.Value23;
    }
    public static T24 GetValue24(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (24 << 1)) & 3);
      return t.Value24;
    }
    public static T25 GetValue25(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (25 << 1)) & 3);
      return t.Value25;
    }
    public static T26 GetValue26(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (26 << 1)) & 3);
      return t.Value26;
    }
    public static T27 GetValue27(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (27 << 1)) & 3);
      return t.Value27;
    }
    public static T28 GetValue28(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (28 << 1)) & 3);
      return t.Value28;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }
    public static void SetValue22(Tuple tuple, T22 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (22 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (22 << 1));
      t.Value22 = value;
    }
    public static void SetValue23(Tuple tuple, T23 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (23 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (23 << 1));
      t.Value23 = value;
    }
    public static void SetValue24(Tuple tuple, T24 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (24 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (24 << 1));
      t.Value24 = value;
    }
    public static void SetValue25(Tuple tuple, T25 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (25 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (25 << 1));
      t.Value25 = value;
    }
    public static void SetValue26(Tuple tuple, T26 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (26 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (26 << 1));
      t.Value26 = value;
    }
    public static void SetValue27(Tuple tuple, T27 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (27 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (27 << 1));
      t.Value27 = value;
    }
    public static void SetValue28(Tuple tuple, T28 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28>)tuple;
      const long mask = 3L << (28 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (28 << 1));
      t.Value28 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21),
        typeof (T22),
        typeof (T23),
        typeof (T24),
        typeof (T25),
        typeof (T26),
        typeof (T27),
        typeof (T28)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Value22 = template.Value22;
      Value23 = template.Value23;
      Value24 = template.Value24;
      Value25 = template.Value25;
      Value26 = template.Value26;
      Value27 = template.Value27;
      Value28 = template.Value28;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29> : RegularTuple
  {
    private const int count = 30;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public T22 Value22;
    [DataMember]
    public T23 Value23;
    [DataMember]
    public T24 Value24;
    [DataMember]
    public T25 Value25;
    [DataMember]
    public T26 Value26;
    [DataMember]
    public T27 Value27;
    [DataMember]
    public T28 Value28;
    [DataMember]
    public T29 Value29;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
        case 22:
          return Value22;
        case 23:
          return Value23;
        case 24:
          return Value24;
        case 25:
          return Value25;
        case 26:
          return Value26;
        case 27:
          return Value27;
        case 28:
          return Value28;
        case 29:
          return Value29;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
            break;
          case 22:
            Value22 = default(T22);
            break;
          case 23:
            Value23 = default(T23);
            break;
          case 24:
            Value24 = default(T24);
            break;
          case 25:
            Value25 = default(T25);
            break;
          case 26:
            Value26 = default(T26);
            break;
          case 27:
            Value27 = default(T27);
            break;
          case 28:
            Value28 = default(T28);
            break;
          case 29:
            Value29 = default(T29);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
          case 22:
            Value22 = (T22)fieldValue;
            break;
          case 23:
            Value23 = (T23)fieldValue;
            break;
          case 24:
            Value24 = (T24)fieldValue;
            break;
          case 25:
            Value25 = (T25)fieldValue;
            break;
          case 26:
            Value26 = (T26)fieldValue;
            break;
          case 27:
            Value27 = (T27)fieldValue;
            break;
          case 28:
            Value28 = (T28)fieldValue;
            break;
          case 29:
            Value29 = (T29)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }
    public static T22 GetValue22(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (22 << 1)) & 3);
      return t.Value22;
    }
    public static T23 GetValue23(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (23 << 1)) & 3);
      return t.Value23;
    }
    public static T24 GetValue24(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (24 << 1)) & 3);
      return t.Value24;
    }
    public static T25 GetValue25(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (25 << 1)) & 3);
      return t.Value25;
    }
    public static T26 GetValue26(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (26 << 1)) & 3);
      return t.Value26;
    }
    public static T27 GetValue27(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (27 << 1)) & 3);
      return t.Value27;
    }
    public static T28 GetValue28(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (28 << 1)) & 3);
      return t.Value28;
    }
    public static T29 GetValue29(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (29 << 1)) & 3);
      return t.Value29;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }
    public static void SetValue22(Tuple tuple, T22 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (22 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (22 << 1));
      t.Value22 = value;
    }
    public static void SetValue23(Tuple tuple, T23 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (23 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (23 << 1));
      t.Value23 = value;
    }
    public static void SetValue24(Tuple tuple, T24 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (24 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (24 << 1));
      t.Value24 = value;
    }
    public static void SetValue25(Tuple tuple, T25 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (25 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (25 << 1));
      t.Value25 = value;
    }
    public static void SetValue26(Tuple tuple, T26 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (26 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (26 << 1));
      t.Value26 = value;
    }
    public static void SetValue27(Tuple tuple, T27 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (27 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (27 << 1));
      t.Value27 = value;
    }
    public static void SetValue28(Tuple tuple, T28 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (28 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (28 << 1));
      t.Value28 = value;
    }
    public static void SetValue29(Tuple tuple, T29 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29>)tuple;
      const long mask = 3L << (29 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (29 << 1));
      t.Value29 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21),
        typeof (T22),
        typeof (T23),
        typeof (T24),
        typeof (T25),
        typeof (T26),
        typeof (T27),
        typeof (T28),
        typeof (T29)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Value22 = template.Value22;
      Value23 = template.Value23;
      Value24 = template.Value24;
      Value25 = template.Value25;
      Value26 = template.Value26;
      Value27 = template.Value27;
      Value28 = template.Value28;
      Value29 = template.Value29;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30> : RegularTuple
  {
    private const int count = 31;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public T22 Value22;
    [DataMember]
    public T23 Value23;
    [DataMember]
    public T24 Value24;
    [DataMember]
    public T25 Value25;
    [DataMember]
    public T26 Value26;
    [DataMember]
    public T27 Value27;
    [DataMember]
    public T28 Value28;
    [DataMember]
    public T29 Value29;
    [DataMember]
    public T30 Value30;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
        case 22:
          return Value22;
        case 23:
          return Value23;
        case 24:
          return Value24;
        case 25:
          return Value25;
        case 26:
          return Value26;
        case 27:
          return Value27;
        case 28:
          return Value28;
        case 29:
          return Value29;
        case 30:
          return Value30;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
            break;
          case 22:
            Value22 = default(T22);
            break;
          case 23:
            Value23 = default(T23);
            break;
          case 24:
            Value24 = default(T24);
            break;
          case 25:
            Value25 = default(T25);
            break;
          case 26:
            Value26 = default(T26);
            break;
          case 27:
            Value27 = default(T27);
            break;
          case 28:
            Value28 = default(T28);
            break;
          case 29:
            Value29 = default(T29);
            break;
          case 30:
            Value30 = default(T30);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
          case 22:
            Value22 = (T22)fieldValue;
            break;
          case 23:
            Value23 = (T23)fieldValue;
            break;
          case 24:
            Value24 = (T24)fieldValue;
            break;
          case 25:
            Value25 = (T25)fieldValue;
            break;
          case 26:
            Value26 = (T26)fieldValue;
            break;
          case 27:
            Value27 = (T27)fieldValue;
            break;
          case 28:
            Value28 = (T28)fieldValue;
            break;
          case 29:
            Value29 = (T29)fieldValue;
            break;
          case 30:
            Value30 = (T30)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }
    public static T22 GetValue22(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (22 << 1)) & 3);
      return t.Value22;
    }
    public static T23 GetValue23(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (23 << 1)) & 3);
      return t.Value23;
    }
    public static T24 GetValue24(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (24 << 1)) & 3);
      return t.Value24;
    }
    public static T25 GetValue25(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (25 << 1)) & 3);
      return t.Value25;
    }
    public static T26 GetValue26(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (26 << 1)) & 3);
      return t.Value26;
    }
    public static T27 GetValue27(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (27 << 1)) & 3);
      return t.Value27;
    }
    public static T28 GetValue28(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (28 << 1)) & 3);
      return t.Value28;
    }
    public static T29 GetValue29(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (29 << 1)) & 3);
      return t.Value29;
    }
    public static T30 GetValue30(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (30 << 1)) & 3);
      return t.Value30;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }
    public static void SetValue22(Tuple tuple, T22 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (22 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (22 << 1));
      t.Value22 = value;
    }
    public static void SetValue23(Tuple tuple, T23 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (23 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (23 << 1));
      t.Value23 = value;
    }
    public static void SetValue24(Tuple tuple, T24 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (24 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (24 << 1));
      t.Value24 = value;
    }
    public static void SetValue25(Tuple tuple, T25 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (25 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (25 << 1));
      t.Value25 = value;
    }
    public static void SetValue26(Tuple tuple, T26 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (26 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (26 << 1));
      t.Value26 = value;
    }
    public static void SetValue27(Tuple tuple, T27 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (27 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (27 << 1));
      t.Value27 = value;
    }
    public static void SetValue28(Tuple tuple, T28 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (28 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (28 << 1));
      t.Value28 = value;
    }
    public static void SetValue29(Tuple tuple, T29 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (29 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (29 << 1));
      t.Value29 = value;
    }
    public static void SetValue30(Tuple tuple, T30 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30>)tuple;
      const long mask = 3L << (30 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (30 << 1));
      t.Value30 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21),
        typeof (T22),
        typeof (T23),
        typeof (T24),
        typeof (T25),
        typeof (T26),
        typeof (T27),
        typeof (T28),
        typeof (T29),
        typeof (T30)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Value22 = template.Value22;
      Value23 = template.Value23;
      Value24 = template.Value24;
      Value25 = template.Value25;
      Value26 = template.Value26;
      Value27 = template.Value27;
      Value28 = template.Value28;
      Value29 = template.Value29;
      Value30 = template.Value30;
      Flags = template.Flags;
    }
  }
  
  [DataContract]
  [Serializable]
  public sealed class Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31> : RegularTuple
  {
    private const int count = 32;
    [DataMember]
    public T0 Value0;
    [DataMember]
    public T1 Value1;
    [DataMember]
    public T2 Value2;
    [DataMember]
    public T3 Value3;
    [DataMember]
    public T4 Value4;
    [DataMember]
    public T5 Value5;
    [DataMember]
    public T6 Value6;
    [DataMember]
    public T7 Value7;
    [DataMember]
    public T8 Value8;
    [DataMember]
    public T9 Value9;
    [DataMember]
    public T10 Value10;
    [DataMember]
    public T11 Value11;
    [DataMember]
    public T12 Value12;
    [DataMember]
    public T13 Value13;
    [DataMember]
    public T14 Value14;
    [DataMember]
    public T15 Value15;
    [DataMember]
    public T16 Value16;
    [DataMember]
    public T17 Value17;
    [DataMember]
    public T18 Value18;
    [DataMember]
    public T19 Value19;
    [DataMember]
    public T20 Value20;
    [DataMember]
    public T21 Value21;
    [DataMember]
    public T22 Value22;
    [DataMember]
    public T23 Value23;
    [DataMember]
    public T24 Value24;
    [DataMember]
    public T25 Value25;
    [DataMember]
    public T26 Value26;
    [DataMember]
    public T27 Value27;
    [DataMember]
    public T28 Value28;
    [DataMember]
    public T29 Value29;
    [DataMember]
    public T30 Value30;
    [DataMember]
    public T31 Value31;
    [DataMember]
    public long Flags;

    public override int Count
    {
      get { return count; }
    }

    public override Tuple CreateNew()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>(descriptor);
    }

    public override Tuple Clone()
    {
      return new Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>(this);
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
        case 4:
          return Value4;
        case 5:
          return Value5;
        case 6:
          return Value6;
        case 7:
          return Value7;
        case 8:
          return Value8;
        case 9:
          return Value9;
        case 10:
          return Value10;
        case 11:
          return Value11;
        case 12:
          return Value12;
        case 13:
          return Value13;
        case 14:
          return Value14;
        case 15:
          return Value15;
        case 16:
          return Value16;
        case 17:
          return Value17;
        case 18:
          return Value18;
        case 19:
          return Value19;
        case 20:
          return Value20;
        case 21:
          return Value21;
        case 22:
          return Value22;
        case 23:
          return Value23;
        case 24:
          return Value24;
        case 25:
          return Value25;
        case 26:
          return Value26;
        case 27:
          return Value27;
        case 28:
          return Value28;
        case 29:
          return Value29;
        case 30:
          return Value30;
        case 31:
          return Value31;
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
          case 4:
            Value4 = default(T4);
            break;
          case 5:
            Value5 = default(T5);
            break;
          case 6:
            Value6 = default(T6);
            break;
          case 7:
            Value7 = default(T7);
            break;
          case 8:
            Value8 = default(T8);
            break;
          case 9:
            Value9 = default(T9);
            break;
          case 10:
            Value10 = default(T10);
            break;
          case 11:
            Value11 = default(T11);
            break;
          case 12:
            Value12 = default(T12);
            break;
          case 13:
            Value13 = default(T13);
            break;
          case 14:
            Value14 = default(T14);
            break;
          case 15:
            Value15 = default(T15);
            break;
          case 16:
            Value16 = default(T16);
            break;
          case 17:
            Value17 = default(T17);
            break;
          case 18:
            Value18 = default(T18);
            break;
          case 19:
            Value19 = default(T19);
            break;
          case 20:
            Value20 = default(T20);
            break;
          case 21:
            Value21 = default(T21);
            break;
          case 22:
            Value22 = default(T22);
            break;
          case 23:
            Value23 = default(T23);
            break;
          case 24:
            Value24 = default(T24);
            break;
          case 25:
            Value25 = default(T25);
            break;
          case 26:
            Value26 = default(T26);
            break;
          case 27:
            Value27 = default(T27);
            break;
          case 28:
            Value28 = default(T28);
            break;
          case 29:
            Value29 = default(T29);
            break;
          case 30:
            Value30 = default(T30);
            break;
          case 31:
            Value31 = default(T31);
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
          case 4:
            Value4 = (T4)fieldValue;
            break;
          case 5:
            Value5 = (T5)fieldValue;
            break;
          case 6:
            Value6 = (T6)fieldValue;
            break;
          case 7:
            Value7 = (T7)fieldValue;
            break;
          case 8:
            Value8 = (T8)fieldValue;
            break;
          case 9:
            Value9 = (T9)fieldValue;
            break;
          case 10:
            Value10 = (T10)fieldValue;
            break;
          case 11:
            Value11 = (T11)fieldValue;
            break;
          case 12:
            Value12 = (T12)fieldValue;
            break;
          case 13:
            Value13 = (T13)fieldValue;
            break;
          case 14:
            Value14 = (T14)fieldValue;
            break;
          case 15:
            Value15 = (T15)fieldValue;
            break;
          case 16:
            Value16 = (T16)fieldValue;
            break;
          case 17:
            Value17 = (T17)fieldValue;
            break;
          case 18:
            Value18 = (T18)fieldValue;
            break;
          case 19:
            Value19 = (T19)fieldValue;
            break;
          case 20:
            Value20 = (T20)fieldValue;
            break;
          case 21:
            Value21 = (T21)fieldValue;
            break;
          case 22:
            Value22 = (T22)fieldValue;
            break;
          case 23:
            Value23 = (T23)fieldValue;
            break;
          case 24:
            Value24 = (T24)fieldValue;
            break;
          case 25:
            Value25 = (T25)fieldValue;
            break;
          case 26:
            Value26 = (T26)fieldValue;
            break;
          case 27:
            Value27 = (T27)fieldValue;
            break;
          case 28:
            Value28 = (T28)fieldValue;
            break;
          case 29:
            Value29 = (T29)fieldValue;
            break;
          case 30:
            Value30 = (T30)fieldValue;
            break;
          case 31:
            Value31 = (T31)fieldValue;
            break;
        }
      }
    }

    public static T0 GetValue0(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (0 << 1)) & 3);
      return t.Value0;
    }
    public static T1 GetValue1(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (1 << 1)) & 3);
      return t.Value1;
    }
    public static T2 GetValue2(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (2 << 1)) & 3);
      return t.Value2;
    }
    public static T3 GetValue3(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (3 << 1)) & 3);
      return t.Value3;
    }
    public static T4 GetValue4(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (4 << 1)) & 3);
      return t.Value4;
    }
    public static T5 GetValue5(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (5 << 1)) & 3);
      return t.Value5;
    }
    public static T6 GetValue6(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (6 << 1)) & 3);
      return t.Value6;
    }
    public static T7 GetValue7(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (7 << 1)) & 3);
      return t.Value7;
    }
    public static T8 GetValue8(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (8 << 1)) & 3);
      return t.Value8;
    }
    public static T9 GetValue9(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (9 << 1)) & 3);
      return t.Value9;
    }
    public static T10 GetValue10(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (10 << 1)) & 3);
      return t.Value10;
    }
    public static T11 GetValue11(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (11 << 1)) & 3);
      return t.Value11;
    }
    public static T12 GetValue12(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (12 << 1)) & 3);
      return t.Value12;
    }
    public static T13 GetValue13(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (13 << 1)) & 3);
      return t.Value13;
    }
    public static T14 GetValue14(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (14 << 1)) & 3);
      return t.Value14;
    }
    public static T15 GetValue15(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (15 << 1)) & 3);
      return t.Value15;
    }
    public static T16 GetValue16(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (16 << 1)) & 3);
      return t.Value16;
    }
    public static T17 GetValue17(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (17 << 1)) & 3);
      return t.Value17;
    }
    public static T18 GetValue18(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (18 << 1)) & 3);
      return t.Value18;
    }
    public static T19 GetValue19(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (19 << 1)) & 3);
      return t.Value19;
    }
    public static T20 GetValue20(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (20 << 1)) & 3);
      return t.Value20;
    }
    public static T21 GetValue21(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (21 << 1)) & 3);
      return t.Value21;
    }
    public static T22 GetValue22(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (22 << 1)) & 3);
      return t.Value22;
    }
    public static T23 GetValue23(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (23 << 1)) & 3);
      return t.Value23;
    }
    public static T24 GetValue24(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (24 << 1)) & 3);
      return t.Value24;
    }
    public static T25 GetValue25(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (25 << 1)) & 3);
      return t.Value25;
    }
    public static T26 GetValue26(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (26 << 1)) & 3);
      return t.Value26;
    }
    public static T27 GetValue27(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (27 << 1)) & 3);
      return t.Value27;
    }
    public static T28 GetValue28(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (28 << 1)) & 3);
      return t.Value28;
    }
    public static T29 GetValue29(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (29 << 1)) & 3);
      return t.Value29;
    }
    public static T30 GetValue30(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (30 << 1)) & 3);
      return t.Value30;
    }
    public static T31 GetValue31(Tuple tuple, out TupleFieldState fieldState)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      fieldState = (TupleFieldState) ((t.Flags >> (31 << 1)) & 3);
      return t.Value31;
    }

    public static void SetValue0(Tuple tuple, T0 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (0 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (0 << 1));
      t.Value0 = value;
    }
    public static void SetValue1(Tuple tuple, T1 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (1 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (1 << 1));
      t.Value1 = value;
    }
    public static void SetValue2(Tuple tuple, T2 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (2 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (2 << 1));
      t.Value2 = value;
    }
    public static void SetValue3(Tuple tuple, T3 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (3 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (3 << 1));
      t.Value3 = value;
    }
    public static void SetValue4(Tuple tuple, T4 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (4 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (4 << 1));
      t.Value4 = value;
    }
    public static void SetValue5(Tuple tuple, T5 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (5 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (5 << 1));
      t.Value5 = value;
    }
    public static void SetValue6(Tuple tuple, T6 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (6 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (6 << 1));
      t.Value6 = value;
    }
    public static void SetValue7(Tuple tuple, T7 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (7 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (7 << 1));
      t.Value7 = value;
    }
    public static void SetValue8(Tuple tuple, T8 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (8 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (8 << 1));
      t.Value8 = value;
    }
    public static void SetValue9(Tuple tuple, T9 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (9 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (9 << 1));
      t.Value9 = value;
    }
    public static void SetValue10(Tuple tuple, T10 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (10 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (10 << 1));
      t.Value10 = value;
    }
    public static void SetValue11(Tuple tuple, T11 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (11 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (11 << 1));
      t.Value11 = value;
    }
    public static void SetValue12(Tuple tuple, T12 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (12 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (12 << 1));
      t.Value12 = value;
    }
    public static void SetValue13(Tuple tuple, T13 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (13 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (13 << 1));
      t.Value13 = value;
    }
    public static void SetValue14(Tuple tuple, T14 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (14 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (14 << 1));
      t.Value14 = value;
    }
    public static void SetValue15(Tuple tuple, T15 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (15 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (15 << 1));
      t.Value15 = value;
    }
    public static void SetValue16(Tuple tuple, T16 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (16 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (16 << 1));
      t.Value16 = value;
    }
    public static void SetValue17(Tuple tuple, T17 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (17 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (17 << 1));
      t.Value17 = value;
    }
    public static void SetValue18(Tuple tuple, T18 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (18 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (18 << 1));
      t.Value18 = value;
    }
    public static void SetValue19(Tuple tuple, T19 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (19 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (19 << 1));
      t.Value19 = value;
    }
    public static void SetValue20(Tuple tuple, T20 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (20 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (20 << 1));
      t.Value20 = value;
    }
    public static void SetValue21(Tuple tuple, T21 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (21 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (21 << 1));
      t.Value21 = value;
    }
    public static void SetValue22(Tuple tuple, T22 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (22 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (22 << 1));
      t.Value22 = value;
    }
    public static void SetValue23(Tuple tuple, T23 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (23 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (23 << 1));
      t.Value23 = value;
    }
    public static void SetValue24(Tuple tuple, T24 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (24 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (24 << 1));
      t.Value24 = value;
    }
    public static void SetValue25(Tuple tuple, T25 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (25 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (25 << 1));
      t.Value25 = value;
    }
    public static void SetValue26(Tuple tuple, T26 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (26 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (26 << 1));
      t.Value26 = value;
    }
    public static void SetValue27(Tuple tuple, T27 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (27 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (27 << 1));
      t.Value27 = value;
    }
    public static void SetValue28(Tuple tuple, T28 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (28 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (28 << 1));
      t.Value28 = value;
    }
    public static void SetValue29(Tuple tuple, T29 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (29 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (29 << 1));
      t.Value29 = value;
    }
    public static void SetValue30(Tuple tuple, T30 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (30 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (30 << 1));
      t.Value30 = value;
    }
    public static void SetValue31(Tuple tuple, T31 value)
    {
      var t = (Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31>)tuple;
      const long mask = 3L << (31 << 1);
      if (value == null) 
        t.Flags |= mask;
      else
        t.Flags = (t.Flags & ~mask) | (1L << (31 << 1));
      t.Value31 = value;
    }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
      descriptor = TupleDescriptor.Create(new[] {
        typeof (T0),
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6),
        typeof (T7),
        typeof (T8),
        typeof (T9),
        typeof (T10),
        typeof (T11),
        typeof (T12),
        typeof (T13),
        typeof (T14),
        typeof (T15),
        typeof (T16),
        typeof (T17),
        typeof (T18),
        typeof (T19),
        typeof (T20),
        typeof (T21),
        typeof (T22),
        typeof (T23),
        typeof (T24),
        typeof (T25),
        typeof (T26),
        typeof (T27),
        typeof (T28),
        typeof (T29),
        typeof (T30),
        typeof (T31)
      });
    }


    // Constructors

    public Tuple(TupleDescriptor descriptor)
      : base(descriptor)
    {
    }

    private Tuple(Tuple<T0,T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,T17,T18,T19,T20,T21,T22,T23,T24,T25,T26,T27,T28,T29,T30,T31> template)
      : base(template.descriptor)
    {
      Value0 = template.Value0;
      Value1 = template.Value1;
      Value2 = template.Value2;
      Value3 = template.Value3;
      Value4 = template.Value4;
      Value5 = template.Value5;
      Value6 = template.Value6;
      Value7 = template.Value7;
      Value8 = template.Value8;
      Value9 = template.Value9;
      Value10 = template.Value10;
      Value11 = template.Value11;
      Value12 = template.Value12;
      Value13 = template.Value13;
      Value14 = template.Value14;
      Value15 = template.Value15;
      Value16 = template.Value16;
      Value17 = template.Value17;
      Value18 = template.Value18;
      Value19 = template.Value19;
      Value20 = template.Value20;
      Value21 = template.Value21;
      Value22 = template.Value22;
      Value23 = template.Value23;
      Value24 = template.Value24;
      Value25 = template.Value25;
      Value26 = template.Value26;
      Value27 = template.Value27;
      Value28 = template.Value28;
      Value29 = template.Value29;
      Value30 = template.Value30;
      Value31 = template.Value31;
      Flags = template.Flags;
    }
  }
  
  internal static class TupleFactory
  {
    public static RegularTuple Create(TupleDescriptor descriptor)
    {
      var fieldCount = descriptor.Count;
      if (fieldCount < 0)
        throw new ArgumentOutOfRangeException("fieldCount");
      if (fieldCount > 32)
        throw new NotImplementedException();
      if (fieldCount == 0)
        return EmptyTuple.Instance;
      Type tupleDef = null;
      switch (fieldCount) {
        case 1:
          tupleDef = typeof(Tuple< >);
          break;
        case 2:
          tupleDef = typeof(Tuple< , >);
          break;
        case 3:
          tupleDef = typeof(Tuple< , , >);
          break;
        case 4:
          tupleDef = typeof(Tuple< , , , >);
          break;
        case 5:
          tupleDef = typeof(Tuple< , , , , >);
          break;
        case 6:
          tupleDef = typeof(Tuple< , , , , , >);
          break;
        case 7:
          tupleDef = typeof(Tuple< , , , , , , >);
          break;
        case 8:
          tupleDef = typeof(Tuple< , , , , , , , >);
          break;
        case 9:
          tupleDef = typeof(Tuple< , , , , , , , , >);
          break;
        case 10:
          tupleDef = typeof(Tuple< , , , , , , , , , >);
          break;
        case 11:
          tupleDef = typeof(Tuple< , , , , , , , , , , >);
          break;
        case 12:
          tupleDef = typeof(Tuple< , , , , , , , , , , , >);
          break;
        case 13:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , >);
          break;
        case 14:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , >);
          break;
        case 15:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , >);
          break;
        case 16:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , >);
          break;
        case 17:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , >);
          break;
        case 18:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , >);
          break;
        case 19:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , >);
          break;
        case 20:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , >);
          break;
        case 21:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 22:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 23:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 24:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 25:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 26:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 27:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 28:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 29:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 30:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 31:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , >);
          break;
        case 32:
          tupleDef = typeof(Tuple< , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , , >);
          break;
      }
      var tupleType = tupleDef.MakeGenericType(descriptor.fieldTypes);
      return (RegularTuple)Activator.CreateInstance(tupleType, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new object[]{descriptor}, null);
    }
  }
}
