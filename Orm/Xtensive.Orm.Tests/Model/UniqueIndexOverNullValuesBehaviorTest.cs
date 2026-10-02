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
  [Index($"{nameof(NullableValue)}:ASC", Unique = true)]
  public class TestEntityAscSingleColumn : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string NullableValue { get; set; }

    [Field]
    public long NonNullableValue { get; set; }

    public TestEntityAscSingleColumn(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  [Index($"{nameof(NullableValue)}:DESC", Unique = true)]
  public class TestEntityDescSingleColumn : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string NullableValue { get; set; }

    [Field]
    public long NonNullableValue { get; set; }

    public TestEntityDescSingleColumn(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  [Index($"{nameof(NullableValue)}:ASC", $"{nameof(NonNullableValue)}:ASC", Unique = true)]
  public class TestEntityAscAscMulticolumn : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string NullableValue { get; set; }

    [Field]
    public long NonNullableValue { get; set; }

    public TestEntityAscAscMulticolumn(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  [Index($"{nameof(NullableValue)}:ASC", $"{nameof(NonNullableValue)}:DESC", Unique = true)]
  public class TestEntityAscDescMulticolumn : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string NullableValue { get; set; }

    [Field]
    public long NonNullableValue { get; set; }

    public TestEntityAscDescMulticolumn(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  [Index($"{nameof(NullableValue)}:DESC", $"{nameof(NonNullableValue)}:ASC", Unique = true)]
  public class TestEntityDescAscMulticolumn : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string NullableValue { get; set; }

    [Field]
    public long NonNullableValue { get; set; }

    public TestEntityDescAscMulticolumn(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  [Index($"{nameof(NullableValue)}:DESC", $"{nameof(NonNullableValue)}:DESC", Unique = true)]
  public class TestEntityDescDescMulticolumn : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string NullableValue { get; set; }

    [Field]
    public long NonNullableValue { get; set; }

    public TestEntityDescDescMulticolumn(Session session)
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
      config.Types.Register(typeof(TestEntityAscSingleColumn));
      config.Types.Register(typeof(TestEntityDescSingleColumn));
      config.Types.Register(typeof(TestEntityAscAscMulticolumn));
      config.Types.Register(typeof(TestEntityAscDescMulticolumn));
      config.Types.Register(typeof(TestEntityDescAscMulticolumn));
      config.Types.Register(typeof(TestEntityDescDescMulticolumn));
      config.UpgradeMode = DomainUpgradeMode.Recreate;

      return config;
    }

    [Test]
    public void TrueDistinctNullsTest()
    {
      Require.ProviderIs(StorageProvider.PostgreSql | StorageProvider.MySql | StorageProvider.Sqlite);

      TestNullsDistinct();
    }

    [Test]
    public void TrueNonDistinctNullsTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);

      TestNullsNotDistinct();
    }

    [Test]
    public void InconsistentBehaviorTest()
    {
      Require.ProviderIs(StorageProvider.Firebird | StorageProvider.Oracle);

      using (var session = Domain.OpenSession()) {
        TestSigleColumnAsc(session, expectDistinct: true);
        TestSigleColumnDsc(session,
          expectDistinct: StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird));
        TestMultiColumnAscAsc(session, expectDistinct: false);
        TestMultiColumnAscDesc(session, expectDistinct: false);
        TestMultiColumnDescAsc(session, expectDistinct: false);
        TestMultiColumnDescDesc(session, expectDistinct: false);
      }
    }

    private void TestNullsDistinct()
    {
      using (var session = Domain.OpenSession()) {
        TestSigleColumnAsc(session, expectDistinct: true);
        TestSigleColumnDsc(session, expectDistinct: true);
        TestMultiColumnAscAsc(session, expectDistinct: true);
        TestMultiColumnAscDesc(session, expectDistinct: true);
        TestMultiColumnDescAsc(session, expectDistinct: true);
        TestMultiColumnDescDesc(session, expectDistinct: true);
      }
    }

    private void TestNullsNotDistinct()
    {
      using (var session = Domain.OpenSession()) {
        TestSigleColumnAsc(session, expectDistinct: false);
        TestSigleColumnDsc(session, expectDistinct: false);
        TestMultiColumnAscAsc(session, expectDistinct: false);
        TestMultiColumnAscDesc(session, expectDistinct: false);
        TestMultiColumnDescAsc(session, expectDistinct: false);
        TestMultiColumnDescDesc(session, expectDistinct: false);
      }
    }

    private void TestSigleColumnAsc(Session session, bool expectDistinct)
    {
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntityAscSingleColumn(session) { NonNullableValue = 10, NullableValue = null };
        session.SaveChanges();
        _ = new TestEntityAscSingleColumn(session) { NonNullableValue = 10, NullableValue = null };
        if (expectDistinct)
          Assert.DoesNotThrow(() => session.SaveChanges());
        else
          _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
      }
    }

    private void TestSigleColumnDsc(Session session, bool expectDistinct)
    {
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntityDescSingleColumn(session) { NonNullableValue = 10, NullableValue = null };
        session.SaveChanges();
        _ = new TestEntityDescSingleColumn(session) { NonNullableValue = 10, NullableValue = null };
        if (expectDistinct)
          Assert.DoesNotThrow(() => session.SaveChanges());
        else
          _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
      }
    }

    private void TestMultiColumnAscAsc(Session session, bool expectDistinct)
    {
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntityAscAscMulticolumn(session) { NonNullableValue = 10, NullableValue = null };
        session.SaveChanges();
        _ = new TestEntityAscAscMulticolumn(session) { NonNullableValue = 10, NullableValue = null };
        if (expectDistinct)
          Assert.DoesNotThrow(() => session.SaveChanges());
        else
          _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
      }
    }

    private void TestMultiColumnAscDesc(Session session, bool expectDistinct)
    {
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntityAscDescMulticolumn(session) { NonNullableValue = 10, NullableValue = null };
        session.SaveChanges();
        _ = new TestEntityAscDescMulticolumn(session) { NonNullableValue = 10, NullableValue = null };
        if (expectDistinct)
          Assert.DoesNotThrow(() => session.SaveChanges());
        else
          _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
      }
    }

    private void TestMultiColumnDescAsc(Session session, bool expectDistinct)
    {
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntityDescAscMulticolumn(session) { NonNullableValue = 10, NullableValue = null };
        session.SaveChanges();
        _ = new TestEntityDescAscMulticolumn(session) { NonNullableValue = 10, NullableValue = null };
        if (expectDistinct)
          Assert.DoesNotThrow(() => session.SaveChanges());
        else
          _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
      }
    }

    private void TestMultiColumnDescDesc(Session session, bool expectDistinct)
    {
      using (var tx = session.OpenTransaction()) {
        _ = new TestEntityDescDescMulticolumn(session) { NonNullableValue = 10, NullableValue = null };
        session.SaveChanges();
        _ = new TestEntityDescDescMulticolumn(session) { NonNullableValue = 10, NullableValue = null };
        if (expectDistinct)
          Assert.DoesNotThrow(() => session.SaveChanges());
        else
          _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
      }
    }
  }
}
