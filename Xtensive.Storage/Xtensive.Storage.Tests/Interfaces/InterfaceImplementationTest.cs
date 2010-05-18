// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.05.18

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Interfaces.Model;
using System.Linq;

namespace Xtensive.Storage.Tests.Interfaces
{
  namespace Model
  {
    public interface IHasName : IEntity
    {
      [Field]
      string Name { get; set; }
    }

    public interface ITagged : IHasName
    {
      [Field]
      string Tag { get; set; }
    }

    [HierarchyRoot]
    public class A : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }
    }

    public class B : A, IHasName
    {
      
    }

    public class C : A, ITagged
    {
      public string Tag { get; set; }
    }
  }

  [TestFixture]
  public class InterfaceImplementationTest
  {
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
      using (var session = Session.Open(domain))
      using (var t = Transaction.Open()) {
        new A() {Name = "A"};
        new B() {Name = "B"};
        new C() {Name = "C"};
        var hasNames = Query.All<IHasName>().ToList();
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