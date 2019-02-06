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

  public class IssueJira0759_UnableToTranslateOfTypeSelectExpression
  {
    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test1(InheritanceSchema schema)
    {
      using (OpenSessionTransaction(schema)) {
        var result = Query.All<TestEntity1>().OfType<ITestEntity2>().Select(x => x.Field2 + x.Field3).ToArray();
      }
    }

    private IDisposable OpenSessionTransaction(InheritanceSchema schema = InheritanceSchema.ClassTable)
    {
      return OpenSessionTransaction(
        (c, d) => d.Hierarchies.Where(x => !x.Root.IsSystem).ForEach(x => x.Schema = schema));
    }

    private IDisposable OpenSessionTransaction(Action<BuildingContext, DomainModelDef> onDefinitionsBuiltAction)
    {
      TestModule.OnDefinitionsBuiltAction = onDefinitionsBuiltAction;
      var domain = Domain.Build(this.BuildConfiguration());
      PopulateData(domain);
      var session = domain.OpenSession();
      var t = session.OpenTransaction();
      return new Disposable(
        x => {
          t.Dispose();
          session.Dispose();
          domain.Dispose();
        });
    }

    protected void PopulateData(Domain domain)
    {
      using (var s = domain.OpenSession())
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

        t.Complete();
      }
    }

    private DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (TestEntity1).Assembly, typeof (TestEntity1).Namespace);
      config.Types.Register(typeof (TestModule));
      return config;
    }

    public sealed class TestModule : IModule
    {
      public static Action<BuildingContext, DomainModelDef> OnDefinitionsBuiltAction { get; set; }

      public void OnBuilt(Domain domain)
      {
      }

      public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
      {
        if (OnDefinitionsBuiltAction!=null)
          OnDefinitionsBuiltAction(context, model);
      }
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
}
