using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Tests.Issues.IssueJira0760_OverrideFieldNameAttributeRuinsFieldMappingOnUpgradeModel;
using Xtensive.Orm.Upgrade;


namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0760_OverrideFieldNameAttributeRuinsFieldMappingOnUpgrade
  {
    [Test]
    public void IssueImplementationTest()
    {
      var initialConfiguration = DomainConfigurationFactory.Create();
      initialConfiguration.Types.Register(typeof (EntityWithState));
      initialConfiguration.Types.Register(typeof (FieldNamesModifier));
      initialConfiguration.Types.Register(typeof (CustomUpgradeHandler));
      initialConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;

      Domain domain = null;
      using (domain = Domain.Build(initialConfiguration)) {
        var metadata = domain.Extensions.Get<MetadataSet>();
        var savedModel = StoredDomainModel.Deserialize(metadata.Extensions.First().Value);
        var type = savedModel.Types.First(t => t.Name=="EntityWithState");
        var stateField = type.Fields.First(f => f.PropertyName=="State");

        Assert.That(stateField.Name, Is.Not.EqualTo(stateField.PropertyName));
        Assert.That(stateField.MappingName, Is.Not.EqualTo(stateField.PropertyName));
        Assert.That(stateField.OriginalName, Is.Not.EqualTo(stateField.PropertyName));

        Assert.That(stateField.Name, Is.EqualTo("EntityWithState.State"));
        Assert.That(stateField.MappingName, Is.EqualTo(stateField.Name));
        Assert.That(stateField.OriginalName, Is.EqualTo(stateField.Name));
      }

      var upgradeConfiguration = DomainConfigurationFactory.Create();
      upgradeConfiguration.Types.Register(typeof (EntityWithState));
      upgradeConfiguration.Types.Register(typeof (CustomUpgradeHandler));
      upgradeConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;

      Assert.DoesNotThrow(() => domain = Domain.Build(upgradeConfiguration));

      using (domain) {
        var metadata = domain.Extensions.Get<MetadataSet>();
        var savedModel = StoredDomainModel.Deserialize(metadata.Extensions.First().Value);
        var type = savedModel.Types.First(t => t.Name=="EntityWithState");
        var stateField = type.Fields.First(f => f.PropertyName=="State");

        Assert.That(stateField.Name, Is.EqualTo(stateField.PropertyName));
        Assert.That(stateField.MappingName, Is.EqualTo(stateField.PropertyName));
        Assert.That(stateField.OriginalName, Is.EqualTo(stateField.PropertyName));
      }
    }

    [Test]
    public void MappingNameTest()
    {
      var initialConfiguration = DomainConfigurationFactory.Create();
      initialConfiguration.Types.Register(typeof (EntityWithState));
      initialConfiguration.Types.Register(typeof (FieldNamesModifier));
      initialConfiguration.Types.Register(typeof (CustomUpgradeHandler));
      initialConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;

      Domain domain = null;
      using (domain = Domain.Build(initialConfiguration)) {
        var metadata = domain.Extensions.Get<MetadataSet>();
        var savedModel = StoredDomainModel.Deserialize(metadata.Extensions.First().Value);
        var type = savedModel.Types.First(t => t.Name=="EntityWithState");
        var stateField = type.Fields.First(f => f.PropertyName=="State");

        Assert.That(stateField.Name, Is.Not.EqualTo(stateField.PropertyName));
        Assert.That(stateField.MappingName, Is.Not.EqualTo(stateField.PropertyName));
        Assert.That(stateField.OriginalName, Is.Not.EqualTo(stateField.PropertyName));

        Assert.That(stateField.Name, Is.EqualTo("EntityWithState.State"));
        Assert.That(stateField.MappingName, Is.EqualTo(stateField.Name));
        Assert.That(stateField.OriginalName, Is.EqualTo(stateField.Name));
      }

      var upgradeConfiguration = DomainConfigurationFactory.Create();
      upgradeConfiguration.Types.Register(typeof (EntityWithState));
      upgradeConfiguration.Types.Register(typeof (MappingNameModifier));
      upgradeConfiguration.Types.Register(typeof (CustomUpgradeHandler));
      upgradeConfiguration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      Assert.DoesNotThrow(() => domain = Domain.Build(upgradeConfiguration));

      using (domain) {
        var metadata = domain.Extensions.Get<MetadataSet>();
        var savedModel = StoredDomainModel.Deserialize(metadata.Extensions.First().Value);
        var type = savedModel.Types.First(t => t.Name=="EntityWithState");
        var stateField = type.Fields.First(f => f.PropertyName=="State");

        Assert.That(stateField.Name, Is.EqualTo(stateField.PropertyName));
        Assert.That(stateField.OriginalName, Is.EqualTo(stateField.PropertyName));

        Assert.That(stateField.MappingName, Is.EqualTo("EntityWithState.State"));
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0760_OverrideFieldNameAttributeRuinsFieldMappingOnUpgradeModel
{
  [HierarchyRoot]
  public class EntityWithState : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public SomeEntityState State { get; set; }

  }


  public enum SomeEntityState
  {
    On,
    Off,
    Suspended,
  }

  public class FieldNamesModifier : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      var stateField = model.Types[typeof (EntityWithState)].Fields["State"];

      stateField.Name = "EntityWithState.State";
      stateField.MappingName = "EntityWithState.State";
    }
  }

  public class MappingNameModifier : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      var stateField = model.Types[typeof(EntityWithState)].Fields["State"];

      stateField.MappingName = "EntityWithState.State";
    }
  }

  public class CustomUpgradeHandler : UpgradeHandler
  {
    public override void OnComplete(Domain domain)
    {
      domain.Extensions.Set(UpgradeContext.Metadata);
    }
  }
}
