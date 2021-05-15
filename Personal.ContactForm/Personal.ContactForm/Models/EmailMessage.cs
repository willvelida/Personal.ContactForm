using System;
using System.Collections.Generic;
using System.Text;

namespace Personal.ContactForm.Models
{
    public class EmailMessage
    {
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
    }
}
