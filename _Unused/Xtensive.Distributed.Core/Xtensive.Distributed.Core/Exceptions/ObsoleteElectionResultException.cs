// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.09.26

using System;
using System.Runtime.Serialization;
using Xtensive.Distributed.Core.Resources;

namespace Xtensive.Distributed.Core
{
  /// <summary>
  /// Thrown by <see cref="ElectionResult"/> on attempts 
  /// to read its properties in 
  /// <see cref="ElectionResult.IsActual"/>==<see langword="false"/> state.
  /// </summary>
  [Serializable]
  public class ObsoleteElectionResultException : Exception
  {
    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ObsoleteElectionResultException()
      : base(Strings.ExObsoleteElectionResult)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="text">Text of message.</param>
    public ObsoleteElectionResultException(string text)
      : base(text)
    {
    }

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    /// <param name="info"><see cref="SerializationInfo"/> object.</param>
    /// <param name="context"><see cref="StreamingContext"/> object.</param>
    protected ObsoleteElectionResultException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}