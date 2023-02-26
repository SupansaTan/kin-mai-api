using KinMai.Mail.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KinMai.Mail.Implement
{
    public class MailService: IMailService
    {
        private readonly IMailRepository MailRepository;
        static readonly Regex mailRegex = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);

        public MailService(IMailRepository mailRepository)
        {
            MailRepository = mailRepository;
        }

    }
}
