// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using System.Linq;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class HierarchyInfo : MappingNode
  {
    private int keyGeneratorCacheSize;

    /// <summary>
    /// Gets the root of the hierarchy.
    /// </summary>
    public TypeInfo Root { get; private set; }

    /// <summary>
    /// Gets the <see cref="InheritanceSchema"/> for this hierarchy.
    /// </summary>
    public InheritanceSchema Schema { get; private set; }

    /// <summary>
    /// Gets the fields that are included in the key.
    /// </summary>
    public DirectionCollection<FieldInfo> KeyFields { get; private set; }

    /// <summary>
    /// Gets the columns that are included in the key.
    /// </summary>
    public ColumnInfoCollection KeyColumns { get; private set; }

    /// <summary>
    /// Gets the types of the current <see cref="HierarchyInfo"/>.
    /// </summary>
    public ReadOnlyList<TypeInfo> Types { get; private set; }

    /// <summary>
    /// Gets <see cref="MappingInfo"/> for hierarchy key.
    /// </summary>
    public Segment<int> MappingInfo { get; private set; }

    /// <summary>
    /// Gets or sets the size of the generator cache.
    /// </summary>
    public int KeyGeneratorCacheSize {
      [DebuggerStepThrough]
      get { return keyGeneratorCacheSize; }
      [DebuggerStepThrough]
      set {
        this.EnsureNotLocked();
        keyGeneratorCacheSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the type instance of which is responsible for key generation.
    /// </summary>
    public Type KeyGeneratorType { get; private set; }

    /// <summary>
    /// Gets the tuple descriptor of the key.
    /// </summary>
    /// <value></value>
    public TupleDescriptor KeyTupleDescriptor { get; private set; }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      KeyTupleDescriptor = TupleDescriptor.Create(
        from c in KeyColumns select c.ValueType);
      MappingInfo = new Segment<int>(0, KeyTupleDescriptor.Count);
      var list = new List<TypeInfo> {Root};
      list.AddRange(Root.GetDescendants(true));
      Types = new ReadOnlyList<TypeInfo>(list);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="root">The hierarchy root.</param>
    /// <param name="schema">The schema.</param>
    /// <param name="keyProvider">The key provider.</param>
    public HierarchyInfo(TypeInfo root, InheritanceSchema schema, Type keyProvider)
    {
      KeyColumns = new ColumnInfoCollection();
      KeyFields = new DirectionCollection<FieldInfo>();
      this.Root = root;
      this.KeyGeneratorType = keyProvider;
      this.Schema = schema;
    }
  }
}