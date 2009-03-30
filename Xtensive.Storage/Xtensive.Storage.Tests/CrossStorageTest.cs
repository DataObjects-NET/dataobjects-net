// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.18

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Tests
{
  public abstract class CrossStorageTest
  {
    private static readonly string[] providers = new[] {
        "memory", "mssql2005", "pgsql", "vistadb"
      };

    private DisposableSet disposableSet;
    private List<Domain> domains;
    
    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      disposableSet = new DisposableSet();
      domains = new List<Domain>();

      foreach (string provider in GetProviders()) {
        var config = BuildConfiguration(provider);
        var domain = BuildDomain(config);
        disposableSet.Add(domain);
        domains.Add(domain);
      }

      foreach (var domain in domains)
        using (domain.OpenSession())
        using (var t = Transaction.Open()) {
          FillData(domain);
          t.Complete();
        }
    }

    [TestFixtureTearDown]
    public void TestFixtureTearDown()
    {
      disposableSet.DisposeSafely();
    }

    [Test]
    public void CompareTest()
    {
      foreach (var generator in GetRecordSetGenerators()) {
        var oldResult = ExecuteRecordSet(domains.First(), generator);
        foreach (var domain in domains.Skip(1)) {
          var newResult = ExecuteRecordSet(domain, generator);
          CompareResults(oldResult, newResult);
          oldResult = newResult;
        }
      }
    }

    protected abstract IEnumerable<Func<Domain, RecordSet>> GetRecordSetGenerators();

    protected virtual IEnumerable<string> GetProviders()
    {
      return providers;
    }

    protected virtual DomainConfiguration BuildConfiguration(string provider)
    {
      return DomainConfigurationFactory.Create(provider);
    }

    protected virtual Domain BuildDomain(DomainConfiguration config)
    {
      return Domain.Build(config);
    }

    protected virtual void FillData(Domain domain)
    {
    }

    private static List<Tuple> ExecuteRecordSet(Domain domain, Func<Domain, RecordSet> createRecordSet)
    {
      List<Tuple> result;
      using (domain.OpenSession())
      using (Transaction.Open()) {
        result = createRecordSet(domain).ToList();
      }
      return result;
    }

    private static void CompareResults(IEnumerable<Tuple> left, IEnumerable<Tuple> right)
    {
      var leftEnumerator = left.GetEnumerator();
      var rightEnumerator = right.GetEnumerator();
      bool hasLeft = leftEnumerator.MoveNext();
      bool hasRight = rightEnumerator.MoveNext();
      while (hasLeft && hasRight) {
        Assert.IsTrue(leftEnumerator.Current.Equals(rightEnumerator.Current));
        hasLeft = leftEnumerator.MoveNext();
        hasRight = rightEnumerator.MoveNext();
      }
      Assert.AreEqual(hasLeft, hasRight);
    }
  }
}
