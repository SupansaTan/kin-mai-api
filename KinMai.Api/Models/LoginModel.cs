using System.ComponentModel.DataAnnotations;

namespace KinMai.Api.Models
{
    public class LoginModel
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
