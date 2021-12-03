// Copyright (C) 2013-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Andrey Turkov
// Created:    2013.08.21

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Orm.Tests.Upgrade.RecycledDefinitionTestModel;
using Xtensive.Orm.Upgrade;
using V1 = Xtensive.Orm.Tests.Upgrade.RecycledDefinitionTestModel.Version1;
using V2 = Xtensive.Orm.Tests.Upgrade.RecycledDefinitionTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace RecycledDefinitionTestModel
  {
    [HierarchyRoot]
    public class Person : Entity
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public int Age { get; set; }
    }

    [HierarchyRoot]
    public class Shape : Structure
    {
      [Key, Field]
      public long Id { get; private set; }

      [Field]
      public int Growth { get; set; }

      [Field]
      public int Weight { get; set; }
    }

    namespace Version1
    {
      [HierarchyRoot]
      public class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public Person Person { get; set; }

        [Field]
        public Shape Shape { get; set; }

        [Field]
        public int Code { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        protected override string DetectAssemblyVersion() => "1";
      }
    }

    namespace Version2
    {
      [HierarchyRoot]
      public class MyEntity : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public Person Person { get; set; }

        [Field]
        public Shape Shape { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override string DetectAssemblyVersion() => "2";

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          _ = hints.Add(new RenameTypeHint(typeof(V1.MyEntity).FullName, typeof(MyEntity)));
        }

        protected override void AddRecycledDefinitions(ICollection<RecycledDefinition> recycledDefinitions)
        {
          //field Recycled with originalName
          var recycledDefinition = new RecycledFieldDefinition(typeof(MyEntity), "OldName", typeof(string), "Name");
          recycledDefinitions.Add(recycledDefinition);

          //field Recycled without originalName
          recycledDefinition = new RecycledFieldDefinition(typeof(MyEntity), "Code", typeof(int));
          recycledDefinitions.Add(recycledDefinition);

          //link Recycled
          recycledDefinition = new RecycledFieldDefinition(typeof(MyEntity), "OldPerson", typeof(Person), "Person");
          recycledDefinitions.Add(recycledDefinition);

          //struct Recycled
          recycledDefinition = new RecycledFieldDefinition(typeof(MyEntity), "OldShape", typeof(Shape), "Shape");
          recycledDefinitions.Add(recycledDefinition);
        }

        public override void OnUpgrade()
        {
          var session = Session.Demand();
          var myEntities = session.Query.All<MyEntity>().ToList();
          foreach (var myEntity in myEntities) {
            var oldName = myEntity.GetProperty<string>("OldName");
            var oldCode = myEntity.GetProperty<int>("Code");
            var oldPerson = myEntity.GetProperty<Person>("OldPerson");
            var oldShape = myEntity.GetProperty<Shape>("OldShape");
            myEntity.Name = "Old" + oldName;
            myEntity.Person = new Person {Age = oldPerson.Age * 2};
            myEntity.Shape = new Shape {Growth = oldShape.Growth * 2, Weight = oldShape.Weight * 2};
            Assert.That(oldCode, Is.EqualTo(123));
          }
        }
      }
    }
  }

  [TestFixture]
  public class RecycledDefinitionTest
  {
    [Test]
    public void MainTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entityV1 = new V1.MyEntity {
          Name = "MyName1",
          Person = new Person { Age = 10 },
          Shape = new Shape { Growth = 100, Weight = 50 },
          Code = 123,
        };
        var entityV2 = new V1.MyEntity {
          Name = "MyName2",
          Person = new Person { Age = 20 },
          Shape = new Shape { Growth = 200, Weight = 100 },
          Code = 123,
        };
        tx.Complete();
      }

      using (var domain = BuildUpgradedDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entityV1 = session.Query.All<V2.MyEntity>().Single(t => t.Name == "OldMyName1");
        Assert.That(entityV1.Person.Age, Is.EqualTo(20));
        Assert.That(entityV1.Shape.Growth, Is.EqualTo(200));
        Assert.That(entityV1.Shape.Weight, Is.EqualTo(100));

        var entityV2 = session.Query.All<V2.MyEntity>().Single(t => t.Name == "OldMyName2");
        Assert.That(entityV2.Person.Age, Is.EqualTo(40));
        Assert.That(entityV2.Shape.Growth, Is.EqualTo(400));
        Assert.That(entityV2.Shape.Weight, Is.EqualTo(200));
        tx.Complete();
      }
    }

    [Test]
    public async Task MainAsyncTest()
    {
      using (var domain = BuildInitialDomain())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entityV1 = new V1.MyEntity {
          Name = "MyName1",
          Person = new Person { Age = 10 },
          Shape = new Shape { Growth = 100, Weight = 50 },
          Code = 123,
        };
        var entityV2 = new V1.MyEntity {
          Name = "MyName2",
          Person = new Person { Age = 20 },
          Shape = new Shape { Growth = 200, Weight = 100 },
          Code = 123,
        };
        tx.Complete();
      }

      using (var domain = await BuildUpgradedDomainAsync())
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var entityV1 = session.Query.All<V2.MyEntity>().Single(t => t.Name == "OldMyName1");
        Assert.That(entityV1.Person.Age, Is.EqualTo(20));
        Assert.That(entityV1.Shape.Growth, Is.EqualTo(200));
        Assert.That(entityV1.Shape.Weight, Is.EqualTo(100));

        var entityV2 = session.Query.All<V2.MyEntity>().Single(t => t.Name == "OldMyName2");
        Assert.That(entityV2.Person.Age, Is.EqualTo(40));
        Assert.That(entityV2.Shape.Growth, Is.EqualTo(400));
        Assert.That(entityV2.Shape.Weight, Is.EqualTo(200));
        tx.Complete();
      }
    }

    private Domain BuildInitialDomain() =>
      BuildDomain(DomainUpgradeMode.Recreate, typeof(V1.MyEntity));

    private Domain BuildUpgradedDomain() =>
      BuildDomain(DomainUpgradeMode.PerformSafely, typeof(V2.MyEntity));

    private Task<Domain> BuildUpgradedDomainAsync() =>
      BuildDomainAsync(DomainUpgradeMode.PerformSafely, typeof(V2.MyEntity));

    private Domain BuildDomain(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return Domain.Build(configuration);
    }

    private Task<Domain> BuildDomainAsync(DomainUpgradeMode upgradeMode, Type sampleType)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return Domain.BuildAsync(configuration);
    }
  }
}