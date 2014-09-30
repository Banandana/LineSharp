using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineSharp.Common
{
    public class Room
    {
        private Datatypes.Room wrapper;
        internal Room(Datatypes.Room room)
        {
            wrapper = room;
        }
        public string ID
        {
            get { return wrapper.Mid; }
        }

        public List<Contact> Contacts
        {
            get
            {
                List<Contact> contacts = new List<Contact>();

                foreach (Datatypes.Contact C in wrapper.Contacts)
                {
                    contacts.Add(new Contact(C));
                }
                return contacts;
            }
        }

        public bool NotificationsDisabled
        {
            get { return wrapper.NotificationDisabled; }
        }
    }
}
