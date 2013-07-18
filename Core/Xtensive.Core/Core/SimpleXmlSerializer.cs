// Copyright (C) 2013 Xtensive LLC
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.07.18

using System.IO;
using System.Xml.Serialization;

namespace Xtensive.Core
{
  /// <summary>
  /// Convenient wrapper for <see cref="XmlSerializer"/>.
  /// </summary>
  public sealed class SimpleXmlSerializer<T>
    where T : class
  {
    private readonly XmlSerializer serializer = new XmlSerializer(typeof (T));

    /// <summary>
    /// Deserializes value of <typeparamref name="T"/> from string.
    /// </summary>
    /// <param name="value">Serialized instance.</param>
    /// <returns>Deserialized instance.</returns>
    public T Deserialize(string value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "serialized");

      using (var reader = new StringReader(value))
        return (T) serializer.Deserialize(reader);
    }

    /// <summary>
    /// Serializes value of <typeparamref name="T"/> to string.
    /// </summary>
    /// <param name="value">Instance to serialize.</param>
    /// <returns>Serialized instance.</returns>
    public string Serialize(T value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");

      using (var writer = new StringWriter()) {
        serializer.Serialize(writer, value);
        return writer.ToString();
      }
    }
  }
}