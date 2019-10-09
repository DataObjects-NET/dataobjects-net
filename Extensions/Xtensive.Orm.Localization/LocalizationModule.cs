using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Localization
{
  /// <summary>
  /// <see cref="IModule"/> implementation for localization extension
  /// </summary>
  public class LocalizationModule : IModule
  {
    /// <summary>
    /// Called when 'complex' build process is completed.
    /// </summary>
    /// <param name="domain">The built domain.</param>
    public void OnBuilt(Domain domain)
    {
      TypeLocalizationMap.Initialize(domain);
    }

    /// <summary>
    /// Called when the build of <see cref="T:Xtensive.Orm.Building.Definitions.DomainModelDef"/> is completed.
    /// </summary>
    /// <param name="context">The domain building context.</param>
    /// <param name="model">The domain model definition.</param>
    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
    }
  }
}
