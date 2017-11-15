// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using System.Linq;

namespace Xtensive.Orm.Tests.Storage.SessionDeactivationTest
{
  [TestFixture]
  public class SessionDeactivationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      // Domain with system types only - test as well ;)
      return configuration;
    }

    [Test]
    public void StandardTest()
    {
      AssertEx.Throws<InvalidOperationException>(() => {
        var session = Domain.OpenSession();
        using (session.Activate()) {
          using (var tx = session.OpenTransaction()) {
            var types = session.Query.All<Metadata.Type>().ToList();
            session.Dispose();
            tx.Complete();
          } // Error, since Dispose is allowed to throw an exception in this case
        }
      });

      {
        var session = Domain.OpenSession();
        using (session.Activate()) {
          using (var tx = session.OpenTransaction()) {
            var types = session.Query.All<Metadata.Type>().ToList();
            session.Dispose();
            // tx.Complete();
          } // No error, since Dispose must be silent in this case
        }
      }
    }
  }
}
