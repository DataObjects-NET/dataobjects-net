// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.11

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Hashing;

namespace Xtensive.Indexing.Hashing
{
  /// <summary>
  /// A struct providing faster access for key 
  /// <see cref="Hashing.Hasher{T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IHasher{T}"/> generic argument.</typeparam>
  [Serializable]
  public struct HasherStruct<T> : ISerializable
  {
    /// <summary>
    /// Gets <see cref="HasherStruct{T}"/> for 
    /// <see cref="Hashing.Hasher{T}.Default"/> hasher.
    /// </summary>
    public static readonly HasherStruct<T> Default = new HasherStruct<T>(Hasher<T>.Default);


    /// <summary>
    /// Gets the underlying hasher for this cache.
    /// </summary>
    public readonly Hasher<T> Hasher;

    /// <summary>
    /// Gets <see cref="IHasher{T}.GetHash"/> method delegate.
    /// </summary>
    public readonly Func<T, long> GetHash;

    /// <summary>
    /// Gets <see cref="IHasher{T}.GetHashes"/> method delegate.
    /// </summary>
    public readonly Func<T, int, long[]> GetHashes;

    /// <summary>
    /// Implicit conversion of <see cref="Hashing.Hasher{T}"/> 
    /// to <see cref="HasherStruct{T}"/>.
    /// </summary>
    /// <param name="hasher">Hasher to provide the struct for.</param>
    public static implicit operator HasherStruct<T>(Hasher<T> hasher)
    {
      return new HasherStruct<T>(hasher);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="hasher">Hasher to provide the delegates for.</param>
    private HasherStruct(Hasher<T> hasher)
    {
      Hasher = hasher;
      GetHash = Hasher==null ? null : Hasher.GetHash;
      GetHashes = Hasher==null ? null : Hasher.GetHashes;
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true"/>
    private HasherStruct(SerializationInfo info, StreamingContext context)
    {
      Hasher = (Hasher<T>) info.GetValue("Hasher", typeof (Hasher<T>));
      GetHash = Hasher==null ? null : Hasher.GetHash;
      GetHashes = Hasher==null ? null : Hasher.GetHashes;
    }

    /// <see cref="SerializableDocTemplate.GetObjectData" copy="true"/>
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Hasher", Hasher);
    }
  }
}