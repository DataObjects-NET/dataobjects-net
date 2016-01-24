using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model.Stored;

namespace Xtensive.Orm.Upgrade.Internals
{
  internal static class StoredDomainModelExtensions
  {
    public static ClassifiedCollection<string, Pair<string, string[]>> GetGenericTypes(this StoredDomainModel model)
    {
      var genericTypes = new ClassifiedCollection<string, Pair<string, string[]>>(pair => new[] { pair.First });
      foreach (var typeInfo in model.Types.Where(type => type.IsGeneric)) {
        var typeDefinitionName = typeInfo.GenericTypeDefinition;
        genericTypes.Add(new Pair<string, string[]>(typeDefinitionName, typeInfo.GenericArguments));
      }
      return genericTypes;
    }

    public static IEnumerable<StoredTypeInfo> GetNonConnectorTypes(this StoredDomainModel model)
    {
      var connectorTypes = (
        from association in model.Associations
        let type = association.ConnectorType
        where type!=null
        select type
        ).ToHashSet();
      return model.Types.Where(type => !connectorTypes.Contains(type));
    }
  }
}