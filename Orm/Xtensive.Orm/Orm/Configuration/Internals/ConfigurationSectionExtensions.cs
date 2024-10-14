// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Xtensive.Collections;

namespace Xtensive.Orm.Configuration
{
  internal static class ConfigurationSectionExtensions
  {
    public static IEnumerable<IConfigurationSection> GetSelfOrChildren(this IConfigurationSection section, bool requiredName = false)
    {
      var children = section.GetChildren().ToList();
      if (children.Count > 0) {
        if (requiredName) {
          var anyItemWithName = children.Any(i => i["name"] != null);
          if (anyItemWithName) {
            return children;
          }
        }
        var firstChild = children[0];
        var isIndexed = firstChild.Key == "0";
        if (isIndexed)
          return children;
        else
          return EnumerableUtils.One(section);
      }
      return children;
    }
  }
}
