// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.29

using NUnit.Framework;
using System.Linq;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade.OldModel;
using Xtensive.Storage.Tests.Upgrade.ForeignKeyUpgrade.NewModel;

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

namespace Xtensive.Storage.Tests.Upgrade
{
  using OldOrder = ForeignKeyUpgrade.OldModel.Order;
  using NewOrder = ForeignKeyUpgrade.NewModel.Order;
  using Building;
  
  [TestFixture]
  public class ForeignKeyUpgradeTest : UpgradeTestBase
  {

    [Test]
    public void SetReferencingFieldToDefaultTest()
    {
      BuildDomain(SchemaUpgradeMode.Recreate, typeof(OldOrder), typeof(Person));

      using (Domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          
          var person = new Person {Name = "Person1"};

          new OldOrder {Consumer = person};
          new OldOrder {Consumer = person};
          
          transactionScope.Complete();
        }
      }
      
      BuildDomain(SchemaUpgradeMode.Perform, typeof(NewOrder), typeof(Company));

      using (Domain.OpenSession()) {
        using (Transaction.Open()) {          
          Assert.AreEqual(2, Query<NewOrder>.All.Count());          
          foreach (var order in Query<NewOrder>.All) 
            Assert.IsNull(order.Consumer);
        }
      }
    }
  }
}