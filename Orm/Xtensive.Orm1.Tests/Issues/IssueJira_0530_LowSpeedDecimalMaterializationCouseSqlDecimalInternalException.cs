// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2015.04.22

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Issues.IssueJira_0530_LowSpeedDecimalMaterializationCouseSqlDecimalInternalExceptionModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira_0530_LowSpeedDecimalMaterializationCouseSqlDecimalInternalExceptionModel
{
  [HierarchyRoot]
  public class EntityWithNormalDecimal : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Precision = 9, Scale = 2)]
    public decimal Decimal { get; set; }

  }

  [HierarchyRoot]
  public class EntityWithUnnormalDecimal : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(DefaultSqlExpression = "792281625142643375935439503351", Precision = 38, Scale = 5)]
    public decimal Decimal { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira_0530_LowSpeedDecimalMaterializationCouseSqlDecimalInternalException : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var watch1 = new Stopwatch();
        watch1.Start();
        var query1 = session.Query.All<EntityWithNormalDecimal>().Select(el => el.Decimal).ToArray();
        watch1.Stop();
        Console.WriteLine(watch1.ElapsedMilliseconds);

        watch1.Reset();
        Assert.AreEqual(0, watch1.ElapsedMilliseconds);
        watch1.Start();
        var query2 = session.Query.All<EntityWithUnnormalDecimal>().Select(el => el.Decimal).ToArray();
        watch1.Stop();
        Console.WriteLine(watch1.ElapsedMilliseconds);
      }
    }

    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var service = session.Services.Get<DirectSqlAccessor>();
        using (var command = service.CreateCommand()) {
          command.CommandText = "INSERT INTO [EntityWithUnnormalDecimal](Id) Values (0)";
          command.ExecuteNonQuery();
        }
        for (int i = 0; i < 1000000; i++) {
          new EntityWithNormalDecimal() {Decimal = new decimal(1 + i)};
          new EntityWithUnnormalDecimal() {Decimal = new decimal(2000 + i)};
        }
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithNormalDecimal).Assembly, typeof (EntityWithNormalDecimal).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
