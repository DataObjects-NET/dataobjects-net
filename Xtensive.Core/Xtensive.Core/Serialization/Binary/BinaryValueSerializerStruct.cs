// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.12

using System.IO;
using Xtensive.Core.Serialization.Implementation;

namespace Xtensive.Core.Serialization.Binary
{
  /// <summary>
  /// A class providing just <see cref="Default"/> member.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IValueSerializer{TStream,T}"/> generic argument.</typeparam>
  public static class BinaryValueSerializerStruct<T>
  {
    /// <summary>
    /// Gets <see cref="ValueSerializerStruct{TStream,T}"/> for <see cref="BinaryValueSerializer{T}.Default"/> hasher.
    /// </summary>
    public static readonly ValueSerializerStruct<Stream, T> Default = 
      new ValueSerializerStruct<Stream, T>(BinaryValueSerializer<T>.Default);
  }
}