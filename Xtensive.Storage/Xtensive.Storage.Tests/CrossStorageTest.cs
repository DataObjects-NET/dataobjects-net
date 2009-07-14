// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.03.18

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Tests
{
  public abstract class CrossStorageTest
  {
    private static readonly string[] providers = new[] {
        "memory", "mssql2005", "pgsql"
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
        using (Session.Open(domain))
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

    protected void RunTest(Func<Domain, RecordSet> recordSetCreator)
    {
      var oldResult = ExecuteRecordSet(domains.First(), recordSetCreator);
      foreach (var domain in domains.Skip(1)) {
        var newResult = ExecuteRecordSet(domain, recordSetCreator);
        CompareResults(oldResult, newResult);
        oldResult = newResult;
      }
    }

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

    private static List<Tuple> ExecuteRecordSet(Domain domain, Func<Domain, RecordSet> recordSetCreator)
    {
      List<Tuple> result;
      using (Session.Open(domain))
      using (Transaction.Open()) {
        Log.Info("Running '{0}' under '{1}'", recordSetCreator.Method.Name, domain.Configuration.Name);
        result = recordSetCreator(domain).ToList();
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
        Assert.AreEqual(leftEnumerator.Current, rightEnumerator.Current);
        hasLeft = leftEnumerator.MoveNext();
        hasRight = rightEnumerator.MoveNext();
      }
      Assert.IsFalse(hasLeft);
      Assert.IsFalse(hasRight);
    }
  }
}
