using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using LineSharp.Globals;
using Newtonsoft.Json;

namespace LineSharp.Json_Datatypes
{
    [DataContract]
    class PinVerificationResponse
    {
        internal static PinVerificationResponse FromJSON(string json)
        {
            if (json == null) return null;
            Debug.Print("[PinVerificationResponse] Attempting to deserialize JSON response:");
            Debug.Print(json);
            return JsonConvert.DeserializeObject<PinVerificationResponse>(json);
        }
        [DataMember(Name = "timestamp")]
        string Timestamp
        {
            get; set;
        }
        [DataMember(Name = "errorCode")]
        string ErrorCode
        {
            get; set;
        }
        [DataMember(Name = "errorMessage")]
        string ErrorMessage
        {
            get; set;
        }
        [DataMember(Name = "verifier")]
        string Verifier
        {
            get; set;
        }
    }
}
