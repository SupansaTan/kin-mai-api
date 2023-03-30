using KinMai.Mail.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Mail.UnitOfWork
{
    public interface IMailUnitOfWork
    {
        public IMailRepository MailRepository { get; set; }
        public IMailService MailService { get; set; }
    }
}
