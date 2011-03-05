// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.22

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Linq;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Rse.VirtualAndRealIndexesModel;

namespace Xtensive.Orm.Tests.Rse
{
  #region Domain model
  namespace VirtualAndRealIndexesModel
  {
    [Serializable]
    [HierarchyRoot]
    [Index("HierarchyField")]
    public class A : Entity
    {
      [Field, Key]
      public Int32 Id { get; private set; }

      [Field]
      public string HierarchyField { get; set; }
    }

    [Serializable]
    [Index("ClassField")]
    public class B : A
    {
      [Field]
      public Int32 ClassField { get; set; }
    }
  }
  #endregion

  [TestFixture, Category("Rse")]
  public class IndexOptimizerVirtualAndRealIndexesTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.Index);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Orm.Tests.Rse.VirtualAndRealIndexesModel");
      return config;
    }

    [SetUp]
    public void SetUp()
    {
      FillStorage(300);
    }

    [Test]
    public void CombinedTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        Expression<Func<B, bool>> predicate = b => b.HierarchyField.GreaterThan("Random String 1")
          && b.ClassField < int.MinValue + 100000
            || b.HierarchyField.LessThanOrEqual("Random String 2") && b.ClassField > int.MinValue;
        var entities = session.Query.All<B>().ToList();
        var expected = session.Query.All<B>().ToList().Where(predicate.CachingCompile()).OrderBy(o => o.Id);
        var query = session.Query.All<B>().Where(predicate).OrderBy(o => o.Id);
        var actual = query.ToList();
        var virtualIndex = Domain.Model.Types[typeof (B)].Indexes.GetIndex("HierarchyField");
        if (!ConcreteTableSchemaModifier.IsEnabled)
          Assert.IsTrue(virtualIndex.IsVirtual);
        else
          Assert.IsFalse(virtualIndex.IsVirtual);
        IndexOptimizerTestHelper.ValidateUsedIndex(query, Domain.Model, virtualIndex,
          IndexOptimizerTestHelper.GetIndexForField<B>("ClassField", Domain.Model));
        IndexOptimizerTestHelper.ValidateQueryResult(expected, actual);
      }
    }

    private void FillStorage(int count)
    {
      var random = RandomManager.CreateRandom();
      var stringGenerator = InstanceGeneratorProvider.Default.GetInstanceGenerator<string>();
      var intGenerator = InstanceGeneratorProvider.Default.GetInstanceGenerator<Int32>();
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          for (int i = 0; i < count; i++) {
            new A {HierarchyField = stringGenerator.GetInstance(random)};
            var b1 = new B
            {
              HierarchyField = stringGenerator.GetInstance(random),
              ClassField = intGenerator.GetInstance(random)
            };
            new B
            {
              HierarchyField = stringGenerator.GetInstance(random),
              ClassField = intGenerator.GetInstance(random)
            };
          }
          t.Complete();
        }
      }
    }
  }
}