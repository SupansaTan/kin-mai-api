using Rakmao.External.Mail.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Mail.Interface
{
    public interface IMailRepository
    {
        MailDetailModel GetMailDetailModel(string emailCode, string language);
    }
}
