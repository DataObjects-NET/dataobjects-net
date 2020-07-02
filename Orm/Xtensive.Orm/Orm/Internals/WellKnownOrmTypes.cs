using System;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Rse;
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
    public static readonly Type ParameterOfT = typeof(Parameter<>);
    public static readonly Type ParameterOfTuple = typeof(Parameter<Tuple>);

    public static readonly Type Persistent = typeof(Persistent);

    public static readonly Type Structure = typeof(Structure);

    public static readonly Type Tuple = typeof(Tuple);

    public static readonly Type Query = typeof(Query);
    public static readonly Type QueryProvider = typeof(QueryProvider);
  }

  internal static class WellKnownOrmInterfaces
  {
    public static readonly Type Entity = typeof(IEntity);
  }

}