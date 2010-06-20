// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.31

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.IoC;
using Xtensive.Integrity.Atomicity;
using Xtensive.Integrity.Aspects;
using Xtensive.Integrity.Validation;

namespace Xtensive.Integrity.Tests
{
  [Serializable]
  public class AtomicBase: 
    ISessionBound,
    IAtomicityAware, 
    IValidationAware, 
    ISerializable
  {
    [NonSerialized]
    private Session session;
    private readonly Dictionary<string, object> properties = new Dictionary<string, object>();


    public Session Session
    {
      get { return session; }
    }

    [Changer]
    [Atomic("UndoSetProperty")]
    protected object this[string index] {
      get {
        object value;
        if (properties.TryGetValue(index, out value))
          return value;
        else
          return null;
      }
      set
      {
        IUndoDescriptor undoDescriptor = UndoScope.CurrentDescriptor;
        IDictionary<string, object> undoArguments = undoDescriptor.Arguments;
        object oldValue = null;
        if (properties.TryGetValue(index, out oldValue))
          undoArguments["Value"] = oldValue;
        undoArguments["Index"] = index;
        undoArguments["ExpectedValue"] = value;

        properties[index] = value;

        undoDescriptor.Complete();
      }
    }

    private void UndoSetProperty(IUndoDescriptor undoDescriptor)
    {
      bool validateVersions = (AtomicityScope.CurrentContext.Options & AtomicityContextOptions.Validate)!=0;
      IDictionary<string, object> arguments = undoDescriptor.Arguments;
      string index = (string)arguments["Index"];
      object expectedValue;
      arguments.TryGetValue("ExpectedValue", out expectedValue);
      if (validateVersions && !expectedValue.Equals(this[index]))
        throw new VersionConflictException(this, index, expectedValue, this[index]);

      object value;
      arguments.TryGetValue("Value", out value);
      this[index] = value;
    }

    [Atomic]
    public void DummyAtomicToAtomicToUndoable(int callCount, int nestedCallCount)
    {
      for (int i = 0; i<callCount; i++)
        DummyAtomicToUndoable(nestedCallCount);
    }

    [Atomic]
    public void DummyAtomicToUndoable(int callCount)
    {
      for (int i = 0; i<callCount; i++)
        DummyUndoableAtomic();
    }

    [Atomic("UndoDoNothing")]
    public void DummyUndoableAtomic()
    {
      IUndoDescriptor undoDescriptor = UndoScope.CurrentDescriptor;
      undoDescriptor.Complete();
    }

    private void UndoDoNothing(IUndoDescriptor undoDescriptor)
    {
    }

    #region IValidationAware methods

    public virtual void OnValidate()
    {      
    }

    public bool IsCompatibleWith(ValidationContextBase context)
    {
      return session.ValidationContext==context;
    }
    
    public ValidationContextBase Context
    {
      get { return session.ValidationContext; }
    }
    
    #endregion

    #region IAtomicityAware methods

    public bool IsCompatibleWith(AtomicityContextBase context)
    {
      return session.AtomicityContext==context;
    }

    AtomicityContextBase IContextBound<AtomicityContextBase>.Context {
      get {
        return session.AtomicityContext;
      }
    }

    #endregion


    // Constructors

    public AtomicBase()
    {
      AtomicityContext context = AtomicityScope.CurrentContext as AtomicityContext;
      if (context==null)
        throw Exceptions.ContextRequired(typeof(AtomicityContext), typeof(AtomicityScope));
      session = context.Session;
    }

    protected AtomicBase(SerializationInfo info, StreamingContext streamingContext)
    {
      AtomicityContext context = AtomicityScope.CurrentContext as AtomicityContext;
      if (context==null)
        throw Exceptions.ContextRequired(typeof(AtomicityContext), typeof(AtomicityScope));
      session = context.Session;
      properties = (Dictionary<string, object>) info.GetValue("Properties", typeof (object));
    }

    #region ISerializable Members
    
    #if NET40
    [SecurityCritical]
    #else
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
    #endif
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("Properties", properties);
    }

    #endregion

  }
}