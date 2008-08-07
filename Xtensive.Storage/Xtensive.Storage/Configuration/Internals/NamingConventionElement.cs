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
    [ConfigurationProperty("LetterCasePolicy", IsRequired = false, IsKey = false)]
    public LetterCasePolicy LetterCasePolicy
    {
      get { return (LetterCasePolicy)this["LetterCasePolicy"]; }
      set { this["LetterCasePolicy"] = value; }
    }

    [ConfigurationProperty("NamespacePolicy", IsRequired = false, IsKey = false)]
    public NamespacePolicy NamespacePolicy
    {
      get { return (NamespacePolicy)this["NamespacePolicy"]; }
      set { this["NamespacePolicy"] = value; }
    }

    [ConfigurationProperty("NamingRules", IsRequired = false, IsKey = false)]
    public NamingRules NamingRules
    {
      get { return (NamingRules)this["NamingRules"]; }
      set { this["NamingRules"] = value; }
    }

    [ConfigurationProperty("NamespaceSynonyms", IsRequired = false, IsKey = false)]
    public ConfigurationCollection<NamespaceSynonymElement> NamespaceSynonyms
    {
      get { return (ConfigurationCollection<NamespaceSynonymElement>)this["NamespaceSynonyms"]; }
    }

    
    public static implicit operator NamingConvention(NamingConventionElement namingConventionElement)
    {
      var result = new NamingConvention();
      if (namingConventionElement != null) {
        result.LetterCasePolicy = namingConventionElement.LetterCasePolicy;
        result.NamespacePolicy = namingConventionElement.NamespacePolicy;
        result.NamingRules = namingConventionElement.NamingRules;
        foreach (NamespaceSynonymElement namespaceSynonym in namingConventionElement.NamespaceSynonyms) {
          result.NamespaceSynonyms.Add(namespaceSynonym.Namespace, namespaceSynonym.Synonym);
        }
      }
      return result;
    }
  }
}