// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.02

namespace Xtensive.Storage.Commands
{
  /// <summary>
  /// Query command interface.
  /// </summary>
  public interface IQueryCommand
  {
    /// <summary>
    /// Gets or sets the definition of the query.
    /// </summary>
    object Definition { get; set; }
  }
}