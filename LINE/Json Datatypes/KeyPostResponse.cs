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
    class KeyPostResponse
    {
        internal static KeyPostResponse FromJSON(string json)
        {
            if (json == null) return null;
            Debug.Print("[KeyPostResponse] Attempting to deserialize JSON response:");
            Debug.Print(json);
            try
            {
                return JsonConvert.DeserializeObject<KeyPostResponse>(json);
            }
            catch (Exception e)
            {
                Debug.Print("[KeyPostResponse] Parsing of KeyPostResponse failed:");
                Debug.Print(e.Message);
                Debug.Print(e.Source);
                Debug.Print(e.StackTrace);
                throw;
            }
            return null;
        }

        [DataMember(Name = "session_key")]
        public string SessionKey
        {
            get; set;
        }

        [DataMember(Name = "rsa_key")]
        public string RSAKey
        {
            get; set;
        }
    }
}
