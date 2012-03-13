// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.13

using System;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Describes key for a particular <see cref="hierarchy"/>.
  /// </summary>
  [Serializable]
  public sealed class KeyInfo : Node
  {
    private HierarchyInfo hierarchy;
    private SequenceInfo sequence;
    private object equalityIdentifier;
    private bool isFirstAmongSimilarKeys;
    private string generatorName;
    private string generatorBaseName;
    private KeyGeneratorKind generatorKind;
    private Type singleColumnType;

    /// <summary>
    /// Gets single column type if this <see cref="KeyInfo"/>
    /// has single column (excluding possible TypeId column).
    /// If this <see cref="KeyInfo"/> has multiple columns
    /// returns <see langword="null"/>.
    /// </summary>
    public Type SingleColumnType { get { return singleColumnType; } }

    /// <summary>
    /// Gets the hierarchy this key belongs to.
    /// </summary>
    public HierarchyInfo Hierarchy
    {
      get { return hierarchy; }
      set {
        this.EnsureNotLocked();
        hierarchy = value;
      }
    }

    /// <summary>
    /// Gets the fields forming the key.
    /// </summary>
    public ReadOnlyList<FieldInfo> Fields { get; private set; }

    /// <summary>
    /// Gets the columns forming the key.
    /// </summary>
    public ReadOnlyList<ColumnInfo> Columns { get; private set; }

    /// <summary>
    /// Gets the key generator name.
    /// This name is used as service name in IoC.
    /// </summary>
    public string GeneratorName
    {
      get { return generatorName; }
      set
      {
        this.EnsureNotLocked();
        generatorName = value;
      }
    }

    /// <summary>
    /// Gets generator base name.
    /// This name don't include database suffix
    /// and is used to build physical table/sequence name.
    /// </summary>
    public string GeneratorBaseName
    {
      get { return generatorBaseName; }
      set
      {
        this.EnsureNotLocked();
        generatorBaseName = value;
      }
    }

    /// <summary>
    /// Gets <see cref="KeyGeneratorKind"/> for this <see cref="KeyInfo"/>.
    /// </summary>
    public KeyGeneratorKind GeneratorKind
    {
      get { return generatorKind; }
      set
      {
        this.EnsureNotLocked();
        generatorKind = value;
      }
    }


    /// <summary>
    /// Gets the tuple descriptor of the key.
    /// </summary>
    /// <value></value>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets the index of the column related to field with <see cref="FieldInfo.IsTypeId"/>==<see langword="true" />.
    /// If there is no such field, returns <see langword="-1" />.
    /// </summary>
    public int TypeIdColumnIndex { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether key contains foreign keys.
    /// </summary>
    public bool ContainsForeignKeys { get; private set; }

    /// <summary>
    /// Gets the information on associated sequence.
    /// </summary>
    public SequenceInfo Sequence {
      get { return sequence; }
      set {
        this.EnsureNotLocked();
        sequence = value;
      }
    }

    /// <summary>
    /// Gets the value indicating this key is the first one built among similar keys.
    /// All similar keys share the same <see cref="EqualityIdentifier"/> value.
    /// </summary>
    public bool IsFirstAmongSimilarKeys
    {
      get { return isFirstAmongSimilarKeys; }
      set {
        this.EnsureNotLocked();
        isFirstAmongSimilarKeys = value;
      }
    }

    /// <summary>
    /// Gets the equality identifier for this key.
    /// <see cref="EqualityIdentifier"/> is used as an additional value to compare
    /// when actual keys are compared for equality. 
    /// So two keys are equal when their fields are equal and 
    /// they share the same <see cref="EqualityIdentifier"/> value.
    /// </summary>
    public object EqualityIdentifier
    {
      get { return equalityIdentifier; }
      set {
        this.EnsureNotLocked();
        equalityIdentifier = value;
      }
    }

    /// <inheritdoc/>
    public override void UpdateState(bool recursive)
    {
      base.UpdateState(recursive);
      if (!recursive)
        return;
      if (Sequence!=null)
        Sequence.UpdateState(true);
    }
 
    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Hierarchy must be set before locking this instance.</exception>
    public override void Lock(bool recursive)
    {
      if (Hierarchy==null)
        throw Exceptions.NotInitialized("Hierarchy");
      if (EqualityIdentifier==null)
        throw Exceptions.NotInitialized("EqualityIdentifier");
      base.Lock(recursive);
      if (!recursive)
        return;
      // Hierarchy.Lock() is not necessary, because it isn't a contained object (= likely, already locked)
      if (Sequence!=null)
        Sequence.Lock(true);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="fields">The key fields.</param>
    /// <param name="columns">The key columns.</param>
    /// <param name="tupleDescriptor">Key tuple descriptor.</param>
    /// <param name="typeIdColumnIndex">Index of the type id column.</param>
    public KeyInfo(string name, ReadOnlyList<FieldInfo> fields, ReadOnlyList<ColumnInfo> columns,
      TupleDescriptor tupleDescriptor, int typeIdColumnIndex)
      : base(name)
    {
      Fields = fields;
      Columns = columns;

      TupleDescriptor = tupleDescriptor;
      TypeIdColumnIndex = typeIdColumnIndex;

      ContainsForeignKeys = fields.Any(f => f.Parent!=null);

      if (Columns.Where((c, i) => i!=TypeIdColumnIndex).Count()==1)
        singleColumnType = Columns.Where((c, i) => i!=TypeIdColumnIndex).First().ValueType;
    }
  }
}