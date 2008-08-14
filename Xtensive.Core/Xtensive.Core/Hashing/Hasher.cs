// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.11

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Hashing
{
  /// <summary>
  /// Provides delegates allowing to call hashing methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IHasher{T}"/> generic argument.</typeparam>
  /// <remarks>
  /// <para id="About"><see cref="HasStaticDefaultDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public sealed class Hasher<T> : MethodCacheBase<IHasher<T>>
  {
    private static readonly object syncRoot = new object();
    private static Hasher<T> @default;

    /// <summary>
    /// Gets default hasher for type <typeparamref name="T"/>
    /// (uses <see cref="HasherProvider.Default"/> <see cref="HasherProvider"/>).
    /// </summary>
    public static Hasher<T> Default {
      [DebuggerStepThrough]
      get {
        if (@default==null) lock (syncRoot) if (@default==null) {
          try {
            var hasher = HasherProvider.Default.GetHasher<T>();
            Thread.MemoryBarrier();
            @default = hasher;
          }
          catch(Exception e) {
            Log.Info(e, String.Format(Resources.Strings.LogUnableToGetDefaultHasherForTypeXxx, typeof (T).FullName));
          }
        }
        return @default;
      }
    }

    /// <summary>
    /// Gets the provider underlying hasher is associated with.
    /// </summary>
    public readonly IHasherProvider Provider;

    /// <summary>
    /// Gets <see cref="IHasher{T}.GetHash"/> method delegate.
    /// </summary>
    public readonly Func<T, long> GetHash;

    /// <summary>
    /// Gets <see cref="IHasher{T}.GetHashes"/> method delegate.
    /// </summary>
    public readonly Func<T, int, long[]> GetHashes;
    

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="implementation">Hasher to provide the delegates for.</param>
    public Hasher(IHasher<T> implementation)
      : base(implementation)
    {
      Provider = Implementation.Provider;
      GetHashes = Implementation.GetHashes;
      GetHash = Implementation.GetHash;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    public Hasher(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Provider = Implementation.Provider;
      GetHashes = Implementation.GetHashes;
      GetHash = Implementation.GetHash;
    }
  }
}