// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class NamespaceSynonymOptions
  {
    /// <summary>
    /// Namespace to synonymize.
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// Synonym of namespace.
    /// </summary>
    public string Synonym { get; set; }
  }
}
