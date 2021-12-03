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
    /// Clones the <paramref name="source"/> using 
    /// provided <see cref="Formatter"/>.
    /// </summary>
    /// <param name="source">The source to clone.</param>
    public static T Clone<T>(T source)
    {
      using (var stream = new MemoryStream()) {
        Formatter.Serialize(stream, source);
        stream.Position = 0;
        return (T) Formatter.Deserialize(stream);
      }
    }
  }
}