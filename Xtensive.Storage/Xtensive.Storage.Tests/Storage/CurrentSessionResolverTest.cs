// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.07.20

using NUnit.Framework;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class CurrentSessionResolverTest
  {
    [Test]
    public void Tests()
    {
      var config = DomainConfigurationFactory.Create("memory");
      var domain = Domain.Build(config);
      var sessionConsumptionScope = Session.Open(domain, false);
      var session = sessionConsumptionScope.Session;

      bool isSessionActive = false;
      int resolveCount = 0;

      Session.Resolver = () => {
        resolveCount++;
        return isSessionActive ? session : null;
      };

      Assert.AreNotEqual(session, Session.Current);
      Assert.AreEqual(1, resolveCount);
      Assert.IsFalse(session.IsActive);
      Assert.AreEqual(2, resolveCount);
      Assert.AreEqual(CompilationContext.Default, CompilationContext.Current);
      Assert.AreEqual(3, resolveCount);

      isSessionActive = true;

      Assert.AreEqual(session, Session.Current);
      Assert.AreEqual(4, resolveCount);
      Assert.IsTrue(session.IsActive);
      Assert.AreEqual(5, resolveCount);
      Assert.AreEqual(session.CompilationContext, CompilationContext.Current);
      Assert.AreEqual(6, resolveCount);

      isSessionActive = false;

      using (session.Activate()) {

        Assert.AreEqual(session, Session.Current);
        Assert.IsTrue(session.IsActive);

        isSessionActive = true;

        Assert.AreEqual(session, Session.Current);
        Assert.IsTrue(session.IsActive);

        Assert.AreEqual(session.CompilationContext, CompilationContext.Current);

        session.Activate();
      }

      isSessionActive = false;

      using (session.Activate()) {

        Assert.AreEqual(session, Session.Current);
        Assert.IsTrue(session.IsActive);

        isSessionActive = true;

        Assert.AreEqual(session, Session.Current);
        Assert.IsTrue(session.IsActive);

        Assert.AreEqual(session.CompilationContext, CompilationContext.Current);

        session.Activate();
      }

      Assert.AreEqual(6, resolveCount);
    }
  }
}