// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.02

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.LegacyDb.CrazyColumns2008TestModel;

namespace Xtensive.Orm.Tests.Storage.LegacyDb.CrazyColumns2008TestModel
{
  [Serializable]
  [HierarchyRoot]
  public class Crazy : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field]
    public DateTime Time { get; set; }

    [Field]
    public DateTime Date { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.LegacyDb
{

  [TestFixture]
  public class CrazyColumns2008Test : LegacyDbAutoBuildTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.ProviderVersionAtLeast(StorageProviderVersion.SqlServer2008);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(Crazy));
      return config;
    }

    [Test]
    public void CombinedTest()
    {
      var date = new DateTime(2000, 01, 01);
      var time = new DateTime(1,1,1,12, 00,00);
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var crazy1 = new Crazy {Date = date, Time = DateTime.Now};
        var crazy2 = new Crazy {Date = DateTime.Now, Time = time};
        ts.Complete();
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        foreach (var item in session.Query.All<Crazy>()) {
          Console.WriteLine(item.Date);
          Console.WriteLine(item.Time);
        }
        Assert.AreEqual(1, session.Query.All<Crazy>().Where(o => o.Date==date).Count());
        Assert.AreEqual(1, session.Query.All<Crazy>().Where(o => o.Time==time).Count());
      }
    }

    protected override string GetCreateDbScript(DomainConfiguration config)
    {
      return @"
        CREATE TABLE [dbo].[Crazy](
          [Id] [uniqueidentifier] NOT NULL,
          [Date] [date] NOT NULL,
          [Time] [time](7) NOT NULL,
          CONSTRAINT [PK_Crazy] PRIMARY KEY CLUSTERED ([Id] ASC) ON [PRIMARY]
        ) ON [PRIMARY]";
    }
  }
}