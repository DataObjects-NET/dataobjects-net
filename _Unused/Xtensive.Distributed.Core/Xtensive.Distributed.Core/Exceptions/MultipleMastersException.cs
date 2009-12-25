// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.09.26

using System;
using System.Runtime.Serialization;
using Xtensive.Distributed.Core.Resources;

namespace Xtensive.Distributed.Core
{
  [Serializable]
  public class MultipleMastersException : Exception
  {
    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public MultipleMastersException()
      : base(Strings.ExObsoleteElectionResult)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="text">Text of message.</param>
    public MultipleMastersException(string text)
      : base(text)
    {
    }

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    /// <param name="info"><see cref="SerializationInfo"/> object.</param>
    /// <param name="context"><see cref="StreamingContext"/> object.</param>
    protected MultipleMastersException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}