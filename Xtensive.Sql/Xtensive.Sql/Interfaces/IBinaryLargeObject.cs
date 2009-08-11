// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

namespace Xtensive.Sql
{
  /// <summary>
  /// A server-independed wrapper for using native binary large objects (BLOBs) as query parameters.
  /// </summary>
  public interface IBinaryLargeObject : ILargeObject
  {
    /// <summary>
    /// Writes a part of the specified buffer to this LOB.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="count">The length.</param>
    void Write(byte[] buffer, int offset, int count);
  }
}