// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.29

using System;
using NUnit.Framework;
using System.Linq;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building.Builders;
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
    private Domain domain;

    private void BuildDomain(SchemaUpgradeMode schemaUpgradeMode, params Type[] persistentTypes)
    {
      var configuration = DomainConfigurationFactory.Create();
      foreach (Type type in persistentTypes)
        configuration.Types.Register(type);
      configuration.TypeNameProviderType = typeof (SimpleTypeNameProvider);

      domain = DomainBuilder.BuildDomain(configuration, schemaUpgradeMode);
    }

    [Test]
    public void SetReferencingFieldToDefaultTest()
    {
      BuildDomain(SchemaUpgradeMode.Recreate, typeof(OldOrder), typeof(Person));

      using (domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          
          var person = new Person {Name = "Person1"};

          new OldOrder {Consumer = person};
          new OldOrder {Consumer = person};
          
          transactionScope.Complete();
        }
      }
      
      BuildDomain(SchemaUpgradeMode.Upgrade, typeof(NewOrder), typeof(Company));

      using (domain.OpenSession()) {
        using (Transaction.Open()) {          
          Assert.AreEqual(2, Query<NewOrder>.All.Count());          
          foreach (var order in Query<NewOrder>.All) 
            Assert.IsNull(order.Consumer);
        }
      }

    }
  }
}