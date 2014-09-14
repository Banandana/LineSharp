using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineSharp.Common
{
    public enum ContactType
    {
        MID = 0,
        Phone = 1,
        Email = 2,
        UserID = 3,
        Proximity = 4,
        Group = 5,
        User = 6,
        QRCode = 7,
        PromotionBot = 8,
        Repair = 128,
        Facebook = 2305,
        Sina = 2306,
        Renren = 2307,
        Feixin = 2308,
    }
    public enum ContactStatus
    {
        Unspecified = 0,
        Friend = 1,
        Blocked = 2,
        Recommended = 3,
        RecommendedBlocked = 4,
        Deleted = 5,
        DeletedBlocked = 6,
    }
    public enum ContactRelation
    {
        OneWay = 0,
        Both = 1,
        NotRegistered = 2,
    }
}
