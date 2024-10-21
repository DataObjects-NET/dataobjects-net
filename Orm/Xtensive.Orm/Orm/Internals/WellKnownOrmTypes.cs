// Copyright (C) 2020-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Validation;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals
{
  internal static class WellKnownOrmTypes
  {
    public static readonly Type ApplyParameter = typeof(ApplyParameter);

    public static readonly Type Entity = typeof(Entity);
    public static readonly Type EntitySetBase = typeof(EntitySetBase);
    public static readonly Type EntitySetOfT = typeof(EntitySet<>);
    public static readonly Type EntitySetItemOfT1T2 = typeof(EntitySetItem<,>);

    public static readonly Type Key = typeof(Key);
    public static readonly Type KeyOfT = typeof(Key<>);

    public static readonly Type Parameter = typeof(Parameter);
    public static readonly Type ParameterContext = typeof(ParameterContext);
    public static readonly Type ParameterOfT = typeof(Parameter<>);
    public static readonly Type ParameterOfTuple = typeof(Parameter<Tuple>);

    public static readonly Type Persistent = typeof(Persistent);

    public static readonly Type Structure = typeof(Structure);

    public static readonly Type Tuple = typeof(Tuple);

    public static readonly Type Session = typeof(Session);
    public static readonly Type Query = typeof(Query);
    public static readonly Type QueryEndpoint = typeof(QueryEndpoint);
    public static readonly Type QueryProvider = typeof(QueryProvider);

    public static readonly Type TranslatedQuery = typeof(TranslatedQuery);
    public static readonly Type GroupingOfTKeyTElement = typeof(Grouping<,>);
    public static readonly Type SubQueryOfT = typeof(SubQuery<>);

    public static readonly Type FullTextMatchOfT = typeof(FullTextMatch<>);

    public static readonly Type ItemMaterializationContext = typeof (ItemMaterializationContext);

    public static readonly Type ProjectionExpression = typeof(ProjectionExpression);

  }

  internal static class WellKnownOrmInterfaces
  {
    public static readonly Type Entity = typeof(IEntity);
    public static readonly Type ObjectValidator = typeof(IObjectValidator);
    public static readonly Type PropertyValidator = typeof(IPropertyValidator);
  }

}