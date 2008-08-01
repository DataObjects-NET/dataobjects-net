// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.11

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.ActivatorTests
{
  [HierarchyRoot(typeof (DefaultGenerator), "ID")]
  public abstract class Ancestor : Entity
  {
    [Field]
    public abstract int ID { get; set; }
  }

  public class Descendant : Ancestor
  {
    [Field]
    public override int ID{ get; set; }

    [Field]
    public int Number { get; set; }
  }

  public class ActivatorTests : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.ActivatorTests");
      return config;
    }

    [Test]
    public void Test()
    {
      Key key;
      using (Domain.OpenSession()) {
        Descendant descendant = new Descendant();
        key = descendant.Key;
        Session.Current.Persist();
      }

      using (Domain.OpenSession()) {
        Ancestor ancestor = key.Resolve<Ancestor>();
        Assert.IsNotNull(ancestor);

        Descendant descendant = key.Resolve<Descendant>();
        Assert.IsNotNull(descendant);
      }
    }
  }
}