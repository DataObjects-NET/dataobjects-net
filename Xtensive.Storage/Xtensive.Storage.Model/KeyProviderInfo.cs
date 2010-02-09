// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.13

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Model
{
  [Serializable]
  public sealed class KeyProviderInfo : MappingNode
  {
    private SequenceInfo sequenceInfo;

    /// <summary>
    /// Gets the key generator type.
    /// </summary>
    public Type KeyGeneratorType { get; private set; }

    /// <summary>
    /// Gets the key generator name.
    /// </summary>
    public string KeyGeneratorName { get; private set; }

    /// <summary>
    /// Gets the tuple descriptor of the key.
    /// </summary>
    /// <value></value>
    public TupleDescriptor KeyTupleDescriptor { get; private set; }

    /// <summary>
    /// Gets the index of the column related to field with <see cref="FieldInfo.IsTypeId"/>==<see langword="true" />.
    /// If there is no such field, returns <see langword="-1" />.
    /// </summary>
    public int TypeIdColumnIndex { get; private set; }

    /// <summary>
    /// Gets the information on associated sequence.
    /// </summary>
    public SequenceInfo SequenceInfo {
      get { return sequenceInfo; }
      set {
        this.EnsureNotLocked();
        sequenceInfo = value;
      }
    }

    /// <inheritdoc/>
    public override void UpdateState(bool recursive)
    {
      base.UpdateState(recursive);
      if (!recursive)
        return;
      if (SequenceInfo!=null)
        SequenceInfo.UpdateState(true);
    }
 
    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (!recursive)
        return;
      if (SequenceInfo!=null)
        SequenceInfo.Lock(true);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyGeneratorType">Type of the key generator.</param>
    /// <param name="keyGeneratorName">Name of the key generator (<see langword="null" /> means unnamed).</param>
    /// <param name="keyTupleDescriptor">Key tuple descriptor.</param>
    /// <param name="typeIdColumnIndex">Index of the type id column.</param>
    public KeyProviderInfo(Type keyGeneratorType, string keyGeneratorName,  TupleDescriptor keyTupleDescriptor, int typeIdColumnIndex)
    {
      KeyGeneratorType = keyGeneratorType;
      KeyGeneratorName = keyGeneratorName;
      KeyTupleDescriptor = keyTupleDescriptor;
      TypeIdColumnIndex = typeIdColumnIndex;
    }
  }
}