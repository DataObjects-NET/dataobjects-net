// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.22

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Xtensive.Storage.Tests.Issues.Issue0587_ByteArrayEquals_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Xtensive.Storage.Tests.Issues.Issue0587_ByteArrayEquals_Model
  {
    // [Index("Name", Unique = true)]
    // [Index("UniqueIndentifier", Unique = true)]
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
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Fill();
          t.Complete();
        }
      }
    }

    private void Fill()
    {
      for (byte i = 0; i < 10; i++) {
        var user = new User {
          Name = string.Format("name_{0}", i),
          Photo = new byte[] {i, i, i}
        };
      }
      Session.Current.SaveChanges();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (User).Assembly, typeof (User).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          int pageIndex = 1;
          int pageSize = 1;
          var usersQuery = session.Query.All<User>().Skip(pageIndex * pageSize).Take(pageSize);
          var key = new byte[]{1,1,1};
          var query = session.Query.All<User>().Where(user => user.Photo==key);
          var result = query.ToList();
          Assert.Greater(result.Count, 0);
        }
      }
    }
  }
}