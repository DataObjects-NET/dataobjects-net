// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.10.01

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Tuples;
using Xtensive.Reflection;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal abstract class ItemToTupleConverter
  {
    private static readonly Type ItemToTupleConverterType = typeof(ItemToTupleConverter<>);
    protected static readonly Type RefOfTType = typeof(Ref<>);

    public abstract Expression<Func<ParameterContext, IEnumerable<Tuple>>> GetEnumerable();

    public TupleDescriptor TupleDescriptor { get; protected set; }

    public Expression Expression { get; protected set; }

    public static ItemToTupleConverter BuildConverter(Type type, Type storedEntityType, object enumerable, DomainModel model, Expression sourceExpression)
    {
      return (ItemToTupleConverter) ItemToTupleConverterType
        .CachedMakeGenericType(type)
        .GetConstructors()[0]
        .Invoke(new[] { enumerable, model, sourceExpression, storedEntityType });
    }
  }
}