// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.26

using System;
using System.Runtime.Serialization;

using Xtensive.Orm.Disconnected;


namespace Xtensive.Orm
{
  /// <summary>
  /// An exception indicating that <see cref="DisconnectedState"/>
  /// isn't <see cref="DisconnectedState.Connect">connected</see>, although
  /// connection is required to perform the operation.
  /// </summary>
  [Serializable]
  public sealed class ConnectionRequiredException : StorageException
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ConnectionRequiredException()
      : base(Strings.ExConnectionIsRequired)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="text">Text of message.</param>
    public ConnectionRequiredException(string text)
      : base(text)
    {
    }

    // Serialization

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    /// <param name="info"><see cref="SerializationInfo"/> object.</param>
    /// <param name="context"><see cref="StreamingContext"/> object.</param>
    protected ConnectionRequiredException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}