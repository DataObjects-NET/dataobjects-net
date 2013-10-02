// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.20

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// Action handler contract.
  /// </summary>
  public interface IActionHandler
  {
    /// <summary>
    /// Executes the specified action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void Execute(NodeAction action);
  }
}