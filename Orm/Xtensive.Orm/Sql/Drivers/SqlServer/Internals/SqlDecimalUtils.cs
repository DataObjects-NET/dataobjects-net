// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.03.15

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace Xtensive.Sql.Drivers.SqlServer.Internals
{
  internal class SqlDecimalUtils
  {
    private static readonly uint[] PowersOf10 = Enumerable.Range(0, 10).Select(x => (uint)Math.Pow(10, x)).ToArray();

    internal static unsafe decimal TruncateToNetDecimal(SqlDecimal sqlDecimal)
    {
      var inputData = sqlDecimal.Data;
      var scale = sqlDecimal.Scale;

      if (inputData[3]==0) {
        if (scale <= 28)
          return sqlDecimal.Value;
      }
      else if (scale==0) {
        return sqlDecimal.Value; // throws OverflowException.
      }

      fixed (int* inputS = inputData) {
        var input = (uint*) inputS;
        byte maxZeroCount = 0;
        while (maxZeroCount < scale && (input[maxZeroCount >> 6] & (1 << maxZeroCount))==0)
          maxZeroCount++;

        var realScale = scale;
        var outputData = sqlDecimal.Data;
        fixed (int* outputS = outputData) {
          var output = (uint*) outputS;
          var dividerPow = maxZeroCount > 9 ? (byte) 9 : maxZeroCount;
          if (dividerPow > 5) {
            var divider = PowersOf10[dividerPow];
            for (byte length = 4; realScale >= dividerPow; realScale -= dividerPow) {
              if (DivHasRem(input, ref length, divider))
                break;

              output[0] = input[0];
              output[1] = input[1];
              output[2] = input[2];
              output[3] = input[3];
            }
          }

          for (byte length = 4; realScale > 0 && output[3]!=0; realScale--)
            DivHasRem(output, ref length, 10);

          if (output[3]!=0)
            return sqlDecimal.Value; // throws OverflowException.

          return new decimal(outputS[0], outputS[1], outputS[2], !sqlDecimal.IsPositive, realScale);
        }
      }
    }

    private static unsafe bool DivHasRem(uint* bits, ref byte length, uint divider)
    {
      unchecked {
        ulong rem = 0;
        var tempBits = bits + length;
        for (byte i = 0; i < length; i++) {
          var bit = *(--tempBits);
          if (bit==0 && i==0) {
            length--;
            i--;
            continue;
          }

          var num = (rem << 32) + bit;
          bit = (uint) (num / divider);
          rem = (uint) (num - (ulong) bit * divider);
          *tempBits = bit;
        }

        return rem > 0;
      }
    }
  }
}
