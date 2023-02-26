using System;
using System.Collections.Generic;
using System.Text;

namespace Rakmao.Extenal.Mail.Models
{
    public class MailModel
    {
        public Dictionary<string, string> Parameters { get; set; }
        public string ReceiverEmail { get; set; }
        public string Language { get; set; }
        public List<StreamAttachment> StreamAttachments { get; set; }
    }

    /// <summary>
    /// Class for information about file attachments for mail messages
    /// </summary>
    public class StreamAttachment
    {
        /// <summary>
        /// Creates a new stream attachment information
        /// </summary>
        /// <param name="stream">Stream to add as an attachment</param>
        /// <param name="displayName">Name and extension as the reader of the mail should see it</param>
        /// <param name="mimeType">Mime type of the stream</param>
        public StreamAttachment(byte[] stream, string displayName, string mimeType)
        {
            Stream = stream;
            DisplayName = displayName;
            MimeType = mimeType;
        }

        /// <summary>
        /// Gets the name of the file in the file system
        /// </summary>b
        public byte[] Stream { get; private set; }

        /// <summary>
        /// Gets the name used in the attachment.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the MimeType as a string, like "text/plain" or "application/pdf"
        /// </summary>
        public string MimeType { get; private set; }
    }
}
