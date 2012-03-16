// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.16

using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.ObjectModel.Cms;

namespace Xtensive.Orm.Tests.Storage
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
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var webSite = new WebSite();
          webSite.Title = "Title";
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var webSite = session.Query.All<WebSite>().First();
          var result = session.Query.All<ContentReference>()
            .Where(
            r => r.ReferenceType == ContentReferenceType.Embedded &&
              r.ContentID.In(session.Query.All<NewsList>().Where(nl => nl.WebSite == webSite).Select(nl => nl.Id)));
          var list = result.ToList();
          t.Complete();
        }
      }
    }
  }
}