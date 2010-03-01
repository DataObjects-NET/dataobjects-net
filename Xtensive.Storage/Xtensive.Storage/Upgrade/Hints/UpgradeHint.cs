// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.29

using System;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Abstract base class for any upgrade hint.
  /// </summary>
  [Serializable]
  public abstract class UpgradeHint
  {
    // Constructors

    internal UpgradeHint()
    {
    }
  }
}