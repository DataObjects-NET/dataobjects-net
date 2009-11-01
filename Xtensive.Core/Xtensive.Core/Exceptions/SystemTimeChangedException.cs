// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Core
{
  /// <summary>
  /// Thrown by <see cref="HighResolutionTime.Now"/> method when it
  /// notices system time has been changed.
  /// </summary>
  [Serializable]
  public class SystemTimeChangedException : Exception
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public SystemTimeChangedException()
      : base(Strings.ExSystemTimeChanged)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="text">Text of message.</param>
    public SystemTimeChangedException(string text)
      : base(text)
    {
    }

    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    /// <param name="info"><see cref="SerializationInfo"/> object.</param>
    /// <param name="context"><see cref="StreamingContext"/> object.</param>
    protected SystemTimeChangedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}