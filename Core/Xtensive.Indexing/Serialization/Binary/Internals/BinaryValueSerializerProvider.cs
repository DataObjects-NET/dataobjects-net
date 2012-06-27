// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.29

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Serialization.Binary
{
  /// <summary>
  /// An implementation of <see cref="IValueSerializerProvider"/> for binary serialization.
  /// </summary>
  /// <remarks>
  /// <see cref="HasStaticDefaultDocTemplate" copy="true"/>
  /// </remarks>
  [Serializable]
  public class BinaryValueSerializerProvider : ValueSerializerProvider
  {
    private static readonly BinaryValueSerializerProvider @default = new BinaryValueSerializerProvider();
    
    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static new BinaryValueSerializerProvider Default {
      [DebuggerStepThrough]
      get { return @default; }
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public BinaryValueSerializerProvider()
    {
      Type t = typeof (SerializationDataValueSerializer);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}