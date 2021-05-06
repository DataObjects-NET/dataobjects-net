// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2010.01.22

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0587_ByteArrayEquals_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0587_ByteArrayEquals_Model
  {
    [HierarchyRoot]
    public class User : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public byte[] Photo { get; set; }

    }
  }

  [Serializable]
  public class Issue0587_ByteArrayEquals : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (User).Assembly, typeof (User).Namespace);
      return config;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        for (byte i = 0; i < 10; i++) {
          _ = new User {
            Name = string.Format("name_{0}", i),
            Photo = new byte[] { i, i, i }
          };
        }
        t.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        int pageIndex = 1;
        int pageSize = 1;
        var usersQuery = session.Query.All<User>().Skip(pageIndex * pageSize).Take(pageSize);
        var key = new byte[] { 1, 1, 1 };
        var result = session.Query.All<User>().Where(user => user.Photo == key).ToList();
        Assert.Greater(result.Count, 0);
      }
    }
  }
}