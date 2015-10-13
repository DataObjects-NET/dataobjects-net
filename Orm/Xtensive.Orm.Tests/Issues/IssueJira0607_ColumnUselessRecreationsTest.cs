using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade;
using model = Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0607_ColumnUselessRecreationsTest
  {
    [Test]
    public void Test1()
    {
      using (var domain = Domain.Build(BuildConfiguration(true, typeof (model.V1.Initial.TestEntity)))){}
      using (var domain = Domain.Build(BuildConfiguration(false, typeof (model.V1.Final.TestEntity)))){} //error
    }

    [Test]
    public void Test2()
    {
      using (var domain = Domain.Build(BuildConfiguration(true, typeof(model.V2.Initial.TestEntity)))) { }
      using (var domain = Domain.Build(BuildConfiguration(false, typeof(model.V2.Final.TestEntity)))) { } //error
    }

    [Test]
    public void Test3()
    {
      using (var domain = Domain.Build(BuildConfiguration(true, typeof(model.V3.Initial.TestEntity)))) { }
      Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(BuildConfiguration(false, typeof (model.V3.Final.TestEntity))));
    }

    [Test]
    public void Test4()
    {
      using (var domain = Domain.Build(BuildConfiguration(true, typeof(model.V4.Initial.TestEntity)))) { }
      using (var domain = Domain.Build(BuildConfiguration(false, typeof(model.V4.Final.TestEntity)))) { }
    }

    [Test]
    public void Test5()
    {
      using (var domain = Domain.Build(BuildConfiguration(true, typeof(model.V5.Initial.TestEntity)))) { }
      using (var domain = Domain.Build(BuildConfiguration(false, typeof(model.V5.Final.TestEntity)))) { }
    }

    [Test]
    public void Test6()
    {
      using (var domain = Domain.Build(BuildConfiguration(true, typeof(model.V6.Initial.TestEntity)))) { }
      using (var domain = Domain.Build(BuildConfiguration(false, typeof(model.V6.Final.TestEntity)))) { }
    }

    private DomainConfiguration BuildConfiguration(bool isInitial, params Type[] types)
    {
      var configuration = DomainConfigurationFactory.Create();
      foreach (var type in types) {
        configuration.Types.Register(type);
      }
      configuration.UpgradeMode = (isInitial) ? DomainUpgradeMode.Recreate : DomainUpgradeMode.PerformSafely;
      return configuration;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0607_ColumnUselessRecreationsTestModel
{
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
