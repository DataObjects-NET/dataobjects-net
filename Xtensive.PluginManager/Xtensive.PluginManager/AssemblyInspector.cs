// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.12

using System;
using System.Reflection;

namespace Xtensive.PluginManager
{
  /// <summary>
  /// Represents an object that is capable for assembly inspection.
  /// </summary>
  public class AssemblyInspector: MarshalByRefObject, IAssemblyInspector, IDisposable
  {
    private RemoteAssemblyInspector inspector;
    private AppDomainManager domainManager;

    #region Events

    /// <summary>
    /// Occurs when assembly inspection process starts.
    /// </summary>
    public event EventHandler<AssemblyInspectorEventArgs> InspectionStart;

    /// <summary>
    /// Occurs when assembly inspection process completed.
    /// </summary>
    public event EventHandler<AssemblyInspectorEventArgs> InspectionComplete;

    /// <summary>
    /// Occurs when requested type is found in assembly that is being inspected.
    /// </summary>
    public event EventHandler<TypeFoundEventArgs> TypeFound;

    private void RemoteInspectionStart(object sender, AssemblyInspectorEventArgs e)
    {
      if (InspectionStart != null)
        InspectionStart(this, e);
    }

    private void RemoteInspectionComplete(object sender, AssemblyInspectorEventArgs e)
    {
      if (InspectionComplete != null)
        InspectionComplete(this, e);
    }

    private void RemoteTypeFound(object sender, TypeFoundEventArgs e)
    {
      if (TypeFound != null)
        TypeFound(this, e);
    }

    #endregion

    /// <summary>
    /// Searches for types accepted by the given filter and filter criteria. 
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="filterCriteria">The filter criteria.</param>
    public void FindTypes(string file, TypeFilter filter, object filterCriteria)
    {
      inspector.FindTypes(file, filter, filterCriteria);
    }

    /// <summary>
    /// Searches for types inherited from baseType.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="baseType">The base type.</param>
    public void FindTypes(string file, Type baseType)
    {
      FindTypes(file, baseType, null);
    }

    /// <summary>
    /// Searches for types inherited from baseType and optionally marked with attributes of attributeType.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="baseType">The base type.</param>
    /// <param name="attributeType">The attribute type.</param>
    public void FindTypes(string file, Type baseType, Type attributeType)
    {
      inspector.FindTypes(file, baseType, attributeType);
    }

    private void DomainRecreate(object sender, EventArgs e)
    {
      InitializeRemoteInspector();
    }

    private void InitializeRemoteInspector()
    {
      inspector =
        (RemoteAssemblyInspector)
        domainManager.Domain.CreateInstanceAndUnwrap(typeof (RemoteAssemblyInspector).Assembly.FullName,
                                                     typeof (RemoteAssemblyInspector).FullName);
      inspector.InspectionStart += new EventHandler<AssemblyInspectorEventArgs>(RemoteInspectionStart);
      inspector.InspectionComplete += new EventHandler<AssemblyInspectorEventArgs>(RemoteInspectionComplete);
      inspector.TypeFound += new EventHandler<TypeFoundEventArgs>(RemoteTypeFound);
    }

    #region IDisposable Members

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
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
        if (domainManager != null)
          domainManager.Dispose();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyInspector"/> class.
    /// </summary>
    public AssemblyInspector()
    {
      domainManager = new AppDomainManager();
      domainManager.DomainRecreate += new EventHandler<EventArgs>(DomainRecreate);
      InitializeRemoteInspector();
    }
  }
}