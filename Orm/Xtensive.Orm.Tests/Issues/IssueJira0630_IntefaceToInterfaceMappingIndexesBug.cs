using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0630_IntefaceToInterfaceMappingIndexesBugModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0630_IntefaceToInterfaceMappingIndexesBugModel
{
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
    bool IgnoreLockedForOutput { get; set; }

    [Field]
    bool IgnoreContentType { get; set; }

    [Field]
    bool IgnoreMaxWeight { get; set; }

    [Field]
    bool IgnoreMaxHeight { get; set; }

    [Field]
    bool IgnoreLockedInBeam { get; set; }
  }

  public interface IStoredMaterialCreator : IStoredObjectCreator
  {
  }

  public enum ActionState
  {
    State1,
    State2,
    State3
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

    public bool IgnoreLockedForOutput { get; set; }

    public bool IgnoreContentType { get; set; }

    public bool IgnoreMaxWeight { get; set; }

    public bool IgnoreMaxHeight { get; set; }

    public bool IgnoreLockedInBeam { get; set; }

    public string IntermediateInterfaceText { get; set; }

    public Size Size { get; set; }

    public ActionState ActionState { get; set; }
  }

  [HierarchyRoot]
  public class ComplexStoredMaterialCreator : MesObject, IStoredMaterialCreator
  {
    public bool IgnoreLockedForInput { get; set; }

    public bool IgnoreLockedForOutput { get; set; }

    public bool IgnoreContentType { get; set; }

    public bool IgnoreMaxWeight { get; set; }

    public bool IgnoreMaxHeight { get; set; }

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
}

namespace Xtensive.Orm.Tests.Issues
{
  public sealed class IssueJira0630_IntefaceToInterfaceMappingIndexesBug : AutoBuildTest
  {
    [Test]
    public void SelectInterfaceFieldTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<StoredMaterialLot>().Select(el => el.Creator).ToArray();
      }
    }

    [Test]
    public void SelectInterfaceDirectlyTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var iActions = session.Query.All<IAction>().ToList();
        var iSomeIntermediateInterfaceItems = session.Query.All<ISomeIntermediateInterface>().ToList();
        var iStoredObjectCreators = session.Query.All<IStoredObjectCreator>().ToList();
        var directInterfaceQuery = session.Query.All<IStoredMaterialCreator>().ToList();
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var simpleStoredMaterialCreator = new SimpleStoredMaterialCreator {
          ActionState = ActionState.State1,
          IgnoreContentType = true,
          IgnoreLockedForInput = true,
          IgnoreLockedForOutput = true,
          IgnoreLockedInBeam = true,
          IgnoreMaxHeight = true,
          IgnoreMaxWeight = true,
          IntermediateInterfaceText = "SimpleStoredMaterialCreator:IntemediateText",
        };
        var complexStoredMaterialCreator = new ComplexStoredMaterialCreator {
          ActionState = ActionState.State1,
          IgnoreContentType = false,
          IgnoreLockedForInput = false,
          IgnoreLockedForOutput = false,
          IgnoreLockedInBeam = false,
          IgnoreMaxHeight = false,
          IgnoreMaxWeight = false,
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
