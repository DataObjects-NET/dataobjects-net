// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.20

using System;

namespace Xtensive.Modelling.Actions
{
  /// <summary>
  /// An <see cref="ActionHandler"/> that does nothing.
  /// </summary>
  [Serializable]
  public class NullActionHandler : ActionHandler
  {
    private static readonly NullActionHandler instance = new NullActionHandler();

    /// <summary>
    /// Gets the only instance of this class.
    /// </summary>
    public static NullActionHandler Instance {
      get { return instance; }
    }

    /// <inheritdoc/>
    public override void Execute(NodeAction action)
    {
      return;
    }


    // Constructors

    private NullActionHandler()
    {
    }
  }
}