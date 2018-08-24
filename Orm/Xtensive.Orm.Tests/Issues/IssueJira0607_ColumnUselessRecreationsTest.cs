using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Upgrade.Model;
using model = Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0607_ColumnUselessRecreationsTest
  {
    [Test]
    public void MainTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      var initialConfig = DomainConfigurationFactory.Create();
      initialConfig.Types.Register(typeof(model.MainTestModel.TestEntity));
      initialConfig.Types.Register(typeof(model.MainTestModel.TestEntity2));
      initialConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      using (var domain = Domain.Build(initialConfig)) { }

      // useful work
      var configration = DomainConfigurationFactory.Create();
      configration.Types.Register(typeof(model.MainTestModel.TestEntity));
      configration.Types.Register(typeof(model.MainTestModel.TestEntity2));
      configration.UpgradeMode = DomainUpgradeMode.PerformSafely;

      Exception exception = null;
      int step = 0;
      for (var i = 0; i < 30; i++, step++) {
        try {
          using (var domain = Domain.Build(configration)) { }
        }
        catch (Exception e) {
          exception = e;
          break;
        }
      }
      Assert.That(exception, Is.Null);
      Assert.That(step, Is.EqualTo(30));
    }

    [Test]
    public void Test1()
    {
      Require.AnyFeatureSupported(ProviderFeatures.TransactionalDdl);

      using (var initialDomain = Domain.Build(BuildConfiguration(true, typeof (model.V1.Initial.TestEntity))))
      using (var session = initialDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new model.V1.Initial.TestEntity() {
            Name = "Name",
            ReferenceField = new model.V1.Initial.TestReferencedEntity(),
            StateField = model.V1.Initial.State.Live,
            TimeSpanField = new TimeSpan(3, 3, 3, 3),
          };
        }
        transaction.Complete();
      }

      Assert.Throws<CheckConstraintViolationException>(
        () => {
          Domain domain = null;
          try {
            domain = Domain.Build(BuildConfiguration(false, typeof (model.V1.Final.TestEntity)));
          }
          finally {
            if (domain!=null)
              domain.Dispose();
          }
        });
      using (var domain = Domain.Build(BuildConfiguration(false, typeof(model.V1.Initial.TestEntity))))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entities = session.Query.All<model.V1.Initial.TestEntity>().ToList();
        Assert.That(entities.Count, Is.EqualTo(10));
      }
    }

    [Test]
    public void Test2()
    {
      using (var initialDomain = Domain.Build(BuildConfiguration(true, typeof(model.V2.Initial.TestEntity))))
      using (var session = initialDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new model.V2.Initial.TestEntity() {
            Name = "Name",
            ReferenceField = new model.V2.Initial.TestReferencedEntity(),
            StateField = model.V2.Initial.State.Live,
            TimeSpanField = new TimeSpan(3, 3, 3, 3),
          };
        }
        transaction.Complete();
      }
      Assert.Throws<CheckConstraintViolationException>(() => {
        Domain domain=null;
        try {
          domain = Domain.Build(BuildConfiguration(false, typeof (model.V2.Final.TestEntity)));
        }
        finally {
          if (domain!=null)
            domain.Dispose();
        }
      });

      using (var initialDomain = Domain.Build(BuildConfiguration(false, typeof(model.V2.Initial.TestEntity))))
      using (var session = initialDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.That(session.Query.All<model.V2.Initial.TestEntity>().Count(), Is.EqualTo(10));
        Assert.That(session.Query.All<model.V2.Initial.TestReferencedEntity>().Count(), Is.EqualTo(10));
        foreach (var entity in session.Query.All<model.V2.Initial.TestEntity>()) {
          Assert.That(entity.Name, Is.EqualTo("Name"));
          Assert.That(entity.ReferenceField, Is.Not.Null);
          Assert.That(entity.StateField, Is.EqualTo(model.V2.Initial.State.Live));
          Assert.That(entity.TimeSpanField, Is.EqualTo(new TimeSpan(3, 3, 3, 3)));
        }
      }
    }

    [Test]
    public void Test3()
    {
      using (var domain = Domain.Build(BuildConfiguration(true, typeof (model.V3.Initial.TestEntity))))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()){
        for (int i = 0; i < 10; i++) {
          new model.V3.Initial.TestEntity {
            Name = "Name",
            ReferenceField = new model.V3.Initial.TestReferencedEntity(),
            StateField = model.V3.Initial.State.Live,
            TimeSpanField = new TimeSpan(3, 3, 3, 3),
          };
        }
        transaction.Complete();
      }

      var exception = Assert.Throws<SchemaSynchronizationException>(
        () => Domain.Build(BuildConfiguration(false, typeof (model.V3.Final.TestEntity))));
      Assert.That(exception, Is.Not.Null);
      Assert.That(exception.ComparisonResult.HasUnsafeActions, Is.True);
      Assert.That(exception.ComparisonResult.UnsafeActions.Count, Is.EqualTo(4));
      foreach (var unsafeAction in exception.ComparisonResult.UnsafeActions) {
        Assert.IsInstanceOf(typeof(PropertyChangeAction), unsafeAction);
        var propertyChangeAction = unsafeAction as PropertyChangeAction;
        Assert.That(propertyChangeAction.Properties.ContainsKey("Type"), Is.True);
        var targetColumnInfo = unsafeAction.Difference.Target as StorageTypeInfo;
        var sourceColumnInfo = unsafeAction.Difference.Source as StorageTypeInfo;
        Assert.That(IsOnlyNullableChanged(sourceColumnInfo, targetColumnInfo), Is.True);
      }

      using (var initialDomain = Domain.Build(BuildConfiguration(false, typeof(model.V3.Initial.TestEntity))))
      using (var session = initialDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.That(session.Query.All<model.V3.Initial.TestEntity>().Count(), Is.EqualTo(10));
        Assert.That(session.Query.All<model.V3.Initial.TestReferencedEntity>().Count(), Is.EqualTo(10));
        foreach (var entity in session.Query.All<model.V3.Initial.TestEntity>()) {
          Assert.That(entity.Name, Is.EqualTo("Name"));
          Assert.That(entity.ReferenceField, Is.Not.Null);
          Assert.That(entity.StateField, Is.EqualTo(model.V3.Initial.State.Live));
          Assert.That(entity.TimeSpanField, Is.EqualTo(new TimeSpan(3, 3, 3, 3)));
        }
      }
    }

    [Test]
    public void Test4()
    {
      using (var initialDomain = Domain.Build(BuildConfiguration(true, typeof(model.V4.Initial.TestEntity))))
      using (var session = initialDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new model.V4.Initial.TestEntity {
            Name = "Name",
            ReferenceField = new model.V4.Initial.TestReferencedEntity(),
            StateField = model.V4.Initial.State.Live,
            TimeSpanField = new TimeSpan(3, 3, 3, 3),
          };
        }
        transaction.Complete();
      }
      Domain domain = null;
      Assert.DoesNotThrow(
        () => {
          domain = Domain.Build(BuildConfiguration(false, typeof (model.V4.Final.TestEntity)));
        });
      Assert.That(domain, Is.Not.Null);
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entities = session.Query.All<model.V4.Final.TestEntity>().ToList();
        Assert.That(entities.Count, Is.EqualTo(11));
        Assert.That(entities.Count(el => string.IsNullOrEmpty(el.Name)), Is.EqualTo(1));
      }
    }

    [Test]
    public void Test5()
    {
      using (var initialDomain = Domain.Build(BuildConfiguration(true, typeof(model.V5.Initial.TestEntity))))
      using (var session = initialDomain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new model.V5.Initial.TestEntity {
            Name = "Name",
            ReferenceField = new model.V5.Initial.TestReferencedEntity(),
            StateField = model.V5.Initial.State.Live,
            TimeSpanField = new TimeSpan(3, 3, 3, 3),
          };
        }
        transaction.Complete();
      }
      Domain domain = null;
      Assert.DoesNotThrow(
        () => domain = Domain.Build(BuildConfiguration(false, typeof (model.V5.Final.TestEntity))));
      Assert.That(domain, Is.Not.Null);

      using (domain)
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.That(session.Query.All<model.V5.Final.TestEntity>().Count(), Is.EqualTo(10));
        Assert.That(session.Query.All<model.V5.Final.TestReferencedEntity>().Count(), Is.EqualTo(10));
        foreach (var entity in session.Query.All<model.V5.Final.TestEntity>().Where(el => el.Name!=null)) {
          Assert.That(entity.Name, Is.EqualTo("Name"));
          Assert.That(entity.ReferenceField, Is.Not.Null);
          Assert.That(entity.StateField, Is.EqualTo(model.V5.Final.State.Live));
          Assert.That(entity.TimeSpanField, Is.EqualTo(new TimeSpan(3, 3, 3, 3)));
        }
      }
    }

    [Test]
    public void Test6()
    {
      using (var domain = Domain.Build(BuildConfiguration(true, typeof(model.V6.Initial.TestEntity))))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new model.V6.Initial.TestEntity {
            Name = "Name",
            ReferenceField = new model.V6.Initial.TestReferencedEntity(),
            StateField = model.V6.Initial.State.Live,
            TimeSpanField = new TimeSpan(3, 3, 3, 3),
          };
        }
        transaction.Complete();
      }

      using (var domain = Domain.Build(BuildConfiguration(false, typeof(model.V6.Final.TestEntity))))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.That(session.Query.All<model.V6.Final.TestEntity>().Count(), Is.EqualTo(11));
        Assert.That(session.Query.All<model.V6.Final.TestReferencedEntity>().Count(), Is.EqualTo(10));
        foreach (var entity in session.Query.All<model.V6.Final.TestEntity>().Where(el=>el.Name!=null)) {
          Assert.That(entity.Name, Is.EqualTo("Name"));
          Assert.That(entity.ReferenceField, Is.Not.Null);
          Assert.That((model.V6.Initial.State) entity.StateField, Is.EqualTo(model.V6.Initial.State.Live));
          Assert.That(entity.TimeSpanField, Is.EqualTo(new TimeSpan(3, 3, 3, 3)));
        }
      }
    }

    [OneTimeSetUp]
    public void TestFixtureSetup()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    private DomainConfiguration BuildConfiguration(bool isInitial, params Type[] types)
    {
      var configuration = DomainConfigurationFactory.Create();
      foreach (var type in types) {
        configuration.Types.Register(type.Assembly, type.Namespace);
      }
      configuration.UpgradeMode = (isInitial) ? DomainUpgradeMode.Recreate : DomainUpgradeMode.PerformSafely;
      return configuration;
    }

    private static bool IsOnlyNullableChanged(StorageTypeInfo source, StorageTypeInfo target)
    {
      return source.Scale == target.Scale && 
        source.Precision == target.Precision &&
        source.NativeType == target.NativeType &&
        source.Length == target.Length &&
        source.IsNullable != target.IsNullable;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel
{
  namespace MainTestModel
  {
    [HierarchyRoot]
    public class TestEntity : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField01 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField02 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField03 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField04 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField05 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField06 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField07 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField08 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField09 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField10 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField11 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField12 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField13 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField14 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField15 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField16 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField17 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField18 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField19 { get; set; }

      [Field(Nullable = false, NullableOnUpgrade = true)]
      public TestEntity2 LongField20 { get; set; }
    }

    [HierarchyRoot]
    public class TestEntity2 : Entity
    {
      [Field, Key]
      public long Id { get; set; }
    }
  }

  namespace V1
  {
    namespace Initial
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = false)]
        public string Name { get; set; }

        [Field]
        public State StateField { get; set; }

        [Field]
        public TimeSpan TimeSpanField { get; set; }

        [Field(Nullable = false)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }
    }

    namespace Final
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = true, NullableOnUpgrade = false)]
        public string Name { get; set; }

        [Field]
        public State? StateField { get; set; }

        [Field]
        public TimeSpan? TimeSpanField { get; set; }
        
        [Field(Nullable = true, NullableOnUpgrade = false)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }

      public class CustomUpgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        public override void OnUpgrade()
        {
          new TestEntity() {Name = null};
        }
      }
    }
  }

  namespace V2
  {
    namespace Initial
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = false)]
        public string Name { get; set; }

        [Field]
        public State StateField { get; set; }

        [Field]
        public TimeSpan TimeSpanField { get; set; }
        
        [Field(Nullable = false)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }
    }

    namespace Final
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = false, NullableOnUpgrade = true)]
        public string Name { get; set; }

        [Field]
        public State StateField { get; set; }

        [Field]
        public TimeSpan TimeSpanField { get; set; }
        
        [Field(Nullable = false, NullableOnUpgrade = true)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }

      public class CustomUpgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        public override void OnUpgrade()
        {
          new TestEntity() { Name = null, ReferenceField = null};
        }
      }
    }
  }

  namespace V3
  {
    namespace Initial
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = true)]
        public string Name { get; set; }

        [Field]
        public State? StateField { get; set; }

        [Field]
        public TimeSpan? TimeSpanField { get; set; }
        
        [Field(Nullable = true)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }
    }

    namespace Final
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = false, NullableOnUpgrade = false)]
        public string Name { get; set; }

        [Field]
        public State StateField { get; set; }

        [Field]
        public TimeSpan TimeSpanField { get; set; }
        
        [Field(Nullable = false, NullableOnUpgrade = false)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }

      public class CustomUpgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        public override void OnUpgrade()
        {
          new TestEntity() { Name = null };
        }
      }
    }
  }

  namespace V4
  {
    namespace Initial
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = true)]
        public string Name { get; set; }

        [Field]
        public State? StateField { get; set; }

        [Field]
        public TimeSpan? TimeSpanField { get; set; }
        
        [Field(Nullable = true)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }
    }

    namespace Final
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = true, NullableOnUpgrade = false)]
        public string Name { get; set; }

        [Field]
        public State? StateField { get; set; }

        [Field]
        public TimeSpan? TimeSpanField { get; set; }

        [Field(Nullable = true, NullableOnUpgrade = false)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }

      public class CustomUpgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        public override void OnUpgrade()
        {
          new TestEntity() { Name = null };
        }
      }
    }
  }

  namespace V5
  {
    namespace Initial
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = true)]
        public string Name { get; set; }

        [Field]
        public State StateField { get; set; }

        [Field]
        public TimeSpan TimeSpanField { get; set; }
        
        [Field(Nullable = true)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }
    }

    namespace Final
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = false, NullableOnUpgrade = true)]
        public string Name { get; set; }

        [Field]
        public State StateField { get; set; }

        [Field]
        public TimeSpan TimeSpanField { get; set; }
        
        [Field(Nullable = false, NullableOnUpgrade = true)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }
    }
  }

  namespace V6
  {
    namespace Initial
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = true)]
        public string Name { get; set; }

        [Field]
        public State? StateField { get; set; }

        [Field]
        public TimeSpan? TimeSpanField { get; set; }

        [Field(Nullable = true)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }
    }

    namespace Final
    {
      [HierarchyRoot]
      public class TestEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field(Nullable = true, NullableOnUpgrade = true)]
        public string Name { get; set; }

        [Field]
        public State? StateField { get; set; }

        [Field]
        public TimeSpan? TimeSpanField { get; set; }
        
        [Field(Nullable = true, NullableOnUpgrade = true)]
        public TestReferencedEntity ReferenceField { get; set; }
      }

      [HierarchyRoot]
      public class TestReferencedEntity : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string Text { get; set; }
      }

      public enum State
      {
        Live,
        Dead
      }

      public class CustomUpgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        public override void OnUpgrade()
        {
          new TestEntity() { Name = null };
        }
      }
    }
  }
}
