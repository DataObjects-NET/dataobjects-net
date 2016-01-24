using System;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Upgrade.Internals.Extensions
{
  internal static class DomainModelExtensions
  {
    public static ClassifiedCollection<Type, Pair<Type, Type[]>> GetGenericTypes(this DomainModel model)
    {
      var genericTypes = new ClassifiedCollection<Type, Pair<Type, Type[]>>(pair => new[] { pair.First });
      foreach (var typeInfo in model.Types.Where(type => type.UnderlyingType.IsGenericType)) {
        var typeDefinition = typeInfo.UnderlyingType.GetGenericTypeDefinition();
        genericTypes.Add(new Pair<Type, Type[]>(typeDefinition, typeInfo.UnderlyingType.GetGenericArguments()));
      }
      return genericTypes;
    }
  }
}