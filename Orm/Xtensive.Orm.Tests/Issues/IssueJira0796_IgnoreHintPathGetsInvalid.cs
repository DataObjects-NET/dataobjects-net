// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2020.03.04

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0796_IgnoreHintPathGetsInvalidModel;
using Xtensive.Orm.Validation;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Caching;
using Xtensive.Orm.Upgrade;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0796_IgnoreHintPathGetsInvalid
  {
    private const string CreateIndexQuery = @"
      CREATE NONCLUSTERED INDEX [custom_NonClusteredIndex-20200304-164347] ON [dbo].[SomeEntity2] (
        [FirstName] ASC,
        [LastName] ASC)
      WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, 
        DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)";

    [Test]
    public void MainTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (SomeEntity1));
      configuration.Types.Register(typeof (SomeEntity2));

      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var accessor = session.Services.Get<DirectSqlAccessor>();
        using (var command = accessor.CreateCommand()) {
          command.CommandText = CreateIndexQuery;
          _ = command.ExecuteNonQuery();
        }
        transaction.Complete();
      }

      configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Perform;
      configuration.Types.Register(typeof(SomeEntity1));
      configuration.Types.Register(typeof(CustomUpgradeHandler));

      Assert.DoesNotThrow(() => Domain.Build(configuration).Dispose());
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0796_IgnoreHintPathGetsInvalidModel
{
  [HierarchyRoot]
  public class SomeEntity1 : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }
  }

  [HierarchyRoot]
  public class SomeEntity2 : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }
  }

  public class CustomUpgradeHandler : UpgradeHandler
  {
    private class KeepCustomIndicesMarkerHint : Hint
    {
      public override IEnumerable<HintTarget> GetTargets() => Enumerable.Empty<HintTarget>();
    }

    private const string CustomIndexPrefix = "custom_";

    public override void OnSchemaReady()
    {
      if (UpgradeContext.Stage == UpgradeStage.Upgrading) {
        var schemaHints = UpgradeContext.SchemaHints;
        var storageModel = (StorageModel) schemaHints.SourceModel;

        foreach (var table in storageModel.Tables) {
          foreach (var index in table.SecondaryIndexes) {
            var name = index.Name;
            if (!name.StartsWith(CustomIndexPrefix)) {
              continue;
            }
            var ignoreHint = new IgnoreHint(index.Path);
            schemaHints.Add(ignoreHint);
          }
        }
      }
    }
  }
}