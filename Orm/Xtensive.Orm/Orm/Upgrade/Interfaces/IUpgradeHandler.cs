// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.04.08

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// A handler responsible for upgrading a specific assembly or its part.
  /// </summary>
  public interface IUpgradeHandler
  {
    /// <summary>
    /// Gets a value indicating whether this handler is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the assembly this handler is made for.
    /// </summary>
    Assembly Assembly { get; }

    /// <summary>
    /// Gets the name of the assembly described by this handler.
    /// </summary>
    string AssemblyName { get; }

    /// <summary>
    /// Gets the version of the assembly described by this handler.
    /// </summary>
    string AssemblyVersion { get; }

    /// <summary>
    /// Gets the upgrade context this handler is bound to.
    /// </summary>
    UpgradeContext UpgradeContext { get; }

    /// <summary>
    /// Determines whether handler is enabled autodetect of types, which moved from one namespace to another.
    /// <para>
    /// Detection is enabled by default.
    /// </para>
    /// </summary>
    bool TypesMovementsAutoDetection { get; }

    /// <summary>
    /// Override this method to perform actions before any operation on database
    /// is performed.
    /// </summary>
    void OnPrepare();

    /// <summary>
    /// Override this method to perform actions before any operation on database
    /// is performed.
    /// </summary>
    /// <remarks>
    /// Default implementation calls <see cref="OnPrepare"/> method and completes synchronously.
    /// </remarks>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="token">The cancellation token to terminate this operation if necessary.</param>
    ValueTask OnPrepareAsync(CancellationToken token = default) {
      OnPrepare();
      return default;
    }

    /// <summary>
    /// Override this method to perform actions before upgrade domain is created.
    /// </summary>
    void OnConfigureUpgradeDomain();

    /// <summary>
    /// Override this method to perform actions before schemas are compared
    /// and synchronized. Note that database schema and metadata are already extracted here.
    /// </summary>
    void OnBeforeStage();

    /// <summary>
    /// Override this method to perform actions before schemas are compared
    /// and synchronized. Note that database schema and metadata are already extracted here.
    /// </summary>
    /// <remarks>
    /// Default implementation calls <see cref="OnBeforeStage"/> method and completes synchronously.
    /// </remarks>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="token">The cancellation token to terminate this operation if necessary.</param>
    ValueTask OnBeforeStageAsync(CancellationToken token = default) {
      OnBeforeStage();
      return default;
    }

    /// <summary>
    /// Override this method to handle "at schema ready" event. 
    /// The both extracted schema and target schema are ready at this moment.
    /// </summary>
    void OnSchemaReady();

    /// <summary>
    /// Override this method to handle "at schema ready" event.
    /// The both extracted schema and target schema are ready at this moment.
    /// </summary>
    /// <remarks>
    /// Default implementation calls <see cref="OnSchemaReady"/> method and completes synchronously.
    /// </remarks>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="token">The cancellation token to terminate this operation if necessary.</param>
    ValueTask OnSchemaReadyAsync(CancellationToken token = default) {
      OnSchemaReady();
      return default;
    }

    /// <summary>
    /// Override this method to make correction to upgrade action sequence.
    /// </summary>
    /// <param name="actions">
    /// The sequence of statements to be executed on database while upgrading database schema.</param>
    void OnBeforeExecuteActions(UpgradeActionSequence actions);

    /// <summary>
    /// Override this method to make correction to upgrade action sequence.
    /// </summary>
    /// <remarks>
    /// Default implementation calls <see cref="OnBeforeExecuteActions"/> method and completes synchronously.
    /// </remarks>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="actions">
    /// The sequence of statements to be executed on database while upgrading database schema.</param>
    /// <param name="token">The cancellation token to terminate this operation if necessary.</param>
    ValueTask OnBeforeExecuteActionsAsync(UpgradeActionSequence actions, CancellationToken token = default) {
      OnBeforeExecuteActions(actions);
      return default;
    }

    /// <summary>
    /// Override this method to handle "at upgrade stage" event.
    /// </summary>
    void OnStage();

    /// <summary>
    /// Override this method to handle "at upgrade stage" event.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <remarks>
    /// Default implementation calls <see cref="OnStage"/> method and completes synchronously.
    /// </remarks>
    /// <param name="token">The cancellation token to terminate this operation if necessary.</param>
    ValueTask OnStageAsync(CancellationToken token = default) {
      OnStage();
      return default;
    }

    /// <summary>
    /// Override this method to perform any actions after all database
    /// operations are completed.
    /// </summary>
    /// <param name="domain">Domain that would be returned by <see cref="Domain.Build"/> method.</param>
    void OnComplete(Domain domain);

    /// <summary>
    /// Override this method to perform any actions after all database
    /// operations are completed.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="domain">Domain that would be returned by <see cref="Domain.BuildAsync"/> method.</param>
    /// <param name="token">The cancellation token to terminate this operation if necessary.</param>
    ValueTask OnCompleteAsync(Domain domain, CancellationToken token = default) {
      OnComplete(domain);
      return default;
    }

    /// <summary>
    /// Determines whether this handler can upgrade the assembly
    /// from the specified version of it.
    /// </summary>
    /// <param name="oldVersion">The old assembly version.</param>
    /// <returns>
    /// <see langword="true"/> if this instance can upgrade 
    /// from the specified version of an old assembly; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool CanUpgradeFrom(string oldVersion);

    /// <summary>
    /// Determines whether specified persistent type should be included into the model
    /// in the specified <paramref name="upgradeStage"/>, or not.
    /// </summary>
    /// <param name="type">The type to filter.</param>
    /// <param name="upgradeStage">The upgrade stage to check the availability at.</param>
    /// <returns>
    /// <see langword="true"/> if type should be included into the model in the specified upgrade stage;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsTypeAvailable(Type type, UpgradeStage upgradeStage);

    /// <summary>
    /// Determines whether specified persistent field (property) should be included into the model
    /// in the specified <paramref name="upgradeStage"/>, or not.
    /// </summary>
    /// <param name="field">The field to filter.</param>
    /// <param name="upgradeStage">The upgrade stage to check the availability at.</param>
    /// <returns>
    ///   <see langword="true"/> if type should be included into the model in the specified upgrade stage;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsFieldAvailable(PropertyInfo field, UpgradeStage upgradeStage);
  }
}