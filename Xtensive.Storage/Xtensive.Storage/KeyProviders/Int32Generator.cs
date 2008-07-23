// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.26

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.KeyProviders
{
  [Serializable]
  public class Int32Generator : Generator
  {
    private int counter = 1;

    /// <inheritdoc/>
    public override Tuple Next()
    {
      return Tuple.Create(counter++);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hierarchy">The hierarchy to serve.</param>
    public Int32Generator(HierarchyInfo hierarchy)
      : base(hierarchy)
    {
    }
  }
}
