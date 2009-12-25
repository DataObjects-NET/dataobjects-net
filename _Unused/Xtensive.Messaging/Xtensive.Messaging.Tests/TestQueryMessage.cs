// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.24

using System;

namespace Xtensive.Messaging.Tests
{
  [Serializable]
  public class TestQueryMessage : QueryMessage
  {
    private object data;
    object Data { get { return data; } }

    public TestQueryMessage(object data)
    {
      this.data = data;
    }

  }
}