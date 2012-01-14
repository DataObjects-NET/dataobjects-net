// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.04

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// A set of rules for model definition objects naming.
  /// </summary>
  [Serializable]
  public class NamingConvention : LockableBase,
    ICloneable,
    IEquatable<NamingConvention>
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
        namingRules = value;
      }
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
      NamingConvention result = new NamingConvention();
      result.letterCasePolicy = letterCasePolicy;
      result.namespacePolicy = namespacePolicy;
      result.namingRules = namingRules;
      result.namespaceSynonyms = new Dictionary<string, string>(namespaceSynonyms);
      return result;
    }

    #endregion

    #region Equals, GetHashCode methods

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type. 
    /// </summary>
    /// <param name="other">The object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the other parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(NamingConvention other)
    {
      if (other==null)
        return false;
      if (letterCasePolicy!=other.letterCasePolicy)
        return false;
      if (namespacePolicy!=other.namespacePolicy)
        return false;
      if (namingRules!=other.namingRules)
        return false;
      if (!namespaceSynonyms.EqualsTo(other.namespaceSynonyms))
        return false;

      return true;
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type. 
    /// </summary>
    /// <param name="obj">The object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the other parameter; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as NamingConvention);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
      int result = letterCasePolicy.GetHashCode();
      result = 29 * result + namespacePolicy.GetHashCode();
      result = 29 * result + namingRules.GetHashCode();
      return result;
    }

    #endregion
  }
}