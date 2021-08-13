using System;
using System.Collections.Generic;
using System.Text;

namespace tacarez_docusign_webhook.Models
{
    public class DocusignRecipientSigner
    {
        public string creationReason { get; set; }
        public string isBulkRecipient { get; set; }
        public string requireUploadSignature { get; set; }
        public string name { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string recipientId { get; set; }
        public string recipientIdGuid { get; set; }
        public string requireIdLookup { get; set; }
        public string userId { get; set; }
        public string routingOrder { get; set; }
        public string status { get; set; }
        public string completedCount { get; set; }
        public DateTime? signedDateTime { get; set; }
        public DateTime? deliveredDateTime { get; set; }
        public string deliveryMethod { get; set; }
        public string recipientType { get; set; }
    }
}
