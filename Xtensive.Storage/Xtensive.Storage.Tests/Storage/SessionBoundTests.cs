// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.25

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.StructureTests;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class SessionBoundTests
  {
    internal class TestHelper : SessionBound
    {
      public void TestMethod(Ray x, Ray y, Session parentSession)
      {
        Assert.AreNotEqual(Session.Current, parentSession);
        Assert.AreNotEqual(x.Session, y.Session);

        Assert.AreEqual(Session.Current, x.Session);
        Assert.AreNotEqual(Session.Current, y.Session);

        Assert.AreEqual(x.Direction, y.Direction);
      }

      public TestHelper(Session session)
        : base(session)
      {
      }
    }

    private Domain domain;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      DomainConfiguration config = new DomainConfiguration("memory://localhost/Points");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.StructureTests");
      domain = Domain.Build(config);
    }

    [Test]
    public void Test()
    {
      var scoupe1 = domain.OpenSession();
      Ray ray = new Ray();
      var testHelper = new TestHelper(Session.Current);
      Session.Current.Persist();

      var scoupe2 = domain.OpenSession();
      Ray ray1 = new Ray();

      Assert.AreNotEqual(scoupe1.Session, scoupe2.Session);
      testHelper.TestMethod(ray, ray1, Session.Current);
    }
  }
}