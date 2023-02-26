using System;
using System.Collections.Generic;
using System.Text;

namespace Rakmao.External.Mail.Models
{
    public class MailDetailModel
    {
        public string CodeName { get; set; }
        public string SenderEmail { get; set; }
        public string SubjectEmail { get; set; }
        public string ReceiverDetail { get; set; }
        public string ReceiverEmail { get; set; }
        public string Language { get; set; }
        public string HtmlFile { get; set; }
        public string SenderName { get; set; }
        public string CssFile { get; set; }
    }
}
