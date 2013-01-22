// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.29

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xtensive.Tuples.Packed
{
  internal static class PackedTupleAccessor
  {
    private abstract class FieldAccessor
    {
      public Delegate Getter;
      public Delegate Setter;

      public Delegate NullableGetter;
      public Delegate NullableSetter;

      public Func<long, object> Boxer;
      public Func<object, long> Unboxer;
    }

    private class FieldAccessor<T> : FieldAccessor
      where T : struct
    {
      private readonly Func<T, long> packer;
      private readonly Func<long, T> unpacker;

      private object Box(long value)
      {
        return unpacker.Invoke(value);
      }

      private long Unbox(object value)
      {
        return packer.Invoke((T) value);
      }

      public T GetValue(Tuple tuple, int fieldIndex, out TupleFieldState state)
      {
        var packedTuple = (PackedTuple) tuple;
        state = tuple.GetFieldState(fieldIndex);
        return unpacker.Invoke(packedTuple.GetPackedValue(fieldIndex));
      }

      public void SetValue(Tuple tuple, int fieldIndex, T value)
      {
        var packedTuple = (PackedTuple) tuple;
        packedTuple.SetPackedValue(fieldIndex, packer.Invoke(value));
      }

      public T? GetNullableValue(Tuple tuple, int fieldIndex, out TupleFieldState fieldState)
      {
        var value = GetValue(tuple, fieldIndex, out fieldState);
        if (fieldState==TupleFieldState.Available)
          return value;
        return null;
      }

      public void SetNullableValue(Tuple tuple, int fieldIndex, T? value)
      {
        if (value.HasValue)
          SetValue(tuple, fieldIndex, value.Value);
        else
          tuple.SetValue(fieldIndex, null);
      }

      public FieldAccessor(MethodInfo packerMethod, MethodInfo unpackerMethod)
      {
        packer = (Func<T, long>) Delegate.CreateDelegate(typeof (Func<T, long>), packerMethod);
        unpacker = (Func<long, T>) Delegate.CreateDelegate(typeof (Func<long, T>), unpackerMethod);

        Getter = (GetValueDelegate<T>) GetValue;
        Setter = (SetValueDelegate<T>) SetValue;

        NullableGetter = (GetValueDelegate<T?>) GetNullableValue;
        NullableSetter = (SetValueDelegate<T?>) SetNullableValue;

        Boxer = Box;
        Unboxer = Unbox;
      }
    }

    private static readonly Dictionary<Type, FieldAccessor> Accessors;

    public static bool TryGetAccessors(Type valueType, PackedTupleFieldDescriptor descriptor)
    {
      FieldAccessor accessor;
      
      if (!Accessors.TryGetValue(valueType, out accessor))
        return false;

      descriptor.GetValueDelegate = accessor.Getter;
      descriptor.SetValueDelegate = accessor.Setter;

      descriptor.GetNullableValueDelegate = accessor.NullableGetter;
      descriptor.SetNullableValueDelegate = accessor.NullableSetter;

      descriptor.Boxer = accessor.Boxer;
      descriptor.Unboxer = accessor.Unboxer;

      return true;
    }

    #region Pack methods

    private static long PackBool(bool value)
    {
      return value ? 1L : 0L;
    }

    private static long PackFloat(float value)
    {
      return BitConverter.DoubleToInt64Bits(value);
    }

    private static long PackDouble(double value)
    {
      return BitConverter.DoubleToInt64Bits(value);
    }

    private static long PackTimeSpan(TimeSpan value)
    {
      return value.Ticks;
    }

    private static long PackDateTime(DateTime value)
    {
      return value.ToBinary();
    }

    private static long PackByte(byte value)
    {
      return value;
    }

    private static long PackSByte(sbyte value)
    {
      return value;
    }

    private static long PackShort(short value)
    {
      return value;
    }

    private static long PackUShort(ushort value)
    {
      return value;
    }

    private static long PackInt(int value)
    {
      return value;
    }

    private static long PackUInt(uint value)
    {
      return value;
    }

    private static long PackLong(long value)
    {
      return value;
    }

    private static long PackULong(ulong value)
    {
      return unchecked((long) value);
    }

    #endregion

    #region Unpack methods

    private static bool UnpackBool(long value)
    {
      return value!=0L;
    }

    private static float UnpackFloat(long value)
    {
      return (float) BitConverter.Int64BitsToDouble(value);
    }

    private static double UnpackDouble(long value)
    {
      return BitConverter.Int64BitsToDouble(value);
    }

    private static TimeSpan UnpackTimeSpan(long value)
    {
      return TimeSpan.FromTicks(value);
    }

    private static DateTime UnpackDateTime(long value)
    {
      return DateTime.FromBinary(value);
    }

    private static byte UnpackByte(long value)
    {
      return (byte) value;
    }

    private static sbyte UnpackSByte(long value)
    {
      return (sbyte) value;
    }

    private static short UnpackShort(long value)
    {
      return (short) value;
    }

    private static ushort UnpackUShort(long value)
    {
      return (ushort) value;
    }

    private static int UnpackInt(long value)
    {
      return (int) value;
    }

    private static uint UnpackUInt(long value)
    {
      return (uint) value;
    }

    private static long UnpackLong(long value)
    {
      return value;
    }

    private static ulong UnpackULong(long value)
    {
      return unchecked((ulong) value);
    }

    #endregion

    static PackedTupleAccessor()
    {
      var allMethods = typeof (PackedTupleAccessor)
        .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
        .ToList();

      var packers = allMethods
        .Where(m => m.Name.StartsWith("Pack"))
        .Select(m => new {Method = m, Type = m.GetParameters()[0].ParameterType});

      var unpackers = allMethods
        .Where(m => m.Name.StartsWith("Unpack"))
        .Select(m => new {Method = m, Type = m.ReturnType});

      var items =
        from p in packers
        join u in unpackers
          on p.Type equals u.Type
        let accessorType = typeof (FieldAccessor<>).MakeGenericType(p.Type)
        select new {
          p.Type,
          Accessor = (FieldAccessor) Activator.CreateInstance(accessorType, p.Method, u.Method),
        };

      Accessors = items.ToDictionary(i => i.Type, i => i.Accessor);
    }
  }
}