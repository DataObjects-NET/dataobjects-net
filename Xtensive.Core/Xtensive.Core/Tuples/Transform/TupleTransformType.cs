// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.06

using System;

namespace Xtensive.Tuples.Transform
{
  /// <summary>
  /// Enumerates possible tuple transformation types.
  /// </summary>
  [Serializable]
  public enum TupleTransformType
  {
    /// <summary>
    /// Default transform type (<see cref="Auto"/>).
    /// </summary>
    Default = Auto,
    /// <summary>
    /// Transform type is detected automatically based on sources.
    /// </summary>
    Auto = 0,
    /// <summary>
    /// <see cref="TransformedTuple"/> must be returned.
    /// A wrapper performing specified transform on-the-fly is returned.
    /// </summary>
    TransformedTuple,
    /// <summary>
    /// <see cref="RegularTuple"/> must be returned.
    /// Transform is performed right now and a newly created tuple is returned.
    /// </summary>
    Tuple
  }
}