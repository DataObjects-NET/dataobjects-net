// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2010.10.22

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0851_AnyMappingError_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0851_AnyMappingError_Model
{
  [HierarchyRoot]
  public class Item : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }


  public interface IHasItems : IEntity
  {
    [Field]
    EntitySet<Reference> Items { get; }
  }

  [HierarchyRoot]
  public class ReferenceContainer : Entity, IHasItems
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Reference> Items { get; private set; }
  }


  [HierarchyRoot]
  public class Reference : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Item Item { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0851_AnyMappingError : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (IHasItems).Assembly, typeof (IHasItems).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {

          var item = new Item();
          var referenceContainer1 = new ReferenceContainer();
          var referenceContainer2 = new ReferenceContainer{Items = {new Reference {Item = item}}};

          session.Persist();
          var l = Query.All<IHasItems>()
            .Where(container => container.Items.Any())
            .ToList();

          Assert.AreEqual(1, l.Count);
        }
      }
    }
  }
}