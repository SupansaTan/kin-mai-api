using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Authentication.Model
{
    public static class AWSCredential
    {
        public static string PoolId { get; set; }
        public static string ClientId { get; set; }
        public static string ClientSecret { get; set; }
        public static string Region { get; set; }
        public static string AccessKey { get; set; }
        public static string SecretKey { get; set; }
    }
}
