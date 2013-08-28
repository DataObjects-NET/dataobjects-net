// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Andrey Turkov
// Created:    2013.08.21

using System;
using System.Linq;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Metadata;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Upgrade.Model;
using Type = Xtensive.Orm.Metadata.Type;

namespace Xtensive.Orm.Building
{
  public sealed class SystemModule : IModule
  {
    /// <inheritdoc/>
    public void OnBuilt(Domain domain)
    {
    }

    /// <inheritdoc/>
    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (context.BuilderConfiguration.Stage==UpgradeStage.Upgrading &&
        context.BuilderConfiguration.RecycledDefinitions!=null) {
        var recycledFieldDefinitions =
          context.BuilderConfiguration.RecycledDefinitions.OfType<RecycledFieldDefinition>();
        foreach (var recycledFieldDefinition in recycledFieldDefinitions) {
          var entity = model.Types.TryGetValue(recycledFieldDefinition.OwnerType);
          if (entity==null)
            throw new ArgumentException(
              string.Format(
                Strings.ExUnableToProcessRecycledFieldDefinitionXOwnerTypeIsNotRegisteredInModel,
                recycledFieldDefinition.OwnerType));
          entity.DefineField(recycledFieldDefinition.FieldName, recycledFieldDefinition.FieldType);
        }
      }
      if (context.Configuration.ConnectionInfo.Provider!=WellKnown.Provider.MySql)
        return;

      BuildLog.Info("Applying changes to Metadata-related types for MySQL");

      // Fixing length of Assembly.Name field
      TypeDef type = model.Types.TryGetValue(typeof (Assembly));
      FieldDef field;
      if (type!=null && type.Fields.TryGetValue("Name", out field))
        field.Length = 255;

      // Fixing length of Extension.Name field
      type = model.Types.TryGetValue(typeof (Extension));
      if (type!=null && type.Fields.TryGetValue("Name", out field))
        field.Length = 255;

      // Removing index on Type.Name field
      type = model.Types.TryGetValue(typeof (Type));
      if (type!=null && type.Indexes.Count > 0) {
        var indexes = type.Indexes.Where(i => i.KeyFields.ContainsKey("Name")).ToList();
        foreach (var index in indexes)
          type.Indexes.Remove(index);
      }
    }
  }
}
