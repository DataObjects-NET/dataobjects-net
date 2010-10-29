// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.04

using System;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Enumerates all possible modification types to names.
  /// </summary>
  [Serializable]
  public enum LetterCasePolicy
  {
    /// <summary>
    /// Default mode. The same as <see cref="AsIs"/>.
    /// </summary>
    Default = 0,

    /// <summary>
    /// No modifications should be applied.
    /// </summary>
    AsIs = Default,

    /// <summary>
    /// Name should be in upper case.
    /// </summary>
    Uppercase = 1,

    /// <summary>
    /// Name should be in lower case.
    /// </summary>
    Lowercase = 2,
  }
}