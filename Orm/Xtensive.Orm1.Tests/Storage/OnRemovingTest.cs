// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.21

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.OnRemovingTestModel;

namespace Xtensive.Orm.Tests.Storage.OnRemovingTestModel
{
  [HierarchyRoot]
  public class Victim : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public Killer Killer { get; set; }

    protected override void OnRemoving()
    {
      base.OnRemoving();
      Console.WriteLine(Killer.Name);
    }

    public Victim(string name)
    {
      Name = name;
    }
  }

  [HierarchyRoot]
  public class Killer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Killer", OnOwnerRemove = OnRemoveAction.Cascade)]
    public EntitySet<Victim> Victims { get; private set; }

    protected override void OnRemoving()
    {
      base.OnRemoving();
      Console.WriteLine(Name);
    }

    public Killer(string name)
    {
      Name = name;
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class OnRemovingTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Victim).Assembly, typeof (Victim).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var k = new Killer("K");
          k.Victims.Add(new Victim("V"));

          session.SaveChanges();

          k.Remove();
          // Rollback
        }
      }
    }
  }
}