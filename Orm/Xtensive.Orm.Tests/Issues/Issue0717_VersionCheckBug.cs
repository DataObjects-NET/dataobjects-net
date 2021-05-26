// Copyright (C) 2010-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Collections.Generic;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0717.Model;
using Xtensive.Core;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Issues.Issue0717.Model
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

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0717_VersionCheckBug : AutoBuildTest
  {
    private const string VersionFieldName = "Version";

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      // MySQL does not store milliseconds
      // and we increase Millisecond part of DateTime value on Version increase
      // so test is irrelevant to this RDBMS
      Require.ProviderIsNot(StorageProvider.MySql);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      configuration.Sessions.Default.BatchSize = 25;
      configuration.Sessions.Default.DefaultIsolationLevel = IsolationLevel.ReadCommitted;
      configuration.Sessions.Default.CacheSize = 1000;
      return configuration;
    }

    [Test]
    public void Test()
    {
      using (var session = Domain.OpenSession()) {
        Person person;
        Key key;
        VersionInfo version;
        using (var tx = session.OpenTransaction()) {
          person = new Person() {Name = "Name"};
          key = person.Key;
          version = person.VersionInfo;
          tx.Complete();
        }

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

      var session = Session.Demand();
      using (VersionValidator.Attach(session, expectedVersions)) {
        using (var tx = session.OpenTransaction()) {
          var entity = session.Query.Single<T>(key);
          updater.Invoke(entity);
          session.Validate();
          tx.Complete();
        }
      }
    }
  }
}