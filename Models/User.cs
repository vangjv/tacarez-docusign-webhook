using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tacarez_docusign_webhook.Models
{
    public class User
    {
        public string GUID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email{ get; set; }
        public string Role { get; set; }
        public bool IsRegistered { get; set; }

    }
}
