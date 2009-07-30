// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.14

using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Configuration;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Tests.Storage.Services;

namespace Xtensive.Storage.Tests.Storage
{
  namespace Services
  {
    public class SimpleService : SessionBound
    {
      private int value;

      public int Value
      {
        get { return value; }
      }

      public CoreServiceAccessor GetCoreServices()
      {
        return CoreServices;
      }

      public SimpleService(int value)
      {
        this.value = value;
      }
    }
  }

  [TestFixture]
  public class ServiceTest
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void SetUp()
    {
      var config = DomainConfiguration.Load("ServicesTest", "memory");
      domain = Domain.Build(config);
    }

    [Test]
    public void BehaviorTest()
    {
      using (Session.Open(domain)) {
        using (Transaction.Open()) {
          Assert.IsNotNull(Session.Current.Services.Get<SimpleService>("ss1").GetCoreServices());
          var ss1_1 = Session.Current.Services.Get<SimpleService>("ss1");
          var ss1_2 = Session.Current.Services.Get<SimpleService>("ss1");
          Assert.AreSame(ss1_1, ss1_2);
          Assert.AreEqual(123, ss1_1.Value);
          var ss2_1 = Session.Current.Services.Get<SimpleService>("ss2");
          var ss2_2 = Session.Current.Services.Get<SimpleService>("ss2");
          Assert.AreNotSame(ss2_1, ss2_2);
          Assert.AreEqual(123321, ss2_1.Value);
        }
      }
    }

    [Test]
    public void PerformanceTest()
    {
      const int iterationsCount = 100000;
      using (Session.Open(domain)) {
        using (Transaction.Open()) {
          using (new Measurement("Getting session-singleton service.", iterationsCount)) {
            var session = Session.Current;
            for (int i = 0; i < iterationsCount; i++) {
              session.Services.Get<SimpleService>("ss1");
            }
          }
          using (new Measurement("Getting transient service.", iterationsCount)) {
            var session = Session.Current;
            for (int i = 0; i < iterationsCount; i++) {
              session.Services.Get<SimpleService>("ss2");
            }
          }
        }
      }
    }

    [TestFixtureTearDown]
    public void TearDown()
    {
      domain.DisposeSafely();
    }
  }
}