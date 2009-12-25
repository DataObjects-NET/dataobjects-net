// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.10.23

namespace Xtensive.Distributed.Test.ServerService
{
  public enum State
  {
    Stopped = 0x00000001,
    StartPending = 0x00000002,
    StopPending = 0x00000003,
    Running = 0x00000004,
    ContinuePending = 0x00000005,
    PausePending = 0x00000006,
    Paused = 0x00000007,
  }
}