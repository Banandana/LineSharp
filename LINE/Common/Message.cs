using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LineSharp.Datatypes;

namespace LineSharp.Common
{
    public class Message
    {
        private Datatypes.Message wrapper;

        public Message()
        {
            ContentMetadata = new Dictionary<string, string>();
            wrapper = new Datatypes.Message();
        }
        internal Message(Datatypes.Message message)
        {
            wrapper = message;
        }

        //The MID of the person who sent the message
        public string Sender
        {
            get { return wrapper.From; }
            set { wrapper.From = value; }
        }
        //The MID of the person recieving it
        public string Reciever
        {
            get { return wrapper.To; }
            set { wrapper.To = value; }
        }
        //This is used to identify what kind of place the message is getting sent to.
        

        public RecieverType Type
        {
            get { return (RecieverType) wrapper.ToType; }
            
            //I don't think this is necissary
            //set { wrapper.ToType = (MIDType) value; }
        }

        //This is a unique message identifier, which is used later to set the 'read' flag on the message, etc
        public string MessageId
        {
            get { return wrapper.Id; }
        }

        //CreatedTime not implemented
        //DeliveredTime not implemented

        public string Text
        {
            get { return wrapper.Text; }
            set { wrapper.Text = value; }
        }

        //Location not implemented

        //public Location Location
        //{
        //    get { return new Location(); }
        //}

        public bool HasContent
        {
            get { return wrapper.HasContent; }

            //Not imp.
        }

        public ContentType ContentType
        {
            get; set;
        }

        public byte[] ContentPreview
        {
            get; set;
        }

        public Dictionary<string, string> ContentMetadata
        {
            get; set;
        }
    }
}
