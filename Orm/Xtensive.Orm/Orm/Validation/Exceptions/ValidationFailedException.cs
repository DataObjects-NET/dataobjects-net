using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Validation failure error.
  /// </summary>
  [Serializable]
  public class ValidationFailedException : StorageException
  {
    [NonSerialized]
    private IList<EntityErrorInfo> validationErrors;

    /// <summary>
    /// Gets validation errors associated with this instance.
    /// </summary>
    public IList<EntityErrorInfo> ValidationErrors
    {
      get { return validationErrors; }
      set { validationErrors = value; }
    }

    /// <summary>
    /// Initailizes new instance of this type.
    /// </summary>
    /// <param name="message">Exception message.</param>
    public ValidationFailedException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="inner">Inner exception.</param>
    public ValidationFailedException(string message, Exception inner)
      : base(message, inner)
    {
    }

    /// <summary>
    /// Performs deserialization.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    protected ValidationFailedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}