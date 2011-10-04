// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0717.Model;
using Xtensive.Core;
using Xtensive.Core.Testing;

namespace Xtensive.Storage.Tests.Issues.Issue0717.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field, Version]
    public DateTime Version { get; set; }

    /// <summary>
    /// Gets or sets Name.
    /// </summary>
    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0717_VersionCheckBug : AutoBuildTest
  {
    private const string VersionFieldName = "Version";

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Sessions.Add(new SessionConfiguration("Default")
      {
        BatchSize = 25,
        DefaultIsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
        CacheSize = 1000,
        Options = SessionOptions.AutoShortenTransactions
      }); 
      configuration.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      return configuration;
    }

    [Test]
    public void Test()
    {
      using (var session = Session.Open(Domain)) {
        var person  = new Person() { Name = "Name" };
        var key     = person.Key;
        var version = person.VersionInfo;

        // 1st update (ok)
        OptimisticUpdate<Person>(key, version, p => p.Name = "ANewName" );

        // 2nd update (must fail)
        AssertEx.Throws<VersionConflictException>(() =>
          OptimisticUpdate<Person>(key, version, p => p.Name = "ANewName" ));
      }
    }

    public virtual void OptimisticUpdate<T>(Key key, VersionInfo expectedVersion, Action<T> updater)
      where T: class, IEntity
    {
      var expectedVersions = new VersionSet(
        new List<KeyValuePair<Key, VersionInfo>> {
          new KeyValuePair<Key, VersionInfo>(key, expectedVersion),
        });

      using (VersionValidator.Attach(expectedVersions)) {
        using (var tx = Transaction.Open()) {
          var entity = Query.Single<T>(key);
          using (var ir = Validation.Disable()) {
            updater.Invoke(entity);
            ir.Complete();
          }
          tx.Complete();
        }
      }
    }
  }
}