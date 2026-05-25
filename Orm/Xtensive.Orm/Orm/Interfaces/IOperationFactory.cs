// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Collections.Generic;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Operations
{
  public interface IOperationFactory : IDomainService
  {
    // Operations defined in kind-of-entity-lifecycle order

    /// <summary>
    /// Creates operation of Key value being generated.
    /// </summary>
    /// <param name="key">Generated key.</param>
    /// <returns>Operation instance.</returns>
    IOperation KeyGenerateOperation(Key key);

    // Entity creation
    /// <summary>
    /// Creates Operation of <see cref="Entity"/> creation.
    /// </summary>
    /// <param name="key">Key of created entity</param>
    /// <returns>Operation instance.</returns>
    IOperation EntityCreationOperation(Key key);

    /// <summary>
    /// Creates <see cref="Entity"/> initialization operation.
    /// </summary>
    /// <param name="key">Initializable entity key.</param>
    /// <returns>Operation instance.</returns>
    /// <remarks>
    /// Actually, the operation does nothing - it is used to suppress nested
    /// system operations.
    /// </remarks>
    IOperation EntityInitializeOperation(Key key);

    // Entity Fields set calls
    /// <summary>
    /// Creates Operation of <see cref="Entity"/> Field set.
    /// </summary>
    /// <param name="key">Field owner Entity.</param>
    /// <param name="field">The field that was set</param>
    /// <param name="value">The value set to field.</param>
    /// <returns>Operation instance.</returns>
    IOperation EntityFieldSetOperation(Key key, FieldInfo field, object value);
    /// <summary>
    /// Creates Operation of <see cref="Entity"/> Field set (field that references another <see cref="Entity"/>).
    /// </summary>
    /// <param name="key">Field owner Entity.</param>
    /// <param name="field">The field that was set.</param>
    /// <param name="valueKey">Referenced Entity key.</param>
    /// <returns>Operation instance.</returns>
    IOperation EntityFieldSetOperation(Key key, FieldInfo field, Key valueKey);

    // EntitySet Operations - Add, Remove, Clear
    /// <summary>
    /// Creates Operation of item addition to EntitySet.
    /// </summary>
    /// <param name="key">Owner entity key</param>
    /// <param name="field">The field of EntitySet.</param>
    /// <param name="itemKey">Key of added item.</param>
    /// <returns>Operation instance.</returns>
    IOperation EntitySetItemAddOperation(Key key, FieldInfo field, Key itemKey);
    /// <summary>
    /// Creates Operation of item removal from EntitySet.
    /// </summary>
    /// <param name="key">Owner entity key</param>
    /// <param name="field">The field of EntitySet.</param>
    /// <param name="itemKey">Key of removed item.</param>
    /// <returns>Operation instance.</returns>
    IOperation EntitySetItemRemoveOperation(Key key, FieldInfo field, Key itemKey);

    /// <summary>
    /// Creates EntitySet' clear Operation.
    /// </summary>
    /// <param name="key">Owner entity key</param>
    /// <param name="field">The field of EntitySet</param>
    /// <returns>Operation instance.</returns>
    IOperation EntitySetClearOperation(Key key, FieldInfo field);

    /// <summary>
    /// Creates Operation of entity removal.
    /// </summary>
    /// <param name="key">Key of removed entity.</param>
    /// <returns>Operation instance.</returns>
    IOperation EntityRemovalOperation(Key key);
    /// <summary>
    /// Creates Operation of number of entites removed.
    /// </summary>
    /// <param name="keys">Keys of removed entites.</param>
    /// <returns>Operation instance.</returns>
    IOperation EntitiesRemoveOperation(IEnumerable<Key> keys);


    // other operations

    /// <summary>
    /// Creates Operation of method call.
    /// </summary>
    /// <param name="executeAction">Action that will be executed on replay of the operation.</param>
    /// <param name="arguments">The action arguments.</param>
    /// <returns>Operation instance.</returns>
    IOperation MethodCallOperation(Action<object, object[]> executeAction, params object[] arguments);

    /// <summary>
    /// Creates  Operation of method call.
    /// </summary>
    /// <param name="prepareAction">The peraration action which will be executed before <paramref name="executeAction"/> on replay of the operation.</param>
    /// <param name="executeAction">Action that will be executed on replay of operation.</param>
    /// <param name="arguments">The actions' arguments.</param>
    /// <returns>Operation instance.</returns>
    IOperation MethodCallOperation(
      Action<object, object[]> prepareAction,
      Action<object, object[]> executeAction,
      params object[] arguments);

    /// <summary>
    /// Creates Operation of Entity version's validation
    /// </summary>
    /// <param name="key">Entity key.</param>
    /// <param name="version">Version that was validated.</param>
    /// <returns>Operation instance.</returns>
    IOperation ValidateVersionOperation(Key key, VersionInfo version);
  }
}