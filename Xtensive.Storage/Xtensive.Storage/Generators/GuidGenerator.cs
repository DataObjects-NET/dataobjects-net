// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.19

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Generators
{
  /// <summary>
  /// Guid generator.
  /// </summary>
  public class GuidGenerator : GeneratorBase
  {
    /// <inheritdoc/>
    public override Tuple Next()
    {
      Tuple result = Tuple.Create(Hierarchy.TupleDescriptor);
      result.SetValue(0, Guid.NewGuid());
      return result;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hierarchy">The hierarchy to serve.</param>
    public GuidGenerator(HierarchyInfo hierarchy)
      : base(hierarchy)
    {
    }
  }
}