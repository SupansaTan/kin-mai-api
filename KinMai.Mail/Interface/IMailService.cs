using Rakmao.Extenal.Mail.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Mail.Interface
{
    public interface IMailService
    {
        Task SendEmailAsync(string emailCode, MailModel mailModel);
    }
}
