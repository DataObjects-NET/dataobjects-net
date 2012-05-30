// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.25

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Testing;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.StructureModel;

namespace Xtensive.Orm.Tests.Storage
{
  public class SessionBoundTest : AutoBuildTest
  {
    internal class TestHelper : SessionBound
    {
      [Transactional(ActivateSession = true)]
      public void Validate(Ray first, Ray second, Session secondSession)
      {
        // Inside this method this.Session is activated

        Assert.AreNotEqual(Session.Current, secondSession);
        Assert.AreNotEqual(first.Session, second.Session);

        Assert.AreEqual(Session.Current, first.Session);
        Assert.AreNotEqual(Session.Current, second.Session);
        
        AssertEx.ThrowsInvalidOperationException(() => {
          bool result = first.Direction==second.Direction;
        });

        using (Session.Deactivate())
          Assert.AreEqual(first.Direction, second.Direction);
      }

      public TestHelper(Session session)
        : base(session)
      {
      }
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.SingleSessionAccess);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Storage.StructureModel");
      return config;
    }

    [Test]
    public void Test()
    {
      var sc1 = new SessionConfiguration("First", SessionOptions.LegacyProfile);
      var sc2 = new SessionConfiguration("Second", SessionOptions.LegacyProfile);
      using (var session1 = Domain.OpenSession(sc1)) {
        using (session1.OpenTransaction()) {
          Ray ray1 = new Ray();
          var helper = new TestHelper(Session.Current);
          Session.Current.SaveChanges();

          using (var session2 = Domain.OpenSession(sc2)) {
            Assert.IsNull(Transaction.Current);

            using (session2.OpenTransaction()) {
              Ray ray2 = new Ray();
              using (Session.Deactivate()) // To allow helper from session1 to activate its session
                helper.Validate(ray1, ray2, Session.Current);
            }
          }
        }
      }
    }
  }
}