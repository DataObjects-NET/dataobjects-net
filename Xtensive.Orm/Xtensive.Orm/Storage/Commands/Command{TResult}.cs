// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.02

using System;
using System.Diagnostics;

namespace Xtensive.Storage.Commands
{
  /// <summary>
  /// <see cref="CommandResult"/> with specific result type.
  /// </summary>
  /// <typeparam name="TResult">The type of the result.</typeparam>
  [Serializable]
  public abstract class Command<TResult> : Command
  {
    public override Type ResultType {
      get { return typeof (TResult); }
    }

    
    // Constructors

    /// <inheritdoc/>
    protected Command(CommandType type)
      : base(type)
    {
    }
  }
}