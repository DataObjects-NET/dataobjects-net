// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.10

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// <see cref="Guid"/> generator.
  /// </summary>
  [Serializable]
  internal sealed class GuidKeyGenerator : KeyGenerator
  {
    /// <inheritdoc/>
    public override Tuple Next()
    {
      return Tuple.Create(Hierarchy.KeyTupleDescriptor, Guid.NewGuid());
    }


    // Constructor

    public GuidKeyGenerator(HierarchyInfo hierarchy)
      : base(hierarchy)
    {
    }
  }
}