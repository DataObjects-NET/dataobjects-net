// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.06

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0512_NullableGetValueOrDefaultIsNotSupported : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.RegisterCaching(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        new X {FNInt = 5};
        new X();
        var sum1 = session.Query.All<X>().Sum(x => x.FNInt.GetValueOrDefault());
        Assert.That(sum1, Is.EqualTo(5));
        var sum2 = session.Query.All<X>().Sum(x => x.FNInt.GetValueOrDefault(1));
        Assert.That(sum2, Is.EqualTo(6));
      }
    }
  }
}