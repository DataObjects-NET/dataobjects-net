// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.07

using System;
using NUnit.Framework;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Storage.CustomEntitySetModel;

namespace Xtensive.Storage.Tests.Storage.CustomEntitySetModel
{
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Root : Entity
  {
    [Field]
    public int Id { get; private set; }
  }

  public class Master : Root
  {
    [Field]
    public CustomEntitySet<Slave> ZeroToMany { get; private set; }

    [Field]
    public CustomEntitySet<Slave> OneToMany { get; private set; }

    [Field]
    public CustomEntitySet<Slave> ManyToMany { get; private set; }
  }

  public class Slave : Root
  {
    [Field(PairTo="OneToMany")]
    public Master ManyToOne { get; set; }

    [Field(PairTo="ManyToMany")]
    public CustomEntitySet<Master> ManyToMany { get; private set; }
  }

  public class CustomEntitySet<T> : EntitySet<T> where T : Entity
  {
    public CustomEntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
    }
  }
}


namespace Xtensive.Storage.Tests.Storage
{
  public class CustomEntitySetTest : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Master).Assembly, typeof (Master).Namespace);
      return config;
    }

    public void Maintest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Master m = new Master();
          m.ManyToMany.Add(new Slave());
          Assert.AreEqual(1, m.ManyToMany.Count);
          t.Complete();
        }
      }
    }
  }
}