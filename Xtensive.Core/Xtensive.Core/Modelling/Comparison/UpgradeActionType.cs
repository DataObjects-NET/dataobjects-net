// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.23

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Enumerates possible upgrade action types.
  /// </summary>
  public enum UpgradeActionType
  {
    /// <summary>
    /// The action is precondition, so it should be added to appropriate preconditions chain.
    /// </summary>
    PreCondition,
    /// <summary>
    /// Regular action.
    /// </summary>
    Regular,
    /// <summary>
    /// The action is a rename of temporarily named node.
    /// </summary>
    Rename,
    /// <summary>
    /// The action is postcondition, so it should be added to appropriate postconditions chain.
    /// </summary>
    PostCondition,
  }
}