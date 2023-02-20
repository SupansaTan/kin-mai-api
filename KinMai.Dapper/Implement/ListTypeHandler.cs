using Dapper;
using System.Data;

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

    public class IntListTypeHandler<T> : SqlMapper.TypeHandler<List<int>>
    {
        public override List<int> Parse(object value)
        {
            return ((int[])value).ToList();
        }

        public override void SetValue(IDbDataParameter parameter, List<int> value)
        {
            parameter.Value = value.ToArray();
        }
    }
}
