// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.07

using System;
using NUnit.Framework;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.CustomEntitySetModel;

namespace Xtensive.Orm.Tests.Storage.CustomEntitySetModel
{
  [Serializable]
  [HierarchyRoot]
  public class Root : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [Serializable]
  public class Master : Root
  {
    [Field]
    public CustomEntitySet<Slave> ZeroToMany { get; private set; }

    [Field]
    public CustomEntitySet<Slave> OneToMany { get; private set; }

    [Field]
    public CustomEntitySet<Slave> ManyToMany { get; private set; }
  }

  [Serializable]
  public class Slave : Root
  {
    [Field, Association(PairTo = "OneToMany")]
    public Master ManyToOne { get; set; }

    [Field, Association(PairTo = "ManyToMany")]
    public CustomEntitySet<Master> ManyToMany { get; private set; }
  }

  public class CustomEntitySet<T> : EntitySet<T>
    where T : Entity
  {
    public CustomEntitySet(Entity owner, FieldInfo field)
      : base(owner, field)
    {
    }
  }
}


namespace Xtensive.Orm.Tests.Storage
{
  public class CustomEntitySetTest : AutoBuildTest
  {
    protected override Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Master).Assembly, typeof (Master).Namespace);
      return config;
    }

    [Test]
    public void Maintest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Master m = new Master();
          
          m.ManyToMany.Add(new Slave());
          Assert.AreEqual(1, m.ManyToMany.Count);

          m.ManyToMany.Add(new Slave());
          Assert.AreEqual(2, m.ManyToMany.Count);

          t.Complete();
        }
      }
    }
  }
}