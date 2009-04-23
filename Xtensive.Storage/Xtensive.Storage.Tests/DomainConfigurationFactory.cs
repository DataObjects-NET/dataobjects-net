// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.05

using System;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests
{
  public static class DomainConfigurationFactory
  {
    public static DomainConfiguration Create()
    {
      DomainConfiguration config;
//      config = Create("memory");
//      config = Create("memory", InheritanceSchema.SingleTable);
//      config = Create("memory", InheritanceSchema.ConcreteTable);
//      config = Create("memory", InheritanceSchema.Default, TypeIdBehavior.Include);

     config = Create("mssql2005");
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
      config.Builders.Add(TypeIdModifier.GetModifier(typeIdBehavior));
      return config;
    }
  }
}