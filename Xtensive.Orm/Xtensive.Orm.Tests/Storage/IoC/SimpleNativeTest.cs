// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.17

using NUnit.Framework;
using Xtensive.IoC;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.IoC.SimpleNativeTestModel;

namespace Xtensive.Orm.Tests.Storage.IoC.SimpleNativeTestModel
{
  [HierarchyRoot]
  public class FakeClass : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

    public interface IMyService : ISessionService
    {
    }

    [Service(typeof(IMyService))]
    public class MyService : SessionBound, IMyService
    {
      [ServiceConstructor]
      public MyService(Session session) :
        base(session)
      {
      }
    }
}

namespace Xtensive.Orm.Tests.Storage.IoC
{
  public class SimpleNativeTest : AutoBuildTest
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

          var s = session.Services.GetService<IMyService>();
          Assert.IsNotNull(s);
          s = session.Services.Get<IMyService>();
          Assert.IsNotNull(s);
          Assert.IsNotNull((s as SessionBound).Session);
          // Rollback
        }
      }
    }
  }
}