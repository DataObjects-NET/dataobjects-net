// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Base class for any regular tuple.
  /// </summary>
  [Serializable]
  public abstract class RegularTuple: Tuple
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected RegularTuple()
    {
    }
  }
}