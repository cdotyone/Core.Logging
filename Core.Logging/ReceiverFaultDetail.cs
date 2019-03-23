using System;
using System.Runtime.Serialization;

namespace Core.Logging
{
    [DataContract(Namespace = "http://schemas.thatindigogirl.com/samples/2006/06")]
    public class ReceiverFaultDetail
    {
        public ReceiverFaultDetail(string message, bool contactAdmin) : this(message, "", contactAdmin)
        {
        }

        public ReceiverFaultDetail(string message, string description, bool contactAdmin)
        {
            Message = message;
            Description = description;
            ContactAdministrator = contactAdmin;
        }

        [DataMember(Name = "Message", IsRequired = true, Order = 0)]
        public string Message { get; set; }

        [DataMember(Name = "Description", IsRequired = false, Order = 1)]
        public string Description { get; set; }

        [DataMember(Name = "ContactAdministrator", IsRequired = true, Order = 2)]
        public bool ContactAdministrator { get; set; }

        [DataMember(Name = "ReferenceID", IsRequired = true, Order = 3)]
        public Guid ReferenceID { get; set; }
    }
}
