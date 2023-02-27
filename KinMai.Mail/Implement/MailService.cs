using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using KinMai.Mail.Interface;
using Rakmao.Extenal.Mail.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KinMai.Mail.Implement
{
    public class MailService: IMailService
    {
        private readonly IMailRepository MailRepository;
        private readonly AmazonSimpleEmailServiceClient awsSimpleEmailServiceClient;
        static readonly Regex mailRegex = new Regex(@"\{(\w+)\}", RegexOptions.Compiled);

        public MailService(IMailRepository mailRepository, AmazonSimpleEmailServiceClient amazonSimpleEmailServiceClient)
        {
            MailRepository = mailRepository;
            awsSimpleEmailServiceClient = amazonSimpleEmailServiceClient;
        }

        public async Task SendEmailAsync(string emailCode, MailModel mailModel)
        {
            try
            {
                var BaseDirectory = AppContext.BaseDirectory;
                var SendDetail = MailRepository.GetMailDetailModel(emailCode, mailModel.Language);

                if (SendDetail != null)
                {
                    string html = File.ReadAllText($"{BaseDirectory}/Contents/Templates/{SendDetail.HtmlFile}");
                    string css = File.ReadAllText($"{BaseDirectory}/Contents/Css/{SendDetail.CssFile}");

                    // Parsing CSS to html file
                    html = html.Replace("<!--CSS-->", "<style>" + css + "</style>");

                    var senderMail = mailRegex.Replace(SendDetail.SenderEmail, match => mailModel.Parameters[match.Groups[1].Value]);
                    var senderName = mailRegex.Replace(SendDetail.SenderName, match => "{" + mailModel.Parameters[match.Groups[1].Value] + "}");
                    var subject = mailModel.Parameters["SendSubject"];
                    var receiverMail = mailRegex.Replace(mailModel.ReceiverEmail, match => mailModel.Parameters[match.Groups[1].Value]);

                    string templateHtml = html;
                    var contentHtml = mailRegex.Replace(templateHtml, match => mailModel.Parameters[match.Groups[1].Value]);
                    var cleansedHtml = new HtmlCleanser.HtmlCleanser().CleanseFull(contentHtml, true);
                    var plainTextContent = Regex.Replace(cleansedHtml, "<[^>]*>", "");

                    var request = new SendEmailRequest
                    {
                        Destination = new Destination
                        {
                            ToAddresses = new List<string>() { receiverMail }
                        },
                        Message = new Message
                        {
                            Body = new Body
                            {
                                Html = new Content
                                {
                                    Charset = "UTF-8",
                                    Data = cleansedHtml
                                },
                                Text = new Content
                                {
                                    Charset = "UTF-8",
                                    Data = plainTextContent
                                }
                            },
                            Subject = new Content
                            {
                                Charset = "UTF-8",
                                Data = subject
                            }
                        },
                        Source = senderMail
                    };
                    await awsSimpleEmailServiceClient.SendEmailAsync(request);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
