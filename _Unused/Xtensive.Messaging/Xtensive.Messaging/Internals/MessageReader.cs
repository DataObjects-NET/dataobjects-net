// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.28

using System;
using System.IO;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Messaging.Resources;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Represents temporary storage for message data.
  /// </summary>
  internal class MessageReader : IDisposable
  {
    private readonly MemoryStream headerStream = new MemoryStream(6);
    private readonly MemoryStream bodyStream = new MemoryStream(512);
    private MessageHeader header = new MessageHeader(0, 0);
    private bool isHeaderAvailable;
    private bool isBodyAvailable;

    /// <summary>
    /// Drops existing data and prepare <see cref="MessageReader"/> to receive next message.
    /// </summary>
    public void Clear()
    {
      headerStream.Position = 0;
      bodyStream.Position = 0;
      headerStream.SetLength(0);
      bodyStream.SetLength(0);
      isHeaderAvailable = false;
      isBodyAvailable = false;
    }

    /// <summary>
    /// Reads data from <paramref name="receivedData"/> buffer starting from <paramref name="position"/>.
    /// </summary>
    /// <param name="receivedData">Buffer containing data for read.</param>
    /// <param name="position">Position in the <paramref name="receivedData"/> buffer from where <see cref="MessageReader"/> will read data.</param>
    /// <returns></returns>
    public bool Read(byte[] receivedData, ref int position)
    {
      if (isBodyAvailable)
        throw new MessagingException(Strings.ExMessageIsAlreadyRead);

      // Read header
      if (!isHeaderAvailable) {
        int headerPartLength = Math.Min(MessageHeader.HeaderLength-(int)headerStream.Position, receivedData.Length-position);
        headerStream.Write(receivedData, position, headerPartLength);
        position += headerPartLength;
        if (headerStream.Position==MessageHeader.HeaderLength) {
          header = MessageHeader.FromBytes(headerStream.GetBuffer(), 0);
          isHeaderAvailable = true;
        }
      }

      // Read body
      if (isHeaderAvailable && position<receivedData.Length) {
        int bodyPartLength = Math.Min(header.Length-(int)bodyStream.Position, receivedData.Length-position);
        bodyStream.Write(receivedData, position, bodyPartLength);
        position += bodyPartLength;
        if (bodyStream.Length==header.Length)
          isBodyAvailable = true;
      }
      return isBodyAvailable;
    }

    /// <summary>
    /// Gets <see cref="MemoryStream"/> represents message body.
    /// </summary>
    public MemoryStream Body
    {
      get
      {
        if (!isBodyAvailable)
          throw new MessagingException(Strings.ExMessageBodyIsNotAvailableYet);
        return bodyStream;
      }
    }

    // Dispose and Finalize

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
      if (disposing) {
        headerStream.Dispose();
        bodyStream.Dispose();
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dtor" copy="true"/>
    /// </summary>
    ~MessageReader()
    {
      Dispose(false);
    }
  }
}