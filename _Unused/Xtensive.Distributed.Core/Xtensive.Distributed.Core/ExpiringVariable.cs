// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.10.12

using System;
using Xtensive.Core.Diagnostics;

namespace Xtensive.Distributed.Core
{
  public class ExpiringVariable<T>
    where T: class
  {
    private T value;
    private readonly TimeSpan defaultExpiration;
    private DateTime validUntil;

    public T Value
    {
      get
      {
        if (!IsValid)
          return null;
        return value;
      }
      set 
      { 
        this.value = value;
        this.validUntil = HighResolutionTime.Now + defaultExpiration;
      }
    }

    public bool IsValid
    {
      get { return HighResolutionTime.Now < ValidUntil; }
    }

    public DateTime ValidUntil
    {
      get { return validUntil; }
    }

    public TimeSpan ValidTimeLeft
    {
      get
      {
        TimeSpan tl = (validUntil - HighResolutionTime.Now);
        return (tl > TimeSpan.Zero) ? tl : TimeSpan.Zero;
      }
    }

    public void Set(T value, DateTime validUntil)
    {
      this.value = value;
      this.validUntil = validUntil;
    }


    // Constructors

    public ExpiringVariable(T value, TimeSpan defaultExpiration)
    {
      this.defaultExpiration = defaultExpiration;
      this.value = value;
      this.validUntil = HighResolutionTime.Now + defaultExpiration;
    }
  }
}