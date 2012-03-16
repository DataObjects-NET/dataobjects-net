// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.05

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Core
{
  /// <summary>
  /// Object cloning helper.
  /// </summary>
  [Serializable]
  public class Cloner
  {
    private readonly static object @lock = new object();
    private volatile static Cloner @default;

    #region Default property (static)

    public static Cloner Default {
      get {
        if (@default==null) lock (@lock) if (@default==null)
          @default = CreateDefaultCloner();
        return @default;
      }
    }

    private static Cloner CreateDefaultCloner()
    {
      return new Cloner(new BinaryFormatter());
    }

    #endregion

    /// <summary>
    /// Gets the formatter used by this cloner.
    /// </summary>
    public IFormatter Formatter { get; private set; }

    /// <summary>
    /// Clones the <paramref name="source"/> using 
    /// provided <see cref="Formatter"/>.
    /// </summary>
    /// <param name="source">The source to clone.</param>
    public T Clone<T>(T source)
    {
      using (var stream = CreateStream()) {
        Formatter.Serialize(stream, source);
        stream.Position = 0;
        return (T) Formatter.Deserialize(stream);
      }
    }

    /// <summary>
    /// Creates the stream to use for cloning.
    /// </summary>
    /// <returns>Newly created stream.</returns>
    protected virtual Stream CreateStream()
    {
      return new MemoryStream();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="formatter">The <see cref="Formatter"/>.</param>
    public Cloner(IFormatter formatter)
    {
      ArgumentValidator.EnsureArgumentNotNull(formatter, "formatter");
      Formatter = formatter;
    }
  }
}