// Copyright (C) 2011-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.06.10

using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Security.Tests.Model;

namespace Xtensive.Orm.Security.Tests
{
  public class AuthenticationTests : SecurityTestBase
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {

        var employee = new Employee(session);
        employee.Name = "Steve Ballmer";
        employee.SetPassword("developers, developers, developers, developers");
        Assert.That(employee.PasswordHash, Is.Not.Null.Or.Empty);

        trx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var trx = session.OpenTransaction()) {

        Assert.That(session.Authenticate("Steve Ballmer", "Steve Ballmer"), Is.Null);
        Assert.That(session.Authenticate("developers, developers, developers, developers", "Steve Ballmer"), Is.Null);
        Assert.That(session.Authenticate("Steve Ballmer", "developers, developers, developers, developers"), Is.Not.Null);

        trx.Complete();
      }
    }
  }
}