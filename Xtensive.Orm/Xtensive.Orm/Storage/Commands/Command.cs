// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.31

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Storage.Commands
{
  /// <summary>
  /// Abstract base class for any command.
  /// </summary>
  [Serializable]
  public abstract class Command
  {
    /// <summary>
    /// Gets the type of the command.
    /// </summary>
    public CommandType Type { get; protected set; }

    /// <summary>
    /// Gets the type of the result.
    /// </summary>
    public abstract Type ResultType { get; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Type.ToString();
    }


    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type of the command.</param>
    protected Command(CommandType type)
    {
      Type = type;
    }
  }
}