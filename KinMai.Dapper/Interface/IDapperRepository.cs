using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Dapper.Interface
{
    public interface IDapperRepository
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql);
        Task<List<IDictionary<string, object>>> QueryRowAsync(string sql);
        Task<int> ExecuteAsync(string sql);
    }
}
