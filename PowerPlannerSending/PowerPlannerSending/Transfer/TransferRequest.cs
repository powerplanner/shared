using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ToolsPortable;

namespace PowerPlannerSending.Transfer
{
    [DataContract]
    public class TransferRequest
    {
        [DataMember]
        public string Username;

        /// <summary>
        /// The password with just Encryption.Salt
        /// </summary>
        [DataMember]
        public byte[] PasswordBefore;
    }

    [DataContract]
    public class TransferResponse : PlainResponse
    {
        //nothing extra
    }
}
