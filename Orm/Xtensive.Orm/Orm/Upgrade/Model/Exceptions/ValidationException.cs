// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;

using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Describes errors detected during 
  /// <see cref="Node.Validate"/>.<see cref="Node"/> execution.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of this class.
  /// </remarks>
  /// <param name="message">The message.</param>
  /// <param name="nodePath">The path of the invalid node.</param>
  public class ValidationException(string message, string nodePath) : Exception(message)
  {
    /// <summary>
    /// Gets the path of the node which validation has failed.
    /// </summary>
    public string NodePath { get; } = nodePath;
  }
}