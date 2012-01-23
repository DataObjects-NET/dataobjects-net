// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.02.17

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueA424_QueryByInterfaceException_Model;
using System.Linq;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueA424_QueryByInterfaceException_Model
  {
    [Serializable]
    [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
    public abstract class Animal : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      protected Animal(Session session) : base(session) { }
    }

    public interface IHasLegs : IEntity
    {
      [Field]
      string NumberOfLegs { get; set; }
    }

    public abstract class Mammal : Animal, IHasLegs
    {
      [Field]
      public string NumberOfLegs { get; set; }

      protected Mammal(Session session) : base(session) { }
    }

    public interface ICanRun : IHasLegs
    {

    }

    public abstract class Cat : Animal, ICanRun
    {
      protected Cat(Session session) : base(session) { }

      [Field]
      public string NumberOfLegs { get; set; }
    }

    public class Lion : Cat
    {
      public Lion(Session session) : base(session) { }
    }
  }

  [Serializable]
  public class IssueA424_QueryByInterfaceException : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (IHasLegs).Assembly, typeof (IHasLegs).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction())
      {

        Query.All<IHasLegs>().ToList();
        t.Complete();
      }
    }
  }
}