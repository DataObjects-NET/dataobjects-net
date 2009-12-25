// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.23
using System.ComponentModel;
using System.ServiceProcess;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Distributed.Test.ServerService.Resources;


namespace Xtensive.Distributed.Test.ServerService
{
  /// <summary>
  /// Server service installer.
  /// </summary>
  [RunInstaller(true)]
  public class Installer : System.Configuration.Install.Installer
  {
    private readonly ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
    private readonly ServiceInstaller serviceInstaller = new ServiceInstaller();

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor"/>
    /// </summary>
    public Installer()
    {
      serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
      serviceInstaller.ServiceName = Strings.ServiceName;
      serviceInstaller.Description = Strings.ServiceDescription;
      serviceInstaller.StartType = ServiceStartMode.Automatic;
      Installers.AddRange(new System.Configuration.Install.Installer[] {serviceProcessInstaller, serviceInstaller});
    }

    ///<summary>
    ///Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Component"></see> and optionally releases the managed resources.
    ///</summary>
    ///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        serviceProcessInstaller.Dispose();
        serviceInstaller.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}