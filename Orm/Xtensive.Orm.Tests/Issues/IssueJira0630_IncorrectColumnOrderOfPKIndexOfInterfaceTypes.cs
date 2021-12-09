// Copyright (C) 2016-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2016.03.31

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0630_IncorrectColumnOrderOfPKIndexOfInterfaceTypesModel;
using Xtensive.Orm.Tests.Issues.IssueJira0630_IncorrectColumnOrderOfPKIndexOfInterfaceTypesModel.Populator;
namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0630_IncorrectColumnOrderOfPKIndexOfInterfaceTypes : AutoBuildTest
  {
    [Test]
    public void ColumnsOrderTestTest()
    {
      foreach (var type in Domain.Model.Types.Where(t=>t.IsEntity && !t.IsSystem)) {
        var index = type.Indexes.PrimaryIndex;
        int currentIndex = -1;
        foreach (var indexColumn in index.Columns) {
          var typeColumnIndex = -1;
          foreach (var typeColumn in type.Columns) {
            typeColumnIndex++;
            if (typeColumn==indexColumn) {
              break;
            }
          }
          Assert.That(typeColumnIndex > currentIndex, Is.True);
          currentIndex = typeColumnIndex;
        }
      }
    }

    [Test]
    public void SelectInterfaceFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        IStoredMaterialCreator[] creators = null;
        Assert.DoesNotThrow(() => creators = session.Query.All<StoredMaterialLot>().Select(el => el.Creator).ToArray());
        Assert.That(creators.Length, Is.EqualTo(2));
        foreach (var creator in creators) {
          Assert.That(creator, Is.Not.Null);
          var simple = creator as SimpleStoredMaterialCreator;
          var complex = creator as ComplexStoredMaterialCreator;
          Assert.That(simple!=null || complex!=null, Is.True);
        }
      }
    }

    [Test]
    public void SelectInterfaceDirectlyTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        IAction[] actions = null;
        Assert.DoesNotThrow(()=> actions = session.Query.All<IAction>().ToArray());
        Assert.That(actions.Length, Is.EqualTo(2));
        foreach (var action in actions) {
          Assert.That(action, Is.Not.Null);
          var simple = action as SimpleStoredMaterialCreator;
          var complex = action as ComplexStoredMaterialCreator;
          Assert.That(simple!=null || complex!=null, Is.True);
        }

        ISomeIntermediateInterface[] intermediateThings = null;
        Assert.DoesNotThrow(() => intermediateThings = session.Query.All<ISomeIntermediateInterface>().ToArray());
        Assert.That(intermediateThings.Length, Is.EqualTo(2));
        foreach (var thing in intermediateThings) {
          Assert.That(thing, Is.Not.Null);
          var simple = thing as SimpleStoredMaterialCreator;
          var complex = thing as ComplexStoredMaterialCreator;
          Assert.That(simple!=null || complex!=null, Is.True);
        }

        IStoredObjectCreator[] objectCreators = null;
        Assert.DoesNotThrow(() => objectCreators = session.Query.All<IStoredObjectCreator>().ToArray());
        Assert.That(objectCreators.Length, Is.EqualTo(2));
        foreach (var creator in objectCreators) {
          Assert.That(creator, Is.Not.Null);
          var simple = creator as SimpleStoredMaterialCreator;
          var complex = creator as ComplexStoredMaterialCreator;
          Assert.That(simple!=null || complex!=null, Is.True);
        }

        IStoredMaterialCreator[] materialCreators = null;
        Assert.DoesNotThrow(() => materialCreators = session.Query.All<IStoredMaterialCreator>().ToArray());
        Assert.That(materialCreators.Length, Is.EqualTo(2));
        foreach (var creator in materialCreators) {
          Assert.That(creator, Is.Not.Null);
          var simple = creator as SimpleStoredMaterialCreator;
          var complex = creator as ComplexStoredMaterialCreator;
          Assert.That(simple!=null || complex!=null, Is.True);
        }
      }
    }

    [Test]
    public void SelectInterfaceFieldAdditonalTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(() => session.Query.All<InterfaceFieldsContainer>().Select(e => e.AField).ToArray());
        Assert.DoesNotThrow(() => session.Query.All<InterfaceFieldsContainer>().Select(e => e.AHField).ToArray());
        Assert.DoesNotThrow(() => session.Query.All<InterfaceFieldsContainer>().Select(e => e.AHandEField).ToArray());
        Assert.DoesNotThrow(() => session.Query.All<InterfaceFieldsContainer>().Select(e => e.EField).ToArray());
        Assert.DoesNotThrow(() => session.Query.All<InterfaceFieldsContainer>().Select(e => e.EandAHField).ToArray());
        Assert.DoesNotThrow(() => session.Query.All<InterfaceFieldsContainer>().Select(e => e.HAEField).ToArray());
        Assert.DoesNotThrow(() => session.Query.All<InterfaceFieldsContainer>().Select(e => e.HAField).ToArray());
        Assert.DoesNotThrow(() => session.Query.All<InterfaceFieldsContainer>().Select(e => e.HEAField).ToArray());
        Assert.DoesNotThrow(() => session.Query.All<InterfaceFieldsContainer>().Select(e => e.HField).ToArray());
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new Populator(session).Populate();
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (StoredMaterialLot).Assembly, typeof (StoredMaterialLot).Namespace);
      return configuration;
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0630_IncorrectColumnOrderOfPKIndexOfInterfaceTypesModel
{
  namespace Populator
  {
    internal sealed class Populator
    {
      public Session Session { get; private set; }

      public void Populate()
      {
        PopulateUserCaseRelatedData();
        PopulateExtraCasesRelatedData();
      }

      private void PopulateUserCaseRelatedData()
      {
        var simpleStoredMaterialCreator = new SimpleStoredMaterialCreator {
          ActionState = ActionState.State1,
          IgnoreLockedForInput = true,
          IgnoreLockedInBeam = true,
          IntermediateInterfaceText = "SimpleStoredMaterialCreator:IntemediateText",
        };

        var complexStoredMaterialCreator = new ComplexStoredMaterialCreator {
          ActionState = ActionState.State1,
          IgnoreLockedForInput = false,
          IgnoreLockedInBeam = false,
          IntermediateInterfaceText = "ComplexStoredMaterialCreator:IntemediateText",
          FieldWhichMakesItComplex = "CompexStoredMaterialCreator:ComplexFieldText"
        };

        new StoredMaterialLot {
          SomeNonPersistentProperty = "Text which will be ommited because it's non-persistent field",
          Creator = simpleStoredMaterialCreator,
        };

        new StoredMaterialLot {
          SomeNonPersistentProperty = "Text which willbe ommited because of non-persistence",
          Creator = complexStoredMaterialCreator
        };
      }

      private void PopulateExtraCasesRelatedData()
      {
        const int testDataCount = 10;
        var randomizer = new Random();
        var testEntities = GetSimpleTestEntities(testDataCount);
        var testStructures = GetSimpleStructures(testDataCount);

        Func<SimpleTestEntity> getEntityAction = () => testEntities[randomizer.Next(0, testDataCount)];
        Func<SimpleStructure> getStructureAction = () => testStructures[randomizer.Next(0, testDataCount)];

        PopulateZeroLevelEntities(getEntityAction, getStructureAction);
        PopulateFirstLevelEntities(getEntityAction, getStructureAction);
        PopulateSecondLevelEntities(getEntityAction, getStructureAction);
        PopulateThirdLevelEntities(getEntityAction, getStructureAction);
        PopulateContainers();
      }

      private void PopulateZeroLevelEntities(Func<SimpleTestEntity> entityProvider, Func<SimpleStructure> structureProvider)
      {
        new ACorrectOrder {
          ABoolField = true,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AIntField = 10,
          AStringField = "jdfhkjghjdfhgjhdf",
          AStructureField = structureProvider.Invoke()
        };

        new AIncorrectOrder {
          ABoolField = true,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State2,
          AIntField = 10,
          AStringField = "jdfhkjghjdfhgjhdf",
          AStructureField = structureProvider.Invoke()
        };

        new ECorrectOrder {
          EBoolField = false,
          EEntityField = entityProvider.Invoke(),
          EEnumField = ActionState.State1,
          EIntField = 10,
          EStringField = "jdfhkjghjdfhgjhdf",
          EStructureField = structureProvider.Invoke()
        };

        new EIncorrectOrder {
          EBoolField = false,
          EEntityField = entityProvider.Invoke(),
          EEnumField = ActionState.State2,
          EIntField = 10,
          EStringField = "jdfhkjghjdfhgjhdf",
          EStructureField = structureProvider.Invoke()
        };

        new HCorrectOrder {
          HBoolField = true,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State1,
          HIntField = 10,
          HStringField = "jdfhkjghjdfhgjhdf",
          HStructureField = structureProvider.Invoke()
        };

        new HIncorrectOrder {
          HBoolField = true,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State1,
          HIntField = 10,
          HStringField = "jdfhkjghjdfhgjhdf",
          HStructureField = structureProvider.Invoke()
        };
      }

      private void PopulateFirstLevelEntities(Func<SimpleTestEntity> entityProvider, Func<SimpleStructure> structureProvider)
      {
        new AHCorrectOrder {
          ABoolField = false,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AIntField = 11,
          AStringField = "dhfkghkdhfgkjhdfkjhgkjfd",
          AStructureField = structureProvider.Invoke(),
          HBoolField = false,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State1,
          HIntField = 11,
          HStringField = "dhfkghkdhfgkjhdfkjhgkjfd",
          HStructureField = structureProvider.Invoke(),
        };

        new AHIncorrectOrder {
          ABoolField = false,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AIntField = 11,
          AStringField = "dhfkghkdhfgkjhdfkjhgkjfd",
          AStructureField = structureProvider.Invoke(),
          HBoolField = false,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State1,
          HIntField = 11,
          HStringField = "dhfkghkdhfgkjhdfkjhgkjfd",
          HStructureField = structureProvider.Invoke(),
        };

        new HACorrectOrder {
          ABoolField = false,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AIntField = 11,
          AStringField = "dhfkghkdhfgkjhdfkjhgkjfd",
          AStructureField = structureProvider.Invoke(),
          HBoolField = false,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State1,
          HIntField = 11,
          HStringField = "dhfkghkdhfgkjhdfkjhgkjfd",
          HStructureField = structureProvider.Invoke(),
        };

        new HAIncorrectOrder {
          ABoolField = false,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AIntField = 11,
          AStringField = "dhfkghkdhfgkjhdfkjhgkjfd",
          AStructureField = structureProvider.Invoke(),
          HBoolField = false,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State1,
          HIntField = 11,
          HStringField = "dhfkghkdhfgkjhdfkjhgkjfd",
          HStructureField = structureProvider.Invoke(),
        };
      }

      private void PopulateSecondLevelEntities(Func<SimpleTestEntity> entityProvider, Func<SimpleStructure> structureProvider)
      {
        new HAECorrectOrder {
          HAEBoolField = true,
          HAEEntityField = entityProvider.Invoke(),
          HAEEnumField = ActionState.State3,
          HAEIntField = 20,
          HAEStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
          HAEStructureField = structureProvider.Invoke(),
          HBoolField = false,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State2,
          HIntField = 21,
          HStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
          HStructureField = structureProvider.Invoke(),
          ABoolField = true,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AIntField = 22,
          AStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
          AStructureField = structureProvider.Invoke(),
        };
        new HAEIncorrectOrder {
          HAEBoolField = true,
          HAEEntityField = entityProvider.Invoke(),
          HAEEnumField = ActionState.State3,
          HAEIntField = 20,
          HAEStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
          HAEStructureField = structureProvider.Invoke(),
          HBoolField = false,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State2,
          HIntField = 21,
          HStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
          HStructureField = structureProvider.Invoke(),
          ABoolField = true,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AIntField = 22,
          AStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
          AStructureField = structureProvider.Invoke(),
        };

        var entity = new HEACorrectOrder {
          HEABoolField = true,
          HEAEntityField = entityProvider.Invoke(),
          HEAEnumField = ActionState.State3,
          HEAIntField = 20,
          HEAStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
          HBoolField = false,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State2,
          HIntField = 21,
          HStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
          HStructureField = structureProvider.Invoke(),
          ABoolField = true,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AIntField = 22,
          AStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
        };
        ((IHEA)entity).AStructureField = structureProvider.Invoke();
        ((IA)entity).AStructureField = structureProvider.Invoke();

        var entity2 = new HEAIncorrectOrder {
          HEABoolField = true,
          HEAEntityField = entityProvider.Invoke(),
          HEAEnumField = ActionState.State3,
          HEAIntField = 20,
          HEAStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
          HBoolField = false,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State2,
          HIntField = 21,
          HStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
          HStructureField = structureProvider.Invoke(),
          ABoolField = true,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AIntField = 22,
          AStringField = "dfhkhwrhjerhjhgdkjhfgjkfg",
        };
        ((IHEA)entity2).AStructureField = structureProvider.Invoke();
        ((IA)entity2).AStructureField = structureProvider.Invoke();
      }

      private void PopulateThirdLevelEntities(Func<SimpleTestEntity> entityProvider, Func<SimpleStructure> structureProvider)
      {
        new AHandE {
          AIntField = 30,
          ABoolField = false,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AStringField = "jdfhkgjhdkjfhgkjhskjfhkdsjg",
          AStructureField = structureProvider.Invoke(),
          AHIntField = 30,
          AHBoolField = false,
          AHEntityField = entityProvider.Invoke(),
          AHEnumField = ActionState.State2,
          AHStringField = "jdfhkgjhdkjfhgkjhskjfhkdsjg",
          AHStructureField = structureProvider.Invoke(),
          HIntField = 30,
          HBoolField = false,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State3,
          HStringField = "jdfhkgjhdkjfhgkjhskjfhkdsjg",
          HStructureField = structureProvider.Invoke(),
          EIntField = 30,
          EBoolField = false,
          EEntityField = entityProvider.Invoke(),
          EEnumField = ActionState.State1,
          EStringField = "jdfhkgjhdkjfhgkjhskjfhkdsjg",
          EStructureField = structureProvider.Invoke(),
        };
        new EandAH {
          AIntField = 30,
          ABoolField = false,
          AEntityField = entityProvider.Invoke(),
          AEnumField = ActionState.State1,
          AStringField = "jdfhkgjhdkjfhgkjhskjfhkdsjg",
          AStructureField = structureProvider.Invoke(),
          AHIntField = 30,
          AHBoolField = false,
          AHEntityField = entityProvider.Invoke(),
          AHEnumField = ActionState.State2,
          AHStringField = "jdfhkgjhdkjfhgkjhskjfhkdsjg",
          AHStructureField = structureProvider.Invoke(),
          HIntField = 30,
          HBoolField = false,
          HEntityField = entityProvider.Invoke(),
          HEnumField = ActionState.State3,
          HStringField = "jdfhkgjhdkjfhgkjhskjfhkdsjg",
          HStructureField = structureProvider.Invoke(),
          EIntField = 30,
          EBoolField = false,
          EEntityField = entityProvider.Invoke(),
          EEnumField = ActionState.State1,
          EStringField = "jdfhkgjhdkjfhgkjhskjfhkdsjg",
          EStructureField = structureProvider.Invoke(),
        };
      }

      private void PopulateContainers()
      {
        var AFieldValue = Session.Query.CreateDelayedQuery((q) => q.All<AIncorrectOrder>().First());
        var AHFieldValue = Session.Query.CreateDelayedQuery((q) => q.All<AHIncorrectOrder>().First());
        var AHandEFieldValue = Session.Query.CreateDelayedQuery((q) => q.All<AHandE>().First());
        var EFieldValue = Session.Query.CreateDelayedQuery((q) => q.All<EIncorrectOrder>().First());
        var EandAHFieldValue = Session.Query.CreateDelayedQuery((q) => q.All<EandAH>().First());
        var HAEFieldValue = Session.Query.CreateDelayedQuery((q) => q.All<HAEIncorrectOrder>().First());
        var HAFieldValue = Session.Query.CreateDelayedQuery((q) => q.All<HAIncorrectOrder>().First());
        var HEAFieldValue = Session.Query.CreateDelayedQuery((q) => q.All<HEAIncorrectOrder>().First());
        var HFieldValue = Session.Query.All<HIncorrectOrder>().First();

        new InterfaceFieldsContainer {
          AField = AFieldValue.Value,
          AHField = AHFieldValue.Value,
          AHandEField = AHandEFieldValue.Value,
          EField = EFieldValue.Value,
          EandAHField = EandAHFieldValue.Value,
          HAEField = HAEFieldValue.Value,
          HAField = HAFieldValue.Value,
          HEAField = HEAFieldValue.Value,
          HField = HFieldValue
        };
      }

      private SimpleTestEntity[] GetSimpleTestEntities(int arrayLength)
      {
        var array = new SimpleTestEntity[arrayLength];
        for (int i = 0; i < arrayLength; i++) {
          array[i] = new SimpleTestEntity {Text = Guid.NewGuid().ToString()};
        }
        return array;
      }

      private SimpleStructure[] GetSimpleStructures(int arrayLength)
      {
        var array = new SimpleStructure[arrayLength];
        for (int i = 0; i < arrayLength; i++) {
          array[i] = new SimpleStructure {Value = i, CurrencyCode = 100 + i};
        }
        return array;
      }

      public Populator(Session session)
      {
        Session = session;
      }
    }
  }

  #region User case types
  public interface IMesObject
  {
    [Field, Key]
    long Id { get; }

    string SomeNonPersistentProperty { get; set; }
  }

  public interface IAction : IEntity, IMesObject
  {
    [Field]
    Size Size { get; set; }

    [Field]
    ActionState ActionState { get; set; }
  }

  public interface ISomeIntermediateInterface : IAction
  {
    [Field]
    string IntermediateInterfaceText { get; set; }
  }

  public interface IStoredObjectCreator : ISomeIntermediateInterface
  {
    [Field]
    bool IgnoreLockedForInput { get; set; }

    [Field]
    bool IgnoreLockedInBeam { get; set; }
  }

  public interface IStoredMaterialCreator : IStoredObjectCreator
  {
  }

  public abstract class MesObject : Entity, IMesObject
  {
    public long Id { get; set; }

    public string SomeNonPersistentProperty { get; set; }
  }

  [HierarchyRoot]
  public class Size : MesObject
  {
    [Field]
    public string Text { get; set; }
  }

  [HierarchyRoot]
  public class SimpleStoredMaterialCreator : MesObject, IStoredMaterialCreator
  {
    public bool IgnoreLockedForInput { get; set; }

    public bool IgnoreLockedInBeam { get; set; }

    public string IntermediateInterfaceText { get; set; }

    public Size Size { get; set; }

    public ActionState ActionState { get; set; }
  }

  [HierarchyRoot]
  public class ComplexStoredMaterialCreator : MesObject, IStoredMaterialCreator
  {
    public bool IgnoreLockedForInput { get; set; }

    public bool IgnoreLockedInBeam { get; set; }

    public string IntermediateInterfaceText { get; set; }

    public Size Size { get; set; }

    public ActionState ActionState { get; set; }

    [Field]
    public string FieldWhichMakesItComplex { get; set; }
  }

  [HierarchyRoot]
  public class StoredMaterialLot : MesObject
  {
    [Field]
    public IStoredMaterialCreator Creator { get; set; }
  }
  #endregion

  #region Supporting types
  [HierarchyRoot]
  public class SimpleTestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Text { get; set; }
  }

  public class SimpleStructure : Structure
  {
    [Field]
    public double Value { get; set; }

    [Field]
    public int CurrencyCode { get; set; }
  }

  public enum ActionState
  {
    State1,
    State2,
    State3
  }
#endregion

  #region Zero level interfaces

  public interface IA : IEntity
  {
    [Field]
    string AStringField { get; set; }

    [Field]
    bool ABoolField { get; set; }

    [Field]
    int AIntField { get; set; }

    [Field]
    ActionState AEnumField { get; set; }

    [Field]
    IE AInterfaceField { get; set; }

    [Field]
    SimpleTestEntity AEntityField { get; set; }

    [Field]
    SimpleStructure AStructureField { get; set; }
  }

  public interface IE : IEntity
  {
    [Field]
    string EStringField { get; set; }

    [Field]
    bool EBoolField { get; set; }

    [Field]
    int EIntField { get; set; }

    [Field]
    ActionState EEnumField { get; set; }

    [Field]
    IE EInterfaceField { get; set; }

    [Field]
    SimpleTestEntity EEntityField { get; set; }

    [Field]
    SimpleStructure EStructureField { get; set; }
  }

  public interface IH : IEntity
  {
    [Field]
    string HStringField { get; set; }

    [Field]
    bool HBoolField { get; set; }

    [Field]
    int HIntField { get; set; }

    [Field]
    ActionState HEnumField { get; set; }
    
    [Field]
    IE HInterfaceField { get; set; }

    [Field]
    SimpleTestEntity HEntityField { get; set; }

    [Field]
    SimpleStructure HStructureField { get; set; }
  }

#endregion

  #region The first level intefaces

  public interface IAH : IA, IH
  {
    [Field]
    string AHStringField { get; set; }

    [Field]
    bool AHBoolField { get; set; }

    [Field]
    int AHIntField { get; set; }

    [Field]
    ActionState AHEnumField { get; set; }
    
    [Field]
    IE AHInterfaceField { get; set; }

    [Field]
    SimpleTestEntity AHEntityField { get; set; }

    [Field]
    SimpleStructure AHStructureField { get; set; }
  }

  public interface IHA : IH, IA
  {
    [Field]
    string HAStringField { get; set; }

    [Field]
    bool HABoolField { get; set; }

    [Field]
    int HAIntField { get; set; }

    [Field]
    ActionState HAEnumField { get; set; }
    
    [Field]
    IE HAInterfaceField { get; set; }

    [Field]
    SimpleTestEntity HAEntityField { get; set; }

    [Field]
    SimpleStructure HAStructureField { get; set; }
  }

#endregion

  #region The second level interfaces

  public interface IHAE : IH, IA, IE
  {
    [Field]
    string HAEStringField { get; set; }

    [Field]
    bool HAEBoolField { get; set; }

    [Field]
    int HAEIntField { get; set; }

    [Field]
    ActionState HAEEnumField { get; set; }

    [Field]
    IE HAEInterfaceField { get; set; }

    [Field]
    SimpleTestEntity HAEEntityField { get; set; }

    [Field]
    SimpleStructure HAEStructureField { get; set; }
  }

  public interface IHEA : IH, IE, IA
  {
    [Field]
    string HEAStringField { get; set; }

    [Field]
    bool HEABoolField { get; set; }

    [Field]
    int HEAIntField { get; set; }

    [Field]
    ActionState HEAEnumField { get; set; }

    [Field]
    IE HEAInterfaceField { get; set; }

    [Field]
    SimpleTestEntity HEAEntityField { get; set; }

    [Field]
    new SimpleStructure AStructureField { get; set; }
  }

#endregion

  #region Three level interfaces

  public interface IAHandE : IAH, IE
  {
    [Field]
    string AHandEStringField { get; set; }

    [Field]
    bool AHandEBoolField { get; set; }

    [Field]
    int AHandEIntField { get; set; }

    [Field]
    ActionState AHandEEnumField { get; set; }
    
    [Field]
    IE AHandEInterfaceField { get; set; }

    [Field]
    SimpleTestEntity AHandEEntityField { get; set; }

    [Field]
    SimpleStructure AHandEStructureField { get; set; }
  }

  public interface IEandAH : IE, IAH
  {
    [Field]
    string EandAHStringField { get; set; }

    [Field]
    bool EandAHBoolField { get; set; }

    [Field]
    int EandAHIntField { get; set; }

    [Field]
    ActionState EandAHEnumField { get; set; }
    
    [Field]
    IE EandAHInterfaceField { get; set; }

    [Field]
    SimpleTestEntity EandAHEntityField { get; set; }

    [Field]
    SimpleStructure EandAHStructureField { get; set; }
  }
#endregion

  #region Implementations

  public abstract class BaseEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }
  }

  [HierarchyRoot]
  public class ACorrectOrder : BaseEntity, IA
  {
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    public SimpleStructure AStructureField { get; set; }
  }

  [HierarchyRoot]
  public class AIncorrectOrder : BaseEntity, IA
  {
    public string AStringField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    public SimpleStructure AStructureField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
  }

  [HierarchyRoot]
  public class HCorrectOrder : BaseEntity, IH
  {
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
  }

  [HierarchyRoot]
  public class HIncorrectOrder : BaseEntity, IH
  {
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
  }

  [HierarchyRoot]
  public class ECorrectOrder : BaseEntity, IE
  {
    public string EStringField { get; set; }
    public bool EBoolField { get; set; }
    public int EIntField { get; set; }
    public ActionState EEnumField { get; set; }
    public IE EInterfaceField { get; set; }
    public SimpleTestEntity EEntityField { get; set; }
    public SimpleStructure EStructureField { get; set; }
  }

  [HierarchyRoot]
  public class EIncorrectOrder : BaseEntity, IE
  {
    public string EStringField { get; set; }
    public bool EBoolField { get; set; }
    public int EIntField { get; set; }
    public SimpleTestEntity EEntityField { get; set; }
    public SimpleStructure EStructureField { get; set; }
    public ActionState EEnumField { get; set; }
    public IE EInterfaceField { get; set; }
  }

  [HierarchyRoot]
  public class AHCorrectOrder : BaseEntity, IAH
  {
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    public SimpleStructure AStructureField { get; set; }
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public string AHStringField { get; set; }
    public bool AHBoolField { get; set; }
    public int AHIntField { get; set; }
    public ActionState AHEnumField { get; set; }
    public IE AHInterfaceField { get; set; }
    public SimpleTestEntity AHEntityField { get; set; }
    public SimpleStructure AHStructureField { get; set; }
  }

  [HierarchyRoot]
  public class AHIncorrectOrder : BaseEntity, IAH
  {
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public string AHStringField { get; set; }
    public bool AHBoolField { get; set; }
    public int AHIntField { get; set; }
    public ActionState AHEnumField { get; set; }
    public IE AHInterfaceField { get; set; }
    public SimpleTestEntity AHEntityField { get; set; }
    public SimpleStructure AHStructureField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    public SimpleStructure AStructureField { get; set; }
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
  }

  [HierarchyRoot]
  public class HACorrectOrder : BaseEntity, IHA
  {
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    public SimpleStructure AStructureField { get; set; }
    public string HAStringField { get; set; }
    public bool HABoolField { get; set; }
    public int HAIntField { get; set; }
    public ActionState HAEnumField { get; set; }
    public IE HAInterfaceField { get; set; }
    public SimpleTestEntity HAEntityField { get; set; }
    public SimpleStructure HAStructureField { get; set; }
  }

  [HierarchyRoot]
  public class HAIncorrectOrder : BaseEntity, IHA
  {
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    public SimpleStructure AStructureField { get; set; }
    public string HAStringField { get; set; }
    public bool HABoolField { get; set; }
    public int HAIntField { get; set; }
    public ActionState HAEnumField { get; set; }
    public IE HAInterfaceField { get; set; }
    public SimpleTestEntity HAEntityField { get; set; }
    public SimpleStructure HAStructureField { get; set; }
  }

  [HierarchyRoot]
  public class HAECorrectOrder : BaseEntity, IHAE
  {
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    public SimpleStructure AStructureField { get; set; }
    public string EStringField { get; set; }
    public bool EBoolField { get; set; }
    public int EIntField { get; set; }
    public ActionState EEnumField { get; set; }
    public IE EInterfaceField { get; set; }
    public SimpleTestEntity EEntityField { get; set; }
    public SimpleStructure EStructureField { get; set; }
    public string HAEStringField { get; set; }
    public bool HAEBoolField { get; set; }
    public int HAEIntField { get; set; }
    public ActionState HAEEnumField { get; set; }
    public IE HAEInterfaceField { get; set; }
    public SimpleTestEntity HAEEntityField { get; set; }
    public SimpleStructure HAEStructureField { get; set; }
  }

  [HierarchyRoot]
  public class HAEIncorrectOrder : BaseEntity, IHAE
  {
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    public SimpleStructure AStructureField { get; set; }
    public string EStringField { get; set; }
    public bool EBoolField { get; set; }
    public int EIntField { get; set; }
    public ActionState EEnumField { get; set; }
    public IE EInterfaceField { get; set; }
    public SimpleTestEntity EEntityField { get; set; }
    public SimpleStructure EStructureField { get; set; }
    public string HAEStringField { get; set; }
    public bool HAEBoolField { get; set; }
    public int HAEIntField { get; set; }
    public ActionState HAEEnumField { get; set; }
    public IE HAEInterfaceField { get; set; }
    public SimpleTestEntity HAEEntityField { get; set; }
    public SimpleStructure HAEStructureField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
  }

  [HierarchyRoot]
  public class HEACorrectOrder : BaseEntity, IHEA
  {
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public string EStringField { get; set; }
    public bool EBoolField { get; set; }
    public int EIntField { get; set; }
    public ActionState EEnumField { get; set; }
    public IE EInterfaceField { get; set; }
    public SimpleTestEntity EEntityField { get; set; }
    public SimpleStructure EStructureField { get; set; }
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    SimpleStructure IHEA.AStructureField { get; set; }
    public string HEAStringField { get; set; }
    public bool HEABoolField { get; set; }
    public int HEAIntField { get; set; }
    public ActionState HEAEnumField { get; set; }
    public IE HEAInterfaceField { get; set; }
    public SimpleTestEntity HEAEntityField { get; set; }
    SimpleStructure IA.AStructureField { get; set; }
  }

  [HierarchyRoot]
  public class HEAIncorrectOrder : BaseEntity, IHEA
  {
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public string EStringField { get; set; }
    public bool EBoolField { get; set; }
    public int EIntField { get; set; }
    public ActionState EEnumField { get; set; }
    public IE EInterfaceField { get; set; }
    public SimpleTestEntity EEntityField { get; set; }
    public SimpleTestEntity HEAEntityField { get; set; }
    SimpleStructure IA.AStructureField { get; set; }
    public SimpleStructure EStructureField { get; set; }
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    SimpleStructure IHEA.AStructureField { get; set; }
    public string HEAStringField { get; set; }
    public bool HEABoolField { get; set; }
    public int HEAIntField { get; set; }
    public ActionState HEAEnumField { get; set; }
    public IE HEAInterfaceField { get; set; }
  }

  [HierarchyRoot]
  public class AHandE : BaseEntity, IAHandE
  {
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    public SimpleStructure AStructureField { get; set; }
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public string AHStringField { get; set; }
    public bool AHBoolField { get; set; }
    public int AHIntField { get; set; }
    public ActionState AHEnumField { get; set; }
    public IE AHInterfaceField { get; set; }
    public SimpleTestEntity AHEntityField { get; set; }
    public SimpleStructure AHStructureField { get; set; }
    public string EStringField { get; set; }
    public bool EBoolField { get; set; }
    public int EIntField { get; set; }
    public ActionState EEnumField { get; set; }
    public IE EInterfaceField { get; set; }
    public SimpleTestEntity EEntityField { get; set; }
    public SimpleStructure EStructureField { get; set; }
    public string AHandEStringField { get; set; }
    public bool AHandEBoolField { get; set; }
    public int AHandEIntField { get; set; }
    public ActionState AHandEEnumField { get; set; }
    public IE AHandEInterfaceField { get; set; }
    public SimpleTestEntity AHandEEntityField { get; set; }
    public SimpleStructure AHandEStructureField { get; set; }
  }

  [HierarchyRoot]
  public class EandAH : BaseEntity, IE, IEandAH
  {
    public string EStringField { get; set; }
    public bool EBoolField { get; set; }
    public int EIntField { get; set; }
    public ActionState EEnumField { get; set; }
    public IE EInterfaceField { get; set; }
    public SimpleTestEntity EEntityField { get; set; }
    public SimpleStructure EStructureField { get; set; }
    public string AStringField { get; set; }
    public bool ABoolField { get; set; }
    public int AIntField { get; set; }
    public ActionState AEnumField { get; set; }
    public IE AInterfaceField { get; set; }
    public SimpleTestEntity AEntityField { get; set; }
    public SimpleStructure AStructureField { get; set; }
    public string HStringField { get; set; }
    public bool HBoolField { get; set; }
    public int HIntField { get; set; }
    public ActionState HEnumField { get; set; }
    public IE HInterfaceField { get; set; }
    public SimpleTestEntity HEntityField { get; set; }
    public SimpleStructure HStructureField { get; set; }
    public string AHStringField { get; set; }
    public bool AHBoolField { get; set; }
    public int AHIntField { get; set; }
    public ActionState AHEnumField { get; set; }
    public IE AHInterfaceField { get; set; }
    public SimpleTestEntity AHEntityField { get; set; }
    public SimpleStructure AHStructureField { get; set; }
    public string EandAHStringField { get; set; }
    public bool EandAHBoolField { get; set; }
    public int EandAHIntField { get; set; }
    public ActionState EandAHEnumField { get; set; }
    public IE EandAHInterfaceField { get; set; }
    public SimpleTestEntity EandAHEntityField { get; set; }
    public SimpleStructure EandAHStructureField { get; set; }
  }

  [HierarchyRoot]
  public class InterfaceFieldsContainer : BaseEntity
  {
    [Field]
    public IA AField { get; set; }

    [Field]
    public IE EField { get; set; }

    [Field]
    public IH HField { get; set; }

    [Field]
    public IAH AHField { get; set; }

    [Field]
    public IHA HAField { get; set; }

    [Field]
    public IHAE HAEField { get; set; }

    [Field]
    public IHEA HEAField { get; set; }

    [Field]
    public IAHandE AHandEField { get; set; }

    [Field]
    public IEandAH EandAHField { get; set; }
  }
  #endregion
}
