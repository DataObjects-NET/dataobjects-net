// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.09

using NUnit.Framework;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Issues.Issue0096_NumerousSchemaExtraction_Model;
using Xtensive.Storage.Building;

namespace Xtensive.Storage.Tests.Issues.Issue0096_NumerousSchemaExtraction_Model
{
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  public class Ancestor : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0096_NumerousSchemaExtraction
  {
    public class ModelChanger : IDomainBuilder
    {
      public static bool IsActive { get; set; }

      public void Build(BuildingContext context, DomainModelDef model)
      {
        if (!IsActive)
          return;

        if (!model.Types.Contains("Ancestor"))
          return;

        var type = model.Types["Ancestor"];
        var newField = new FieldDef(typeof (int));
        newField.Name = "NewField";
        type.Fields.Add(newField);
      }
    }

    protected void BuildDomain(DomainUpgradeMode mode)
    {
      var config = DomainConfigurationFactory.Create("mssql2005");
      config.UpgradeMode = mode;
      config.Types.Register(typeof (Ancestor).Assembly, typeof (Ancestor).Namespace);
      config.Builders.Add(typeof (ModelChanger));
      var domain = Domain.Build(config);

      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          new Ancestor();
          t.Complete();
        }
      }

    }

    [Test]
    public void MainTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate);

      BuildDomain(DomainUpgradeMode.Validate);

      ModelChanger.IsActive = true;

      BuildDomain(DomainUpgradeMode.Perform);
    }
  }
}