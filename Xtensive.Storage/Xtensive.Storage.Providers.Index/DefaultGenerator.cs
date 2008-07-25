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
    private const string NEXTGUID = "NextGuid";
    private const string NEXTINTEGER = "NextInteger";
    private Action<Tuple,Tuple> getNext;
    private Tuple counter;

    /// <inheritdoc/>
    public override void Initialize()
    {
      counter = Tuple.Create(Hierarchy.TupleDescriptor);
      counter.SetValue(0, counter.GetValueOrDefault(0));

      Type fieldType = Hierarchy.TupleDescriptor[0];
      getNext = (Action<Tuple,Tuple>) Delegate.CreateDelegate(typeof (Action<Tuple, Tuple>), this, GetGenericMethodFor(fieldType));
    }

    /// <inheritdoc/>
    public override Tuple Next()
    {
      Tuple result = Tuple.Create(Hierarchy.TupleDescriptor);
      getNext(counter, result);
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
      string methodName = fieldType==typeof (Guid) ? NEXTGUID : NEXTINTEGER;
      MethodInfo mi = typeof (DefaultGenerator).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
      return mi.MakeGenericMethod(new[] {fieldType});
    }
  }
}