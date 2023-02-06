using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Dapper.Implement
{
    public class StringListTypeHandler<T> : SqlMapper.TypeHandler<List<string>>
    {
        public override List<string> Parse(object value)
        {
            return ((string[])value).ToList();
        }

        public override void SetValue(IDbDataParameter parameter, List<string> value)
        {
            parameter.Value = value.ToArray();
        }
    }
}
