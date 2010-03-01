// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.07.25

using System;
using System.Runtime.Serialization;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  ///<summary>
  ///</summary>
  [Serializable]
  public class InstanceMustHaveSyncRootException : InvalidOperationException
  {
    ///<summary>
    ///</summary>
    public InstanceMustHaveSyncRootException()
      : base(Strings.ExInstanceMustHaveSyncRoot)
    {
    }
    
    ///<summary>
    ///</summary>
    ///<param name="text"></param>
    public InstanceMustHaveSyncRootException(string text)
      : base(text)
    {
    }

    ///<summary>
    ///</summary>
    ///<param name="info"></param>
    ///<param name="context"></param>
    public InstanceMustHaveSyncRootException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}