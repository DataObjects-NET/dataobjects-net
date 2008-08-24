// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.13

using System.Diagnostics;
using System.IO;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Implementation of <see cref="ValueSerializerProvider{TStream}"/> for binary (de)serializing.
  /// </summary>
  public class BinaryValueSerializerProvider :
    ValueSerializerProvider<Stream>,
    IBinaryValueSerializerProvider
  {
    private static readonly BinaryValueSerializerProvider @default = new BinaryValueSerializerProvider();

    /// <see cref="HasStaticDefaultDocTemplate.Default" copy="true" />
    [DebuggerHidden]
    public new static IBinaryValueSerializerProvider Default {
      get { return @default; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public BinaryValueSerializerProvider()
      : base() {
      var t = typeof (BinaryValueSerializerProvider);
      AddHighPriorityLocation(t.Assembly, t.Namespace);
    }
  }
}