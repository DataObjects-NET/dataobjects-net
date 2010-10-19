// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xtensive.Internals.DocTemplates;
using Xtensive.Serialization.Implementation;
using Xtensive.Threading;

namespace Xtensive.Serialization.Binary
{
  /// <summary>
  /// Legacy binary serializer.
  /// </summary>
  /// <remarks>
  /// <para id="About"><see cref="SingletonDocTemplate" copy="true" /></para>
  /// </remarks>
  public class LegacyBinarySerializer : FormatterAsSerializer
  {
    private static ThreadSafeCached<LegacyBinarySerializer> cachedInstance =
      ThreadSafeCached<LegacyBinarySerializer>.Create(new object());

    /// <see cref="SingletonDocTemplate.Instance" copy="true"/>
    public static LegacyBinarySerializer Instance {
      [DebuggerStepThrough]
      get {
        return cachedInstance.GetValue(
          () => new LegacyBinarySerializer(new SerializerConfiguration()));
      }
    }

    /// <inheritdoc/>
    protected override void OnConfigured()
    {
      base.OnConfigured();
      var bf = (BinaryFormatter) Formatter;
      bf.AssemblyFormat = Configuration.AssemblyStyle;
      bf.TypeFormat = Configuration.TypeFormat;
    }


    // Constructors 

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public LegacyBinarySerializer()
      : base(new BinaryFormatter()) 
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public LegacyBinarySerializer(SerializerConfiguration configuration)
      : base(new BinaryFormatter(), configuration)
    {
    }
  }
}