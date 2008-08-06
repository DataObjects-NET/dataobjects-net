// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Default binary serializer.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="SingletonDocTemplate" copy="true" /></para>
  /// </remarks>
  public class BinarySerializer: FormatterWrapper
  {
    [ThreadStatic]
    private static BinarySerializer instance;

    /// <see cref="SingletonDocTemplate.Instance" copy="true"/>
    [DebuggerHidden]
    public static BinarySerializer Instance {
      get {
        if (instance==null)
          instance = new BinarySerializer();
        return instance;
      }
    }

    /// <summary>
    /// Clones the specified graph of objects using this serializer.
    /// </summary>
    /// <param name="graph">The graph to clone.</param>
    /// <returns>Clone of the <paramref name="graph"/>.</returns>
    public static object Clone(object graph)
    {
      MemoryStream stream = new MemoryStream();
      Instance.Serialize(stream, graph);
      stream.Seek(0, SeekOrigin.Begin);
      return Instance.Deserialize(stream);
    }


    // Constructors 

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public BinarySerializer() 
      : base(new BinaryFormatter())
    {
      BinaryFormatter formatter = (BinaryFormatter) Formatter;
      formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
    }
  }
}