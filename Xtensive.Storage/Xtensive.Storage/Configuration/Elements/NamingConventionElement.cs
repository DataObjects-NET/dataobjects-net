// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.Configuration;

namespace Xtensive.Storage.Configuration.Elements
{
  /// <summary>
  /// <see cref="NamingConvention"/> configuration element within a configuration file.
  /// </summary>
  public class NamingConventionElement : ConfigurationElement
  {
    private const string LetterCasePolicyElementName = "letterCasePolicy";
    private const string NamespacePolicyElementName = "namespacePolicy";
    private const string NamingRulesElementName = "namingRules";
    private const string NamespaceSynonymsElementName = "namespaceSynonyms";

    /// <summary>
    /// <see cref="NamingConvention.LetterCasePolicy" copy="true"/>
    /// </summary>
    [ConfigurationProperty(LetterCasePolicyElementName, IsRequired = false, IsKey = false)]
    public LetterCasePolicy LetterCasePolicy
    {
      get { return (LetterCasePolicy) this[LetterCasePolicyElementName]; }
      set { this[LetterCasePolicyElementName] = value; }
    }

    /// <summary>
    /// <see cref="NamingConvention.NamespacePolicy" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NamespacePolicyElementName, IsRequired = false, IsKey = false)]
    public NamespacePolicy NamespacePolicy
    {
      get { return (NamespacePolicy) this[NamespacePolicyElementName]; }
      set { this[NamespacePolicyElementName] = value; }
    }

    /// <summary>
    /// <see cref="NamingConvention.NamingRules" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NamingRulesElementName, IsRequired = false, IsKey = false)]
    public NamingRules NamingRules
    {
      get { return (NamingRules) this[NamingRulesElementName]; }
      set { this[NamingRulesElementName] = value; }
    }

    /// <summary>
    /// <see cref="NamingConvention.NamespaceSynonyms" copy="true"/>
    /// </summary>
    [ConfigurationProperty(NamespaceSynonymsElementName, IsRequired = false, IsKey = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<NamespaceSynonymElement>), AddItemName = "synonym")]
    public ConfigurationCollection<NamespaceSynonymElement> NamespaceSynonyms
    {
      get { return (ConfigurationCollection<NamespaceSynonymElement>) this[NamespaceSynonymsElementName]; }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="NamingConvention"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public NamingConvention ToNative()
    {
      var result = new NamingConvention{
          LetterCasePolicy = LetterCasePolicy,
          NamespacePolicy = NamespacePolicy,
          NamingRules = NamingRules
        };
      foreach (var namespaceSynonym in NamespaceSynonyms)
        result.NamespaceSynonyms.Add(namespaceSynonym.Namespace, namespaceSynonym.Synonym);
      return result;
    }
  }
}