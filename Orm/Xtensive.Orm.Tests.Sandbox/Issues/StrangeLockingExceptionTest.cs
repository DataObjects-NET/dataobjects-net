// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.20

using System;
using System.Linq;
#if NET40
using System.Threading.Tasks;
#endif
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.StrangeLockingExceptionModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace StrangeLockingExceptionModel
  {
    [Serializable]
    [HierarchyRoot]
    public class TestA : Entity
    {
      public TestA(Session session) : base(session) { }

      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public int Val { get; set; }
    }

    [Serializable]
    [HierarchyRoot]
    public class TestB : Entity
    {
      public TestB(Session session) : base(session) { }

      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public int Val { get; set; }
    }
  }

  public class StrangeLockingExceptionTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TestA).Assembly, typeof(TestA).Namespace);
      return configuration;
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.ExclusiveWriterConnection);
    }

    protected override void PopulateData()
    {
      base.PopulateData();
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ServerProfile))) {
        using (var transaction = session.OpenTransaction()) {
          for (int I = 0; I < 0x01FF; I++)
            new TestA(session);
          // To much records???
          for (int I = 0; I < 0x01FFF; I++)
            new TestB(session);
          transaction.Complete();
        }
      }
    }

    [Test]
    public void Test1()
    {
      using (var sessionA = Domain.OpenSession(new SessionConfiguration(SessionOptions.ServerProfile))) {
        using (sessionA.OpenTransaction()) {
          sessionA.Query.All<TestA>().Count();
          sessionA.Query.All<TestB>().Count();
          using (var sessionB = Domain.OpenSession(new SessionConfiguration(SessionOptions.ServerProfile))) {
            using (sessionB.OpenTransaction()) {
              sessionB.Query.All<TestA>().Count();
              // Exception
              sessionB.Query.All<TestB>().Count();
            }
          }
        }
      }
    }

    [Test]
    public void Test2()
    {
      using (var sessionA = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        sessionA.Query.All<TestA>().Count();
        sessionA.Query.All<TestB>().Count();

        using (var sessionB = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
          sessionB.Query.All<TestA>().Count();
          // Exception
          sessionB.Query.All<TestB>().Count();
        }
      }
    }

    [Test]
    public void Test3()
    {
      using (var sessionA = Domain.OpenSession(new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.AutoTransactionOpenMode))) {
        sessionA.Query.All<TestA>().Count();
        sessionA.Query.All<TestB>().Count();
        using (var sessionB = Domain.OpenSession(new SessionConfiguration(SessionOptions.ServerProfile | SessionOptions.AutoTransactionOpenMode))) {
          sessionB.Query.All<TestA>().Count();
          sessionB.Query.All<TestB>().Count();
        }
      }
    }
#if NET40
    [Test]
    public void Test4()
    {
      Execute(SessionOptions.ServerProfile);
    }

    [Test]
    public void Test5()
    {
      Execute(SessionOptions.ClientProfile);
    }

    [Test]
    public void Test6()
    {
      Execute(SessionOptions.ServerProfile | SessionOptions.AutoTransactionOpenMode);
    }

    private void Execute(SessionOptions options)
    {
      Action action = () => {
        using (var session = Domain.OpenSession(new SessionConfiguration(options))) {
          using (var t = session.OpenTransaction()) {
            session.Query.All<TestA>().ToList();
            // Exception
            session.Query.All<TestB>().ToList();
            t.Complete();
          }
        }
      };
      Parallel.Invoke(action, action, action, action, action, action);
    }
#endif
  }
}