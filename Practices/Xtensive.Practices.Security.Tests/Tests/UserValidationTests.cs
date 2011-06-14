// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.06.10

using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Practices.Security.Tests.Model;

namespace Xtensive.Practices.Security.Tests
{
  public class UserValidationTests : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var trx = session.OpenTransaction()) {
          
          var employee = new Employee(session);
          employee.Name = "Steve Ballmer";
          employee.SetPassword("developers, developers, developers, developers");
          Assert.IsNotNullOrEmpty(employee.PasswordHash);

          trx.Complete();
        }
      }

      using (var session = Domain.OpenSession()) {
        using (var trx = session.OpenTransaction()) {

          Assert.IsNull(session.ValidatePrincipal("Steve Ballmer", "Steve Ballmer"));
          Assert.IsNull(session.ValidatePrincipal("developers, developers, developers, developers", "Steve Ballmer"));
          Assert.IsNotNull(session.ValidatePrincipal("Steve Ballmer", "developers, developers, developers, developers"));

          trx.Complete();
        }
      }
    }
  }
}