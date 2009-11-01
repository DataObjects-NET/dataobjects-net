// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Tuples.Internals
{
  /// <summary>
  /// Base class for all generated tuple descriptors.
  /// </summary>
  [Serializable]
  public abstract class GeneratedTupleDescriptor: TupleDescriptor
  {
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="fieldTypes">List of field <see cref="Type"/>s described by this descriptor.</param>
    protected internal GeneratedTupleDescriptor(IList<Type> fieldTypes) 
      : base(fieldTypes)
    {
    }
  }
}