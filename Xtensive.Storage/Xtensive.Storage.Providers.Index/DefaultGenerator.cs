// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.24

using System;
using System.Reflection;
using Xtensive.Core.Arithmetic;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Index
{
  [Serializable]
  public class DefaultGenerator : Storage.DefaultGenerator
  {
    private Delegate getNext;
    private Tuple counter;

    private delegate void GetNext<TFieldType>(Tuple counter, Tuple result);

    /// <inheritdoc/>
    public override void Initialize()
    {
      counter = Tuple.Create(Hierarchy.TupleDescriptor);
      counter.SetValue(0, counter.GetValueOrDefault(0));

      Type fieldType = Hierarchy.TupleDescriptor[0];
      Type delegateType = typeof (GetNext<>).MakeGenericType(new[] {fieldType});

      getNext = Delegate.CreateDelegate(delegateType, this, GetGenericMethodFor(fieldType));
    }

    /// <inheritdoc/>
    public override Tuple Next()
    {
      Tuple result = Tuple.Create(Hierarchy.TupleDescriptor);
      getNext.DynamicInvoke(counter, result);
      return result;
    }

    private void NextInteger<TFieldType>(Tuple counter, Tuple result)
    {
      Arithmetic<TFieldType> ar = Arithmetic<TFieldType>.Default;
      counter.SetValue(0, ar.Add(counter.GetValue<TFieldType>(0), ar.One));
      result.SetValue(0, counter.GetValue<TFieldType>(0));
    }

    private void NextGuid<TFieldType>(Tuple counter, Tuple result)
    {
      result.SetValue(0, Guid.NewGuid());
    }

    private static MethodInfo GetGenericMethodFor(Type fieldType)
    {
      string methodName = fieldType==typeof (Guid) ? "NextGuid" : "NextInteger";
      MethodInfo mi = typeof (DefaultGenerator).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
      return mi.MakeGenericMethod(new[] {fieldType});
    }
  }
}