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
    //This section is for the json response when entering a pin.

    [DataContract]
    class PinVerificationResponse
    {
        [DataContract]
        public class PinVerification_Result
        {
            [DataMember(Name = "verifier")]
            public string Verifier
            {
                get; set;
            }

            [DataMember(Name = "authPhase")]
            public string AuthPhase
            {
                get; set;
            }
        }
        internal static PinVerificationResponse FromJSON(string json)
        {
            if (json == null) return null;
            Debug.Print("[PinVerificationResponse] Attempting to deserialize JSON response:");
            Debug.Print(json);
            try
            {
                return JsonConvert.DeserializeObject<PinVerificationResponse>(json);
            }
            catch (Exception e)
            {
                Debug.Print("[PinVerificationResponse] Parsing of PinVerificationResponse failed:");
                Debug.Print(e.Message);
                Debug.Print(e.Source);
                Debug.Print(e.StackTrace);
                throw;
            }
            return null;
        }
        [DataMember(Name = "timestamp")]
        public string Timestamp
        {
            get; set;
        }
        [DataMember(Name = "errorCode")]
        public string ErrorCode
        {
            get; set;
        }
        [DataMember(Name = "errorMessage")]
        public string ErrorMessage
        {
            get; set;
        }
        [DataMember(Name = "result")]
        public PinVerification_Result Result
        {
            get; set;
        }

    }
}
