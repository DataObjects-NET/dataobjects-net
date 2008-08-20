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
    private Func<Tuple> getNext;
    private Tuple counter;

    protected override Tuple NextNumber()
    {
      return getNext();
    }

// ReSharper disable UnusedPrivateMember
    private Tuple NextNumberInternal<TFieldType>()
// ReSharper restore UnusedPrivateMember
    {
      Tuple result = Tuple.Create(Hierarchy.KeyTupleDescriptor);
      Arithmetic<TFieldType> ar = Arithmetic<TFieldType>.Default;
      lock (this) {
        counter.SetValue(0, ar.Add(counter.GetValue<TFieldType>(0), ar.One));
        result.SetValue(0, counter.GetValue<TFieldType>(0));
      }
      return result;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      base.Initialize();
      counter = Tuple.Create(Hierarchy.KeyTupleDescriptor);
      counter.SetValue(0, counter.GetValueOrDefault(0));

      Type fieldType = Hierarchy.KeyTupleDescriptor[0];
      var method = typeof(DefaultGenerator)
        .GetMethod("NextNumberInternal", BindingFlags.Instance | BindingFlags.NonPublic)
        .MakeGenericMethod(new[] { fieldType });
      getNext = (Func<Tuple>) Delegate.CreateDelegate(typeof (Func<Tuple>), this, method);
    }
  }
}