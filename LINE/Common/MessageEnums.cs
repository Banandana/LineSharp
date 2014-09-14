using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineSharp.Common
{
    public enum RecieverType
    {
        User = 0,
        Room = 1,
        Group = 2
    }
    public enum ContentType
    {
        None = 0,
        Image,
        Video,
        Audio,
        HTML,
        PDF,
        Call,
        Sticker,
        Presence,
        Gift,
        Groupboard,
        Applink,
        Link,
        Contact,
        File,
        Location,
        PostNotification,
        Rich,
        ChatEvent
    }
}
