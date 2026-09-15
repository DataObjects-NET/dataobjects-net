// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.UniqueIndexOverNullValuesBehaviorTestModel;

namespace Xtensive.Orm.Tests.Model.UniqueIndexOverNullValuesBehaviorTestModel
{
  [HierarchyRoot]
  [Index(nameof(NullableValue), nameof(NonNullableValue), Unique = true)]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string NullableValue { get; set; }

    [Field]
    public long NonNullableValue { get; set; }

    public TestEntity(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class UniqueIndexOverNullValuesBehaviorTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(TestEntity));
      config.UpgradeMode = DomainUpgradeMode.Recreate;

      return config;
    }

    [Test]
    public void MsSqlTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      TestNullsNotDistinct();
    }

    [Test]
    public void PgSqlTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql);
      Require.ProviderVersionAtMost(new Version(14, 9));

      TestNullsDistinct();
    }

    [Test]
    public void FirebirdTest()
    {
      Require.ProviderIs(StorageProvider.Firebird);

      TestNullsNotDistinct();
    }

    [Test]
    public void MySqlTest()
    {
      Require.ProviderIs(StorageProvider.MySql);

      TestNullsDistinct();
    }

    [Test]
    public void SqliteTest()
    {
      Require.ProviderIs(StorageProvider.Sqlite);
      
      TestNullsDistinct();
    }

    public void TestNullsDistinct()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntity(session) { NonNullableValue = 10, NullableValue = null };
        session.SaveChanges();
        _ = new TestEntity(session) { NonNullableValue = 10, NullableValue = null };
        Assert.DoesNotThrow(() => session.SaveChanges());
      }
    }

    public void TestNullsNotDistinct()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntity(session) { NonNullableValue = 10, NullableValue = null };
        session.SaveChanges();
        _ = new TestEntity(session) { NonNullableValue = 10, NullableValue = null };
        _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
      }
    }

  }
}
