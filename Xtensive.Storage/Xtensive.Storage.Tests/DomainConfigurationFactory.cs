// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.05

using System;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests
{
  public static class DomainConfigurationFactory
  {
    private const string StorageTypeKey =          "env.X_STORAGE";
    private const string ForeignKeysModeKey =      "env.X_FOREIGN_KEYS";
    private const string TypeIdKey =               "env.X_TYPE_ID";
    private const string InheritanceSchemaKey =    "env.X_INHERITANCE_SCHEMA";

    public static DomainConfiguration Create()
    {
      // Default values
      var storageType = "memory";
      var foreignKeyMode = ForeignKeyMode.Default;
      var typeIdBehavior = TypeIdBehavior.Default;
      var inheritanceSchema = InheritanceSchema.Default;

      // Getting values from the environment variables
      var value = GetEnvironmentVariable(StorageTypeKey);
      if (!string.IsNullOrEmpty(value))
        storageType = value;

      value = GetEnvironmentVariable(TypeIdKey);
      if (!string.IsNullOrEmpty(value)) {
        typeIdBehavior = (TypeIdBehavior) Enum.Parse(typeof (TypeIdBehavior), value, true);
      }

      value = GetEnvironmentVariable(InheritanceSchemaKey);
      if (!string.IsNullOrEmpty(value)) {
        inheritanceSchema = (InheritanceSchema) Enum.Parse(typeof (InheritanceSchema), value, true);
      }

      value = GetEnvironmentVariable(ForeignKeysModeKey);
      if (!string.IsNullOrEmpty(value)) {
        foreignKeyMode = (ForeignKeyMode) Enum.Parse(typeof (ForeignKeyMode), value, true);
      }

      DomainConfiguration config;

      config = Create(storageType, inheritanceSchema, typeIdBehavior, foreignKeyMode);

      // Here you still have the ability to override the above values

//      config = Create("memory");
//      config = Create("memory", InheritanceSchema.SingleTable);
//      config = Create("memory", InheritanceSchema.ConcreteTable);
//      config = Create("memory", InheritanceSchema.Default, TypeIdBehavior.Include);

//        config = Create("mssql2005");
//      config = Create("mssql2005", InheritanceSchema.SingleTable);
//      config = Create("mssql2005", InheritanceSchema.ConcreteTable);
//      config = Create("mssql2005", InheritanceSchema.Default, TypeIdBehavior.Include);

//      config = Create("pgsql");
//      config = Create("mssql2005", InheritanceSchema.SingleTable);
//      config = Create("mssql2005", InheritanceSchema.ConcreteTable);
//      config = Create("mssql2005", InheritanceSchema.Default, TypeIdBehavior.Include);

//      config = Create("vistadb");
//      config = Create("vistadb", InheritanceSchema.SingleTable);
//      config = Create("vistadb", InheritanceSchema.ConcreteTable);
//      config = Create("vistadb", InheritanceSchema.Default, TypeIdBehavior.Include);
      return config;
    }

    public static DomainConfiguration Create(string protocol)
    {
      return DomainConfiguration.Load(protocol);
    }

    public static DomainConfiguration Create(string protocol, InheritanceSchema schema)
    {
      DomainConfiguration config = Create(protocol);
      config.Builders.Add(InheritanceSchemaModifier.GetModifier(schema));
      return config;
    }

    public static DomainConfiguration Create(string protocol, InheritanceSchema schema, TypeIdBehavior typeIdBehavior)
    {
      DomainConfiguration config = Create(protocol, schema);
      config.Builders.Add(InheritanceSchemaModifier.GetModifier(schema));
      config.Builders.Add(TypeIdModifier.GetModifier(typeIdBehavior));
      return config;
    }

    public static DomainConfiguration Create(string protocol, InheritanceSchema schema, TypeIdBehavior typeIdBehavior, ForeignKeyMode foreignKeyMode)
    {
      DomainConfiguration config = Create(protocol, schema);
      config.Builders.Add(InheritanceSchemaModifier.GetModifier(schema));
      config.Builders.Add(TypeIdModifier.GetModifier(typeIdBehavior));
      config.ForeignKeyMode = foreignKeyMode;
      return config;
    }

    private static string GetEnvironmentVariable(string key)
    {
      string result = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
      if (!string.IsNullOrEmpty(result))
        return result;
      return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
    }
  }
}
