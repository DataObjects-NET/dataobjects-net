// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.26

using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Linq;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Issues.Issue0767_QueryByInterfaceException_Model;

namespace Xtensive.Storage.Tests.Issues
{
  namespace Issue0767_QueryByInterfaceException_Model
  {
    /*Some specifics of my model:
       
     * All entities inherit from BusinessEntity (which inherits from Entity). 
     * BusinessEntity implements IBusinessEntity. 
     * Key field is defined on IBusinessEntity (of type long). 
     * [HierarchyRoot(InheritanceSchema.ConcreteTable)] is applied on BusinessEntity.

      I am querying for type IMyInterface, and IMyInterface inherits from IBusinessEntity. 
     * There are also other interfaces that implement/inherit IMyInterface*/

    public interface IBusinessEntity : IEntity
    {
      [Field, Key]
      long Id { get; }
    }

    public interface IHasName : IBusinessEntity
    {
      [Field]
      string Name { get; set; }
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public abstract class BusinessEntity : Entity, IBusinessEntity
    {
      public long Id { get; private set; }
      [Field]
      public string Tag { get; set; }
    }

    public class Foo : BusinessEntity, IHasName
    {
      [Field]
      public int Some { get; set; }

      public string Name { get; set; }
    }

    public class Bar : BusinessEntity, IHasName
    {
      [Field]
      public Guid Some { get; set; }
      public string Name { get; set; }
    }


  }

  [Serializable]
  public class Issue0767_QueryByInterfaceException : AutoBuildTest
  {

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof(BusinessEntity).Assembly, typeof(BusinessEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open())
      {
        new Foo() {Name = "foo", Some = 10, Tag = "foo tag"};
        new Bar() {Name = "bar", Some = Guid.NewGuid(), Tag = "bar tag"};

        var all = Query.All<IBusinessEntity>().ToList();
        Assert.AreEqual(2, all.Count);
        var hasNames = Query.All<IHasName>().ToList();
        Assert.AreEqual(2, hasNames.Count);
        var foos = Query.All<IHasName>().Where(i => i.Name == "foo").ToList();
        Assert.AreEqual(1, foos.Count);
        var bars = Query.All<IHasName>().Where(i => i.Name == "bar").ToList();
        Assert.AreEqual(1, bars.Count);

        t.Complete();
      }
    }
  }
}