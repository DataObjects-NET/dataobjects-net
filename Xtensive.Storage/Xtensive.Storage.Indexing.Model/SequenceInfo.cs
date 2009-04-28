// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.28

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// Column sequence.
  /// </summary>
  [Serializable]
  public sealed class SequenceInfo
  {
    /// <summary>
    /// Gets the start value.
    /// </summary>
    public long StartValue { get; private set; }

    /// <summary>
    /// Gets the increment.
    /// </summary>
    public long Increment { get; private set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="startValue">The start value.</param>
    /// <param name="increment">The increment.</param>
    public SequenceInfo(long startValue, long increment)
    {
      StartValue = startValue;
      Increment = increment;
    }
  }
}