// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.01

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  internal abstract class ItemToTupleConverter : IEnumerable<Tuple>
  {
    public abstract IEnumerator<Tuple> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public TupleDescriptor TupleDescriptor { get; protected set; }

    public Expression Expression { get; protected set; }

    public static ItemToTupleConverter BuildConverter(Type type, object enumerable, int[] columnMap, DomainModel model)
    {
      return (ItemToTupleConverter) typeof (ItemToTupleConverter<>)
        .MakeGenericType(type)
        .GetConstructors()[0]
        .Invoke(new[]{enumerable, columnMap, model});
    }
  }
}