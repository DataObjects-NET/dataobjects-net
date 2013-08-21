// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

namespace Xtensive.Orm.Weaver
{
  internal static class WellKnown
  {
    public static readonly string OrmAssemblyFullName;
    public static readonly string CoreAssemblyFullName;

    public static readonly string EntityType = "Xtensive.Orm.Entity";
    public static readonly string StructureType = "Xtensive.Orm.Structure";

    public static readonly string FieldAttribute = "Xtensive.Orm.FieldAttribute";
    public static readonly string ProcessedByWeaverAttribute = "Xtensive.Orm.ProcessedByWeaverAttribute";
    public static readonly string EntityTypeAttribute = "Xtensive.Orm.EntityTypeAttribute";
    public static readonly string StructureTypeAttribute = "Xtensive.Orm.StructureTypeAttribute";

    public static readonly string CompilerGeneratedAttribute = "System.Runtime.CompilerServices.CompilerGeneratedAttribute";

    public static readonly string Constructor = ".ctor";
    public static readonly string FactoryMethod = "~Xtensive.Aspects.FactoryMethod";

    private static string GetFullAssemblyName(string shortName)
    {
      return string.Format(
        "{0}, Version={1}, Culture=neutral, PublicKeyToken={2}",
        shortName, ThisAssembly.Version, ThisAssembly.PublicKeyToken);
    }

    static WellKnown()
    {
      OrmAssemblyFullName = GetFullAssemblyName("Xtensive.Orm");
      CoreAssemblyFullName = GetFullAssemblyName("Xtensive.Core");
    }
  }
}