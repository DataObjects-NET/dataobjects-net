// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2009.06.04

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// Provides data for <see cref="Domain.SessionOpen"/> event.
  /// </summary>
  public readonly struct SessionEventArgs
  {
    /// <summary>
    /// Gets the session.
    /// </summary>
    public Session Session { get; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session.</param>
    public SessionEventArgs(Session session)
    {
      Session = session;
    }
  }
}
