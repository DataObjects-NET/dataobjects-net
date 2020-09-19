﻿// Copyright (C) 2014-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2014.06.23

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq.ExecuteDelayedForIOrderedQueryableQueryModel;

namespace Xtensive.Orm.Tests.Linq.ExecuteDelayedForIOrderedQueryableQueryModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public int OrderByThisField { get; set; }
  }

}

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  public class ExecuteDelayedForIOrderedQueryableQuery : AutoBuildTest
  {
    private readonly int[] unsortedValueSequence = new[] {10, 5, 100, 2, 15, 1, 18, 9};
    
    [Test]
    public void MainTest()
    {
      var randomer = new Random();
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var delaedQuery = session.Query.CreateDelayedQuery(
          query => from zoo in query.All<TestEntity>()
            orderby zoo.OrderByThisField
            select zoo);
        Assert.IsInstanceOf<IEnumerable<TestEntity>>(delaedQuery);
        session.ExecuteUserDefinedDelayedQueries(true);
        var sortedValues = new int[unsortedValueSequence.Length];
        unsortedValueSequence.CopyTo(sortedValues, 0);
        Array.Sort(sortedValues);
        var currentValueIndex = 0;
        foreach (var testEntity in delaedQuery) {
          Assert.AreEqual(sortedValues[currentValueIndex], testEntity.OrderByThisField);
          currentValueIndex++;
        }
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(TestEntity));
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }

    protected override void PopulateData()
    {
      var randomer = new Random();
      using (var session = Domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        foreach (var value in unsortedValueSequence)
          new TestEntity { OrderByThisField = value };

        transaction.Complete();
      }
    }
  }
}
