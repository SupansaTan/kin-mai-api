using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Authentication.Model
{
    public class TokenResponseModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiredToken { get; set; }
    }
}
