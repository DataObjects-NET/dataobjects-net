// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Attributes
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class HierarchyRootAttribute : StorageAttribute
  {
    internal int? keyGeneratorCacheSize;

    /// <summary>
    /// Gets or sets the key generator.
    /// </summary>
    public Type KeyGenerator { get; set; }

    /// <summary>
    /// Gets or sets the size of the key generator cache.
    /// </summary>
    public int KeyGeneratorCacheSize
    {
      get { return keyGeneratorCacheSize.HasValue ? keyGeneratorCacheSize.Value : 0; }
      set { keyGeneratorCacheSize = value; }
    }

    /// <summary>
    /// Key fields that are included into the primary index.
    /// </summary>
    public string[] KeyFields { get; set; }

    /// <summary>
    /// Gets the inheritance schema for this hierarchy.
    /// </summary>
    public InheritanceSchema InheritanceSchema { get; set; }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyGenerator">The key provider.</param>
    /// <param name="keyField">The key field.</param>
    public HierarchyRootAttribute(Type keyGenerator, string keyField)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyGenerator, "keyGenerator");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(keyField, "keyField");
      KeyGenerator = keyGenerator;
      KeyFields = new[] {keyField};
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyField">The key field.</param>
    /// <param name="keyFields">The key fields.</param>
    public HierarchyRootAttribute(string keyField, params string[] keyFields)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyField, "keyField");
      if (keyFields==null || keyFields.Length==0) {
        KeyFields = new[] {keyField};
      }
      else {
        KeyFields = new string[keyFields.Length + 1];
        KeyFields[0] = keyField;
        Array.Copy(keyFields, 0, KeyFields, 1, keyFields.Length);
      }
    }
  }
}