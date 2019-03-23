using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Core.Logging
{
    [DataContract(Namespace = "http://schemas.thatindigogirl.com/samples/2006/06")]
    public class SenderFaultDetail
    {
        public SenderFaultDetail(string message, List<string> bodyElements)
            : this(message, "", bodyElements)
        {
        }

        public SenderFaultDetail(string message)
            : this(message, "", null)
        {
        }

        public SenderFaultDetail(string message, string description, List<string> bodyElements)
        {
            Message = message;
            Description = description;
            FailedBodyElements = bodyElements ?? new List<string>();
        }

        [DataMember(Name = "Message", IsRequired = true, Order = 0)]
        public string Message { get; set; }

        [DataMember(Name = "Description", IsRequired = false, Order = 1)]
        public string Description { get; set; }

        [DataMember(Name = "FailedBodyElements", IsRequired = true, Order = 2)]
        public List<string> FailedBodyElements { get; set; }

        [DataMember(Name = "ReferenceID", IsRequired = true, Order = 3)]
        public Guid ReferenceID { get; set; }
    }
}
