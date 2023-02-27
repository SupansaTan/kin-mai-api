using CsvHelper;
using KinMai.Mail.Interface;
using Rakmao.External.Mail.Models;
using System.Globalization;

namespace KinMai.Mail.Implement
{
    public class MailRepository: IMailRepository
    {
        private List<MailDetailModel> MailTypeTableReader { get; set; }

        public MailRepository()
        {
            var BaseDirectory = AppContext.BaseDirectory;
            TextReader textReader = File.OpenText(BaseDirectory + @"/Contents/MailTypeEntries.csv");
            MailTypeTableReader = new CsvReader(textReader, CultureInfo.InvariantCulture).GetRecords<MailDetailModel>().ToList();
        }

        public MailDetailModel GetMailDetailModel(string emailCode, string language)
        {
            return MailTypeTableReader.FirstOrDefault(mail => mail.CodeName == emailCode && mail.Language == language);
        }
    }
}
