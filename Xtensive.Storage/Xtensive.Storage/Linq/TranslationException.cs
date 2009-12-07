// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.07

using System;
using System.Runtime.Serialization;

namespace Xtensive.Storage.Linq
{
  [Serializable]
  public sealed class TranslationException : Exception
  {
    protected TranslationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public TranslationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }
  }
}