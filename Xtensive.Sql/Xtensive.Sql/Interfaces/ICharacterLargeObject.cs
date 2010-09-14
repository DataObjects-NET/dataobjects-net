// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

namespace Xtensive.Sql
{
  /// <summary>
  /// A contract for server-independent character large objects (CLOBs) query parameter.
  /// </summary>
  public interface ICharacterLargeObject : ILargeObject
  {
    /// <summary>
    /// Writes a part of the specified buffer to this LOB.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The length.</param>
    void Write(char[] buffer, int offset, int count);
  }
}