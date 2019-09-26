using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tracking
{
  /// <summary>
  /// Represents a pair of original and changed values for a persistent field
  /// </summary>
  public sealed class ChangedValue
  {
    /// <summary>
    /// Gets the field.
    /// </summary>
    public FieldInfo Field { get; private set; }

    /// <summary>
    /// Gets the original value.
    /// </summary>
    public object OriginalValue { get; private set; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public object NewValue { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangedValue"/> class.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <param name="originalValue">The original value.</param>
    /// <param name="newValue">The new value.</param>
    public ChangedValue(FieldInfo field, object originalValue, object newValue)
    {
      Field = field;
      OriginalValue = originalValue;
      NewValue = newValue;
    }
  }
}
