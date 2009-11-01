// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.KeyProviders;
using Xtensive.Storage.Providers.Memory;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class Providers
  {
    [HierarchyRoot(typeof (Int64Provider), "ID")]
    public class Car2 : Entity
    {
      [Field]
      public string Name
      {
        get { return GetValue<string>("Name"); }
        set { SetValue("Name", value); }
      }

      [Field]
      public int Length
      {
        get { return GetValue<int>("Length"); }
        set { SetValue("Length", value); }
      }
    }

    [Test]
    public void TestProviders()
    {
      DomainConfiguration memoryConfig = new DomainConfiguration("memory://localhost/Car");
      memoryConfig.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Car");
      Domain memoryDomain = Domain.Build(memoryConfig);
      Assert.AreEqual(memoryDomain.Handler.GetType(), typeof (DomainHandler));

//      DomainConfiguration sqlConfig = new DomainConfiguration("mssql2005://localhost/Car");
//      sqlConfig.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Car");
//      Xtensive.Storage.Storage sqlStorage = Xtensive.Storage.Storage.Build(sqlConfig);
//      Assert.AreEqual(sqlStorage.Session.Handler.GetType(), typeof(Xtensive.Storage.Providers.Sql.DomainHandler));
    }
  }
}