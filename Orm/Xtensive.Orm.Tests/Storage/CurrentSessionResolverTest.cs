// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.07.20

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class CurrentSessionResolverTest
  {
    [TearDown]
    public void TearDown()
    {
      Session.Resolver = null;
    }

    [Test]
    public void Tests()
    {
      var config = DomainConfigurationFactory.Create();
      config.Sessions[WellKnown.Sessions.Default].Options = SessionOptions.ServerProfile;

      using (var domain = Domain.Build(config))
      using (var session = domain.OpenSession()) {

        bool isSessionActive = false;
        int resolveCount = 0;

        Session.Resolver = () => {
          resolveCount++;
          return isSessionActive ? session : null;
        };

        Assert.That(Session.Current, Is.Null);

        Assert.That(resolveCount, Is.EqualTo(1));
        Assert.That(session.IsActive, Is.False);
        Assert.That(resolveCount, Is.EqualTo(2));

        isSessionActive = true;

        Assert.That(Session.Current, Is.EqualTo(session));
        Assert.That(resolveCount, Is.EqualTo(3));
        Assert.That(session.IsActive, Is.True);
        Assert.That(resolveCount, Is.EqualTo(4));

        isSessionActive = false;

        using (session.Activate()) {
          Assert.That(Session.Current, Is.EqualTo(session));
          Assert.That(session.IsActive, Is.True);
        }

        Assert.That(resolveCount, Is.EqualTo(4));
      }
    }
  }
}