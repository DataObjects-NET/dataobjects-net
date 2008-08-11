// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;

namespace Xtensive.Storage.Configuration
{
  internal class NamingConventionElement : ConfigurationElement
  {
    private const string LetterCasePolicyElementName = "letterCasePolicy";
    private const string NamespacePolicyElementName = "namespacePolicy";
    private const string NamingRulesElementName = "namingRules";
    private const string NamespaceSynonymsElementName = "namespaceSynonyms";

    [ConfigurationProperty(LetterCasePolicyElementName, IsRequired = false, IsKey = false)]
    public LetterCasePolicy LetterCasePolicy
    {
      get { return (LetterCasePolicy) this[LetterCasePolicyElementName]; }
      set { this[LetterCasePolicyElementName] = value; }
    }

    [ConfigurationProperty(NamespacePolicyElementName, IsRequired = false, IsKey = false)]
    public NamespacePolicy NamespacePolicy
    {
      get { return (NamespacePolicy) this[NamespacePolicyElementName]; }
      set { this[NamespacePolicyElementName] = value; }
    }

    [ConfigurationProperty(NamingRulesElementName, IsRequired = false, IsKey = false)]
    public NamingRules NamingRules
    {
      get { return (NamingRules) this[NamingRulesElementName]; }
      set { this[NamingRulesElementName] = value; }
    }

    [ConfigurationProperty(NamespaceSynonymsElementName, IsRequired = false, IsKey = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<NamespaceSynonymElement>), AddItemName = "synonym")]
    public ConfigurationCollection<NamespaceSynonymElement> NamespaceSynonyms
    {
      get { return (ConfigurationCollection<NamespaceSynonymElement>) this[NamespaceSynonymsElementName]; }
    }

    public NamingConvention AsNamingConvention()
    {
      var result = new NamingConvention{
          LetterCasePolicy = LetterCasePolicy,
          NamespacePolicy = NamespacePolicy,
          NamingRules = NamingRules
        };
      foreach (NamespaceSynonymElement namespaceSynonym in NamespaceSynonyms) {
        result.NamespaceSynonyms.Add(namespaceSynonym.Namespace, namespaceSynonym.Synonym);
      }
      return result;
    }
  }
}