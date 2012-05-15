// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Describes errors detected during 
  /// <see cref="Node.Validate"/>.<see cref="Node"/> execution.
  /// </summary>
  [Serializable]
  public class ValidationException : Exception
  {
    /// <summary>
    /// Gets the path of the node which validation has failed.
    /// </summary>
    public string NodePath { get; private set; }


    // Constructors

    /// <inheritdoc/>
    protected ValidationException()
    {
    }

    /// <inheritdoc/>
    public ValidationException(string message)
      : base(message)
    {
    }

    /// <inheritdoc/>
    public ValidationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="nodePath">The path of the invalid node.</param>
    public ValidationException(string message, string nodePath)
      : base(message)
    {
      NodePath = nodePath;
    }

    #region Serializing members

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
    ///   
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
    protected ValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      NodePath = info.GetString("NodePath");
    }

    /// <summary>
    /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic). </exception>
    ///   
    /// <PermissionSet>
    ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/>
    ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/>
    ///   </PermissionSet>
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("NodePath", NodePath);
    }

    #endregion
  }
}