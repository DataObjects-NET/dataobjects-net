// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.06

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0512_NullableGetValueOrDefaultIsNotSupported : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain))
      using (Transaction.Open()) {
        new X {FNInt = 5};
        new X();
        var sum1 = Query.All<X>().Sum(x => x.FNInt.GetValueOrDefault());
        Assert.AreEqual(5, sum1);
        var sum2 = Query.All<X>().Sum(x => x.FNInt.GetValueOrDefault(1));
        Assert.AreEqual(6, sum2);
      }
    }
  }
}