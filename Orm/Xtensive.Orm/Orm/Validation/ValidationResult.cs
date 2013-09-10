using Xtensive.Core;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Validation result.
  /// </summary>
  public class ValidationResult
  {
    private static readonly ValidationResult SuccessInstance = new ValidationResult();

    /// <summary>
    /// Gets successful validation result.
    /// </summary>
    public static ValidationResult Success { get { return SuccessInstance; } }

    private readonly IValidator source;
    private readonly bool isError;
    private readonly string errorMessage;
    private readonly FieldInfo field;
    private readonly object value;

    /// <summary>
    /// Gets validator that produced validation error.
    /// </summary>
    public IValidator Source { get { return source; } }

    /// <summary>
    /// Gets value indicating validation status.
    /// </summary>
    public bool IsError { get { return isError; } }

    /// <summary>
    /// Gets error message.
    /// </summary>
    public string ErrorMessage { get { return errorMessage; } }

    /// <summary>
    /// Gets field validated field.
    /// </summary>
    public FieldInfo Field { get { return field; } }

    /// <summary>
    /// Gets validated value.
    /// </summary>
    public object Value { get { return value; } }

    private ValidationResult()
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="source">Validator that produced this object.</param>
    /// <param name="errorMessage">Validation error message.</param>
    /// <param name="field">Validated field.</param>
    /// <param name="value">Validated value.</param>
    public ValidationResult(IValidator source, string errorMessage, FieldInfo field = null, object value = null)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(errorMessage, "errorMessage");

      isError = true;

      this.source = source;
      this.errorMessage = errorMessage;
      this.field = field;
      this.value = value;
    }
  }
}