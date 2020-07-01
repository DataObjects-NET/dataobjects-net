using System;

namespace Xtensive.Orm.Internals
{
  internal static class WellKnownOrmTypes
  {
    public static readonly Type Entity = typeof(Entity);
    public static readonly Type EntitySetOfT = typeof(EntitySet<>);
    public static readonly Type EntitySetItemOfT1T2 = typeof(EntitySetItem<,>);
  }
}