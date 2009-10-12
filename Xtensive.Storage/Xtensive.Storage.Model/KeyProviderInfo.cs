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
    /// <summary>
    /// Gets the length of the key.
    /// </summary>
    public int Length
    {
      get { return TupleDescriptor.Count; }
    }

    /// <summary>
    /// Gets the index of the column related to field with <see cref="FieldInfo.IsTypeId"/>==<see langword="true" />.
    /// If there is no such field, returns <see langword="-1" />.
    /// </summary>
    public int TypeIdColumnIndex { get; private set; }

    /// <summary>
    /// Gets the tuple descriptor of the key.
    /// </summary>
    /// <value></value>
    public TupleDescriptor TupleDescriptor { get; private set; }

    /// <summary>
    /// Gets or sets the size of the generator cache.
    /// </summary>
    public int CacheSize { get; private set; }

    /// <summary>
    /// Gets the type instance of which is responsible for key generation.
    /// </summary>
    public Type KeyGeneratorType { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public KeyProviderInfo(TupleDescriptor tupleDescriptor, Type keyGeneratorType, int typeIdColumnIndex, int cacheSize)
    {
      TupleDescriptor = tupleDescriptor;
      KeyGeneratorType = keyGeneratorType;
      TypeIdColumnIndex = typeIdColumnIndex;
      CacheSize = cacheSize;
    }
  }
}