// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.16

using System;
using NUnit.Framework;
using Xtensive.Diagnostics;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.IoC.Model;

namespace Xtensive.Orm.Tests.Storage.IoC.Model
{
  [Serializable]
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  public interface IMyService
  {
  }

  public class MyServiceImpl : SessionBound, IMyService
  {
  }
}

namespace Xtensive.Orm.Tests.Storage.IoC
{
  [TestFixture]
  public abstract class ServiceTestBase : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(MyEntity));
      return config;
    }

    [Test]
    public void ContainerTest()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {

          // Domain-level singleton service
          var domainSingleton1 = Domain.Services.Get<IMyService>("singleton");
          var domainSingleton2 = Domain.Services.Get<IMyService>("singleton");
          Assert.AreSame(domainSingleton1, domainSingleton2);

          // Domain-level transient service
          var domainTransient1 = Domain.Services.Get<IMyService>("transient");
          var domainTransient2 = Domain.Services.Get<IMyService>("transient");
          Assert.AreNotSame(domainTransient1, domainTransient2);

          // Session-level singleton service
          var sessionSingleton1 = session.Services.Get<IMyService>();
          var sessionSingleton2 = session.Services.Get<IMyService>();
          Assert.AreSame(sessionSingleton1, sessionSingleton2);

          using (var session2 = Domain.OpenSession()) {
            using (session2.OpenTransaction()) {
              // Session-level singleton service from another session
              var sessionSingleton3 = Session.Current.Services.Get<IMyService>();
              Assert.AreNotSame(sessionSingleton1, sessionSingleton3);
            }
          }
        }
      }
    }

    [Test]
    public void PerformanceTest()
    {
      const int iterationCount = 100000;
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          using (new Measurement("Getting domain-level singleton service.", iterationCount))
            for (int i = 0; i < iterationCount; i++)
              Domain.Services.Get<IMyService>("singleton");

          using (new Measurement("Getting domain-level transient service.", iterationCount))
            for (int i = 0; i < iterationCount; i++)
              Domain.Services.Get<IMyService>("transient");

          using (new Measurement("Getting session-level singleton service.", iterationCount))
            for (int i = 0; i < iterationCount; i++)
              session.Services.Get<IMyService>();
        }
      }
    }
  }
}