using System;
using JetBrains.Annotations;

namespace Xtensive.Orm.Weaving
{
  /// <summary>
  /// Identifies auxiliary type.
  /// You should not use this attribute directly.
  /// It is automatically applied when needed.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  [UsedImplicitly]
  public sealed class AuxiliaryTypeAttribute : Attribute
  {
  }
}