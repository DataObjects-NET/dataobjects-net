// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
      Require.ProviderVersionAtLeast(new Version(10, 0));
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.ForcedServerVersion = "9.0.0.0";
      configuration.Types.Register(typeof (Forced));
      return configuration;
    }

    [Test]
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