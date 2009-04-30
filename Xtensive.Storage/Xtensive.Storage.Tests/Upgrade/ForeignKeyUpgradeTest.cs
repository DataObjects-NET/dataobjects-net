// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.29

using NUnit.Framework;
using System.Linq;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade.OldModel;
using Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade.NewModel;

# region Models

namespace Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade.OldModel
{
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Order : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Number { get; set; }

    [Field]
    public Person Consumer { get; set; }
  }

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Person : Entity
  {
    [Field]
    public int Id { get; private set; }

    public string Name { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade.NewModel
{
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Order : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Number { get; set; }

    [Field]
    public Company Consumer { get; set; }
  }

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Company : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

# endregion

namespace Xtensive.Storage.Tests.Upgrade
{
  using OldOrder = ForeignKeyUpgrade.OldModel.Order;
  using NewOrder = ForeignKeyUpgrade.NewModel.Order;
  using Building;

  [TestFixture]
  public class ForeignKeyUpgradeTest
  {
    private const string Url = "mssql2005://localhost/DO40-Tests";

    public DomainConfiguration OldModelConfiguration
    {
      get
      {
        var config = new DomainConfiguration(Url);
        config.Types.Register(
          typeof (OldOrder).Assembly,
          typeof (OldOrder).Namespace);
        return config;
      }
    }

    public DomainConfiguration NewModelConfiguration
    {
      get
      {
        var config = new DomainConfiguration(Url);
        config.Types.Register(
          typeof (NewOrder).Assembly,
          typeof (NewOrder).Namespace);
        return config;
      }
    }

    [Test]
    public void   SetReferencingFieldToDefaultTest()
    {
      var config = OldModelConfiguration;
      config.BuildMode = DomainBuildMode.Recreate;
      var domain = Domain.Build(config);

      using (var s = domain.OpenSession()) {
        using ( var t = Transaction.Open()) {
          var person = new Person {Name = "Person1"};
          var order1 = new OldOrder {Consumer = person};
          var order2 = new OldOrder {Consumer = person};
          var orders = Query<OldOrder>.All;
          foreach (var order in orders) {
            Assert.IsNotNull(order.Consumer);
          }
          t.Complete();
        }
      }
      domain.DisposeSafely();
      
      config = NewModelConfiguration;
      config.BuildMode = DomainBuildMode.Perform;
      domain = Domain.Build(config);
      using (var s = domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var orders = Query<NewOrder>.All;
          Assert.AreEqual(2, orders.Count());
          orders = Query<NewOrder>.All;
          foreach (var order in orders) {
            Assert.IsNull(order.Consumer);
          }
          t.Complete();
        }
      }

    }
  }
}