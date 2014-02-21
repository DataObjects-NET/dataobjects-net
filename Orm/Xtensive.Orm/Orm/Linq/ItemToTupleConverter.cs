// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal abstract class ItemToTupleConverter
  {
    public abstract Expression<Func<IEnumerable<Tuple>>> GetEnumerable();

    public TupleDescriptor TupleDescriptor { get; protected set; }

    public Expression Expression { get; protected set; }

    public static ItemToTupleConverter BuildConverter(Type type, Type storedEntityType, object enumerable, DomainModel model, Expression sourceExpression)
    {
      return (ItemToTupleConverter) typeof (ItemToTupleConverter<>)
        .MakeGenericType(type)
        .GetConstructors()[0]
        .Invoke(new[] { enumerable, model, sourceExpression, storedEntityType });
    }
  }
}