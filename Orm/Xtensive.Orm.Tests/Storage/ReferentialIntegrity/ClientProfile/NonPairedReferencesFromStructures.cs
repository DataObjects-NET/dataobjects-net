// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.06.22

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Storage.ReferentialIntegrity.ClientProfile.ReferenceFromStructuresTestModel;

namespace Xtensive.Orm.Tests.Storage.ReferentialIntegrity.ClientProfile.ReferenceFromStructuresTestModel
{
  public abstract class BaseEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Comment { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithStructure : BaseEntity
  {
    [Field]
    public ZeroLevelStructure Structure { get; set; }
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
    public FirstLevelStructure FirstLevelStructure { get; set; }

    [Field]
    public EntityReferencedFromStructure EntityField { get; set; }
  }

  public class FirstLevelStructure : Structure
  {
    [Field]
    public SecondLevelStructure SecondLevelStructure { get; set; }

    [Field]
    public EntityReferencedFromStructure EntityField { get; set; }
  }

  public class SecondLevelStructure : Structure
  {
    [Field]
    public int X { get; set; }

    [Field]
    public int Y { get; set; }

    [Field]
    public EntityReferencedFromStructure EntityField { get; set; }
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

  public class Cost : Structure
  {
    [Field]
    public decimal Value { get; set; }

    [Field]
    public Currency Currency { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.ReferentialIntegrity.ClientProfile
{
  public class NonPairedReferencesFromStructures : AutoBuildTest
  {
    private readonly SessionConfiguration sessionConfiguration = new SessionConfiguration(SessionOptions.ClientProfile | SessionOptions.AutoActivation);
    private readonly int EntitiesWithStructureCount = 10;

    [Test]
    public void SetPrimitiveStructureLocalTest()
    {
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        SetPrimitiveStructure(session);
      }
    }

    [Test]
    public void SetStructureWithReferenceLocalTest()
    {
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        SetStructureWithReference(session);
      }
    }

    [Test]
    public void SetStructureWithNestedStructuresBeforeBindTest()
    {
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        ReferenceInitBeforeSetStructureField(session);
      }
    }

    [Test]
    public void SetStructureWithNestedStructresAfterBindTest()
    {
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        ReferenceInitAfterSetStructureField(session);
      }
    }

    [Test]
    public void SetStructureWihtReferencesSavedTest()
    {
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        SetStructureWithReferenceSaved(session);
      }
    }

    [Test]
    public void SetStructureWithNestedStructuresSavedTest()
    {
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        SetStructureWithNestedStructuresSaved(session);
      }
    }

    [Test]
    public void DropReferenceTest()
    {
      using (var session = Domain.OpenSession(sessionConfiguration)) {
        DropStructureWithNestedStructures(session);
      }
    }

    private void SetPrimitiveStructure(Session session)
    {
      var pointA = new Point { X = 10, Y = 10 };
      var pointB = new Point { X = 0, Y = 0 };
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

      var line = new Line { A = pointA, B = pointB };
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(line).Count(), Is.EqualTo(0));

      var triangle = new Triangle { A = pointA, B = pointB, C = new Point { X = 10, Y = 0 } };
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(line).Count(), Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(triangle).Count(), Is.EqualTo(0));

      var pointD = new Point { X = 0, Y = 10 };
      var rectangle = new Rectangle { A = pointA, B = pointB, C = new Point { X = 10, Y = 0 }, D = pointD };
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(line).Count(), Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(triangle).Count(), Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(rectangle).Count(), Is.EqualTo(0));
    }

    private void SetStructureWithReference(Session session)
    {
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

      var localCost = new Cost { Value = 10, Currency = rub };
      var globalCost1 = new Cost { Value = 10, Currency = dollar };
      var globalCost2 = new Cost { Value = 10, Currency = euro };
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(rub).Count(), Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(dollar).Count(), Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(euro).Count(), Is.EqualTo(0));

      var product = new Product {
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
    }

    private void SetStructureWithReferenceSaved(Session session)
    {
      var products = session.Query.All<Product>().ToArray();
      var product = products[0];
      var currency1 = product.LocalCurrencyCost.Currency;
      var currency2 = product.GlobalCurrencyCost.Currency;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      var references = ReferenceFinder.GetReferencesTo(currency1).ToList();
      Assert.That(references.Count, Is.EqualTo(5));
      references = ReferenceFinder.GetReferencesTo(currency2).ToList();
      Assert.That(references.Count, Is.EqualTo(5));

      product.LocalCurrencyCost.Currency = null;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(1));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      references = ReferenceFinder.GetReferencesTo(currency1).ToList();
      Assert.That(references.Count, Is.EqualTo(4));
      references = ReferenceFinder.GetReferencesTo(currency2).ToList();
      Assert.That(references.Count, Is.EqualTo(5));

      product.GlobalCurrencyCost.Currency = null;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(2));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      references = ReferenceFinder.GetReferencesTo(currency1).ToList();
      Assert.That(references.Count, Is.EqualTo(4));
      references = ReferenceFinder.GetReferencesTo(currency2).ToList();
      Assert.That(references.Count, Is.EqualTo(4));

      product = products[1];
      currency1 = product.LocalCurrencyCost.Currency;
      currency2 = product.GlobalCurrencyCost.Currency;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(2));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      references = ReferenceFinder.GetReferencesTo(currency1).ToList();
      Assert.That(references.Count, Is.EqualTo(4));
      references = ReferenceFinder.GetReferencesTo(currency2).ToList();
      Assert.That(references.Count, Is.EqualTo(4));

      product = products[2];
      var currency = new Currency {Sign = "€", FullName = "euro"};
      product.LocalCurrencyCost = new Cost{Value = 20, Currency = currency};
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(3));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(1));
      references = ReferenceFinder.GetReferencesTo(currency1).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      references = ReferenceFinder.GetReferencesTo(currency2).ToList();
      Assert.That(references.Count, Is.EqualTo(4));

      product.GlobalCurrencyCost = new Cost(){Value = 30, Currency = currency};
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(4));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(2));
      references = ReferenceFinder.GetReferencesTo(currency1).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      references = ReferenceFinder.GetReferencesTo(currency2).ToList();
      Assert.That(references.Count, Is.EqualTo(3));

      product = products[3];
      product.LocalCurrencyCost.Currency = currency;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(5));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(3));
      references = ReferenceFinder.GetReferencesTo(currency1).ToList();
      Assert.That(references.Count, Is.EqualTo(2));
      references = ReferenceFinder.GetReferencesTo(currency2).ToList();
      Assert.That(references.Count, Is.EqualTo(3));

      product.GlobalCurrencyCost.Currency = currency;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(6));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(4));
      references = ReferenceFinder.GetReferencesTo(currency1).ToList();
      Assert.That(references.Count, Is.EqualTo(2));
      references = ReferenceFinder.GetReferencesTo(currency2).ToList();
      Assert.That(references.Count, Is.EqualTo(2));

      product = products[4];
      currency1 = product.LocalCurrencyCost.Currency;
      currency2 = product.GlobalCurrencyCost.Currency;
      var cost1 = new Cost {Value = 250, Currency = currency};
      var cost2 = new Cost {Value = 25, Currency = currency2};
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(6));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(4));
      references = ReferenceFinder.GetReferencesTo(currency1).ToList();
      Assert.That(references.Count, Is.EqualTo(2));
      references = ReferenceFinder.GetReferencesTo(currency2).ToList();
      Assert.That(references.Count, Is.EqualTo(2));
    }

    private void SetStructureWithNestedStructuresSaved(Session session)
    {
      var entitiesWithStructure = session.Query.All<EntityWithStructure>().ToList();

      var entity = entitiesWithStructure[0];
      var referencedEntity = entity.Structure.EntityField;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      var references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));

      entity.Structure.EntityField = null;
      entity.Structure.FirstLevelStructure.EntityField = null;
      entity.Structure.FirstLevelStructure.SecondLevelStructure.EntityField = null;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(3));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(0));

      entity = entitiesWithStructure[1];
      referencedEntity = entity.Structure.EntityField;
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));

      var newReferencedEntity = new EntityReferencedFromStructure();
      entity = entitiesWithStructure[3];
      referencedEntity = entity.Structure.EntityField;
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      references = ReferenceFinder.GetReferencesTo(newReferencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(0));
      entity.Structure.EntityField = newReferencedEntity;
      entity.Structure.FirstLevelStructure.EntityField = newReferencedEntity;
      entity.Structure.FirstLevelStructure.SecondLevelStructure.EntityField = newReferencedEntity;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(6));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(3));
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(0));
      references = ReferenceFinder.GetReferencesTo(newReferencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));

      entity = entitiesWithStructure[4];
      referencedEntity = entity.Structure.EntityField;
      newReferencedEntity = new EntityReferencedFromStructure();
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      references = ReferenceFinder.GetReferencesTo(newReferencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(0));
      var struct1 = new ZeroLevelStructure {
        EntityField = newReferencedEntity,
        FirstLevelStructure = new FirstLevelStructure {
          EntityField = newReferencedEntity,
          SecondLevelStructure = new SecondLevelStructure {
            EntityField = newReferencedEntity
          }
        }
      };
      entity.Structure = struct1;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(9));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(6));
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(0));
      references = ReferenceFinder.GetReferencesTo(newReferencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));

      newReferencedEntity = new EntityReferencedFromStructure();
      var struct2 = new FirstLevelStructure {
        EntityField = newReferencedEntity,
        SecondLevelStructure = new SecondLevelStructure {
          EntityField = newReferencedEntity
        }
      };
      entity = entitiesWithStructure[5];
      referencedEntity = entity.Structure.EntityField;
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      references = ReferenceFinder.GetReferencesTo(newReferencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(0));

      entity.Structure.FirstLevelStructure = struct2;
      var typeInfo = entity.TypeInfo;
      var tAssociations = typeInfo.GetTargetAssociations();
      var oAssociations = typeInfo.GetOwnerAssociations();

      typeInfo = newReferencedEntity.TypeInfo;
      tAssociations = typeInfo.GetTargetAssociations();
      oAssociations = typeInfo.GetOwnerAssociations();

      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(11));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(8));
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(1));
      references = ReferenceFinder.GetReferencesTo(newReferencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(2));

      newReferencedEntity = new EntityReferencedFromStructure();
      var struct3 = new SecondLevelStructure {
        EntityField = newReferencedEntity
      };
      entity = entitiesWithStructure[6];
      referencedEntity = entity.Structure.EntityField;
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      references = ReferenceFinder.GetReferencesTo(newReferencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(0));
      entity.Structure.FirstLevelStructure.SecondLevelStructure = struct3;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(12));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(9));
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(2));
      references = ReferenceFinder.GetReferencesTo(newReferencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(1));

      entity = entitiesWithStructure[7];
      referencedEntity = entity.Structure.EntityField;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(12));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(9));
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      var struct4 = new ZeroLevelStructure {
        EntityField = referencedEntity,
        FirstLevelStructure = new FirstLevelStructure {
          EntityField = referencedEntity,
          SecondLevelStructure = new SecondLevelStructure {
            EntityField = referencedEntity
          }
        }
      };
      entity.Structure = struct4;
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(12));
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(9));
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
    }

    private void DropStructureWithNestedStructures(Session session)
    {
      var referencedEntity1 = new EntityReferencedFromStructure();
      var referencedEntity2 = new EntityReferencedFromStructure();

      var entity1 = new EntityWithStructure {
        Structure = new ZeroLevelStructure {
          EntityField = referencedEntity1,
          FirstLevelStructure = new FirstLevelStructure {
            EntityField = referencedEntity1,
            SecondLevelStructure = new SecondLevelStructure {
              EntityField = referencedEntity1
            }
          }
        }
      };
      var entity2 = new EntityWithStructure {
        Structure = new ZeroLevelStructure {
          EntityField = referencedEntity2,
          FirstLevelStructure = new FirstLevelStructure {
            EntityField = referencedEntity2,
            SecondLevelStructure = new SecondLevelStructure {
              EntityField = referencedEntity2
            }
          }
        }
      };

      var associations = new AssociationInfo[3];
      var typeInfo = entity1.TypeInfo;
      associations[0] = typeInfo.Fields["Structure.EntityField"].Associations.First();
      associations[1] = typeInfo.Fields["Structure.FirstLevelStructure.EntityField"].Associations.First();
      associations[2] = typeInfo.Fields["Structure.FirstLevelStructure.SecondLevelStructure.EntityField"].Associations.First();
      Assert.That(associations.All(a=> !a.IsPaired && a.TargetType==referencedEntity1.TypeInfo && a.OwnerType==entity1.TypeInfo));
     
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(6));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      var references = ReferenceFinder.GetReferencesTo(referencedEntity1).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      Assert.That(references.Any(r => r.Association==associations[0] && r.ReferencingEntity==entity1 && r.ReferencedEntity==referencedEntity1), Is.True);
      Assert.That(references.Any(r => r.Association==associations[1] && r.ReferencingEntity==entity1 && r.ReferencedEntity==referencedEntity1), Is.True);
      Assert.That(references.Any(r => r.Association==associations[2] && r.ReferencingEntity==entity1 && r.ReferencedEntity==referencedEntity1), Is.True);

      references = ReferenceFinder.GetReferencesTo(referencedEntity2).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      Assert.That(references.Any(r => r.Association==associations[0] && r.ReferencingEntity==entity2 && r.ReferencedEntity==referencedEntity2), Is.True);
      Assert.That(references.Any(r => r.Association==associations[1] && r.ReferencingEntity==entity2 && r.ReferencedEntity==referencedEntity2), Is.True);
      Assert.That(references.Any(r => r.Association==associations[2] && r.ReferencingEntity==entity2 && r.ReferencedEntity==referencedEntity2), Is.True);

      entity1.Structure.FirstLevelStructure.SecondLevelStructure.EntityField = null;
      entity1.Structure.FirstLevelStructure.EntityField = null;
      entity1.Structure.EntityField = null;
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(3));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      references = ReferenceFinder.GetReferencesTo(referencedEntity1).ToList();
      Assert.That(references.Count, Is.EqualTo(0));
      Assert.That(references.Any(r => r.Association==associations[0] && r.ReferencingEntity==entity1 && r.ReferencedEntity==referencedEntity1), Is.False);
      Assert.That(references.Any(r => r.Association==associations[1] && r.ReferencingEntity==entity1 && r.ReferencedEntity==referencedEntity1), Is.False);
      Assert.That(references.Any(r => r.Association==associations[2] && r.ReferencingEntity==entity1 && r.ReferencedEntity==referencedEntity1), Is.False);

      references = ReferenceFinder.GetReferencesTo(referencedEntity2).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      Assert.That(references.Any(r => r.Association==associations[0] && r.ReferencingEntity==entity2 && r.ReferencedEntity==referencedEntity2), Is.True);
      Assert.That(references.Any(r => r.Association==associations[1] && r.ReferencingEntity==entity2 && r.ReferencedEntity==referencedEntity2), Is.True);
      Assert.That(references.Any(r => r.Association==associations[2] && r.ReferencingEntity==entity2 && r.ReferencedEntity==referencedEntity2), Is.True);
    }

    private void ReferenceInitBeforeSetStructureField(Session session)
    {
      var referencedEntity = new EntityReferencedFromStructure();
      var twoLevelStructure = new SecondLevelStructure {X = 10, Y = 10, EntityField = referencedEntity};
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(referencedEntity).Count(), Is.EqualTo(0));

      var oneLevelStructure = new FirstLevelStructure {SecondLevelStructure = twoLevelStructure, EntityField = referencedEntity};
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(referencedEntity).Count(), Is.EqualTo(0));

      var zeroLevelStructure = new ZeroLevelStructure {FirstLevelStructure = oneLevelStructure, EntityField = referencedEntity};
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(referencedEntity).Count(), Is.EqualTo(0));

      var entity = new EntityWithStructure();
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(referencedEntity).Count(), Is.EqualTo(0));

      entity.Structure = zeroLevelStructure;
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(3));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      var references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      var expectedAssociation = entity.TypeInfo.Fields["Structure.FirstLevelStructure.SecondLevelStructure.EntityField"].Associations.First();
      Assert.That(expectedAssociation.IsPaired, Is.False);
      Assert.That(expectedAssociation.TargetType, Is.EqualTo(referencedEntity.TypeInfo));
      Assert.That(expectedAssociation.OwnerType, Is.EqualTo(entity.TypeInfo));
      Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencingEntity==entity && r.ReferencedEntity==referencedEntity), Is.True);

      expectedAssociation = entity.TypeInfo.Fields["Structure.FirstLevelStructure.EntityField"].Associations.First();
      Assert.That(expectedAssociation.IsPaired, Is.False);
      Assert.That(expectedAssociation.TargetType, Is.EqualTo(referencedEntity.TypeInfo));
      Assert.That(expectedAssociation.OwnerType, Is.EqualTo(entity.TypeInfo));
      Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencingEntity==entity && r.ReferencedEntity==referencedEntity), Is.True);

      expectedAssociation = entity.TypeInfo.Fields["Structure.EntityField"].Associations.First();
      Assert.That(expectedAssociation.IsPaired, Is.False);
      Assert.That(expectedAssociation.TargetType, Is.EqualTo(referencedEntity.TypeInfo));
      Assert.That(expectedAssociation.OwnerType, Is.EqualTo(entity.TypeInfo));
      Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencingEntity==entity && r.ReferencedEntity==referencedEntity), Is.True);
    }

    private void ReferenceInitAfterSetStructureField(Session session)
    {
      var twoLevelStructure = new SecondLevelStructure { X = 10, Y = 10 };
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

      var oneLevelStructure = new FirstLevelStructure { SecondLevelStructure = twoLevelStructure };
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

      var zeroLevelStructure = new ZeroLevelStructure { FirstLevelStructure = oneLevelStructure };
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

      var entity = new EntityWithStructure { Structure = zeroLevelStructure };
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));

      var referencedEntity = new EntityReferencedFromStructure();
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(0));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      Assert.That(ReferenceFinder.GetReferencesTo(referencedEntity).Count(), Is.EqualTo(0));

      zeroLevelStructure = entity.Structure;
      oneLevelStructure = entity.Structure.FirstLevelStructure;
      twoLevelStructure = entity.Structure.FirstLevelStructure.SecondLevelStructure;

      twoLevelStructure.EntityField = referencedEntity;

      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(1));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      var references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(1));
      var expectedAssociation = entity.TypeInfo.Fields["Structure.FirstLevelStructure.SecondLevelStructure.EntityField"].Associations.First();
      Assert.That(expectedAssociation.IsPaired, Is.False);
      Assert.That(expectedAssociation.TargetType, Is.EqualTo(referencedEntity.TypeInfo));
      Assert.That(expectedAssociation.OwnerType, Is.EqualTo(entity.TypeInfo));
      Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencingEntity==entity && r.ReferencedEntity==referencedEntity), Is.True);

      oneLevelStructure.EntityField = referencedEntity;
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(2));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(2));
      Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencingEntity==entity && r.ReferencedEntity==referencedEntity), Is.True);
      var previousExprectedAssociation = expectedAssociation;
      expectedAssociation = entity.TypeInfo.Fields["Structure.FirstLevelStructure.EntityField"].Associations.First();
      Assert.That(expectedAssociation.IsPaired, Is.False);
      Assert.That(expectedAssociation.TargetType, Is.EqualTo(referencedEntity.TypeInfo));
      Assert.That(expectedAssociation.OwnerType, Is.EqualTo(entity.TypeInfo));
      Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencingEntity==entity && r.ReferencedEntity==referencedEntity), Is.True);

      zeroLevelStructure.EntityField = referencedEntity;
      Assert.That(session.NonPairedReferencesRegistry.AddedReferencesCount, Is.EqualTo(3));
      Assert.That(session.NonPairedReferencesRegistry.RemovedReferencesCount, Is.EqualTo(0));
      references = ReferenceFinder.GetReferencesTo(referencedEntity).ToList();
      Assert.That(references.Count, Is.EqualTo(3));
      Assert.That(references.Any(r => r.Association==previousExprectedAssociation && r.ReferencingEntity==entity && r.ReferencedEntity==referencedEntity), Is.True);
      Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencingEntity==entity && r.ReferencedEntity==referencedEntity), Is.True);

      expectedAssociation = entity.TypeInfo.Fields["Structure.EntityField"].Associations.First();
      Assert.That(expectedAssociation.IsPaired, Is.False);
      Assert.That(expectedAssociation.TargetType, Is.EqualTo(referencedEntity.TypeInfo));
      Assert.That(expectedAssociation.OwnerType, Is.EqualTo(entity.TypeInfo));
      Assert.That(references.Any(r => r.Association==expectedAssociation && r.ReferencingEntity==entity && r.ReferencedEntity==referencedEntity), Is.True);
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var rub = new Currency { Sign = "₽", FullName = "Ruble" };
        var dollar = new Currency { Sign = "$", FullName = "Dollar" };
        var localCost = new Cost { Value = 10, Currency = rub };
        var globalCost = new Cost { Value = 10, Currency = dollar };
        var names = new[] {"Awesome thing", "Gorgeous thing", "Outstanding thing", "Breathtaking thing", "Super cool thing"};

        foreach (var name in names) {
          new Product {
            ProductName = name,
            LocalCurrencyCost = localCost,
            GlobalCurrencyCost = globalCost
          };
        }

        for (int i = 0; i < EntitiesWithStructureCount; i++) {
          var referencedEntity = new EntityReferencedFromStructure();
          var referencingEntity = new EntityWithStructure() {
            Structure = new ZeroLevelStructure() {
              EntityField = referencedEntity,
              FirstLevelStructure = new FirstLevelStructure {
                EntityField = referencedEntity,
                SecondLevelStructure = new SecondLevelStructure {
                  EntityField = referencedEntity
                }
              }
            }
          };
        }
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (BaseEntity).Assembly, typeof (BaseEntity).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}
