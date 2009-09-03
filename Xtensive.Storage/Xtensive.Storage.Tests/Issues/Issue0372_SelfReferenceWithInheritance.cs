// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.03

using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0372_SelfReferenceWithInheritance_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0372_SelfReferenceWithInheritance_Model
{
  [HierarchyRoot]
  public class Item
    : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public WebSite WebSite { get; set; }
  }

  public class WebSite
    : Item
  {
    [Field]
    public string DomainName { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0372_SelfReferenceWithInheritance : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (WebSite).Assembly, typeof (WebSite).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var webSite = new WebSite();
          webSite.WebSite = webSite; // self-refernece
          Session.Current.Persist();
          // Rollback
        }
      }
    }
  }
}