// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System.Collections.ObjectModel;

namespace Xtensive.Orm.Weaver
{
  internal static class WellKnown
  {
    public static readonly ReadOnlyCollection<byte> XtensivePublicKeyToken;

    public static readonly string OrmAssemblyFullName;

    public static readonly string EntityType = "Xtensive.Orm.Entity";
    public static readonly string EntityInterfaceType = "Xtensive.Orm.IEntity";
    public static readonly string EntitySetType = "Xtensive.Orm.EntitySet`1";
    public static readonly string EntitySetItemType = "Xtensive.Orm.Internals.EntitySetItem`2";
    public static readonly string StructureType = "Xtensive.Orm.Structure";

    public static readonly string KeyAttribute = "Xtensive.Orm.KeyAttribute";
    public static readonly string FieldAttribute = "Xtensive.Orm.FieldAttribute";

    public static readonly string ProcessedByWeaverAttribute = "Xtensive.Orm.Weaving.ProcessedByWeaverAttribute";
    public static readonly string EntityTypeAttribute = "Xtensive.Orm.Weaving.EntityTypeAttribute";
    public static readonly string EntityInterfaceAttribute = "Xtensive.Orm.Weaving.EntityInterfaceAttribute";
    public static readonly string EntitySetTypeAttribute = "Xtensive.Orm.Weaving.EntitySetTypeAttribute";
    public static readonly string StructureTypeAttribute = "Xtensive.Orm.Weaving.StructureTypeAttribute";
    public static readonly string OverrideFieldNameAttribute = "Xtensive.Orm.Weaving.OverrideFieldNameAttribute";

    public static readonly string CompilerGeneratedAttribute = "System.Runtime.CompilerServices.CompilerGeneratedAttribute";

    public static readonly string Constructor = ".ctor";
    public static readonly string FactoryMethod = "~Xtensive.Orm.CreateObject";

    private static string GetFullAssemblyName(string shortName)
    {
      return string.Format(
        "{0}, Version={1}, Culture=neutral, PublicKeyToken={2}",
        shortName, ThisAssembly.Version, ThisAssembly.PublicKeyToken);
    }

    static WellKnown()
    {
      OrmAssemblyFullName = GetFullAssemblyName("Xtensive.Orm");
      XtensivePublicKeyToken = new ReadOnlyCollection<byte>(WeavingHelper.ParsePublicKeyToken(ThisAssembly.PublicKeyToken));
    }
  }
}