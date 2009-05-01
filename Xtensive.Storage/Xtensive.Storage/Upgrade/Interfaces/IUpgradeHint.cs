// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.01

using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Upgrade hint contract.
  /// </summary>
  public interface IUpgradeHint
  {
    /// <summary>
    /// Translates the upgrade hint to the schema upgrade hints
    /// at the specified <paramref name="target"/> hint set.
    /// </summary>
    /// <param name="target">The target hint set.</param>
    void Translate(HintSet target);
  }
}