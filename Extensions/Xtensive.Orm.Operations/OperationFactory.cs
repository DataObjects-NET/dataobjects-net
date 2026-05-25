using System;
using System.Collections.Generic;
using Xtensive.IoC;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Creates real instances of operations.
  /// </summary>
  [Service(typeof(IOperationFactory), Singleton = true)]
  public sealed class OperationFactory : IOperationFactory
  {
    // Operations defined in kind-of-entity-lifecycle order

    /// <inheritdoc/>
    public IOperation KeyGenerateOperation(Key key) => new KeyGenerateOperation(key);

    /// <inheritdoc/>
    public IOperation EntityCreationOperation(Key key) => new EntityCreateOperation(key);

    /// <inheritdoc/>
    public IOperation EntityInitializeOperation(Key key) => new EntityInitializeOperation(key);

    // Entity Fields set calls
    /// <inheritdoc/>
    public IOperation EntityFieldSetOperation(Key key, FieldInfo field, object value) => new EntityFieldSetOperation(key, field, value);

    /// <inheritdoc/>
    public IOperation EntityFieldSetOperation(Key key, FieldInfo field, Key valueKey) => new EntityFieldSetOperation(key, field, valueKey);

    // EntitySet Operations - Add, Remove, Clear
    /// <inheritdoc/>
    public IOperation EntitySetItemAddOperation(Key key, FieldInfo field, Key itemKey) => new EntitySetItemAddOperation(key, field, itemKey);

    /// <inheritdoc/>
    public IOperation EntitySetItemRemoveOperation(Key key, FieldInfo field, Key itemKey) => new EntitySetItemRemoveOperation(key, field, itemKey);

    /// <inheritdoc/>
    public IOperation EntitySetClearOperation(Key key, FieldInfo field) => new EntitySetClearOperation(key, field);

    // Entity Removal
    /// <inheritdoc/>
    public IOperation EntitiesRemoveOperation(IEnumerable<Key> keys) => new EntitiesRemoveOperation(keys);

    /// <inheritdoc/>
    public IOperation EntityRemovalOperation(Key key) => new EntitiesRemoveOperation(key);


    // other operations
    /// <inheritdoc/>
    public IOperation MethodCallOperation(Action<object, object[]> executeAction, params object[] arguments)
      => new MethodCallOperation(executeAction, arguments);

    /// <inheritdoc/>
    public IOperation MethodCallOperation(Action<object, object[]> prepareAction, Action<object, object[]> executeAction, params object[] arguments)
      => new MethodCallOperation(prepareAction, executeAction, arguments);

    /// <inheritdoc/>
    public IOperation ValidateVersionOperation(Key key, VersionInfo version) => new ValidateVersionOperation(key, version);
  }
}
