// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Options
{
  internal sealed class NamingConventionOptions : IToNativeConvertable<NamingConvention>
  {
    /// <summary>
    /// Letter case policy.
    /// </summary>
    public LetterCasePolicy LetterCasePolicy { get; set; } = NamingConvention.DefaultLetterCasePolicy;

    /// <summary>
    /// Namespace policy.
    /// </summary>
    public NamespacePolicy NamespacePolicy { get; set; } = NamingConvention.DefaultNamespacePolicy;

    /// <summary>
    /// Rules of naming.
    /// </summary>
    public NamingRules NamingRules { get; set; } = NamingConvention.DefaultNamingRules;

    /// <summary>
    /// Collection of namespace synonyms.
    /// </summary>
    public NamespaceSynonymOptions[] NamespaceSynonyms { get; set; }

    /// <inheritdoc />
    public NamingConvention ToNative()
    {
      var result = new NamingConvention {
        LetterCasePolicy = LetterCasePolicy,
        NamespacePolicy = NamespacePolicy,
        NamingRules = NamingRules,
      };

      foreach (var namespaceSynonym in NamespaceSynonyms) {
        if (namespaceSynonym.Namespace.IsNullOrEmpty()) {
          ArgumentValidator.EnsureArgumentNotNullOrEmpty(namespaceSynonym.Namespace, namespaceSynonym.Namespace);
        }
        if (!result.NamespaceSynonyms.TryAdd(namespaceSynonym.Namespace, namespaceSynonym.Synonym)) {
          throw new Exception($"Synonym for namespace '{namespaceSynonym.Namespace}' has already been assigned.");
        }
      }
      return result;
    }
  }
}
