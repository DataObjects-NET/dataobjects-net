// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.11.30

using System;

namespace Xtensive.Practices.Localization
{
  /// <summary>
  /// Localization policy.
  /// </summary>
  [Serializable]
  public enum LocalizationPolicy
  {
    /// <summary>
    /// Default policy. New localization instance should be created in case it is absent.
    /// </summary>
    Default = 0,
  }
}