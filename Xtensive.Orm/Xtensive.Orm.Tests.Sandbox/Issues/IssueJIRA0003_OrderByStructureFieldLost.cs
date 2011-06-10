// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2011.03.05

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Diagnostics;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Sandbox.Issues.IssueJIRA0003_OrderByStructureFieldLost_Model;
using System.Linq;

namespace Xtensive.Orm.Tests.Sandbox.Issues
{
  namespace IssueJIRA0003_OrderByStructureFieldLost_Model
  {
    [HierarchyRoot]
    public class Person : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public Address Address { get; set; }
    }

    public class Address : Structure
    {
      [Field]
      public string City { get; set; }

      [Field]
      public string PostCode { get; set; }
    }
  }

  [Serializable]
  public class IssueJIRA0003_OrderByStructureFieldLost : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Address).Assembly, typeof (Address).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      var log = LogProvider.GetLog("Storage.Providers.Sql");
      log.RealLog.OnLogEvent += new LogEventHandler(RealLog_OnLogEvent);
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new Person() {Name = "Vasya", Address = new Address {City = "Moscow", PostCode = "444444"}};
        new Person() {Name = "Petya", Address = new Address {City = "Kiev", PostCode = "111111"}};
        session.SaveChanges();

        var query = session.Query.All<Person>()
          .Where(p => p.Name != "Vovka")
          .OrderBy(p => p.Address.PostCode)
          .Skip(() => 1)
          .Take(() => 5);
        var result = query.ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Vasya", result[0].Name);

        t.Complete();
      }
    }

    void RealLog_OnLogEvent(IRealLog source, LogEventTypes eventType, object message, Exception exception, LogCaptureScope capturedBy)
    {
      Console.WriteLine(message);
    }
  }
}