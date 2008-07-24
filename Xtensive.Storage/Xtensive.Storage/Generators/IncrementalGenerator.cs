// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.24

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Generators
{
  /// <summary>
  /// Incremental generator.
  /// </summary>
  public class IncrementalGenerator : GeneratorBase
  {
    /// <inheritdoc/>
    public override Tuple Next()
    {
      Tuple result = Tuple.Create(Hierarchy.TupleDescriptor);
      Handler.Fill(result);
      return result;
    }

    internal IncrementalGeneratorHandler Handler { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hierarchy">The hierarchy to serve.</param>
    public IncrementalGenerator(HierarchyInfo hierarchy)
      : base(hierarchy)
    {
    }
  }
}