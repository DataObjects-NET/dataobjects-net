// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.17

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Tests.Storage.IntermediateQueryTest_Model;

namespace Xtensive.Storage.Tests.Storage
{
  namespace IntermediateQueryTest_Model
  {
    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class Parent : Entity
    {
      [Field, Key]
      public Guid ID { get; private set; }

      [Field]
      public string Name { get; private set; }

      [Field]
      public EntitySet<Child> MyChildren { get; private set; }

      public Parent()
        : base(Guid.NewGuid())
      {
        Name = this.GetType().Name + " " + DateTime.Now;
      }
    }

    [HierarchyRoot]
    [KeyGenerator(KeyGeneratorKind.None)]
    public class Child : Entity
    {
      [Field, Key]
      public Guid ID { get; private set; }

      [Field]
      public string Name { get; private set; }

      public Child()
        : base(Guid.NewGuid())
      {
        Name = this.GetType().Name + " " + DateTime.Now;
      }
    }
  }

  [Serializable]
  public class IntermediateQueryTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Parent).Assembly, typeof (Parent).Namespace);
      return config;
    }

    [Test]
    public void SelectManyTest()
    {
      using (Session.Open(Domain))
      {
        using (var transactionScope = Transaction.Open())
        {
          var parent = new Parent();
          var child = new Child();
          parent.MyChildren.Add(child);

          Session.Current.Persist();

          var list = Query.All<Parent>()
            .SelectMany(p => p.MyChildren, (p, c) => new {MasterId = p.ID, SlaveId = c.ID})
            .ToList();

          Assert.AreEqual(1, list.Count);

          var p0 = Query.All<Parent>().First();
          foreach (var c in p0.MyChildren) {
            Console.WriteLine(c);
          }
        }
      }
    }

    [Test]
    public void CustomQueryTestTest()
    {
      using (Session.Open(Domain))
      {
        using (var transactionScope = Transaction.Open())
        {
          var parent = new Parent();
          var child = new Child();
          parent.MyChildren.Add(child);

          Session.Current.Persist();

          var types = Domain.Model.Types;
          var itemsType = Domain.Model.Types[typeof(Parent)].Fields["MyChildren"].Associations[0].AuxiliaryType.UnderlyingType;
          var qqq = Query.All(itemsType) as IQueryable<EntitySetItem<Parent, Child>>;
          var list = qqq.ToList();
          var qq = qqq
            .Select(e => new { MasterId = e.Master.ID, SlaveId = e.Slave.ID })
            .ToList();

          Assert.AreEqual(1, list.Count);

          var p0 = Query.All<Parent>().First();
          foreach (var c in p0.MyChildren)
          {
            Console.WriteLine(c);
          }
        }
      }
    }
  }
}