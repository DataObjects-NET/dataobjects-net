// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.13

using System;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Describes key for a particular <see cref="hierarchy"/>.
  /// </summary>
  [Serializable]
  public sealed class KeyInfo : MappingNode
  {
    private HierarchyInfo hierarchy;
    private SequenceInfo sequence;
    private object equalityIdentifier;
    private bool isFirstAmongSimilarKeys;

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
    /// Gets the key generator type.
    /// </summary>
    public Type GeneratorType { get; private set; }

    /// <summary>
    /// Gets the key generator name.
    /// </summary>
    public string GeneratorName { get; private set; }

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


    /// <summary>
    /// Updates the state.
    /// </summary>
    /// <param name="recursive">if set to <c>true</c> [recursive].</param>
    public override void UpdateState(bool recursive)
    {
      base.UpdateState(recursive);
      if (!recursive)
        return;
      if (Sequence!=null)
        Sequence.UpdateState(true);
    }
 
    
    /// <summary>
    /// Locks the instance and (possibly) all dependent objects.
    /// 
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
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
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="fields">The key fields.</param>
    /// <param name="columns">The key columns.</param>
    /// <param name="generatorType">Type of the key generator.</param>
    /// <param name="generatorName">Name of the key generator (<see langword="null"/> means unnamed).</param>
    /// <param name="tupleDescriptor">Key tuple descriptor.</param>
    /// <param name="typeIdColumnIndex">Index of the type id column.</param>
    public KeyInfo(
      ReadOnlyList<FieldInfo> fields, 
      ReadOnlyList<ColumnInfo> columns, 
      Type generatorType, 
      string generatorName,  
      TupleDescriptor tupleDescriptor, 
      int typeIdColumnIndex)
    {
      Fields = fields;
      Columns = columns;
      GeneratorType = generatorType;
      GeneratorName = generatorName;
      TupleDescriptor = tupleDescriptor;
      TypeIdColumnIndex = typeIdColumnIndex;
      ContainsForeignKeys = fields.Any(f => f.Parent!=null);
    }
  }
}