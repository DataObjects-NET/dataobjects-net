// Copyright (C) 2008-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.07.30

using System.Linq;

namespace Xtensive.Reflection
{
  /// <summary>
  /// Various well-known constants related to this namespace.
  /// </summary>
  public static partial class WellKnown
  {
    /// <summary>
    /// Returns ".ctor".
    /// </summary>
    public const string CtorName = ".ctor";

    /// <summary>
    /// Returns ".cctor".
    /// </summary>
    public const string CctorName = ".cctor";

    /// <summary>
    /// Returns "get_".
    /// </summary>
    public const string GetterPrefix = "get_";

    /// <summary>
    /// Returns "set_".
    /// </summary>
    public const string SetterPrefix = "set_";

    /// <summary>
    /// Returns "Item"
    /// </summary>
    public const string IndexerPropertyName = "Item";

    /// <summary>
    /// Returns "get_Item"
    /// </summary>
    public const string IndexerPropertyGetterName = "get_Item";

    /// <summary>
    /// Returns "add_".
    /// </summary>
    public const string AddEventHandlerPrefix = "add_";

    /// <summary>
    /// Returns "remove_".
    /// </summary>
    public const string RemoveEventHandlerPrefix = "remove_";

    /// <summary>
    /// Returns "System.Reflection.RuntimeMethodInfo".
    /// </summary>
    public const string RuntimeMethodInfoName = "System.Reflection.RuntimeMethodInfo";

  }
}
