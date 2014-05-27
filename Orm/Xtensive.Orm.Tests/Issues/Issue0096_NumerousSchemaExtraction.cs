// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.09

using System;
using NUnit.Framework;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.Issue0096_NumerousSchemaExtraction_Model;
using Xtensive.Orm.Building;

namespace Xtensive.Orm.Tests.Issues.Issue0096_NumerousSchemaExtraction_Model
{
  [Serializable]
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  public class Ancestor : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  [Explicit("Manual usage only.")]
  public class Issue0096_NumerousSchemaExtraction
  {
    public class ModelChanger : IModule
    {
      public static bool IsActive { get; set; }

      public void OnBuilt(Domain domain)
      {
      }

      public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
      {
        if (!IsActive)
          return;

        if (!model.Types.Contains("Ancestor"))
          return;

        var type = model.Types["Ancestor"];
        var newField = new FieldDef(typeof (int), context.Validator);
        newField.Name = "NewField";
        type.Fields.Add(newField);
      }
    }

    private void BuildDomain(DomainUpgradeMode mode)
    {
      var config = DomainConfigurationFactory.Create();
      config.UpgradeMode = mode;
      config.Types.Register(typeof (Ancestor).Assembly, typeof (Ancestor).Namespace);
      var domain = Domain.Build(config);

      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new Ancestor();
          t.Complete();
        }
      }

    }

    [Test]
    public void MainTest()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
      BuildDomain(DomainUpgradeMode.Recreate);
      BuildDomain(DomainUpgradeMode.Validate);
      ModelChanger.IsActive = true;
      BuildDomain(DomainUpgradeMode.Perform);
      ModelChanger.IsActive = false;
    }
  }
}