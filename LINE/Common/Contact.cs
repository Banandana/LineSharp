using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineSharp.Common
{
    class Contact
    {
        private Datatypes.Contact wrapper;
        internal Contact(Datatypes.Contact contact)
        {
            wrapper = contact;
        }
        public string ID
        {
            get { return wrapper.Mid; }
        }

        public string Name
        {
            get { return wrapper.DisplayName; }
        }

        public string StatusMessage
        {
            get { return wrapper.StatusMessage; }
        }

        public string Picture
        {
            get { return wrapper.PicturePath; }
        }

        public string Thumbnail
        {
            get { return wrapper.ThumbnailUrl; }
        }

        public ContactType Type
        {
            get { return (ContactType)wrapper.Type; }
        }
        public ContactStatus Status
        {
            get { return (ContactStatus)wrapper.Status; }
        }
    }
}
