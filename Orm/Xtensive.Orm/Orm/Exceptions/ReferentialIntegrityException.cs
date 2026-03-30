// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using System.Runtime.Serialization;
using Xtensive.Core;

using Xtensive.Reflection;
using Xtensive.Orm.Model;


namespace Xtensive.Orm
{
  /// <summary>
  /// Thrown on attempt to remove an object having
  /// reference with <see cref="OnRemoveAction.Deny"/>
  /// option pointing to it.
  /// </summary>
  [Serializable]
  public sealed class ReferentialIntegrityException : StorageException
  {
    /// <summary>
    /// Gets the association.
    /// </summary>
    public AssociationInfo Association { get; private set; }

    /// <summary>
    /// Gets the <see cref="Key"/> of the initiator of removing action.
    /// </summary>
    public Key Initiator { get; private set; }

    /// <summary>
    /// Gets the <see cref="Key"/> of the referencing object.
    /// </summary>
    public Key ReferencingObject { get; private set; }

    /// <summary>
    /// Gets the <see cref="Key"/> of the referenced object.
    /// </summary>
    public Key ReferencedObject { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ReferentialIntegrityException(AssociationInfo association, 
      Entity initiator, 
      Entity referencingObject, 
      Entity referencedObject)
      : base(
        string.Format(Strings.ReferentialIntegrityViolationOnAttemptToRemoveXKeyY,
          initiator.GetType().GetFullName(), initiator.Key,
          association, referencingObject.Key, referencedObject.Key))
    {
      Association = association;
      Initiator = initiator.Key;
      ReferencingObject = referencingObject.Key;
      ReferencedObject = referencedObject.Key;
    }

    // Serialization

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferentialIntegrityException"/> class.
    /// </summary>
    /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
#if NET8_0_OR_GREATER
    [Obsolete(DiagnosticId = "SYSLIB0051")]
#endif
    private ReferentialIntegrityException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      // We can't serialize any of members declared in this type
    }
  }
}