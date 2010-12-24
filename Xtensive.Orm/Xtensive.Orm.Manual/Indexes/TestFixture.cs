// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.16

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;

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

  #endregion

  [TestFixture]
  public class TestFixture
  {
    [Test]
    public void MainTest()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
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