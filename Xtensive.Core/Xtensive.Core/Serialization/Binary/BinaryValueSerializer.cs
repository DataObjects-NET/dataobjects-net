// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.13

using System.Diagnostics;
using System.IO;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// Implementation of <see cref="ValueSerializer{TStream,T}"/> for binary (de)serializing.
  /// </summary>
  public class BinaryValueSerializer<T> : ValueSerializer<Stream, T>
  {
    private static readonly object _lock = new object();
    private static volatile BinaryValueSerializer<T> @default;

    /// <summary>
    /// Gets default serializer for type <typeparamref name="T"/>
    /// (uses <see cref="ValueSerializerProvider{TStream}.Default"/> <see cref="ValueSerializerProvider{TStream}"/>).
    /// </summary>
    [DebuggerHidden]
    public new static BinaryValueSerializer<T> Default {
      get {
        if (@default == null)
          lock (_lock)
            if (@default == null)
              try {
                var serializer = new BinaryValueSerializer<T>(BinaryValueSerializerProvider.Default.GetSerializer<T>());
                Thread.MemoryBarrier();
                @default = serializer;
              }
              catch {}
        return @default;
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public BinaryValueSerializer(IValueSerializer<Stream, T> implementation)
      : base(implementation) {}
  }
}