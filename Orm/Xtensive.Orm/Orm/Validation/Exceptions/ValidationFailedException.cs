using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Validation failure error.
  /// </summary>
  public sealed class ValidationFailedException : StorageException
  {
    private IList<EntityErrorInfo> validationErrors;

    /// <summary>
    /// Gets validation errors associated with this instance.
    /// </summary>
    public IList<EntityErrorInfo> ValidationErrors
    {
      get { return validationErrors; }
      set
      {
        if (validationErrors!=null)
          throw Exceptions.AlreadyInitialized("ValidationErrors");
        validationErrors = value;
      }
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
  }
}