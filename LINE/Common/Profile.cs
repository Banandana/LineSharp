namespace LineSharp.Common
{
    public class Profile
    {
        private readonly Datatypes.Profile wrapper;

        public Profile(Datatypes.Profile profile)
        {
            wrapper = profile;
        }

        public string Mid
        {
            get { return wrapper.Mid; }
        }

        public string Username
        {
            get { return wrapper.Userid; }
        }

        //public string PhoneNumber
        //{
        //    get
        //    {
        //        return wrapper.Phone;
        //    }
        //}

        public string Name
        {
            get { return wrapper.DisplayName; }
        }

        public string PhoneticName
        {
            get { return wrapper.PhoneticName; }
        }

        public string StatusMessage
        {
            get { return wrapper.StatusMessage; }
        }

        public string Email
        {
            get { return wrapper.Email; }
        }

        public bool EmailLookupAllowed
        {
            get { return wrapper.AllowSearchByEmail; }
        }

        public bool UsernameLookupAllowed
        {
            get { return wrapper.AllowSearchByUserid; }
        }

        public string ThumbnailUrl
        {
            get { return wrapper.ThumbnailUrl; }
        }

        public string PictureUrl
        {
            get { return wrapper.PicturePath; }
        }
    }
}