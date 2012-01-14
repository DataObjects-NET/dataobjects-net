// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.21

using System;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Possible validation modes.
  /// </summary>
  [Serializable]
  public enum ConstrainMode
  {
    /// <summary>
    /// The same as <see cref="OnValidate"/>.
    /// </summary>
    Default = OnValidate,

    /// <summary>
    /// Property value will be checked on object validation.
    /// </summary>
    /// <remarks>
    /// Note that when inconsistent region is not open validation can be performed immediatly after setting property value.
    /// </remarks>
    OnValidate = 0,

    /// <summary>
    /// Validation is performed before property value is set.
    /// </summary>
    OnSetValue = 1
  }
}