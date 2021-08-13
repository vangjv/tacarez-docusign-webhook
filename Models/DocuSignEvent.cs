using System;
using System.Collections.Generic;
using System.Text;

namespace tacarez_docusign_webhook.Models
{
    public class DocuSignEnvelopeEvents
    {
        public string id { get; set; }
        public string envelopeId { get; set; }
        public string type { get; set; } = "docusign";
        public List<dynamic> eventHistory { get; set; }
    }
}
