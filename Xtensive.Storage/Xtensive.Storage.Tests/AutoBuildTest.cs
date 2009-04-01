// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.31

using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;
using Xtensive.Core.Helpers;
using Xtensive.Core.Disposing;

namespace Xtensive.Storage.Tests
{
  [TestFixture]
  public abstract class AutoBuildTest
  {
    private Domain domain;

    protected Domain Domain
    {
      get { return domain; }
    }

    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp()
    {
      DomainConfiguration config = BuildConfiguration();
        domain = BuildDomain(config);
    }

    [TestFixtureTearDown]
    public virtual void TestFixtureTearDown()
    {
      domain.DisposeSafely();
    }

    protected virtual DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config;
//      config = DomainConfigurationFactory.Create("memory");
//      config = DomainConfigurationFactory.Create("memory", InheritanceSchema.SingleTable);
//      config = DomainConfigurationFactory.Create("memory", InheritanceSchema.ConcreteTable);
//      config = DomainConfigurationFactory.Create("memory", InheritanceSchema.Default, TypeIdBehavior.Include);

      config = DomainConfigurationFactory.Create("mssql2005");
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

    protected virtual Domain BuildDomain(DomainConfiguration configuration)
    {
      return Domain.Build(configuration);
    }
  }
}
