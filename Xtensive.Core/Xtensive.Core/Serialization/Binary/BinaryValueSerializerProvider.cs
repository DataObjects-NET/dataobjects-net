// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.13

using System.Diagnostics;
using System.IO;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Implementation of <see cref="ValueSerializerProvider{TStream}"/> for binary (de)serializing.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  public class BinaryValueSerializerProvider : ValueSerializerProvider<Stream>
  {
    private static readonly BinaryValueSerializerProvider @default = 
      new BinaryValueSerializerProvider();

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    public static BinaryValueSerializerProvider Default {
      [DebuggerStepThrough]
      get { return @default; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public BinaryValueSerializerProvider()
    {
      var t = typeof (BinaryValueSerializerProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}