// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.13

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace Xtensive.Internals.DocTemplates
{
  /// <summary>
  /// Serializable type documentation template.
  /// </summary>
  [Serializable]
  public class SerializableDocTemplate: ISerializable, IDeserializationCallback
  {
    /// <summary>
    /// Serializes the instance of <see cref="SerializableDocTemplate"/> class.
    /// </summary>
    /// <param name="info">Serialization info to store serialization data in.</param>
    /// <param name="context">Streaming context.</param>
    [SecurityCritical]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      throw new NotImplementedException();
    }


    // Constructors

    /// <summary>
    /// Deserializes the instance of <see cref="SerializableDocTemplate"/> class.
    /// </summary>
    /// <param name="info">Serialization info to get the deserialized data from.</param>
    /// <param name="context">Streaming context.</param>
    protected void Ctor(SerializationInfo info, StreamingContext context)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Handles post-deserialization initialization of the deserialized
    /// <see cref="SerializableDocTemplate"/> instance.
    /// </summary>
    /// <param name="sender">The object invoking this method.</param>
    public virtual void OnDeserialization(object sender)
    {
      throw new NotImplementedException();
    }
  }
}