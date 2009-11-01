// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.11

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.Internals;

namespace Xtensive.Storage.Tests.Storage.ActivatorTests
{
  [HierarchyRoot(typeof (StringProvider), "Name")]
  public abstract class Ancestor : Entity
  {
    [Field]
    public abstract string Name { get; set; }
  }

  public class Descendant : Ancestor
  {
    [Field]
    public override string Name { get; set; }

    [Field]
    public int Number { get; set; }
  }

  [TestFixture]
  public class ActivatorTests
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      DomainConfiguration config = new DomainConfiguration("memory://localhost/ActivatorTests");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.ActivatorTests");
      domain = Domain.Build(config);
    }

    [Test]
    public void Test()
    {
      Key key;
      using (domain.OpenSession()) {
        Descendant descendant = new Descendant();
        key = descendant.Key;
        Session.Current.Persist();
      }

      using (domain.OpenSession()) {
        Ancestor ancestor = key.Resolve<Ancestor>();
        Assert.IsNotNull(ancestor);

        Descendant descendant = key.Resolve<Descendant>();
        Assert.IsNotNull(descendant);
      }
    }
  }
}