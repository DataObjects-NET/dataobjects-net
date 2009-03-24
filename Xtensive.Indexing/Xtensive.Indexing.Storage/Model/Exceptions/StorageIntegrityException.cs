// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Xtensive.Indexing.Storage.Exceptions
{
  [Serializable]
  public class ModelIntegrityException: Exception
  {
    public string NodePath { get; private set; }

    protected ModelIntegrityException()
    {
    }

    public ModelIntegrityException(string message, string nodePath)
      : base(message)
    {
      NodePath = nodePath;
    }

    public ModelIntegrityException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}