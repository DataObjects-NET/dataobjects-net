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
    private InheritanceSchema inheritanceSchema;
    private string[] keyFields;
    private Type generator;

    /// <summary>
    /// Key fields that are included into the index.
    /// </summary>
    public string[] KeyFields
    {
      get { return keyFields; }
      set { keyFields = value; }
    }

    /// <summary>
    /// Gets the data placement policy.
    /// </summary>
    public InheritanceSchema InheritanceSchema
    {
      get { return inheritanceSchema; }
      set { inheritanceSchema = value; }
    }

    /// <summary>
    /// Gets or sets the key provider.
    /// </summary>
    public Type Generator
    {
      get { return generator; }
      set { generator = value; }
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyProvider">The key provider.</param>
    public HierarchyRootAttribute(Type keyProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyProvider, "keyProvider");
      this.generator = keyProvider;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyProvider">The key provider.</param>
    /// <param name="keyField">The key field.</param>
    public HierarchyRootAttribute(Type keyProvider, string keyField) : this(keyProvider)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(keyField, "keyField");
      keyFields = new string[1] { keyField };
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyProvider">The key provider.</param>
    /// <param name="keyField">The key field.</param>
    /// <param name="keyFields">The key fields.</param>
    public HierarchyRootAttribute(Type keyProvider, string keyField, params string[] keyFields) : this(keyProvider)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyField, "keyField");
      if (keyFields == null || keyFields.Length == 0) {
        this.keyFields = new string[] { keyField };
      }
      else {
        this.keyFields = new string[keyFields.Length + 1];
        this.keyFields[0] = keyField;
        Array.Copy(keyFields, 0, this.keyFields, 1, keyFields.Length);
      }
    }
  }
}
