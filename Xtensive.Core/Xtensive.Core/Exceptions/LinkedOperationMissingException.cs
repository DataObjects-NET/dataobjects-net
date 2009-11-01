using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// Thrown when links aren't property set on both sides of relation.
  /// </summary>
  [Serializable]
  public class LinkedOperationMissingException : InvalidOperationException
  {
    private Type missingOperationType;
    private string linkedPropertyName;
    private string operationOwnerProperty;
    private Type linkedObjectType;

    /// <summary>
    /// Gets the type of linked object.
    /// </summary>
    public Type LinkedObjectType
    {
      get { return linkedObjectType;}
    }

    /// <summary>
    /// Gets the name of operation owner property.
    /// </summary>
    public string OperationOwnerProperty
    {
      get { return operationOwnerProperty;}
    }

    /// <summary>
    /// Gets the name of the linked property.
    /// </summary>
    public string LinkedPropertyName
    {
      get { return linkedPropertyName;}
    }

    /// <summary>
    /// Gets the type of missing operation.
    /// </summary>
    public Type MissingOperationType
    {
      get { return missingOperationType;}
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="missingOperationType">Initial <see cref="MissingOperationType"/> property value.</param>
    /// <param name="linkedPropertyName">Initial <see cref="LinkedPropertyName"/> property value.</param>
    /// <param name="operationOwnerProperty">Initial <see cref="OperationOwnerProperty"/> property value.</param>
    /// <param name="linkedObjectType">Initial <see cref="LinkedObjectType"/> property value.</param>
    public LinkedOperationMissingException(Type missingOperationType, string linkedPropertyName, string operationOwnerProperty, Type linkedObjectType)
      : base(string.Format(Strings.ExLinkedOperationMissingFormat, operationOwnerProperty))
    {
      this.missingOperationType = missingOperationType;
      this.linkedPropertyName = linkedPropertyName;
      this.operationOwnerProperty = operationOwnerProperty;
      this.linkedObjectType = linkedObjectType;
    }

    /// <see cref="SerializableDocTemplate.Ctor" copy="true" />
    protected LinkedOperationMissingException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      // TODO: Where is serialization code???
    }
  }
}
