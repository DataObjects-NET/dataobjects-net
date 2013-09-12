// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.03.02

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.IoC.QueryFormatterServiceTestModel;

namespace Xtensive.Orm.Tests.Storage.IoC.QueryFormatterServiceTestModel
{
  [HierarchyRoot]
  public class FakeClass : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

}

namespace Xtensive.Orm.Tests.Storage.IoC
{
  public class QueryFormatterServiceTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (FakeClass).Assembly, typeof (FakeClass).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var fs = session.Services.Get<QueryFormatter>();
          Assert.IsNotNull(fs);

          var query = session.Query.All<FakeClass>().Where(f => f.Id > 0);
          string result = fs.ToSqlString(query);

          Assert.IsNotNullOrEmpty(result);
          Console.WriteLine(result);

          result = fs.ToString(query);

          Assert.IsNotNullOrEmpty(result);
          Console.WriteLine(result);
          // Rollback
        }
      }
    }
  }
}