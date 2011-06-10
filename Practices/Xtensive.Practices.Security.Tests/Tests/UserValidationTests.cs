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
      using (var s = Domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {
          
          var u = new Employee(s);
          u.Name = "name";
          u.SetPassword("password");
          Assert.IsNotNullOrEmpty(u.PasswordHash);

          t.Complete();
        }
      }

      using (var s = Domain.OpenSession()) {
        using (var t = s.OpenTransaction()) {

          Assert.IsNull(s.ValidatePrincipal("name", "name"));
          Assert.IsNull(s.ValidatePrincipal("password", "name"));
          Assert.IsNotNull(s.ValidatePrincipal("name", "password"));

          t.Complete();
        }
      }
    }
  }
}