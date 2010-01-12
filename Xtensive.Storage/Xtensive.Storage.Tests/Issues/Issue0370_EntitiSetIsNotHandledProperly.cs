// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.03

using System;
using System.Runtime.Serialization;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Issues.Issue0370_EntitiSetIsNotHandledProperly_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0370_EntitiSetIsNotHandledProperly_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Container : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public ContainerItemSet Items { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class ContainerItem : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public class ContainerItemSet : EntitySet<ContainerItem>
  {
    protected ContainerItemSet(Entity owner, FieldInfo field)
      : base(owner, field)
    {
    }

    protected ContainerItemSet(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0370_EntitiSetIsNotHandledProperly : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Container).Assembly, typeof (Container).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          
          var c = new Container();
          c.Items.Add(new ContainerItem());

          // Rollback
        }
      }
    }
  }
}