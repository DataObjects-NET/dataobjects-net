// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.24

using System;
using System.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.Tests.Tuples
{
  public class DummyTuple: Tuple
  {
    private TupleDescriptor descriptor;
    private BitArray available;
    private object[] values;

    public override TupleDescriptor Descriptor
    {
      get { return descriptor; }
    }

    public object this[int fieldIndex]
    {
      get { return GetValue(fieldIndex); }
      set { SetValue(fieldIndex, value); }
    }

    #region CreateNew, Clone methods

    public override Tuple CreateNew()
    {
      return new DummyTuple(descriptor);
    }

    public override Tuple Clone()
    {
      DummyTuple tuple = (DummyTuple)CreateNew();
      tuple.values = (object[])values.Clone();
      tuple.available = available;
      return tuple;
    }

    #endregion

    #region GetFieldState, GetValueOrDefault methods

    public override TupleFieldState GetFieldState(int fieldIndex)
    {
      if (!available[fieldIndex])
        return 0;
      else {
        return values[fieldIndex] == null ?
          TupleFieldState.IsNull | TupleFieldState.IsAvailable :
          TupleFieldState.IsAvailable;
      }
    }

    public override T GetValueOrDefault<T>(int fieldIndex)
    {
      if (!typeof(T).IsAssignableFrom(descriptor[fieldIndex]))
        throw new InvalidCastException();
      return IsAvailable(fieldIndex) ? (T)values[fieldIndex] : default(T);
    }

    public override object GetValueOrDefault(int fieldIndex)
    {
      if (IsAvailable(fieldIndex))
        return values[fieldIndex];
      if (descriptor[fieldIndex].IsClass)
        return null;
      return Activator.CreateInstance(descriptor[fieldIndex]);
    }

    #endregion

    #region SetValue methods

    public override void SetValue<T>(int fieldIndex, T fieldValue)
    {
      if (!typeof(T).IsAssignableFrom(descriptor[fieldIndex]))
        throw new InvalidCastException();
      values[fieldIndex] = fieldValue;
      available[fieldIndex] = true;
    }

    public override void SetValue(int fieldIndex, object fieldValue)
    {
      if (fieldValue != null && !descriptor[fieldIndex].IsAssignableFrom(fieldValue.GetType()))
        throw new InvalidCastException();
      values[fieldIndex] = fieldValue;
      available[fieldIndex] = true;
    }

    #endregion


    // Constructors

    public DummyTuple(TupleDescriptor descriptor)
    {
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "descriptor");
      this.descriptor = descriptor;
      values = new object[descriptor.Count];
      available = new BitArray(new bool[descriptor.Count]);
    }
  }
}