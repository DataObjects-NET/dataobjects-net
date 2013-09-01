// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;

namespace Xtensive.Orm.Weaver
{
  internal sealed class ConsoleMessageWriter : IMessageWriter
  {
    public void Write(ProcessorMessage message)
    {
      Console.WriteLine(message);
    }
  }
}