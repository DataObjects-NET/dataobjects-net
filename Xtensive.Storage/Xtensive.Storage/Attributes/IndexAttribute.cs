// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Defines secondary index.
  /// </summary>
  /// <example>
  ///   <code lang="cs" source="..\Xtensive.Storage.Manual\IndexAttributeSample.cs" region="Sample" />
  /// </example>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
  public sealed class IndexAttribute : MappingAttribute
  {
    private string[] keyFields;
    private string[] includedFields;
    internal double? fillFactor;
    internal bool? isUnique;

    /// <summary>
    /// Key fields that compose the index.
    /// </summary>
    public string[] KeyFields
    {
      get { return keyFields; }
      set { keyFields = value; }
    }

    /// <summary>
    /// Non key fields that are included into the index.
    /// </summary>
    public string[] IncludedFields
    {
      get { return includedFields; }
      set { includedFields = value; }
    }

    /// <summary>
    /// Fill factor for this index, must be a real number between 
    /// <see langword="0"/> and <see langword="1"/>.
    /// </summary>
    public double FillFactor
    {
      get { return fillFactor.HasValue ? fillFactor.Value : 0; }
      set { fillFactor = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the index is unique.
    /// </summary>
    public bool IsUnique
    {
      get { return isUnique.HasValue ? isUnique.Value : false; }
      set { isUnique = value; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyField">The first key field.</param>
    /// <param name="keyFields">The other (optional) key fields.</param>
    public IndexAttribute(string keyField, params string[] keyFields)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyField, "keyField");
      if (keyFields == null || keyFields.Length == 0)
        this.keyFields = new string[] { keyField };

      this.keyFields = new string[keyFields.Length+1];
      this.keyFields[0] = keyField;
      Array.Copy(keyFields, 0, this.keyFields, 1, keyFields.Length);
    }
  }
}
