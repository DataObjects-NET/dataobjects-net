// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.12.02

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.LegacyDb
{
  [Serializable]
  [HierarchyRoot]
  public class DT : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field]
    public DateTime Time { get; set; }

    [Field]
    public DateTime Date { get; set; }
  }

  [TestFixture]
  public class Sql2008DateAndTimeSupportTest : LegacyDbAutoBuildTest
  {
    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      EnsureProtocolIs(StorageProtocol.SqlServer);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(DT));
      return config;
    }

    [Test]
    public void CombinedTest()
    {
      var date = new DateTime(2000, 01, 01);
      var time = new DateTime(1,1,1,12, 00,00);
      using (Session.Open(Domain)) {
        using (var ts = Transaction.Open()) {

          var dt1 = new DT();
          dt1.Date = date;
          dt1.Time = DateTime.Now;

          var dt2 = new DT();
          dt2.Date = DateTime.Now;
          dt2.Time = time;

          ts.Complete();
        }
      }

      using (Session.Open(Domain)) {
        using (Transaction.Open()) {

          foreach (var item in Query.All<DT>()) {
            Console.WriteLine(item.Date);
            Console.WriteLine(item.Time);
          }

          Assert.AreEqual(1, Query.All<DT>().Where(o => o.Date == date).Count());
          Assert.AreEqual(1, Query.All<DT>().Where(o => o.Time == time).Count());
        }
      }
    }

    protected override string GetCreateDbScript(DomainConfiguration config)
    {
      return @"
  CREATE TABLE [dbo].[DT](
	[Id] [uniqueidentifier] NOT NULL,
	[Date] [date] NOT NULL,
	[Time] [time](7) NOT NULL,
 CONSTRAINT [PK_DT] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
) ON [PRIMARY]
) ON [PRIMARY]";
    }
  }
}