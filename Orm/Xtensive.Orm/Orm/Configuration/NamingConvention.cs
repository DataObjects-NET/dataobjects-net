// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.04

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// A set of rules for model definition objects naming.
  /// </summary>
  [Serializable]
  public class NamingConvention : LockableBase,
    ICloneable
  {
    private LetterCasePolicy letterCasePolicy;
    private NamespacePolicy namespacePolicy;
    private NamingRules namingRules;
    private IDictionary<string, string> namespaceSynonyms = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the letter case policy.
    /// </summary>
    public LetterCasePolicy LetterCasePolicy
    {
      get { return letterCasePolicy; }
      set
      {
        this.EnsureNotLocked();
        letterCasePolicy = value;
      }
    }

    /// <summary>
    /// Gets or sets the namespace policy.
    /// </summary>
    public NamespacePolicy NamespacePolicy
    {
      get { return namespacePolicy; }
      set
      {
        this.EnsureNotLocked();
        namespacePolicy = value;
      }
    }

    /// <summary>
    /// Gets or sets the naming rules.
    /// </summary>
    public NamingRules NamingRules
    {
      get { return namingRules; }
      set
      {
        this.EnsureNotLocked();
        Validate(value, NamingRules.UnderscoreDots, NamingRules.RemoveDots);
        Validate(value, NamingRules.UnderscoreHyphens, NamingRules.RemoveHyphens);
        namingRules = value;
      }
    }

    private void Validate(NamingRules value, NamingRules option1, NamingRules option2)
    {
      if ((value & option1)==option1 && (value & option2)==option2)
        throw new ArgumentException(string.Format(Strings.ExOptionXIsMutuallyExclusiveWithOptionY, option1, option2));
    }

    /// <summary>
    /// Gets namespace synonyms dictionary where key is a namespace name and value is a synonym.
    /// </summary>
    public IDictionary<string, string> NamespaceSynonyms
    {
      get { return namespaceSynonyms; }
    }

    #region ILockable methods

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      namespaceSynonyms = new ReadOnlyDictionary<string, string>(namespaceSynonyms);
    }

    #endregion

    #region ICloneable members

    /// <inheritdoc/>
    public object Clone()
    {
      this.EnsureNotLocked();
      var result = new NamingConvention();
      result.letterCasePolicy = letterCasePolicy;
      result.namespacePolicy = namespacePolicy;
      result.namingRules = namingRules;
      result.namespaceSynonyms = new Dictionary<string, string>(namespaceSynonyms);
      return result;
    }

    #endregion
  }
}