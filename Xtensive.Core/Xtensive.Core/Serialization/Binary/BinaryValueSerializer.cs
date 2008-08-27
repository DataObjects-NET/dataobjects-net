// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Kononchuk
// Created:    2008.08.13

using System.Diagnostics;
using System.IO;
using Xtensive.Core.Serialization.Implementation;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// A class providing just <see cref="Default"/> member.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IValueSerializer{TStream,T}"/> generic argument.</typeparam>
  public static class BinaryValueSerializer<T>
  {
    private static ThreadSafeCached<ValueSerializer<Stream, T>> cachedSerializer =
      ThreadSafeCached<ValueSerializer<Stream, T>>.Create(new object());

    /// <summary>
    /// Gets default serializer for type <typeparamref name="T"/>
    /// (uses <see cref="ObjectSerializerProvider.Default"/>).
    /// </summary>
    public static ValueSerializer<Stream, T> Default {
      [DebuggerStepThrough]
      get {
        return cachedSerializer.GetValue(
          () => BinaryValueSerializerProvider.Default.GetSerializer<T>());
      }
    }
  }
}