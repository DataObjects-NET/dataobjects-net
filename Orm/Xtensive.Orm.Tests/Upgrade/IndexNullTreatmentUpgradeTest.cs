// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Orm.Providers;
using NUnit.Framework;
using Xtensive.Core;
using NoChanges = Xtensive.Orm.Tests.Upgrade.IndexNullTreatmentUpgradeTestModel.NoChanges;
using AddIndex = Xtensive.Orm.Tests.Upgrade.IndexNullTreatmentUpgradeTestModel.AddIndex;
using IncludeNullableColumn = Xtensive.Orm.Tests.Upgrade.IndexNullTreatmentUpgradeTestModel.IncludeNullableColumn;
using IncludeNotNullableColumn = Xtensive.Orm.Tests.Upgrade.IndexNullTreatmentUpgradeTestModel.IncludeNotNullableColumn;
using ExcludeNullableColumn = Xtensive.Orm.Tests.Upgrade.IndexNullTreatmentUpgradeTestModel.ExcludeNullableColumn;
using ExcludeNotNullableColumn = Xtensive.Orm.Tests.Upgrade.IndexNullTreatmentUpgradeTestModel.ExcludeNotNullableColumn;


namespace Xtensive.Orm.Tests.Upgrade
{
  internal class IndexNullTreatmentUpgradeTest
  {
    private bool defaultSingleColumnDistinct;
    private bool defaultMultiColumnDistinct;

    private bool isFirebird;
    private bool isPostgres;

    [OneTimeSetUp]
    public void TestFixtureSetup()
    {
      var instance = StorageProviderInfo.Instance;

      if (instance.CheckProviderIs(StorageProvider.Sqlite))
        throw new IgnoreException($"This test requires any provider except the following: {StorageProvider.Sqlite}");
      isFirebird = instance.CheckProviderIs(StorageProvider.Firebird);
      isPostgres = instance.CheckProviderIs(StorageProvider.PostgreSql);

      (defaultSingleColumnDistinct, defaultMultiColumnDistinct) = instance.Provider switch {
        // strorages treats nulls as unique value for single column indexes
        // but for multicolumn indexes they don't
        StorageProvider.Firebird or StorageProvider.Oracle => (true, false),
        // storages always treat nulls as unique values
        StorageProvider.MySql or StorageProvider.PostgreSql => (true, true),
        // storages always treat nulls as non-unique values
        StorageProvider.SqlServer => (false, false),
        _ => throw new ArgumentException()
      };
    }

    [Test]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.NotDistinct)]
    public void SameStructureTest(UniqueNullTreatment from, UniqueNullTreatment to)
    {
      if (from != UniqueNullTreatment.Default || to != UniqueNullTreatment.Default)
        Require.AllFeaturesSupported(ProviderFeatures.UniqueNullTreatment);

      var initConfig = DomainConfigurationFactory.Create();
      initConfig.Types.RegisterCaching(GetType().Assembly, typeof(NoChanges.UniqueSingleColumn).Namespace);
      initConfig.UniqueNullTreatment = from;
      initConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      bool singleColumnDistinct, multiColumnDistinct, indexBuildCrash;
      (singleColumnDistinct, multiColumnDistinct, indexBuildCrash) = from switch {
        UniqueNullTreatment.Default =>
          (defaultSingleColumnDistinct, defaultMultiColumnDistinct, to == UniqueNullTreatment.NotDistinct),
        UniqueNullTreatment.Distinct =>
          (true, true, (to == UniqueNullTreatment.NotDistinct || (to == UniqueNullTreatment.Default && (!defaultSingleColumnDistinct && !defaultMultiColumnDistinct)))),
        UniqueNullTreatment.NotDistinct => (false, false, false),
        _ => throw new ArgumentOutOfRangeException()
      };

      bool expectSchemaDifference = to switch {
        UniqueNullTreatment.NotDistinct when from is not UniqueNullTreatment.NotDistinct => true,
        UniqueNullTreatment.Distinct or UniqueNullTreatment.Default when from is UniqueNullTreatment.NotDistinct => true,
        _ => false
      };

      using (var initDomain = Domain.Build(initConfig)) {
        FillNoChangesData(initDomain, singleColumnDistinct, multiColumnDistinct);
      }

      var upgradeConfig = DomainConfigurationFactory.Create();
      upgradeConfig.Types.RegisterCaching(GetType().Assembly, typeof(NoChanges.UniqueSingleColumn).Namespace);
      upgradeConfig.UniqueNullTreatment = to;
      upgradeConfig.UpgradeMode = DomainUpgradeMode.Validate;

      if (expectSchemaDifference) {
        _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfig).DisposeSafely());

        upgradeConfig = upgradeConfig.Clone();
        upgradeConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;
      }
      if (indexBuildCrash) {
        _ = Assert.Throws<UniqueConstraintViolationException>(() => Domain.Build(upgradeConfig).DisposeSafely());
      }
      else {
        (singleColumnDistinct, multiColumnDistinct) = to switch {
          UniqueNullTreatment.Default => (defaultSingleColumnDistinct, defaultMultiColumnDistinct),
          UniqueNullTreatment.Distinct => (true, true),
          UniqueNullTreatment.NotDistinct => (false, false),
          _ => throw new ArgumentOutOfRangeException()
        };

        using (var upgraded = Domain.Build(upgradeConfig)) {
          using (var session = upgraded.OpenSession()) {
            using (var countsValidation = session.OpenTransaction()) {
              Assert.That(session.Query.All<NoChanges.UniqueSingleColumn>().Count(), Is.EqualTo(4));
              Assert.That(session.Query.All<NoChanges.UniqueMultiColumn>().Count(), Is.EqualTo(4));
            }

            using (var uniqueInsert1 = session.OpenTransaction()) {
              if (singleColumnDistinct) {
                _ = new NoChanges.UniqueSingleColumn(session) { NullableValue = null };
                Assert.DoesNotThrow(() => session.SaveChanges());
              }
              else {
                _ = new NoChanges.UniqueSingleColumn(session) { NullableValue = null };
                _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
              }
            }

            using (var uniqueInsert2 = session.OpenTransaction()) {
              if (multiColumnDistinct) {
                _ = new NoChanges.UniqueMultiColumn(session) { NullableValue = null, NotNullableValue = 100 };
                Assert.DoesNotThrow(() => session.SaveChanges());
              }
              else {
                _ = new NoChanges.UniqueMultiColumn(session) { NullableValue = null, NotNullableValue = 100 };
                _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
              }
            }
          }
        }
      }
    }

    private static void FillNoChangesData(Domain initDomain, bool nullIsUniqueS, bool nullIsUniqueM)
    {
      using (var poppulateSession = initDomain.OpenSession())
      using (var tx = poppulateSession.OpenTransaction()) {
        if (nullIsUniqueS) {
          _ = new NoChanges.UniqueSingleColumn(poppulateSession) { NullableValue = null };
          _ = new NoChanges.UniqueSingleColumn(poppulateSession) { NullableValue = null };
          _ = new NoChanges.UniqueSingleColumn(poppulateSession) { NullableValue = 10 };
          _ = new NoChanges.UniqueSingleColumn(poppulateSession) { NullableValue = 20 };
        }
        else {
          _ = new NoChanges.UniqueSingleColumn(poppulateSession) { NullableValue = null };
          _ = new NoChanges.UniqueSingleColumn(poppulateSession) { NullableValue = -10 };
          _ = new NoChanges.UniqueSingleColumn(poppulateSession) { NullableValue = 10 };
          _ = new NoChanges.UniqueSingleColumn(poppulateSession) { NullableValue = 20 };
        }

        if (nullIsUniqueM) {
          _ = new NoChanges.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
          _ = new NoChanges.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
          _ = new NoChanges.UniqueMultiColumn(poppulateSession) { NullableValue = 10, NotNullableValue = 200 };
          _ = new NoChanges.UniqueMultiColumn(poppulateSession) { NullableValue = 20, NotNullableValue = 200 };
        }
        else {
          _ = new NoChanges.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
          _ = new NoChanges.UniqueMultiColumn(poppulateSession) { NullableValue = -10, NotNullableValue = 100 };
          _ = new NoChanges.UniqueMultiColumn(poppulateSession) { NullableValue = 10, NotNullableValue = 200 };
          _ = new NoChanges.UniqueMultiColumn(poppulateSession) { NullableValue = 20, NotNullableValue = 200 };
        }

        tx.Complete();
      }
    }

    [Test]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.NotDistinct)]
    public void AddIndexTest(UniqueNullTreatment from, UniqueNullTreatment to)
    {
      if (from != UniqueNullTreatment.Default || to != UniqueNullTreatment.Default)
        Require.AllFeaturesSupported(ProviderFeatures.UniqueNullTreatment);

      var initConfig = DomainConfigurationFactory.Create();
      initConfig.Types.RegisterCaching(GetType().Assembly, typeof(AddIndex.Initial.UniqueSingleColumn).Namespace);
      initConfig.UniqueNullTreatment = from;
      initConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      bool singleColumnDistinct, multiColumnDistinct, indexBuildCrash;
      (singleColumnDistinct, multiColumnDistinct, indexBuildCrash) = from switch {
        UniqueNullTreatment.Default =>
          (defaultSingleColumnDistinct, defaultMultiColumnDistinct, to == UniqueNullTreatment.NotDistinct),
        UniqueNullTreatment.Distinct =>
          (true, true, (to == UniqueNullTreatment.NotDistinct || (to == UniqueNullTreatment.Default && (!defaultSingleColumnDistinct && !defaultMultiColumnDistinct)))),
        UniqueNullTreatment.NotDistinct => (false, false, false),
        _ => throw new ArgumentOutOfRangeException()
      };

      using (var initDomain = Domain.Build(initConfig)) {
        FillAddIndexData(initDomain, singleColumnDistinct, multiColumnDistinct);
      }

      var upgradeConfig = DomainConfigurationFactory.Create();
      upgradeConfig.Types.RegisterCaching(GetType().Assembly, typeof(AddIndex.WithIndexes.UniqueSingleColumn).Namespace);
      upgradeConfig.UniqueNullTreatment = to;
      upgradeConfig.UpgradeMode = DomainUpgradeMode.Validate;

      _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfig).DisposeSafely());

      upgradeConfig = upgradeConfig.Clone();
      upgradeConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      if (indexBuildCrash) {
        _ = Assert.Throws<UniqueConstraintViolationException>(() => Domain.Build(upgradeConfig).DisposeSafely());
      }
      else {
        (singleColumnDistinct, multiColumnDistinct) = to switch {
          UniqueNullTreatment.Default => (defaultSingleColumnDistinct, defaultMultiColumnDistinct),
          UniqueNullTreatment.Distinct => (true, true),
          UniqueNullTreatment.NotDistinct => (false, false),
          _ => throw new ArgumentOutOfRangeException()
        };

        using (var upgraded = Domain.Build(upgradeConfig)) {
          using (var session = upgraded.OpenSession()) {
            using (var countsValidation = session.OpenTransaction()) {
              Assert.That(session.Query.All<AddIndex.WithIndexes.UniqueSingleColumn>().Count(), Is.EqualTo(3));
              Assert.That(session.Query.All<AddIndex.WithIndexes.UniqueMultiColumn>().Count(), Is.EqualTo(3));
            }

            using (var uniqueInsert1 = session.OpenTransaction()) {
              if (singleColumnDistinct) {
                _ = new AddIndex.WithIndexes.UniqueSingleColumn(session) { NullableValue = null };
                Assert.DoesNotThrow(() => session.SaveChanges());
              }
              else {
                _ = new AddIndex.WithIndexes.UniqueSingleColumn(session) { NullableValue = null };
                _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
              }
            }

            using (var uniqueInsert2 = session.OpenTransaction()) {
              if (multiColumnDistinct) {
                _ = new AddIndex.WithIndexes.UniqueMultiColumn(session) { NullableValue = null, NotNullableValue = 100 };
                Assert.DoesNotThrow(() => session.SaveChanges());
              }
              else {
                _ = new AddIndex.WithIndexes.UniqueMultiColumn(session) { NullableValue = null, NotNullableValue = 100 };
                _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
              }
            }
          }
        }
      }
    }

    [Test]
    [TestCase(UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.NotDistinct)]
    public void AddIndexToCorruptData(UniqueNullTreatment to)
    {
      if (to != UniqueNullTreatment.Default)
        Require.AllFeaturesSupported(ProviderFeatures.UniqueNullTreatment);

      var initConfig = DomainConfigurationFactory.Create();
      initConfig.Types.RegisterCaching(GetType().Assembly, typeof(AddIndex.Initial.UniqueSingleColumn).Namespace);
      initConfig.UniqueNullTreatment = UniqueNullTreatment.Default;
      initConfig.UpgradeMode = DomainUpgradeMode.Recreate;


      using (var initDomain = Domain.Build(initConfig)) {
        FillAddIndexData(initDomain, defaultSingleColumnDistinct, defaultMultiColumnDistinct, true, false);
      }

      var upgradeConfig = DomainConfigurationFactory.Create();
      upgradeConfig.Types.RegisterCaching(GetType().Assembly, typeof(AddIndex.WithIndexes.UniqueSingleColumn).Namespace);
      upgradeConfig.UniqueNullTreatment = to;
      upgradeConfig.UpgradeMode = DomainUpgradeMode.Validate;

      _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfig).DisposeSafely());

      upgradeConfig = upgradeConfig.Clone();
      upgradeConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      if (!defaultSingleColumnDistinct || to == UniqueNullTreatment.NotDistinct) {
        _ = isFirebird || isPostgres
          ? Assert.Throws<UniqueConstraintViolationException>(() => Domain.Build(upgradeConfig).DisposeSafely())
          : Assert.Throws<StorageException>(() => Domain.Build(upgradeConfig).DisposeSafely());
      }
      else {
        Assert.DoesNotThrow(() => Domain.Build(upgradeConfig).DisposeSafely());
      }

      using (var initDomain = Domain.Build(initConfig)) {
        FillAddIndexData(initDomain, defaultSingleColumnDistinct, defaultMultiColumnDistinct, false, true);
      }

      if (!defaultMultiColumnDistinct || to == UniqueNullTreatment.NotDistinct) {
        _ = isFirebird || isPostgres
          ? Assert.Throws<UniqueConstraintViolationException>(() => Domain.Build(upgradeConfig).DisposeSafely())
          : Assert.Throws<StorageException>(() => Domain.Build(upgradeConfig).DisposeSafely());
      }
      else {
        Assert.DoesNotThrow(() => Domain.Build(upgradeConfig).DisposeSafely());
      }
    }

    private static void FillAddIndexData(Domain initDomain, bool nullIsUniqueS, bool nullIsUniqueM,
      bool corruptSingleColumn = false, bool corruptMulticolumn = false)
    {
      using (var poppulateSession = initDomain.OpenSession())
      using (var tx = poppulateSession.OpenTransaction()) {

        if (nullIsUniqueS) {
          _ = new AddIndex.Initial.UniqueSingleColumn(poppulateSession) { NullableValue = null };
          _ = new AddIndex.Initial.UniqueSingleColumn(poppulateSession) { NullableValue = null };
          _ = new AddIndex.Initial.UniqueSingleColumn(poppulateSession) { NullableValue = 10 };
        }
        else {
          _ = new AddIndex.Initial.UniqueSingleColumn(poppulateSession) { NullableValue = null };
          if (corruptSingleColumn) {
            _ = new AddIndex.Initial.UniqueSingleColumn(poppulateSession) { NullableValue = null };
          }
          _ = new AddIndex.Initial.UniqueSingleColumn(poppulateSession) { NullableValue = 10 };
          _ = new AddIndex.Initial.UniqueSingleColumn(poppulateSession) { NullableValue = 20 };
        }

        if (nullIsUniqueM) {
          _ = new AddIndex.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
          _ = new AddIndex.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
          _ = new AddIndex.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = 10, NotNullableValue = 200 };
        }
        else {
          _ = new AddIndex.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
          if (corruptMulticolumn) {
            _ = new AddIndex.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
          }
          _ = new AddIndex.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = 10, NotNullableValue = 200 };
          _ = new AddIndex.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = 20, NotNullableValue = 200 };
        }

        tx.Complete();
      }
    }


    [Test]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.NotDistinct)]
    public void IncludeNullableColumn(UniqueNullTreatment from, UniqueNullTreatment to)
    {
      if (from != UniqueNullTreatment.Default || to != UniqueNullTreatment.Default)
        Require.AllFeaturesSupported(ProviderFeatures.UniqueNullTreatment);

      var initConfig = DomainConfigurationFactory.Create();
      initConfig.Types.Register(typeof(IncludeNullableColumn.Initial.UniqueMultiColumn));
      initConfig.UniqueNullTreatment = from;
      initConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      bool multiColumnDistinct, indexBuildCrash;
      (multiColumnDistinct, indexBuildCrash) = from switch {
        UniqueNullTreatment.Default =>
          (defaultMultiColumnDistinct, to == UniqueNullTreatment.NotDistinct),
        UniqueNullTreatment.Distinct =>
          (true, (to == UniqueNullTreatment.NotDistinct || (to == UniqueNullTreatment.Default && (!defaultSingleColumnDistinct && !defaultMultiColumnDistinct)))),
        UniqueNullTreatment.NotDistinct => (false, false),
        _ => throw new ArgumentOutOfRangeException()
      };

      using (var initDomain = Domain.Build(initConfig)) {
        FillIncludeNullColumnData(initDomain, multiColumnDistinct);
      }

      var upgradeConfig = DomainConfigurationFactory.Create();
      upgradeConfig.Types.Register(typeof(IncludeNullableColumn.Upgraded.UniqueMultiColumn));
      upgradeConfig.UniqueNullTreatment = to;
      upgradeConfig.UpgradeMode = DomainUpgradeMode.Validate;

      _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfig).DisposeSafely());

      upgradeConfig = upgradeConfig.Clone();
      upgradeConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      if (indexBuildCrash) {
        _ = Assert.Throws<UniqueConstraintViolationException>(() => Domain.Build(upgradeConfig).DisposeSafely());
      }
      else {
        multiColumnDistinct = to switch {
          UniqueNullTreatment.Default => defaultMultiColumnDistinct,
          UniqueNullTreatment.Distinct => true,
          UniqueNullTreatment.NotDistinct => false,
          _ => throw new ArgumentOutOfRangeException()
        };

        using (var upgraded = Domain.Build(upgradeConfig)) {
          using (var session = upgraded.OpenSession()) {
            using (var tx = session.OpenTransaction()) {
              Assert.That(session.Query.All<IncludeNullableColumn.Upgraded.UniqueMultiColumn>().Count(), Is.EqualTo(5));
            }
            using (var uniqueInsert = session.OpenTransaction()) {
              _ = new IncludeNullableColumn.Upgraded.UniqueMultiColumn(session) { NullableValueA = null, NullableValueB = 100 };
              if (multiColumnDistinct)
                Assert.DoesNotThrow(() => session.SaveChanges());
              else {
                _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
              }
            }
          }
        }
      }
    }

    private static void FillIncludeNullColumnData(Domain initDomain, bool nullIsUniqueM)
    {
      using (var poppulateSession = initDomain.OpenSession())
      using (var tx = poppulateSession.OpenTransaction()) {

        if (nullIsUniqueM) {
          _ = new IncludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = null, NullableValueB = null };
          _ = new IncludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = null, NullableValueB = 100 };
          _ = new IncludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = null, NullableValueB = 100 };
          _ = new IncludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = 10, NullableValueB = null };
          _ = new IncludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = 20, NullableValueB = 200 };
        }
        else {
          _ = new IncludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = null, NullableValueB = 100 };
          _ = new IncludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = 10, NullableValueB = null };
          _ = new IncludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = 20, NullableValueB = 200 };
          _ = new IncludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = 30, NullableValueB = 300 };
          _ = new IncludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = 40, NullableValueB = 400 };
        }

        tx.Complete();
      }
    }

    [Test]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.NotDistinct)]
    public void IncludeNotNullColumn(UniqueNullTreatment from, UniqueNullTreatment to)
    {
      if (from != UniqueNullTreatment.Default || to != UniqueNullTreatment.Default)
        Require.AllFeaturesSupported(ProviderFeatures.UniqueNullTreatment);

      var initConfig = DomainConfigurationFactory.Create();
      initConfig.Types.Register(typeof(IncludeNotNullableColumn.Initial.UniqueMultiColumn));
      initConfig.UniqueNullTreatment = from;
      initConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      bool multiColumnDistinct, indexBuildCrash;
      (multiColumnDistinct, indexBuildCrash) = from switch {
        UniqueNullTreatment.Default =>
          (defaultMultiColumnDistinct, to == UniqueNullTreatment.NotDistinct),
        UniqueNullTreatment.Distinct =>
          (true, (to == UniqueNullTreatment.NotDistinct || (to == UniqueNullTreatment.Default && (!defaultSingleColumnDistinct && !defaultMultiColumnDistinct)))),
        UniqueNullTreatment.NotDistinct => (false, false),
        _ => throw new ArgumentOutOfRangeException()
      };

      using (var initDomain = Domain.Build(initConfig)) {
        FillIncludeNotNullColumnData(initDomain, multiColumnDistinct);
      }

      var upgradeConfig = DomainConfigurationFactory.Create();
      upgradeConfig.Types.Register(typeof(IncludeNotNullableColumn.Upgraded.UniqueMultiColumn));
      upgradeConfig.UniqueNullTreatment = to;
      upgradeConfig.UpgradeMode = DomainUpgradeMode.Validate;

      _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfig).DisposeSafely());

      upgradeConfig = upgradeConfig.Clone();
      upgradeConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      if (indexBuildCrash) {
        _ = Assert.Throws<UniqueConstraintViolationException>(() => Domain.Build(upgradeConfig).DisposeSafely());
      }
      else {
        multiColumnDistinct = to switch {
          UniqueNullTreatment.Default => defaultMultiColumnDistinct,
          UniqueNullTreatment.Distinct => true,
          UniqueNullTreatment.NotDistinct => false,
          _ => throw new ArgumentOutOfRangeException()
        };

        using (var upgraded = Domain.Build(upgradeConfig)) {
          using (var session = upgraded.OpenSession()) {
            using (var tx = session.OpenTransaction()) {
              Assert.That(session.Query.All<IncludeNotNullableColumn.Upgraded.UniqueMultiColumn>().Count(), Is.EqualTo(2));
            }
            using (var uniqueInsert = session.OpenTransaction()) {
              _ = new IncludeNotNullableColumn.Upgraded.UniqueMultiColumn(session) { NullableValue = null, NotNullableValue = 100 };
              if (multiColumnDistinct)
                Assert.DoesNotThrow(() => session.SaveChanges());
              else {
                _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
              }
            }
          }
        }
      }
    }

    private static void FillIncludeNotNullColumnData(Domain initDomain, bool nullIsUniqueM)
    {
      using (var poppulateSession = initDomain.OpenSession())
      using (var tx = poppulateSession.OpenTransaction()) {

        if (nullIsUniqueM) {
          _ = new IncludeNotNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
          _ = new IncludeNotNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
        }
        else {
          _ = new IncludeNotNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
          _ = new IncludeNotNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = 20, NotNullableValue = 200 };
        }

        tx.Complete();
      }
    }

    [Test]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.NotDistinct)]
    public void ExcludeNullColumn(UniqueNullTreatment from, UniqueNullTreatment to)
    {
      if (from != UniqueNullTreatment.Default || to != UniqueNullTreatment.Default)
        Require.AllFeaturesSupported(ProviderFeatures.UniqueNullTreatment);

      var initConfig = DomainConfigurationFactory.Create();
      initConfig.Types.Register(typeof(ExcludeNullableColumn.Initial.UniqueMultiColumn));
      initConfig.UniqueNullTreatment = from;
      initConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var indexBuildCrash = from switch {
        UniqueNullTreatment.Default => to == UniqueNullTreatment.NotDistinct || (to == UniqueNullTreatment.Default && !defaultSingleColumnDistinct),
        UniqueNullTreatment.Distinct => to == UniqueNullTreatment.NotDistinct || (to == UniqueNullTreatment.Default && !defaultSingleColumnDistinct),
        UniqueNullTreatment.NotDistinct => to == UniqueNullTreatment.NotDistinct,
        _ => throw new ArgumentOutOfRangeException()
      };

      using (var initDomain = Domain.Build(initConfig)) {
        FillExcludeNullColumnData(initDomain);
      }

      var upgradeConfig = DomainConfigurationFactory.Create();
      upgradeConfig.Types.Register(typeof(ExcludeNullableColumn.Upgraded.UniqueMultiColumn));
      upgradeConfig.UniqueNullTreatment = to;
      upgradeConfig.UpgradeMode = DomainUpgradeMode.Validate;

      _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfig).DisposeSafely());

      upgradeConfig = upgradeConfig.Clone();
      upgradeConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      if (indexBuildCrash) {
        _ = isFirebird || isPostgres
          ? Assert.Throws<UniqueConstraintViolationException>(() => Domain.Build(upgradeConfig).DisposeSafely())
          : Assert.Throws<StorageException>(() => Domain.Build(upgradeConfig).DisposeSafely());

      }
      else {
        var insertFails = to switch {
          UniqueNullTreatment.Default => !defaultSingleColumnDistinct,
          UniqueNullTreatment.Distinct => false,
          UniqueNullTreatment.NotDistinct => true,
          _ => throw new ArgumentOutOfRangeException()
        };

        using (var upgraded = Domain.Build(upgradeConfig)) {
          using (var session = upgraded.OpenSession()) {
            using (var tx = session.OpenTransaction()) {
              Assert.That(session.Query.All<ExcludeNullableColumn.Upgraded.UniqueMultiColumn>().Count(), Is.EqualTo(3));
            }
            using (var uniqueInsert = session.OpenTransaction()) {
              _ = new ExcludeNullableColumn.Upgraded.UniqueMultiColumn(session) { NullableValueA = null, NullableValueB = 100 };
              if (!insertFails)
                Assert.DoesNotThrow(() => session.SaveChanges());
              else {
                _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
              }
            }
          }
        }
      }
    }

    private static void FillExcludeNullColumnData(Domain initDomain)
    {
      using (var poppulateSession = initDomain.OpenSession())
      using (var tx = poppulateSession.OpenTransaction()) {
        _ = new ExcludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = null, NullableValueB = 100 };
        _ = new ExcludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = null, NullableValueB = 200 };
        _ = new ExcludeNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValueA = 30, NullableValueB = null };

        tx.Complete();
      }
    }

    [Test]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Default, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.Distinct, UniqueNullTreatment.NotDistinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Default)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.Distinct)]
    [TestCase(UniqueNullTreatment.NotDistinct, UniqueNullTreatment.NotDistinct)]
    public void ExcludeNotNullColumn(UniqueNullTreatment from, UniqueNullTreatment to)
    {
      if (from != UniqueNullTreatment.Default || to != UniqueNullTreatment.Default)
        Require.AllFeaturesSupported(ProviderFeatures.UniqueNullTreatment);

      var initConfig = DomainConfigurationFactory.Create();
      initConfig.Types.Register(typeof(ExcludeNotNullableColumn.Initial.UniqueMultiColumn));
      initConfig.UniqueNullTreatment = from;
      initConfig.UpgradeMode = DomainUpgradeMode.Recreate;

      var indexBuildCrash = from switch {
        UniqueNullTreatment.Default => to == UniqueNullTreatment.NotDistinct || (to == UniqueNullTreatment.Default && !defaultSingleColumnDistinct),
        UniqueNullTreatment.Distinct => to == UniqueNullTreatment.NotDistinct || (to == UniqueNullTreatment.Default && !defaultSingleColumnDistinct),
        UniqueNullTreatment.NotDistinct => to == UniqueNullTreatment.NotDistinct,
        _ => throw new ArgumentOutOfRangeException()
      };

      using (var initDomain = Domain.Build(initConfig)) {
        FillExcludeNotNullColumnData(initDomain);
      }

      var upgradeConfig = DomainConfigurationFactory.Create();
      upgradeConfig.Types.Register(typeof(ExcludeNotNullableColumn.Upgraded.UniqueMultiColumn));
      upgradeConfig.UniqueNullTreatment = to;
      upgradeConfig.UpgradeMode = DomainUpgradeMode.Validate;

      _ = Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(upgradeConfig).DisposeSafely());

      upgradeConfig = upgradeConfig.Clone();
      upgradeConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;

      if (indexBuildCrash) {
        _ = isFirebird || isPostgres
          ? Assert.Throws<UniqueConstraintViolationException>(() => Domain.Build(upgradeConfig).DisposeSafely())
          : Assert.Throws<StorageException>(() => Domain.Build(upgradeConfig).DisposeSafely());

      }
      else {
        var insertFails = to switch {
          UniqueNullTreatment.Default => !defaultSingleColumnDistinct,
          UniqueNullTreatment.Distinct => false,
          UniqueNullTreatment.NotDistinct => true,
          _ => throw new ArgumentOutOfRangeException()
        };

        using (var upgraded = Domain.Build(upgradeConfig)) {
          using (var session = upgraded.OpenSession()) {
            using (var tx = session.OpenTransaction()) {
              Assert.That(session.Query.All<ExcludeNotNullableColumn.Upgraded.UniqueMultiColumn>().Count(), Is.EqualTo(3));
            }
            using (var uniqueInsert = session.OpenTransaction()) {
              _ = new ExcludeNotNullableColumn.Upgraded.UniqueMultiColumn(session) { NullableValue = null, NotNullableValue = 100 };
              if (!insertFails)
                Assert.DoesNotThrow(() => session.SaveChanges());
              else {
                _ = Assert.Throws<UniqueConstraintViolationException>(() => session.SaveChanges());
              }
            }
          }
        }
      }
    }

    private static void FillExcludeNotNullColumnData(Domain initDomain)
    {
      using (var poppulateSession = initDomain.OpenSession())
      using (var tx = poppulateSession.OpenTransaction()) {
        _ = new ExcludeNotNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 100 };
        _ = new ExcludeNotNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = null, NotNullableValue = 200 };
        _ = new ExcludeNotNullableColumn.Initial.UniqueMultiColumn(poppulateSession) { NullableValue = 30, NotNullableValue = 300 };

        tx.Complete();
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.IndexNullTreatmentUpgradeTestModel
{
  namespace NoChanges
  {
    [HierarchyRoot]
    [Index(nameof(NullableValue), Unique = true)]
    public class UniqueSingleColumn : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public int? NullableValue { get; set; }

      public UniqueSingleColumn(Session session)
        : base(session)
      {
      }
    }

    [HierarchyRoot]
    [Index(nameof(NullableValue), nameof(NotNullableValue), Unique = true)]
    public class UniqueMultiColumn : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public int? NullableValue { get; set; }

      [Field]
      public int NotNullableValue { get; set; }

      public UniqueMultiColumn(Session session)
        : base(session)
      {
      }
    }
  }

  namespace AddIndex
  {
    namespace Initial
    {
      [HierarchyRoot]
      public class UniqueSingleColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValue { get; set; }

        public UniqueSingleColumn(Session session)
          : base(session)
        {
        }
      }

      [HierarchyRoot]
      public class UniqueMultiColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValue { get; set; }

        [Field]
        public int NotNullableValue { get; set; }

        public UniqueMultiColumn(Session session)
          : base(session)
        {
        }
      }
    }

    namespace WithIndexes
    {
      [HierarchyRoot]
      [Index(nameof(NullableValue), Unique = true)]
      public class UniqueSingleColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValue { get; set; }

        public UniqueSingleColumn(Session session)
          : base(session)
        {
        }
      }

      [HierarchyRoot]
      [Index(nameof(NullableValue), nameof(NotNullableValue), Unique = true)]
      public class UniqueMultiColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValue { get; set; }

        [Field]
        public int NotNullableValue { get; set; }

        public UniqueMultiColumn(Session session)
          : base(session)
        {
        }
      }
    }
  }

  namespace IncludeNotNullableColumn
  {
    namespace Initial
    {
      [HierarchyRoot]
      [Index(nameof(NullableValue), Unique = true)]
      public class UniqueMultiColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValue { get; set; }

        [Field]
        public int NotNullableValue { get; set; }

        public UniqueMultiColumn(Session session)
          : base(session)
        {
        }
      }
    }

    namespace Upgraded
    {
      [HierarchyRoot]
      [Index(nameof(NullableValue), nameof(NotNullableValue), Unique = true)]
      public class UniqueMultiColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValue { get; set; }

        [Field]
        public int NotNullableValue { get; set; }

        public UniqueMultiColumn(Session session)
          : base(session)
        {
        }
      }
    }
  }

  namespace IncludeNullableColumn
  {
    namespace Initial
    {
      [HierarchyRoot]
      [Index(nameof(NullableValueA), Unique = true)]
      public class UniqueMultiColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValueA { get; set; }

        [Field]
        public int? NullableValueB { get; set; }

        public UniqueMultiColumn(Session session)
          : base(session)
        {
        }
      }
    }

    namespace Upgraded
    {
      [HierarchyRoot]
      [Index(nameof(NullableValueA), nameof(NullableValueB), Unique = true)]
      public class UniqueMultiColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValueA { get; set; }

        [Field]
        public int? NullableValueB { get; set; }

        public UniqueMultiColumn(Session session)
          : base(session)
        {
        }
      }
    }
  }

  namespace ExcludeNotNullableColumn
  {
    namespace Initial
    {
      [HierarchyRoot]
      [Index(nameof(NullableValue), nameof(NotNullableValue), Unique = true)]
      public class UniqueMultiColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValue { get; set; }

        [Field]
        public int NotNullableValue { get; set; }

        public UniqueMultiColumn(Session session)
          : base(session)
        {
        }
      }
    }

    namespace Upgraded
    {
      [HierarchyRoot]
      [Index(nameof(NullableValue), Unique = true)]
      public class UniqueMultiColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValue { get; set; }

        [Field]
        public int NotNullableValue { get; set; }

        public UniqueMultiColumn(Session session)
          : base(session)
        {
        }
      }
    }
  }

  namespace ExcludeNullableColumn
  {
    namespace Initial
    {
      [HierarchyRoot]
      [Index(nameof(NullableValueA), nameof(NullableValueB), Unique = true)]
      public class UniqueMultiColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValueA { get; set; }

        [Field]
        public int? NullableValueB { get; set; }

        public UniqueMultiColumn(Session session)
          : base(session)
        {
        }
      }
    }

    namespace Upgraded
    {
      [HierarchyRoot]
      [Index(nameof(NullableValueA), Unique = true)]
      public class UniqueMultiColumn : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int? NullableValueA { get; set; }

        [Field]
        public int? NullableValueB { get; set; }

        public UniqueMultiColumn(Session session)
          : base(session)
        {
        }
      }
    }
  }
}
