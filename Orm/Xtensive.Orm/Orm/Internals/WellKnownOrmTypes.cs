using System;
using Xtensive.Orm.Linq;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals
{
  internal static class WellKnownOrmTypes
  {
    public static readonly Type Entity = typeof(Entity);
    public static readonly Type EntitySetBase = typeof(EntitySetBase);
    public static readonly Type EntitySetOfT = typeof(EntitySet<>);
    public static readonly Type EntitySetItemOfT1T2 = typeof(EntitySetItem<,>);

    public static readonly Type Key = typeof(Key);
    public static readonly Type KeyOfT = typeof(Key<>);

    public static readonly Type Structure = typeof(Structure);

    public static readonly Type Tuple = typeof(Tuple);

    public static readonly Type Query = typeof(Query);
    public static readonly Type QueryProvider = typeof(QueryProvider);
  }
}