// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.18

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.TransactionsTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  [Serializable]
  public class BatchingTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Hexagon).Assembly, typeof (Hexagon).Namespace);
      return configuration;
    }

    [Test]
    public void BatchSize0Test()
    {
      RunTest(0);
    }

    [Test]
    public void BatchSize1Test()
    {
      RunTest(1);
    }

    [Test]
    public void BatchSize2Test()
    {
      RunTest(2);
    }

    [Test]
    public void BatchSize100Test()
    {
      RunTest(100);
    }

    [Test]
    public void BatchSizeNegativeTest()
    {
      RunTest(-666); // Heil, satan ]:->
    }


    private void RunTest(int batchSize)
    {
      using (var session = Domain.OpenSession(new SessionConfiguration {BatchSize = batchSize, Options = SessionOptions.ServerProfile | SessionOptions.AutoActivation})) {
        Key key;
        using (var transactionScope = session.OpenTransaction()) {
          var hexagon = new Hexagon();
          key = hexagon.Key;
          transactionScope.Complete();
        }
        using (var transactionScope = session.OpenTransaction()) {
          var hexagon = session.Query.Single<Hexagon>(key);
          hexagon.IncreaseKwanza();
          transactionScope.Complete();
        }
        using (var transactionScope = session.OpenTransaction()) {
          var hexagon = session.Query.Single<Hexagon>(key);
          hexagon.Remove();
          transactionScope.Complete();
        }
      }      
    }
  }
}