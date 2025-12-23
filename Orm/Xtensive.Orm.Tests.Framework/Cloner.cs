// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.05

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Object cloning helper.
  /// </summary>
  public static class Cloner
  {
    private static readonly IFormatter Formatter = new BinaryFormatter();

    /// <summary>
    /// Clones the <paramref name="source"/> using <see cref="BinaryFormatter"/>.
    /// </summary>
    /// <param name="source">The source to clone.</param>
    public static T CloneViaBinarySerialization<T>(T source)
    {
      using (var stream = new MemoryStream()) {
        Formatter.Serialize(stream, source);
        stream.Position = 0;
        return (T) Formatter.Deserialize(stream);
      }
    }

    public static T CloneViaDataContractSerializer<T>(T source, IEnumerable<Type> knownTypes)
    {
      using (var mStream = new MemoryStream()) {
        var dcSerializer = new DataContractSerializer(typeof(T), knownTypes);
        dcSerializer.WriteObject(mStream, source);
        _ = mStream.Seek(0, SeekOrigin.Begin);
        return (T) dcSerializer.ReadObject(mStream);
      }
    }

    public static T CloneViaDataContractSerializer<T>(T source, DataContractSerializerSettings settings)
    {
      using (var mStream = new MemoryStream()) {
        var dcSerializer = new DataContractSerializer(typeof(T), settings);
        dcSerializer.WriteObject(mStream, source);
        _ = mStream.Seek(0, SeekOrigin.Begin);
        return (T) dcSerializer.ReadObject(mStream);
      }
    }
  }
}