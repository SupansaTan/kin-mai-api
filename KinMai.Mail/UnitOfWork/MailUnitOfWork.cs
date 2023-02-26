using KinMai.Mail.Implement;
using KinMai.Mail.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Mail.UnitOfWork
{
    public class MailUnitOfWork: IMailUnitOfWork
    {
        private IMailRepository _mailRepository;
        private IMailService _mailService;

        public MailUnitOfWork() { }

        public IMailRepository MailRepository
        {
            get { return _mailRepository ?? (_mailRepository = new MailRepository()); }
            set { _mailRepository = value; }
        }
        public IMailService MailService
        {
            get { return _mailService ?? (_mailService = new MailService(MailRepository)); }
            set { _mailService = value; }
        }
    }
}
