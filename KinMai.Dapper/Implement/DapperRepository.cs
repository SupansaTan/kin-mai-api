using Npgsql;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KinMai.Dapper.Interface;

namespace KinMai.Dapper.Implement
{
    public class DapperRepository : IDapperRepository
    {
        private readonly string ConnectionString;

        public DapperRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }
        internal IDbConnection Connection
        {
            get { return new NpgsqlConnection(ConnectionString); }
        }

        private async Task<T> WithConnectionAsync<T>(Func<IDbConnection, Task<T>> getData)
        {
            using (IDbConnection dbConnection = Connection)
            {
                try
                {
                    dbConnection.Open();
                    var result = await getData(dbConnection);
                    dbConnection.Close();
                    return result;
                }
                catch (TimeoutException ex)
                {
                    dbConnection.Close();
                    throw new ArgumentException(string.Format("{0}.WithConnection() experienced a SQL {1}", GetType().FullName, ex.Message), ex);
                }
                catch (Exception ex)
                {
                    dbConnection.Close();
                    throw new ArgumentException(string.Format("{0}.WithConnection() experienced a SQL exception {1}", GetType().FullName, ex.Message), ex);
                }
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql)
        {
            return await WithConnectionAsync(connection => connection.QueryAsync<T>(sql));
        }
        public async Task<List<IDictionary<string, object>>> QueryRowAsync(string sql)
        {
            return (await WithConnectionAsync(connection => connection.QueryAsync(sql))).Select(x => (IDictionary<string, object>)x).ToList();
        }
        /// <summary>
        /// Excute query doesn't have result , have only number of rows affected.
        /// </summary>
        /// <param name="sql"> The SQL to execute for this query.</param>
        /// <returns> The number of rows affected.</returns>
        public async Task<int> ExecuteAsync(string sql)
        {
            return await WithConnectionAsync(c => c.ExecuteAsync(sql));
        }
    }
}
