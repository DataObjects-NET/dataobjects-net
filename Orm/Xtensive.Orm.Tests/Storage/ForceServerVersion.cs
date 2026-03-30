// Copyright (C) 2012-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.03.06

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.Storage.ForceServerVersionModel;

namespace Xtensive.Orm.Tests.Storage
{
  namespace ForceServerVersionModel
  {
    [HierarchyRoot, KeyGenerator(KeyGeneratorKind.None)]
    public class Forced : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public DateTime DateTimeField { get; set; }

      public Forced(Session session, int id)
        : base(session, id)
      {
      }
    }
  }

  public class ForceServerVersion : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      Require.ProviderVersionAtLeast(new Version(16, 0));
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.ForcedServerVersion = "16.0.0.0"; 
      configuration.Types.Register(typeof (Forced));
      return configuration;
    }

    [Test]
    [Ignore(
      "Test is inconclusive, due to shrinked number of version. " +
      "New versions are relatively the same in terms of types/supported features at the moment.")]
    public void MainTest()
    {
      // Check that DateTime is used instead of DateTime2

      using (var session = Domain.OpenSession()) {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        using (var command = accessor.CreateCommand()) {
          command.CommandText =
            "select DATA_TYPE " +
            "from INFORMATION_SCHEMA.COLUMNS " +
            "where TABLE_SCHEMA = 'dbo' and TABLE_NAME = 'Forced' and COLUMN_NAME = 'DateTimeField'";
          var result = command.ExecuteScalar().ToString().ToLowerInvariant();
          Assert.That(result, Is.EqualTo("datetime"));
        }
      }
    }
  }
}