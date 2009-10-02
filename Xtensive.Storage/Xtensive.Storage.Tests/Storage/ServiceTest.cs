// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.14

using System.Configuration;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Tests.Storage.Services;
using Xtensive.Storage.Tests.Storage.SnakesModel;

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
  public class ServiceTest : AutoBuildTest
  {
    private UnityContainer Container;

    public override void TestFixtureSetUp()
    {
      // Building Unity container
      Container = BuildContainer();

      // Setting container to ambient service locator
      ServiceLocator.SetLocatorProvider(GetServiceLocator);

      base.TestFixtureSetUp();
    }

    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Snake).Assembly, typeof(Snake).Namespace);
      return config;
    }

    protected override Domain BuildDomain(Xtensive.Storage.Configuration.DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);

      // Singing up to SessionOpen event to set and configure SessionBound container
      domain.SessionOpen += domain_SessionOpen;
      return domain;
    }

    void domain_SessionOpen(object sender, SessionEventArgs e)
    {
      // Setting locator provider adapter
      e.Session.Services.SetLocatorProvider(GetSessionServiceLocator);
    }

    private IServiceLocator GetServiceLocator()
    {
      // Creating adapter for global Unity container
      return new Microsoft.Practices.Unity.ServiceLocatorAdapter.UnityServiceLocator(Container);
    }

    private IServiceLocator GetSessionServiceLocator()
    {
      // Creating adapter for Session Unity container & registering Session-level services there 
      // Registration could be done in app.config also but we use this approach to test it
      var sessionContainer = Container.CreateChildContainer();
      sessionContainer.RegisterType(typeof (SimpleService), "sessionSingletonService", new ContainerControlledLifetimeManager(), new InjectionConstructor(111333));
      return new Microsoft.Practices.Unity.ServiceLocatorAdapter.UnityServiceLocator(sessionContainer);
    }

    private static UnityContainer BuildContainer()
    {
      // Building global Unity container from app.config
      var section = (UnityConfigurationSection) ConfigurationManager.GetSection("ServicesTest");
      var result = new UnityContainer();
      section.Containers.Default.Configure(result);

      return result;
    }

    [Test]
    public void BehaviorTest()
    {

      using (Session.Open(Domain)) {
        using (Transaction.Open()) {

          // Global singleton service
          Assert.IsNotNull(Session.Current.Services.GetInstance<SimpleService>("singletonService").GetCoreServices());
          var singletonService1 = Session.Current.Services.GetInstance<SimpleService>("singletonService");
          var singletonService2 = Session.Current.Services.GetInstance<SimpleService>("singletonService");
          Assert.AreSame(singletonService1, singletonService2);
          Assert.AreEqual(123, singletonService1.Value);

          // Global transient service
          var transientService1 = Session.Current.Services.GetInstance<SimpleService>("transientService");
          var transientService2 = Session.Current.Services.GetInstance<SimpleService>("transientService");
          Assert.AreNotSame(transientService1, transientService2);
          Assert.AreEqual(123321, transientService1.Value);

          // Session singleton service
          var sessionSingletonService1 = Session.Current.Services.GetInstance<SimpleService>("sessionSingletonService");
          var sessionSingletonService2 = Session.Current.Services.GetInstance<SimpleService>("sessionSingletonService");
          Assert.AreSame(sessionSingletonService1, sessionSingletonService2);
          Assert.AreEqual(111333, sessionSingletonService1.Value);

          using (Session.Open(Domain)) {
            using (Transaction.Open()) {
              // Session singleton service from another session
              var sessionSingletonService3 = Session.Current.Services.GetInstance<SimpleService>("sessionSingletonService");
              Assert.AreNotSame(sessionSingletonService1, sessionSingletonService3);
            }
          }
        }
      }
    }

    [Test]
    public void PerformanceTest()
    {
      const int iterationsCount = 100000;
      using (Session.Open(Domain)) {
        using (Transaction.Open()) {
          using (new Measurement("Getting session-singleton service.", iterationsCount)) {
            var session = Session.Current;
            for (int i = 0; i < iterationsCount; i++) {
              session.Services.GetInstance<SimpleService>("singletonService");
            }
          }
          using (new Measurement("Getting transient service.", iterationsCount)) {
            var session = Session.Current;
            for (int i = 0; i < iterationsCount; i++) {
              session.Services.GetInstance<SimpleService>("transientService");
            }
          }
        }
      }
    }
  }
}