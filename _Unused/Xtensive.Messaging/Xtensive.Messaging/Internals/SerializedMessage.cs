// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.24

using System;
using System.IO;
using Xtensive.Core;
using Xtensive.Core.Serialization;
using Xtensive.Core.Serialization.Binary;

namespace Xtensive.Messaging
{
  [Serializable]
  internal class SerializedMessage: ISerializedMessage
  {
    private readonly byte[] data;

    /// <summary>
    /// Gets serialized message data.
    /// </summary>
    public byte[] Data
    {
      get { return data; }
    }


    /// <summary>
    /// Gets or sets unique query identifier.
    /// </summary>
    public long QueryId { get; set; }


    /// <summary>
    /// Creates new instance of <see cref="SerializedMessage"/>.
    /// </summary>
    /// <param name="message"><see cref="IMessage"/> to serialize.</param>
    /// <param name="serializer"><see cref="ISerializer"/> for serialize <paramref name="message"/>.</param>
    public SerializedMessage(IMessage message, ISerializer serializer)
    {
      ArgumentValidator.EnsureArgumentNotNull(message, "message");
      ArgumentValidator.EnsureArgumentNotNull(serializer, "serializer");
      var stream = new MemoryStream(128) {Position = MessageHeader.HeaderLength};
      serializer.Serialize(stream, message);
      data = stream.ToArray();
      var header = new MessageHeader((int)stream.Position-MessageHeader.HeaderLength, 0);
      header.ToBytes(data, 0);
      QueryId = message.QueryId;
    }
  }
}