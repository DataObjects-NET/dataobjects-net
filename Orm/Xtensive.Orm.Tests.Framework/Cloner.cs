// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2009.05.05

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Object cloning helper.
  /// </summary>
  public static class Cloner
  {
    public static class SerializationDomains
    {
      public static readonly Lazy<IReadOnlyList<Type>> System = new Lazy<IReadOnlyList<Type>>(
        () => [
          typeof(DateOnly),       typeof(DateOnly?),       typeof(DateOnly[]),       typeof(DateOnly?[]),
          typeof(TimeOnly),       typeof(TimeOnly?),       typeof(TimeOnly[]),       typeof(TimeOnly?[]),
          typeof(DateTime),       typeof(DateTime?),       typeof(DateTime[]),       typeof(DateTime?[]),
          typeof(TimeSpan),       typeof(TimeSpan?),       typeof(TimeSpan[]),       typeof(TimeSpan?[]),
          typeof(DateTimeOffset), typeof(DateTimeOffset?), typeof(DateTimeOffset[]), typeof(DateTimeOffset?[]),
          typeof(Type),            typeof(Type[]),
          typeof(MethodInfo),      typeof(MethodInfo[]),
          typeof(MemberInfo),      typeof(MemberInfo[]),
          typeof(ConstructorInfo), typeof(ConstructorInfo[]),
          typeof(bool?),   typeof(bool[]),   typeof(bool?[]),
          typeof(byte?),   typeof(byte[]),   typeof(byte?[]),
          typeof(sbyte?),  typeof(sbyte[]),  typeof(sbyte?[]),
          typeof(short?),  typeof(short[]),  typeof(short?[]),
          typeof(ushort?), typeof(ushort[]), typeof(ushort?[]),
          typeof(int?),     typeof(int[]),     typeof(int?[]),
          typeof(uint?),    typeof(uint[]),    typeof(uint?[]),
          typeof(long?),    typeof(long[]),    typeof(long?[]),
          typeof(ulong?),   typeof(ulong[]),   typeof(ulong?[]),
          typeof(float?),   typeof(float[]),   typeof(float?[]),
          typeof(double?),  typeof(double[]),  typeof(double?[]),
          typeof(decimal?), typeof(decimal[]), typeof(decimal?[]),
          typeof(string[]),
        ]);
    }

    /// <summary>
    /// Clones the <paramref name="source"/> via <see cref="DataContractSerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type of instance.</typeparam>
    /// <param name="source">The instance to clone.</param>
    /// <param name="knownTypes">The known type to pass into the serializer.</param>
    /// <returns>Cloned instance.</returns>
    public static T CloneViaXmlSerialization<T>(T source, IEnumerable<Type> knownTypes)
    {
      var settings = new DataContractSerializerSettings { KnownTypes = knownTypes, PreserveObjectReferences = true };
      return CloneViaXmlSerialization(source, settings);      
    }

    /// <summary>
    /// Clones the <paramref name="source"/> via <see cref="DataContractSerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type of instance.</typeparam>
    /// <param name="source">The instance to clone.</param>
    /// <param name="settings">The settings for the serializer.</param>
    /// <returns>Cloned instance.</returns>
    public static T CloneViaXmlSerialization<T>(T source, DataContractSerializerSettings settings)
    {
      using (var mStream = new MemoryStream()) {
        var dcSerializer = new DataContractSerializer(typeof(T), settings);
        dcSerializer.WriteObject(mStream, source);
#if DEBUG
        var data = mStream.ToArray();
        var serializedVersion = Encoding.UTF8.GetString(data);
#endif
        _ = mStream.Seek(0, SeekOrigin.Begin);
        return (T) dcSerializer.ReadObject(mStream);
      }
    }

    /// <summary>
    /// Clones the <paramref name="source"/> via <see cref="DataContractJsonSerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type of instance.</typeparam>
    /// <param name="source">The instance to clone.</param>
    /// <param name="knownTypes">The known type to pass into the serializer.</param>
    /// <returns>Cloned instance.</returns>
    public static T CloneViaJsonSerialization<T>(T source, IEnumerable<Type> knownTypes)
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = knownTypes };
      return CloneViaJsonSerialization(source, settings);
    }

    /// <summary>
    /// Clones the <paramref name="source"/> via <see cref="DataContractJsonSerializer"/>.
    /// </summary>
    /// <typeparam name="T">The type of instance.</typeparam>
    /// <param name="source">The instance to clone.</param>
    /// <param name="settings">The settings for the serializer.</param>
    /// <returns>Cloned instance.</returns>
    public static T CloneViaJsonSerialization<T>(T source, DataContractJsonSerializerSettings settings)
    {
      using (var mStream = new MemoryStream()) {
        var dcSerializer = new DataContractJsonSerializer(typeof(T), settings);
        dcSerializer.WriteObject(mStream, source);
#if DEBUG
        var data = mStream.ToArray();
        var serializedVersion = Encoding.UTF8.GetString(data);
#endif
        _ = mStream.Seek(0, SeekOrigin.Begin);
        return (T) dcSerializer.ReadObject(mStream);
      }
    }

    /// <summary>
    /// Clones <paramref name="source"/> via <see cref="JsonSerializer"/> with indentation and reference preservation.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <param name="source">The object to clone.</param>
    /// <param name="options">Options for <see cref="JsonSerializer"/>.</param>
    /// <returns>Cloned object</returns>
    public static T CloneViaJsonSerialization<T>(T source, bool useDataContract = false)
    {
      if (useDataContract) {
        var settings = new DataContractJsonSerializerSettings();
        return CloneViaJsonSerialization(source, settings);
      }
      else {
        var settings = new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
        return CloneViaJsonSerialization(source, settings);
      }
    }

    /// <summary>
    /// Clones <paramref name="source"/> via <see cref="JsonSerializer"/>.
    /// </summary>
    /// <typeparam name="T">Type of object.</typeparam>
    /// <param name="source">The object to clone.</param>
    /// <param name="options">Options for <see cref="JsonSerializer"/>.</param>
    /// <returns>Cloned object</returns>
    public static T CloneViaJsonSerialization<T>(T source, JsonSerializerOptions options)
    {
      using (var mStream = new MemoryStream()) {

        JsonSerializer.Serialize<T>(mStream, source, options);
#if DEBUG
        var data = mStream.ToArray();
        var serializedVersion = Encoding.UTF8.GetString(data);
#endif
        _ = mStream.Seek(0, SeekOrigin.Begin);
        return JsonSerializer.Deserialize<T>(mStream, options);

      }
    }
  }
}