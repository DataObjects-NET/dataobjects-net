// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.24

namespace Xtensive.Messaging
{
  /// <summary>
  /// Represents serialized message. Used to send multiple similar messages to multiple recipients to serialize message only once. 
  /// </summary>
  public interface ISerializedMessage : IMessage
  {
    /// <summary>
    /// Gets serialized message data.
    /// </summary>
    byte[] Data { get;}
  }
}