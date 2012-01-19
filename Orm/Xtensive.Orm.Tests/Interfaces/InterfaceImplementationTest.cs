// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.05.18

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Interfaces.Model;
using System.Linq;

namespace Xtensive.Orm.Tests.Interfaces
{
  namespace Model
  {
    public interface IHasName : IEntity
    {
      [Field(Length = 100)]
      string Name { get; set; }
    }

    public interface ITagged : IHasName
    {
      [Field(Length = 100)]
      string Tag { get; set; }
    }

    [HierarchyRoot]
    public class A : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Name { get; set; }
    }

    public class B : A, IHasName
    {
      
    }

    public class C : A, ITagged
    {
      [Field(Length=100, Indexed = true)]
      public string Tag { get; set; }
    }

    [Index("Title", "Name", "Tag")]
    [HierarchyRoot]
    public class D : Entity, IHasName, ITagged
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field(Length = 100)]
      public string Title { get; set; }

      public string Name { get; set; }

      public string Tag { get; set; }
    }
  }

  [TestFixture]
  public class InterfaceImplementationTest
  {
    [Test]
    public void CompsiteIndexTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(IHasName).Assembly, typeof(IHasName).Namespace);
      var domain = GetClassTableDomain(config);
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var d = new D();
        d.Title = "A";
        d.Name = "B";
        d.Tag = "C";
        t.Complete();
      }
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var d = session.Query.All<D>().Single();
        Assert.IsNotNull(d);
        t.Complete();
      }
    }

    [Test]
    public void CombinedTest()
    {
      InternalTest(GetClassTableDomain);
      InternalTest(GetConcreteTableDomain);
      InternalTest(GetSingleTableDomain);
    }

    public void InternalTest(Func<DomainConfiguration, Domain> generator)
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(IHasName).Assembly, typeof(IHasName).Namespace);
      var domain = generator(config);
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new A() {Name = "A"};
        new B() {Name = "B"};
        new C() {Name = "C"};
        var hasNames = session.Query.All<IHasName>().ToList();
        Assert.AreEqual(2, hasNames.Count);
        t.Complete();
      }
    }

    private Domain GetSingleTableDomain(DomainConfiguration config)
    {
      Domain domain;
      try {
        SingleTableSchemaModifier.IsEnabled = true;
        config.Types.Register(typeof(ClassTableSchemaModifier));
        domain = Domain.Build(config);
      }
      finally {
        SingleTableSchemaModifier.IsEnabled = false;
      }
      return domain;
    }

    private Domain GetConcreteTableDomain(DomainConfiguration config)
    {
      Domain domain;
      try {
        ConcreteTableSchemaModifier.IsEnabled = true;
        config.Types.Register(typeof(ClassTableSchemaModifier));
        domain = Domain.Build(config);
      }
      finally {
        ConcreteTableSchemaModifier.IsEnabled = false;
      }
      return domain;
    }

    private Domain GetClassTableDomain(DomainConfiguration config)
    {
      Domain domain;
      try {
        ClassTableSchemaModifier.IsEnabled = true;
        config.Types.Register(typeof(ClassTableSchemaModifier));
        domain = Domain.Build(config);
      }
      finally {
        ClassTableSchemaModifier.IsEnabled = false;
      }
      return domain;
    }
  }
}