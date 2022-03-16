// Copyright (C) 2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using M = Xtensive.Orm.Tests.Storage.Multimapping.CaseSensitiveSchemasTestModel;

namespace Xtensive.Orm.Tests.Storage.Multimapping.CaseSensitiveSchemasTestModel
{
  namespace Schema1
  {
    [HierarchyRoot]
    public class Entity1 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string OriginalSchemaName { get; set; }
    }
  }

  namespace Schema2
  {
    [HierarchyRoot]
    public class Entity2 : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string OriginalSchemaName { get; set; }
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.Multimapping
{
  public sealed class CaseSensitiveSchemasTest : MultimappingTest
  {
    private const string Schema1Name = WellKnownSchemas.Schema1;

    private readonly string schema1UpperCaseName = Schema1Name.ToUpperInvariant();

    protected override void CheckRequirements() => Require.ProviderIs(StorageProvider.Oracle);

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.DefaultSchema = Schema1Name;
      configuration.Types.Register(typeof(M.Schema1.Entity1));
      configuration.Types.Register(typeof(M.Schema2.Entity2));
      var rules = configuration.MappingRules;
      rules.Map(typeof(M.Schema1.Entity1).Namespace).ToSchema(Schema1Name);
      rules.Map(typeof(M.Schema2.Entity2).Namespace).ToSchema(schema1UpperCaseName);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      BuildInitialDomain();
      BuildUpgradedDomain();
    }

    private void BuildInitialDomain()
    {
      var config = BuildConfiguration();
      PrepareSchema(config.ConnectionInfo);
      var domain = Domain.Build(config);
      using (domain) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          _ = new M.Schema1.Entity1 { OriginalSchemaName = Schema1Name };
          _ = new M.Schema2.Entity2 { OriginalSchemaName = schema1UpperCaseName };
          tx.Complete();
        }
      }
    }

    private void BuildUpgradedDomain()
    {
      var config = BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.PerformSafely;
      var domain = Domain.Build(config);
      using (domain) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          var e1 = session.Query.All<M.Schema1.Entity1>().Single();
          var e2 = session.Query.All<M.Schema2.Entity2>().Single();
          Assert.That(e1.OriginalSchemaName, Is.EqualTo(Schema1Name));
          Assert.That(e2.OriginalSchemaName, Is.EqualTo(schema1UpperCaseName));
          tx.Complete();
        }
      }
    }

    private void PrepareSchema(ConnectionInfo connectionInfo)
    {
      StorageTestHelper.DemandSchemas(
        connectionInfo, Schema1Name, schema1UpperCaseName);
    }
  }
}
