// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.03.01

using System;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// A service listening to entity change-related events in <see cref="Session"/>
  /// and writing the information on their original version to <see cref="Versions"/> set
  /// (<see cref="VersionSet"/>).
  /// </summary>
  [Infrastructure]
  public sealed class VersionCapturer : SessionBound, 
    IDisposable
  {
    /// <summary>
    /// Gets the version set updated by this service.
    /// </summary>
    public VersionSet Versions { get; private set; }

    /// <summary>
    /// Gets or sets version capture options.
    /// </summary>
    public VersionCaptureOptions Options { get; set; }

    #region Session event handlers

    private void EntityMaterialized(object sender, EntityEventArgs e)
    {
      if (0!=(Options & VersionCaptureOptions.OnMaterialize))
        Versions.Add(e.Entity, 0!=(Options & VersionCaptureOptions.OverwriteExisting));
    }

    private void EntityChanging(object sender, EntityEventArgs e)
    {
      if (0!=(Options & VersionCaptureOptions.OnChange))
        Versions.Add(e.Entity, 0!=(Options & VersionCaptureOptions.OverwriteExisting));
    }

    private void EntityRemoving(object sender, EntityEventArgs e)
    {
      if (0!=(Options & VersionCaptureOptions.OnRemove))
      Versions.Add(e.Entity, 0!=(Options & VersionCaptureOptions.OverwriteExisting));
    }

    private void EntityCreated(object sender, EntityEventArgs e)
    {
      if (0!=(Options & VersionCaptureOptions.OnCreate))
      Versions.Add(e.Entity, 0!=(Options & VersionCaptureOptions.OverwriteExisting));
    }

    #endregion

    #region Private methods

    private void AttachEventHandlers()
    {
      Session.EntityMaterialized += EntityMaterialized;
      Session.EntityChanging += EntityChanging;
      Session.EntityRemoving += EntityRemoving;
      Session.EntityCreated += EntityCreated;
    }

    private void DetachEventHandlers()
    {
      Session.EntityMaterialized -= EntityMaterialized;
      Session.EntityChanging -= EntityChanging;
      Session.EntityRemoving -= EntityRemoving;
      Session.EntityCreated -= EntityCreated;
    }

    #endregion

    // Factory methods

    /// <summary>
    /// Attaches the version capturer to the current session.
    /// </summary>
    /// <param name="versions">The <see cref="VersionSet"/> to append captured versions to.</param>
    /// <returns>
    /// A newly created <see cref="VersionCapturer"/> attached
    /// to the current session.
    /// </returns>
    public static VersionCapturer Attach(VersionSet versions)
    {
      return Attach(Session.Demand(), versions);
    }

    /// <summary>
    /// Attaches the version capturer to the current session.
    /// </summary>
    /// <param name="session">The session to attach the capturer to.</param>
    /// <param name="versions">The <see cref="VersionSet"/> to append captured versions to.</param>
    /// <returns>
    /// A newly created <see cref="VersionCapturer"/> attached
    /// to the specified <paramref name="session"/>.
    /// </returns>
    public static VersionCapturer Attach(Session session, VersionSet versions)
    {
      return new VersionCapturer(session, versions);
    }


    // Constructors

    private VersionCapturer(Session session, VersionSet versions)
      : base(session)
    {
      ArgumentValidator.EnsureArgumentNotNull(versions, "versions");
      Options = VersionCaptureOptions.Default;
      Versions = versions;
      AttachEventHandlers();
    }

    // Dispose
    
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    public void Dispose()
    {
      DetachEventHandlers();
    }
  }
}