using Xtensive.Orm.Model;

namespace Xtensive.Orm.Validation
{
  /// <summary>
  /// Validation result.
  /// </summary>
  public class ValidationResult
  {
    private static readonly ValidationResult SuccessInstance = new ValidationResult(false);

    /// <summary>
    /// Gets successful validation result.
    /// </summary>
    public static ValidationResult Success { get { return SuccessInstance; } }

    private readonly bool isError;
    private readonly string errorMessage;
    private readonly FieldInfo field;
    private readonly object value;

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

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="isError">Value indicating validation status.</param>
    /// <param name="field">Validated field.</param>
    /// <param name="value">Validated value.</param>
    /// <param name="errorMessage">Validation error message.</param>
    public ValidationResult(bool isError, FieldInfo field = null, object value = null, string errorMessage = null)
    {
      this.isError = isError;
      this.errorMessage = errorMessage;
      this.field = field;
      this.value = value;
    }
  }
}