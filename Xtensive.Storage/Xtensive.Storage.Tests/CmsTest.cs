// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.16

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.ObjectModel.Cms;

namespace Xtensive.Storage.Tests
{
  public class CmsTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(ContentFile).Namespace);
      return config;
    }


    [Test]
    public void Test()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var webSite = new WebSite();
          webSite.Title = "Title";
          t.Complete();
        }
        using (var t = Transaction.Open()) {
          var webSite = Query.All<WebSite>().First();
          var result = Query.All<ContentReference>()
            .Where(
            r => r.ReferenceType == ContentReferenceType.Embedded &&
                 r.ContentID.In(Query.All<NewsList>().Where(nl => nl.WebSite == webSite).Select(nl => nl.Id)));
          var list = result.ToList();
          t.Complete();
        }
      }
    }
  }
}