// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Collections.Generic;
using Xtensive.Orm.Model;
//using System.Reflection;

namespace Xtensive.Orm.Operations
{
  public interface IOperationFactory : IDomainService
  {
    //Operations defined in kind-of-entity-lifecycle order

    /// <summary>
    /// Creates instance of <see cref="Operations.KeyGenerateOperation"/>.
    /// </summary>
    /// <param name="key">Keys of removed entites.</param>
    /// <returns>Created instance.</returns>
    IOperation KeyGenerateOperation(Key key);

    // Entity creation
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IOperation EntityCreationOperation(Key key);
    IOperation EntityInitializeOperation(Key key);

    // Entity Fields set calls
    IOperation EntityFieldSetOperation(Key key, FieldInfo field, object value);
    IOperation EntityFieldSetOperation(Key key, FieldInfo field, Key valueKey);

    //EntitySet Operations - Add, Remove, Clear
    IOperation EntitySetItemAddOperation(Key key, FieldInfo field, Key itemKey);
    IOperation EntitySetItemRemoveOperation(Key key, FieldInfo field, Key itemKey);
    IOperation EntitySetClearOperation(Key key, FieldInfo field);

    /// <summary>
    /// Creates instance of <see cref="Operations.EntitiesRemoveOperation"/>.
    /// </summary>
    /// <param name="key">Keys of removed entites.</param>
    /// <returns>Created instance.</returns>
    IOperation EntityRemovalOperation(Key key);
    /// <summary>
    /// Creates instance of <see cref="Operations.EntitiesRemoveOperation"/>.
    /// </summary>
    /// <param name="keys">Keys of removed entites.</param>
    /// <returns>Created instance.</returns>
    IOperation EntitiesRemoveOperation(IEnumerable<Key> key);


    // other operations

    /// <summary>
    /// Creates a new instance of method call operation
    /// </summary>
    /// <param name="executeAction">Action that will be executed on replay of the operation.</param>
    /// <param name="arguments">The action arguments.</param>
    /// <returns>Operation instance.</returns>
    IOperation MethodCallOperation(Action<object, object[]> executeAction, params object[] arguments);

    /// <summary>
    /// Initializes a new instance of this class.
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
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    IOperation ValidateVersionOperation(Key key, VersionInfo version);
  }
}