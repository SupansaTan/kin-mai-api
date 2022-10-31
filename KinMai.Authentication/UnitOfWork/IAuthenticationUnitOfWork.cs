using KinMai.Authentication.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Authentication.UnitOfWork
{
    public interface IAuthenticationUnitOfWork
    {
        IAWSCognitoService AWSCognitoService { get; set; }
    }
}
