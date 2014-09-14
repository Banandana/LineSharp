namespace LineSharp.Common
{
    internal class Group
    {
        private readonly Datatypes.Group wrapper;

        internal Group(Datatypes.Group group)
        {
            wrapper = group;
        }

        public string ID
        {
            get { return wrapper.Id; }
        }

        public string Name
        {
            get { return wrapper.Name; }
        }

        public string PictureURL
        {
            get { return wrapper.PictureStatus; }
        }

        public bool NotificationsEnabled
        {
            get { return !wrapper.NotificationDisabled; }
        }
    }
}