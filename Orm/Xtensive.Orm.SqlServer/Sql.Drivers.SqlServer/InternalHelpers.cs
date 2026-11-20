// Copyright (C) 2020-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2020.04.10

using System;
using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using Xtensive.Core;

namespace Xtensive.Sql.Drivers.SqlServer
{
  internal static class InternalHelpers
  {
    private static readonly uint[] PowersOf10 = {
      1,
      10,
      100,
      1000,
      10000,
      100000,
      1000000,
      10000000,
      100000000,
      1000000000
    };

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static bool ShouldRetryOn(Exception ex)
    {
      ArgumentNullException.ThrowIfNull(ex);
      if (ex is SqlException sqlException) {
        foreach (SqlError err in sqlException.Errors) {
          switch (err.Number) {
            // SQL Error Code: 49920
            // Cannot process request. Too many operations in progress for subscription "%ld".
            // The service is busy processing multiple requests for this subscription.
            // Requests are currently blocked for resource optimization. Query sys.dm_operation_status for operation status.
            // Wait until pending requests are complete or delete one of your pending requests and retry your request later.
            case 49920:
            // SQL Error Code: 49919
            // Cannot process create or update request. Too many create or update operations in progress for subscription "%ld".
            // The service is busy processing multiple create or update requests for your subscription or server.
            // Requests are currently blocked for resource optimization. Query sys.dm_operation_status for pending operations.
            // Wait till pending create or update requests are complete or delete one of your pending requests and
            // retry your request later.
            case 49919:
            // SQL Error Code: 49918
            // Cannot process request. Not enough resources to process request.
            // The service is currently busy.Please retry the request later.
            case 49918:
            // SQL Error Code: 41839
            // Transaction exceeded the maximum number of commit dependencies.
            case 41839:
            // SQL Error Code: 41325
            // The current transaction failed to commit due to a serializable validation failure.
            case 41325:
            // SQL Error Code: 41305
            // The current transaction failed to commit due to a repeatable read validation failure.
            case 41305:
            // SQL Error Code: 41302
            // The current transaction attempted to update a record that has been updated since the transaction started.
            case 41302:
            // SQL Error Code: 41301
            // Dependency failure: a dependency was taken on another transaction that later failed to commit.
            case 41301:
            // SQL Error Code: 40613
            // Database XXXX on server YYYY is not currently available. Please retry the connection later.
            // If the problem persists, contact customer support, and provide them the session tracing ID of ZZZZZ.
            case 40613:
            // SQL Error Code: 40501
            // The service is currently busy. Retry the request after 10 seconds. Code: (reason code to be decoded).
            case 40501:
            // SQL Error Code: 40197
            // The service has encountered an error processing your request. Please try again.
            case 40197:
            // SQL Error Code: 10929
            // Resource ID: %d. The %s minimum guarantee is %d, maximum limit is %d and the current usage for the database is %d.
            // However, the server is currently too busy to support requests greater than %d for this database.
            // For more information, see http://go.microsoft.com/fwlink/?LinkId=267637. Otherwise, please try again.
            case 10929:
            // SQL Error Code: 10928
            // Resource ID: %d. The %s limit for the database is %d and has been reached. For more information,
            // see http://go.microsoft.com/fwlink/?LinkId=267637.
            case 10928:
            // SQL Error Code: 10060
            // A network-related or instance-specific error occurred while establishing a connection to SQL Server.
            // The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server
            // is configured to allow remote connections. (provider: TCP Provider, error: 0 - A connection attempt failed
            // because the connected party did not properly respond after a period of time, or established connection failed
            // because connected host has failed to respond.)"}
            case 10060:
            // SQL Error Code: 10054
            // A transport-level error has occurred when sending the request to the server.
            // (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
            case 10054:
            // SQL Error Code: 10053
            // A transport-level error has occurred when receiving results from the server.
            // An established connection was aborted by the software in your host machine.
            case 10053:
            // SQL Error Code: 1205
            // Deadlock
            case 1205:
            // SQL Error Code: 233
            // The client was unable to establish a connection because of an error during connection initialization process before login.
            // Possible causes include the following: the client tried to connect to an unsupported version of SQL Server;
            // the server was too busy to accept new connections; or there was a resource limitation (insufficient memory or maximum
            // allowed connections) on the server. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by
            // the remote host.)
            case 233:
            // SQL Error Code: 121
            // The semaphore timeout period has expired
            case 121:
            // SQL Error Code: 64
            // A connection was successfully established with the server, but then an error occurred during the login process.
            // (provider: TCP Provider, error: 0 - The specified network name is no longer available.)
            case 64:
            // DBNETLIB Error Code: 20
            // The instance of SQL Server you attempted to connect to does not support encryption.
            case 20:
              return true;
              // This exception can be thrown even if the operation completed successfully, so it's safer to let the application fail.
              // DBNETLIB Error Code: -2
              // Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding. The statement has been terminated.
              //case -2:
          }
        }

        return false;
      }

      return ex is TimeoutException;
    }

    internal static unsafe decimal TruncateToNetDecimal(SqlDecimal sqlDecimal)
    {
      var inputData = sqlDecimal.Data;
      var scale = sqlDecimal.Scale;

      if (inputData[3] == 0) {
        if (scale <= 28) {
          return sqlDecimal.Value;
        }
      }
      else if (scale == 0) {
        return sqlDecimal.Value; // throws OverflowException.
      }

      fixed (int* inputS = inputData) {
        var input = (uint*) inputS;
        byte maxZeroCount = 0;
        while (maxZeroCount < scale && (input[maxZeroCount >> 6] & (1 << maxZeroCount)) == 0) {
          maxZeroCount++;
        }

        var realScale = scale;
        var outputData = sqlDecimal.Data;
        fixed (int* outputS = outputData) {
          var output = (uint*) outputS;
          var dividerPow = maxZeroCount > 9 ? (byte) 9 : maxZeroCount;
          if (dividerPow > 5) {
            var divider = PowersOf10[dividerPow];
            for (byte length = 4; realScale >= dividerPow; realScale -= dividerPow) {
              if (DivHasRem(input, ref length, divider)) {
                break;
              }

              output[0] = input[0];
              output[1] = input[1];
              output[2] = input[2];
              output[3] = input[3];
            }
          }

          for (byte length = 4; realScale > 0 && output[3] != 0; realScale--) {
            _ = DivHasRem(output, ref length, 10);
          }

          if (output[3] != 0) {
            return sqlDecimal.Value; // throws OverflowException.
          }

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
          if (bit == 0 && i == 0) {
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
