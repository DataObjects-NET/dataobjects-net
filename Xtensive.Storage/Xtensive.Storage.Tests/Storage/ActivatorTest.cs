// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.11

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.ActivatorModel;

namespace Xtensive.Storage.Tests.Storage.ActivatorModel
{
  [HierarchyRoot(typeof (Generator), "ID")]
  public abstract class Ancestor : Entity
  {
    [Field]
    public abstract int ID { get; set; }
  }

  public class Descendant : Ancestor
  {
    public override int ID { get; set; }

    [Field]
    public int Number { get; set; }
  }

}

namespace Xtensive.Storage.Tests.Storage
{
  public class ActivatorTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.ActivatorModel");
      return config;
    }

    [Test]
    public void Test()
    {
      Key key;
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var descendant = new Descendant();
          key = descendant.Key;
          Session.Current.Persist();
        }        
      }
      using (Domain.OpenSession()) {
        using (Transaction.Open()) {
          var ancestor = key.Resolve<Ancestor>();
          Assert.IsNotNull(ancestor);

          var descendant = key.Resolve<Descendant>();
          Assert.IsNotNull(descendant);
        }        
      }
    }
  }
}