using System;
using System.Collections.Generic;
using System.Text;

namespace Rakmao.External.Mail.Models
{
    public class MailDetailModel
    {
        public string CodeName { get; set; }
        public string Email_Sender { get; set; }
        public string SubjectEmail { get; set; }
        public string ReceiverDetail { get; set; }
        public string Email_Receiver_CC { get; set; }
        public string Email_Receiver_BCC { get; set; }
        public string Language { get; set; }
        public string HtmlFile { get; set; }
        public string Name_Sender { get; set; }
        public string CssFile { get; set; }
    }
}
