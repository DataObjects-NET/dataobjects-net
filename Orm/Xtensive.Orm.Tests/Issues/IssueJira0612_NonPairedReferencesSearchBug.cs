using System;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0612_ReferenceFinderBugModel;
using Xtensive.Orm.Tests.ObjectModel.Interfaces.Alphabet;

namespace Xtensive.Orm.Tests.Issues.IssueJira0612_ReferenceFinderBugModel
{
  [HierarchyRoot]
  public class TestA : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field, Association(PairTo = "TestA", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<TestB> TestBs { get; private set; }
  }

  [HierarchyRoot]
  public class TestB : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Text { get; set; }

    [Field]
    public TestA TestA { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class TestC : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
    [Field]
    public string Text { get; set; }

    [Field]
    public string Text2 { get; set; }
    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public TestA TestA { get; set; }
  }

  public abstract class BaseEntity : Entity
  {
    [Field]
    public int Id { get; set; }

    [Field]
    public string Comment { get; set; }
  }

  [HierarchyRoot]
  public class Line : BaseEntity
  {
    public Point A { get; set; }

    public Point B { get; set; }
  }

  public class Triangle : Line
  {
    [Field]
    public Point C { get; set; }
  }

  public class Rectangle : Triangle
  {
    [Field]
    public Point D { get; set; }
  }

  public class Point : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }
  }

  [HierarchyRoot]
  public class Product : BaseEntity
  {
    [Field]
    public string ProductName { get; set; }

    [Field]
    public Producer Producer { get; set; }

    [Field]
    public Cost LocalCurrencyCost { get; set; }

    [Field]
    public Cost GlobalCurrencyCost { get; set; }
  }

  [HierarchyRoot]
  public class Currency : BaseEntity
  {
    [Field]
    public string Sign { get; set; }

    [Field]
    public string FullName { get; set; }
  }

  [HierarchyRoot]
  public class Producer : BaseEntity
  {
    [Field]
    public string Name { get; set; }
  }

  public class Cost : Structure
  {
    [Field]
    public decimal Value { get; set; }

    [Field]
    public Currency Currency { get; set; }
  }

  [HierarchyRoot]
  public class EntityReferencedFromStructure : BaseEntity
  {
    [Field]
    public string Text { get; set; }
  }

  public class ZeroLevelStructure : Structure
  {
    [Field]
    public OneLevelStructure OneLevelStructure { get; set; }

    [Field]
    public EntityReferencedFromStructure EntityField { get; set; }
  }

  public class OneLevelStructure : Structure
  {
    [Field]
    public TwoLevelStructure TwoLevelStructure { get; set; }

    [Field]
    public EntityReferencedFromStructure EntityField { get; set; }
  }

  public class TwoLevelStructure : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }

    [Field]
    public EntityReferencedFromStructure EntityField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0612_NonPairedReferencesSearchBug : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.ClientProfile))) {
        var NewTestA = new TestA {Text = "TestA"};
        new TestB {TestA = NewTestA, Text = "TestB"};
        // Wrong ReferencingEntity "TestB" instead of "TestC" with Association "TestC-TestA-TestA"
        var references = ReferenceFinder.GetReferencesTo(NewTestA).ToList();
        // Exception
        NewTestA.Remove();
      }
    }

    [Test]
    public void SetPrimitiveStructureTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var pointA = new Point {X = 10, Y = 10};
        var pointB = new Point {X = 0, Y = 0};
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

        var line = new Line {A = pointA, B = pointB};
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(line).Count(),Is.EqualTo(0));

        var triangle = new Triangle {A = pointA, B = pointB, C = new Point {X = 10, Y = 0}};
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(line).Count(),Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(triangle).Count(),Is.EqualTo(0));

        var pointD = new Point {X = 0, Y = 10};
        var rectangle = new Rectangle {A = pointA, B = pointB, C = new Point() {X = 10, Y = 0}, D = pointD};
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(line).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(triangle).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(rectangle).Count(), Is.EqualTo(0));
      }
    }

    public void SetReferencedStructureTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var rub = new Currency {
          Sign = "₽",
          FullName = "Ruble"
        };
        var dollar = new Currency {
          Sign = "$",
          FullName = "Dollar"
        };
        var euro = new Currency {
          Sign = "€",
          FullName = "Euro"
        };
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(rub).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(dollar).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(euro).Count(), Is.EqualTo(0));

        var localCost = new Cost {Value = 10, Currency = rub};
        var globalCost1 = new Cost {Value = 10, Currency = dollar};
        var globalCost2 = new Cost {Value = 10, Currency = euro};
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(rub).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(dollar).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(euro).Count(), Is.EqualTo(0));

        var product = new Product {
          Comment = "Product",
          ProductName = "Super cool thing",
          LocalCurrencyCost = localCost,
        };
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(1));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(rub).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(dollar).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(euro).Count(), Is.EqualTo(0));

        product.GlobalCurrencyCost = globalCost1;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(2));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(rub).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(dollar).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(euro).Count(), Is.EqualTo(0));

        product.GlobalCurrencyCost = globalCost2;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(2));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(rub).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(dollar).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(euro).Count(), Is.EqualTo(1));

        product.GlobalCurrencyCost = null;
        Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(1));
        Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(rub).Count(), Is.EqualTo(1));
        Assert.That(ReferenceFinder.GetReferencesTo(dollar).Count(), Is.EqualTo(0));
        Assert.That(ReferenceFinder.GetReferencesTo(euro).Count(), Is.EqualTo(0));
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TestA).Assembly, typeof (TestA).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
