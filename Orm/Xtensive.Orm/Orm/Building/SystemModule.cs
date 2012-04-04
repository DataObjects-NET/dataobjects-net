﻿using System.Linq;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Metadata;

namespace Xtensive.Orm.Building
{
  public sealed class SystemModule : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
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
