using System.Linq;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Metadata;

namespace Xtensive.Orm.Building
{
  /// <summary>
  /// System implementation of <see cref="IModule"/>.
  /// </summary>
  public sealed class SystemModule : IModule
  {
    /// <summary>
    /// Called when 'complex' build process is completed.
    /// </summary>
    /// <param name="domain">The built domain.</param>
    public void OnBuilt(Domain domain)
    {
    }

    /// <summary>
    /// Called when the build of <see cref="DomainModelDef"/> is completed.
    /// </summary>
    /// <param name="context">The domain building context.</param>
    /// <param name="model">The domain model definition.</param>
    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (context.Configuration.ConnectionInfo.Provider!=WellKnown.Provider.MySql)
        return;

      Log.Info("Applying changes to Metadata-related types for MySQL");

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
