using Npgsql.Util;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Npgsql
{
    class NpgsqlServerStatus
    {
        internal static readonly ConcurrentDictionary<string, Status> Cache = new ConcurrentDictionary<string, Status>();

        internal enum Status
        {
            Unknown,
            Down,
            Primary,
            Secondary
        }

        internal static async Task<Status> Load(NpgsqlConnection conn, NpgsqlTimeout timeout, bool async)
        {
            var returnStatus = Status.Unknown;
            try
            {
                var command = conn.CreateCommand();
                command.CommandText = "SELECT pg_is_in_recovery();";
                var recoveryStatus = (bool?)(await command.ExecuteScalarAsync());
                returnStatus = recoveryStatus == false ? Status.Primary : Status.Secondary;
            }
            catch (SocketException)
            {
                returnStatus = Status.Down;
            }
            catch (NpgsqlException)
            {
                returnStatus = Status.Down;
            }

            return returnStatus;
        }

    }
}
