// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Xtensive.Indexing.Storage
{
  [Serializable]
  public class IntegrityException: Exception
  {
    public string NodePath { get; private set; }

    protected IntegrityException()
    {
    }

    public IntegrityException(string message, string nodePath)
      : base(message)
    {
      NodePath = nodePath;
    }

    public IntegrityException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}