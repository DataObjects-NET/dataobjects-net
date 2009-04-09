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
//      config = DomainConfigurationFactory.Create("memory");
//      config = DomainConfigurationFactory.Create("memory", InheritanceSchema.SingleTable);
//      config = DomainConfigurationFactory.Create("memory", InheritanceSchema.ConcreteTable);
//      config = DomainConfigurationFactory.Create("memory", InheritanceSchema.Default, TypeIdBehavior.Include);

      config = Create("mssql2005");
//      config = DomainConfigurationFactory.Create("mssql2005", InheritanceSchema.SingleTable);
//      config = DomainConfigurationFactory.Create("mssql2005", InheritanceSchema.ConcreteTable);
//      config = DomainConfigurationFactory.Create("mssql2005", InheritanceSchema.Default, TypeIdBehavior.Include);

//      config = DomainConfigurationFactory.Create("pgsql");
//      config = DomainConfigurationFactory.Create("mssql2005", InheritanceSchema.SingleTable);
//      config = DomainConfigurationFactory.Create("mssql2005", InheritanceSchema.ConcreteTable);
//      config = DomainConfigurationFactory.Create("mssql2005", InheritanceSchema.Default, TypeIdBehavior.Include);

//      config = DomainConfigurationFactory.Create("vistadb");
//      config = DomainConfigurationFactory.Create("vistadb", InheritanceSchema.SingleTable);
//      config = DomainConfigurationFactory.Create("vistadb", InheritanceSchema.ConcreteTable);
//      config = DomainConfigurationFactory.Create("vistadb", InheritanceSchema.Default, TypeIdBehavior.Include);
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