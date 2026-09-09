// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Text.Json;


namespace Xtensive.Orm.Operations.Serialization.Json
{
  /// <summary>
  /// Produces most used <see cref="JsonException"/>s
  /// </summary>
  internal static class JsonExceptions
  {
    private const string WrongStructureExceptionTemplate = "Wrong Structure: {0}.";

    public static JsonException WrongStructure(string details)
      => new JsonException(string.Format(WrongStructureExceptionTemplate, details));

    public static JsonException WrongStructurePropertyExpected()
      => WrongStructure("Property expected");

    public static JsonException WrongStructurePropertyNameExpected(string expected, string actual)
      => WrongStructure($"Property '{expected}' expected, but was '{actual}'");

    public static JsonException NoTypeForName(string assemblyQualifiedName)
      => new JsonException($"Can't resolve '{assemblyQualifiedName}' type name into System.Type instance.");
  }
}
