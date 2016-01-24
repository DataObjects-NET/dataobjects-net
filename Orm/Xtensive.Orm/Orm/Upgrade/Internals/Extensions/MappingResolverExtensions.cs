using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Upgrade.Internals.Extensions
{
  internal static class MappingResolverExtensions
  {
    public static string GetTableName(this MappingResolver resolver, StoredTypeInfo type)
    {
      return resolver.GetNodeName(
        type.MappingDatabase, type.MappingSchema, type.MappingName);
    }

    private static string GetTablePath(this MappingResolver resolver,StoredTypeInfo type)
    {
      var nodeName = resolver.GetTableName(type);
      return string.Format("Tables/{0}", nodeName);
    }
  }
}