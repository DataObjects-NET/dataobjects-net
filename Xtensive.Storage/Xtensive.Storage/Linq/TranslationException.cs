// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.07

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Linq
{
  /// <summary>
  /// LINQ translation exception. Throws than query translates.
  /// </summary>
  [Serializable]
  public sealed class TranslationException : ApplicationException
  {
    protected TranslationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public TranslationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}