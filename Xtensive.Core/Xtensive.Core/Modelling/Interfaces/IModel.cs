// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using Xtensive.Modelling.Actions;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Tagging interface for root model types.
  /// </summary>
  public interface IModel : INode
  {
    /// <summary>
    /// Gets or sets the sequence of actions to log.
    /// </summary>
    ActionSequence Actions { get; set; }
  }
}