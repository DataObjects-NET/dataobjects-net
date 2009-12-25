// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.28


using System;
using Xtensive.Messaging.Resources;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Represents message header.
  /// </summary>
  internal struct MessageHeader
  {
    public const int HeaderLength = 6;
    public int Length;
    public int FormatCode;

    /// <summary>
    /// Writes  array of <see cref="byte"/> to <paramref name="headerData"/> starting from <paramref name="position"/>.
    /// </summary>
    /// <param name="headerData">Buffer to write header to.</param>
    /// <param name="position">Position in <paramref name="headerData"/> where write header to.</param>
    public void ToBytes(byte[] headerData, int position)
    {
      if (headerData.Length-position<HeaderLength)
        throw new ArgumentOutOfRangeException(Strings.ExMessageHeaderBufferTooSmall);

      int length = Length;
      for (int i = 0; i<4; i++) {
        headerData[position++] = (byte)(length);
        length = length>>8;
      }
      int formatCode = FormatCode;
      for (int i = 0; i<2; i++) {
        headerData[position++] = (byte)(formatCode);
        formatCode = formatCode>>8;
      }
    }

    /// <summary>
    /// Creates new instance of <see cref="MessageHeader"/> from <paramref name="headerData"/> starting from <paramref name="position"/>
    /// </summary>
    /// <param name="headerData">Buffer from where read <see cref="MessageHeader"/> data.</param>
    /// <param name="position">Position in <paramref name="headerData"/> where read header from.</param>
    /// <returns></returns>
    public static MessageHeader FromBytes(byte[] headerData, int position)
    {
      if (headerData.Length-position<HeaderLength)
        throw new ArgumentOutOfRangeException(Strings.ExMessageHeaderBufferTooSmall);
      int length = 0;
      for (int i = 0; i<4; i++)
        length |= headerData[position++]<<i*8;
      int formatCode = 0;
      for (int i = 0; i<2; i++)
        formatCode |= headerData[position++]<<i*8;
      return new MessageHeader(length, formatCode);
    }


    // Constructors
    /// <summary>
    /// Creates new instance of <see cref="MessageHeader"/>.
    /// </summary>
    /// <param name="length">Length of the message.</param>
    /// <param name="formatCode">Format code of the message (reserved for future use).</param>
    public MessageHeader(int length, int formatCode)
    {
      Length = length;
      FormatCode = formatCode;
    }
  }
}