using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
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

    [Test]
    public void AddNewFieldTest()
    {
      
    }

    [Test]
    public void AddNewTypeTest()
    {
      
    }

    [Test]
    public void RemoveFieldWithHintTest()
    {
      
    }

    [Test]
    public void RemoveFieldWithoutHintTest()
    {
      
    }

    [Test]
    public void RemoveTypeWithHintTest()
    {
      
    }

    [Test]
    public void RemoveTypeWithoutHintTest()
    {
      
    }

    [Test]
    public void RenameFieldWithHintTest()
    {
      
    }

    [Test]
    public void RenameFieldWithoutHingTest()
    {
      
    }

    [Test]
    public void RenameTypeWithHintTest()
    {
      
    }

    [Test]
    public void RenameTypeWithoutHintTest()
    {
      
    }

    [Test]
    public void MoveFieldTest()
    {
      
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

  namespace RemoveFieldModel
  {
    namespace Source
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class ClassTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ClassTableHBField { get; set; }

        [Field]
        public string ClassTableRemovableField { get; set; }
      }

      public class ClassTableDescendant : ClassTableHierarchyBase
      {
        [Field]
        public double SomeDescendantField { get; set; }

        [Field]
        public string SomeDescendantRemovableField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public class ConcreteTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ConcreteTableHBField { get; set; }

        [Field]
        public int ConcreteTableRemovableField { get; set; }
      }

      public class ConcreteTableDescendant : ConcreteTableHierarchyBase
      {
        [Field]
        public double SomeDescendantField { get; set; }

        [Field]
        public string SomeDescendantRemovableField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public class SingleTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }

        [Field]
        public string SingleTableHBField { get; set; }

        [Field]
        public double SingleTableRemovableField { get; set; }
      }

      public class SingleTableDescendant : SingleTableHierarchyBase
      {
        [Field]
        public string SomeDescendantField { get; set; }

        [Field]
        public string SomeDescendantRemovableField { get; set; }
      }
    }

    namespace Target
    {
      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class ClassTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ClassTableHBField { get; set; }
      }

      public class ClassTableDescendant : ClassTableHierarchyBase
      {
        [Field]
        public double SomeDescendantField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public class ConcreteTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ConcreteTableHBField { get; set; }
      }

      public class ConcreteTableDescendant : ConcreteTableHierarchyBase
      {
        [Field]
        public double SomeDescendantField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public class SingleTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }

        [Field]
        public string SingleTableHBField { get; set; }
      }

      public class SingleTableDescendant : SingleTableHierarchyBase
      {
        [Field]
        public string SomeDescendantField { get; set; }
      }

      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RemoveFieldHint(typeof (SingleTableHierarchyBase), "SingleTableRemovableField"));
          hints.Add(new RemoveFieldHint(typeof (ConcreteTableHierarchyBase), "ConcreteTableRemovableField"));
          hints.Add(new RemoveFieldHint(typeof (ClassTableHierarchyBase), "ClassTableRemovableField"));

          hints.Add(new RemoveFieldHint(typeof (SingleTableDescendant), "SomeDescendantRemovableField"));
          hints.Add(new RemoveFieldHint(typeof (ConcreteTableDescendant), "SomeDescendantRemovableField"));
          hints.Add(new RemoveFieldHint(typeof (ClassTableDescendant), "SomeDescendantRemovableField"));
        }
      }
    }
  }

  namespace RemoveTypeModel
  {
    namespace Source
    {
      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public class SingleTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string SinlgeTableHBField { get; set; }
      }

      public class SingleTableDescendant1 : SingleTableHierarchyBase
      {
        [Field]
        public string SingleTableD1Field { get; set; }
      }

      public class SingleTableDescendant11 : SingleTableDescendant1
      {
        [Field]
        public string SingleTableD11Field { get; set; }
      }

      public class SingleTableDescendant12Removed : SingleTableDescendant1
      {
        [Field]
        public string SingleTableD12Field { get; set; }
      }

      public class SingleTableDescendant2 : SingleTableHierarchyBase
      {
        [Field]
        public string SingleTableD2Field { get; set; }
      }

      public class SingleTableDescendant21 : SingleTableDescendant2
      {
        [Field]
        public string SingleTableD21Field { get; set; }
      }

      public class SingleTableDescendant22Removed : SingleTableDescendant2
      {
        [Field]
        public string SingleTableD22Field { get; set; }
      }


      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class ClassTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ClassTableHRField { get; set; }
      }

      public class ClassTableDescendant1 : ClassTableHierarchyBase
      {
        [Field]
        public string ClassTableD1Field { get; set; }
      }

      public class ClassTableDescendant11 : ClassTableDescendant1
      {
        [Field]
        public string ClassTableD11Field { get; set; }
      }

      public class ClassTableDescendant12Removed : ClassTableDescendant1
      {
        [Field]
        public string ClassTableD12Field { get; set; }
      }

      public class ClassTableDescendant2 : ClassTableHierarchyBase
      {
        [Field]
        public string ClassTableD2Field { get; set; }
      }

      public class ClassTableDescendant21 : ClassTableDescendant2
      {
        [Field]
        public string ClassTableD21Field { get; set; }
      }

      public class ClassTableDescendant22Removed : ClassTableDescendant2
      {
        [Field]
        public string ClassTableD22Field { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public class ConcreteTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }

        [Field]
        public string ConcreteTableHBField { get; set; }
      }

      public class ConcreteTableDescendant1 : ConcreteTableHierarchyBase
      {
        [Field]
        public string ConcreteTableD1Field { get; set; }
      }

      public class ConcreteTableDescendant11 : ConcreteTableDescendant1
      {
        [Field]
        public string ConcreteTableD11Field { get; set; }
      }

      public class ConcreteTableDescendant12Removed : ConcreteTableDescendant1
      {
        [Field]
        public string ConcreteTableD12Field { get; set; }
      }

      public class ConcreteTableDescendant2 : ConcreteTableHierarchyBase
      {
        [Field]
        public string ConcreteTableD2Field { get; set; }
      }

      public class ConcreteTableDescendant21 : ConcreteTableDescendant2
      {
        [Field]
        public string ConcreteTableD21 { get; set; }
      }

      public class ConcreteTableDescendant22Removed : ConcreteTableDescendant2
      {
        [Field]
        public string ConcreteTableD22Field { get; set; }
      }
    }

    namespace Target
    {
      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public class SingleTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string SinlgeTableHBField { get; set; }
      }

      public class SingleTableDescendant1 : SingleTableHierarchyBase
      {
        [Field]
        public string SingleTableD1Field { get; set; }
      }

      public class SingleTableDescendant11 : SingleTableDescendant1
      {
        [Field]
        public string SingleTableD11Field { get; set; }
      }

      public class SingleTableDescendant2 : SingleTableHierarchyBase
      {
        [Field]
        public string SingleTableD2Field { get; set; }
      }

      public class SingleTableDescendant21 : SingleTableDescendant2
      {
        [Field]
        public string SingleTableD21Field { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class ClassTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ClassTableHRField { get; set; }
      }

      public class ClassTableDescendant1 : ClassTableHierarchyBase
      {
        [Field]
        public string ClassTableD1Field { get; set; }
      }

      public class ClassTableDescendant11 : ClassTableDescendant1
      {
        [Field]
        public string ClassTableD11Field { get; set; }
      }

      public class ClassTableDescendant2 : ClassTableHierarchyBase
      {
        [Field]
        public string ClassTableD2Field { get; set; }
      }

      public class ClassTableDescendant21 : ClassTableDescendant2
      {
        [Field]
        public string ClassTableD21Field { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public class ConcreteTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }

        [Field]
        public string ConcreteTableHBField { get; set; }
      }

      public class ConcreteTableDescendant1 : ConcreteTableHierarchyBase
      {
        [Field]
        public string ConcreteTableD1Field { get; set; }
      }

      public class ConcreteTableDescendant11 : ConcreteTableDescendant1
      {
        [Field]
        public string ConcreteTableD11Field { get; set; }
      }

      public class ConcreteTableDescendant2 : ConcreteTableHierarchyBase
      {
        [Field]
        public string ConcreteTableD2Field { get; set; }
      }

      public class ConcreteTableDescendant21 : ConcreteTableDescendant2
      {
        [Field]
        public string ConcreteTableD21 { get; set; }
      }

      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.RemoveTypeModel.Source.SingleTableDescendant12Removed"));
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.RemoveTypeModel.Source.SingleTableDescendant22Removed"));

          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.RemoveTypeModel.Source.ClassTableDescendant12Removed"));
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.RemoveTypeModel.Source.ClassTableDescendant22Removed"));

          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.RemoveTypeModel.Source.ConcreteTableDescendant12Removed"));
          hints.Add(new RemoveTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.RemoveTypeModel.Source.ConcreteTableDescendant22Removed"));
        }
      }
    }
  }

  namespace RenameFieldModel
  {
    namespace Source
    {
      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public class SingleTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }

        [Field]
        public string SingleTableHBFiele { get; set; }

        [Field]
        public int FieldWithWrongName { get; set; }
      }

      public class SingleTableDescendant : SingleTableHierarchyBase
      {
        [Field]
        public double AnotherFieldWithWrongName { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class ClassTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }

        [Field]
        public string SingleTableHBFiele { get; set; }

        [Field]
        public int FieldWithWrongName { get; set; }
      }

      public class ClassTableDescendant : ClassTableHierarchyBase
      {
        [Field]
        public double AnotherFieldWithWrongName { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public class ConcreteTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }

        [Field]
        public string SingleTableHBFiele { get; set; }

        [Field]
        public int FieldWithWrongName { get; set; }
      }

      public class ConcreteTableDescendant : ConcreteTableHierarchyBase
      {
        [Field]
        public double AnotherFieldWithWrongName { get; set; }
      }
    }

    namespace Target
    {
      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public class SingleTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }

        [Field]
        public string SingleTableHBFiele { get; set; }

        [Field]
        public int FieldWithWrongName { get; set; }
      }

      public class SingleTableDescendant : SingleTableHierarchyBase
      {
        [Field]
        public double AnotherFieldWithWrongName { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class ClassTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }

        [Field]
        public string SingleTableHBFiele { get; set; }

        [Field]
        public int FieldWithWrongName { get; set; }
      }

      public class ClassTableDescendant : ClassTableHierarchyBase
      {
        [Field]
        public double AnotherFieldWithWrongName { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public class ConcreteTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }

        [Field]
        public string SingleTableHBFiele { get; set; }

        [Field]
        public int FieldWithWrongName { get; set; }
      }

      public class ConcreteTableDescendant : ConcreteTableHierarchyBase
      {
        [Field]
        public double AnotherFieldWithWrongName { get; set; }
      }

      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameFieldHint(typeof (SingleTableHierarchyBase), "FieldWithWrongName", "FieldWithRightName"));
          hints.Add(new RenameFieldHint(typeof (SingleTableDescendant), "AnotherFieldWithWrongName", "AnotherFieldWithRightName"));

          hints.Add(new RenameFieldHint(typeof (ConcreteTableHierarchyBase), "FieldWithWrongName", "FieldWithRightName"));
          hints.Add(new RenameFieldHint(typeof (ConcreteTableDescendant), "AnotherFieldWithWrongName", "AnotherFieldWithRightName"));

          hints.Add(new RenameFieldHint(typeof (ClassTableHierarchyBase), "FieldWithWrongName", "FieldWithRightName"));
          hints.Add(new RenameFieldHint(typeof (ClassTableDescendant), "AnotherFieldWithWrongName", "AnotherFieldWithRightName"));
        }
      }
    }
  }

  namespace RenameTypeModel
  {
    namespace Source
    {
      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public class SingleTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }
      }

      public class SingleTableDescendant1 : SingleTableHierarchyBase
      {
        [Field]
        public string SomeStringField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class ClassTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }
      }

      public class ClassTableDescendant1 : ClassTableHierarchyBase
      {
        [Field]
        public string SomeStringField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public class ConcreteTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }
      }

      public class ConcreteTableDescendant1 : Entity
      {
        [Field]
        public string SomeStringField { get; set; }
      }
    }

    namespace Target
    {
      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public class SingleTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }
      }

      public class SingleTableDescendant : SingleTableHierarchyBase
      {
        [Field]
        public string SomeStringField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ClassTable)]
      public class ClassTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }
      }

      public class ClassTableDescendant : ClassTableHierarchyBase
      {
        [Field]
        public string SomeStringField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public class ConcreteTableHierarchyBase : Entity
      {
        [Field]
        public int Id { get; set; }
      }

      public class ConcreteTableDescendant : Entity
      {
        [Field]
        public string SomeStringField { get; set; }
      }

      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.RenameTypeModel.Source.SingleTableDescendant1", typeof (SingleTableDescendant)));
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.RenameTypeModel.Source.ClassTableDescendant1", typeof (ClassTableDescendant)));
          hints.Add(new RenameTypeHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.RenameTypeModel.Source.ConcreteTableDescendant1", typeof (ConcreteTableDescendant)));
        }
      }
    }
  }

  namespace MoveFieldModel
  {
    namespace Source
    {
      [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
      public abstract class ClassTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string CTHBField { get; set; }

        [Field]
        public string MovableField { get; set; }
      }

      public class ClassTableDescendant1 : ClassTableHierarchyBase
      {
        [Field]
        public int Value { get; set; }

        [Field]
        public string Comment { get; set; }
      }

      public class ClassTableDescendant2 : ClassTableHierarchyBase
      {
        [Field]
        public double Price { get; set; }

        [Field]
        public string Summary { get; set; }
      }

      public class ClassTableDescendant3 : ClassTableDescendant1
      {
        [Field]
        public float SomeField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public abstract class ConcreteTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ContcreteTableHBField { get; set; }

        [Field]
        public string MovableField { get; set; }
      }

      public class ConcreteTableDescendant1 : ConcreteTableHierarchyBase
      {
        [Field]
        public int Value { get; set; }

        [Field]
        public string Comment { get; set; }
      }

      public class ConcreteTableDescendant2 : ConcreteTableHierarchyBase
      {
        [Field]
        public double Price { get; set; }

        [Field]
        public string Summary { get; set; }
      }

      public class ConcreteTableDescendant3 : ConcreteTableDescendant1
      {
        [Field]
        public float SomeField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public abstract class SingleTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ContcreteTableHBField { get; set; }

        [Field]
        public string MovableField { get; set; }
      }

      public class SingleTableDescendant1 : SingleTableHierarchyBase
      {
        [Field]
        public int Value { get; set; }

        [Field]
        public string Comment { get; set; }
      }

      public class SingleTableDescendant2 : SingleTableHierarchyBase
      {
        [Field]
        public double Price { get; set; }

        [Field]
        public string Summary { get; set; }
      }

      public class SingleTableDescendant3 : SingleTableDescendant1
      {
        [Field]
        public float SomeField { get; set; }
      }
    }

    namespace Target
    {
      [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
      public abstract class ClassTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ClassTableHBField { get; set; }
      }

      public class ClassTableAncestor1 : ClassTableHierarchyBase
      {
        [Field]
        public int Value { get; set; }

        [Field]
        public string Comment { get; set; }
      }

      public class ClassTableAncestor2 : ClassTableHierarchyBase
      {
        [Field]
        public double Price { get; set; }

        [Field]
        public string Summary { get; set; }
      }

      public class ClassTableAncestor3 : ClassTableAncestor1
      {
        [Field]
        public float SomeField { get; set; }

        [Field]
        public string MovableField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.ConcreteTable)]
      public abstract class ConcreteTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ContcreteTableHBField { get; set; }
      }

      public class ConcreteTableAncestor1 : ConcreteTableHierarchyBase
      {
        [Field]
        public int Value { get; set; }

        [Field]
        public string Comment { get; set; }
      }

      public class ConcreteTableAncestor2 : ConcreteTableHierarchyBase
      {
        [Field]
        public double Price { get; set; }

        [Field]
        public string Summary { get; set; }
      }

      public class ConcreteTableAncestor3 : ConcreteTableAncestor1
      {
        [Field]
        public float SomeField { get; set; }

        [Field]
        public string MovableField { get; set; }
      }

      [HierarchyRoot(InheritanceSchema.SingleTable)]
      public abstract class SingleTableHierarchyBase : Entity
      {
        [Field, Key]
        public int Id { get; set; }

        [Field]
        public string ContcreteTableHBField { get; set; }
      }

      public class SingleTableAncestor1 : SingleTableHierarchyBase
      {
        [Field]
        public int Value { get; set; }

        [Field]
        public string Comment { get; set; }
      }

      public class SingleTableAncestor2 : SingleTableHierarchyBase
      {
        [Field]
        public double Price { get; set; }

        [Field]
        public string Summary { get; set; }
      }

      public class SingleTableAncestor3 : SingleTableAncestor1
      {
        [Field]
        public float SomeField { get; set; }

        [Field]
        public string MovableField { get; set; }
      }

      public class CustomUpgradeHandler : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion)
        {
          return true;
        }

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.MoveFieldModel.Source.ClassTableHierarchyBase", "MovableField", typeof(ClassTableAncestor3)));
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.MoveFieldModel.Source.ConcreteTableHierarchyBase", "MovableField", typeof(ConcreteTableAncestor3)));
          hints.Add(new MoveFieldHint("Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel.MoveFieldModel.Source.SingleTableHierarchyBase", "MovableField", typeof(SingleTableAncestor3)));
        }
      }
    }
  }
}
