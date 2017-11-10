// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.16

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Manual.Indexes
{
  #region Model
  
  [Serializable]
  [HierarchyRoot]
  public class Pet : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Person Owner { get; set; }

    public Pet(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  [Index("Email", Unique = true)]
  [Index("FirstName", "LastName")]
  [Index("Nickname", Unique = true)]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Nickname { get; set; }

    [Field]
    public string Email { get; set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    public Person(Session session)
      : base(session)
    {}
  }

  public enum OrderStatus
  {
    WaitingConfirmation,
    Active,
    Completed,
  }

  [HierarchyRoot, Index("Status", Filter = "StatusIndexRange")]
  public class Order : Entity
  {
    private static Expression<Func<Order, bool>> StatusIndexRange()
    {
      // Skip completed orders
      return order => order.Status!=OrderStatus.Completed;
    }

    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public OrderStatus Status { get; set; }
  }

  #endregion

  [TestFixture]
  public class TestFixture
  {
    [Test]
    public void MainTest()
    {
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Pet).Assembly, typeof (Pet).Namespace);
      var domain = Domain.Build(config);

      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          // ...
        }
      }
    }
  }
}