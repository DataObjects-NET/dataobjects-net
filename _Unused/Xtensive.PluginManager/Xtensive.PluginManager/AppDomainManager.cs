// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.14

using System;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Represents the object that is capable for application domain creating, managing and unloading.
  /// </summary>
  [SecurityPermission(SecurityAction.Demand, ControlAppDomain = true, ControlEvidence = true)]
  public class AppDomainManager: MarshalByRefObject, IDisposable
  {
    private AppDomain domain;
    private AppDomainConstructor domainConstructor;
    private bool isManualUnload;

    #region Events

    /// <summary>
    /// Occurs when applicaation domain is recreated.
    /// </summary>
    public event EventHandler<EventArgs> DomainRecreate;

    /// <summary>
    /// Occurs when application domain is about to be unloaded.
    /// </summary>
    public event EventHandler<EventArgs> DomainUnload;

    private void OnUnloading()
    {
      isManualUnload = true;
    }

    private void OnUnload()
    {
      if (DomainUnload != null)
        DomainUnload(this, EventArgs.Empty);
    }

    private void OnRecreate()
    {
      if (DomainRecreate != null)
        DomainRecreate(this, EventArgs.Empty);
    }

    #endregion

    private void CreateDomain()
    {
      lock (this) {
        isManualUnload = false;
        domain = domainConstructor();
        domain.DomainUnload += new EventHandler(AppDomainUnload);
        domain.InitializeLifetimeService();
      }
    }

    /// <remarks>Domain unloading procedure is executed in a special dedicated thread so 
    /// we should not use it for application domain recreation.</remarks>
    private void AppDomainUnload(object sender, EventArgs e)
    {
      domain.DomainUnload -= new EventHandler(AppDomainUnload);
      if (!isManualUnload) {
        Thread t = new Thread(new ThreadStart(delegate { CreateDomain(); }));
        t.Start();
      }
    }

    /// <summary>
    /// Gets the <see cref="AppDomain"/> which is managed by this instance.
    /// </summary>
    /// <value>The application domain.</value>
    public AppDomain Domain
    {
      get { return domain; }
    }

    /// <summary>
    /// Unloads the <see cref="AppDomain"/> if it exists and is not unloaded yet.
    /// </summary>
    public void UnloadDomain()
    {
      try {
        if (domain == null || domain.IsFinalizingForUnload())
          return;
        lock (this) {
          OnUnloading();
          AppDomain.Unload(Domain);
        }
        OnUnload();
      }
      catch (AppDomainUnloadedException) {
        OnUnload();
      }
      catch (CannotUnloadAppDomainException) {
      }
    }

    /// <summary>
    /// Recreates the <see cref="AppDomain"/>.
    /// </summary>
    public void RecreateDomain()
    {
      UnloadDomain();
      CreateDomain();
      OnRecreate();
    }

    #region IDisposable Members

    ///<summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    public void Dispose()
    {
      Dispose(true);
    }

    #endregion

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
        UnloadDomain();
    }

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDomainManager"/> class.
    /// </summary>
    /// <param name="friendlyName">The friendly name of the domain. This friendly name can be displayed in user interfaces to identify the domain.</param>
    /// <param name="securityInfo">Evidence mapped through the security policy to establish a top-of-stack permission set.</param>
    /// <param name="info">An object that contains application domain initialization information.</param>
    public AppDomainManager(string friendlyName, Evidence securityInfo, AppDomainSetup info)
      : this(delegate { return AppDomain.CreateDomain(friendlyName, securityInfo, info); })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDomainManager"/> class.
    /// </summary>
    public AppDomainManager()
      : this(delegate {
               return AppDomain.CreateDomain(Guid.NewGuid().ToString(),
                                             AppDomain.CurrentDomain.Evidence,
                                             AppDomain.CurrentDomain.SetupInformation);
             })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDomainManager"/> class.
    /// </summary>
    /// <param name="domainConstructor">The AppDomainConstructorDelegate instance.</param>
    public AppDomainManager(AppDomainConstructor domainConstructor)
    {
      if (domainConstructor == null)
        throw new ArgumentNullException("domainConstructor");
      this.domainConstructor = domainConstructor;
      CreateDomain();
    }

    #endregion
  }
}