// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.01.31

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Issues
{
  using IssueJira0759_UnableToTranslateOfTypeSelectExpressionModels;

  public class IssueJira0759_UnableToTranslateOfTypeSelectExpression : AutoBuildTest
  {
    [Test]
    public void Test1()
    {
      using (var sto = OpenSessionTransaction()) {
        sto.Query.All<TestEntity1>().OfType<ITestEntity2>().Select(x => x.Field2 + x.Field3).ToArray();
      }
    }

    [Test]
    public void Test2()
    {
      using (var sto = OpenSessionTransaction()) {
        sto.Query.All<TestEntity>()
          .OfType<IWithStatus>()
          .Select(e => e.Status)
          .Any(); // OK

        sto.Query.All<TestEntity>()
          .OfType<IWithStatus>()
          .Select(e => e.Status)
          .ToArray(); // Exception
      }
    }

    protected override void PopulateData()
    {
      using (var s = Domain.OpenSession())
      using (s.Activate())
      using (var t = s.OpenTransaction()) {
        new TestEntity3() {
          Field1 = DateTime.FromBinary(100000),
          Field2 = 1000,
          Field3 = "String1",
          Field4 = 2.5,
        }.Field5.Add(
          new TestEntity3 {
            Field1 = DateTime.FromBinary(200000),
            Field2 = 2000,
            Field3 = "String2",
            Field4 = 3.5,
          });

        var status = new Status { Name = "test" };
        new TestEntity { TestField = "Test", Status = status };

        t.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(TestEntity1).Assembly, typeof(TestEntity1).Namespace);
      return config;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0759_UnableToTranslateOfTypeSelectExpressionModels
{
  public interface ITestEntity2 : IEntity
  {
    [Field]
    long Field2 { get; set; }

    [Field]
    string Field3 { get; set; }
  }

  [HierarchyRoot]
  public class TestEntity1 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public DateTime Field1 { get; set; }
  }

  public class TestEntity2 : TestEntity1, ITestEntity2
  {
    public long Field2 { get; set; }

    public string Field3 { get; set; }
  }

  public class TestEntity3 : TestEntity2
  {
    [Field]
    public double Field4 { get; set; }

    [Field]
    public EntitySet<TestEntity2> Field5 { get; set; }
  }




  public interface IWithStatus : IEntity
  {
    [Field]
    Status Status { get; set; }
  }

  [HierarchyRoot]
  [Serializable]
  public partial class Status : Entity
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    public bool IsDeletable { get; set; }

    [Field(Nullable = false)]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  [Serializable]
  public class TestEntity : Entity, IWithStatus
  {
    [Key]
    [Field(Nullable = false)]
    public Guid Id { get; private set; }

    [Field]
    public string TestField { get; set; }

    public Status Status { get; set; }
  }
}
