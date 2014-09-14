using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LineSharp.Datatypes;

namespace LineSharp
{
    internal class LineDelegates
    {
        
    }

    public class UserMidArgs
    {
        public UserMidArgs(string mid)
        {
            Mid = mid;
        }

        public string Mid
        {
            get;
            internal set;
        }
    }

    public class ItemUserMidArgs
    {
        public ItemUserMidArgs(string imid, string umid)
        {
            ItemMid = imid;
            UserMid = umid;
        }

        public string ItemMid
        {
            get;
            internal set;
        }

        public string UserMid
        {
            get;
            internal set;
        }
    }

    public class EndofOperationEventArgs : EventArgs
    {
        public EndofOperationEventArgs()
        {

        }
    }
    public class UpdateProfileEventArgs : EventArgs
    {
        public UpdateProfileEventArgs()
        {

        }
    }
    public class NotifiedUpdateProfileEventArgs : EventArgs
    {
        public NotifiedUpdateProfileEventArgs(string mid)
        {
            Mid = mid;
        }

        public string Mid
        {
            get;
            internal set;
        }
    }
    public class RegisterUseridEventArgs : EventArgs
    {
        public RegisterUseridEventArgs()
        {

        }
    }
    public class AddContactEventArgs : EventArgs
    {
        public AddContactEventArgs(string mid)
        {
            Mid = mid;
        }
        public string Mid
        {
            get;
            internal set;
        }
    }
    public class NotifiedAddContactEventArgs : EventArgs
    {
        public NotifiedAddContactEventArgs(string mid)
        {
            Mid = mid;
        }
        public string Mid
        {
            get;
            internal set;
        }
    }
    public class BlockContactEventArgs : EventArgs
    {
        public BlockContactEventArgs(string mid)
        {
            Mid = mid;
        }
        public string Mid
        {
            get;
            internal set;
        }
    }
    public class UnblockContactEventArgs : EventArgs
    {
        public UnblockContactEventArgs(string mid)
        {
            Mid = mid;
        }
        public string Mid
        {
            get;
            internal set;
        }
    }
    public class NotifiedRecommendContactEventArgs : EventArgs
    {
        public NotifiedRecommendContactEventArgs()
        {

        }
    }
    public class CreateGroupEventArgs : EventArgs
    {
        public CreateGroupEventArgs()
        {

        }
    }
    public class UpdateGroupEventArgs : EventArgs
    {
        public UpdateGroupEventArgs(string mid)
        {
            Mid = mid;
        }
        public string Mid
        {
            get;
            internal set;
        }
    }
    public class NotifiedUpdateGroupEventArgs : EventArgs
    {
        public NotifiedUpdateGroupEventArgs(string gmid, string umid)
        {
            GroupMid = gmid;
            UserMid = umid;
        }
        public string GroupMid
        {
            get;
            internal set;
        }
        public string UserMid
        {
            get;
            internal set;
        }
    }
    public class InviteintoGroupEventArgs : EventArgs
    {
        public InviteintoGroupEventArgs(string gmid, string umid)
        {
            GroupMid = gmid;
            UserMid = umid;
        }
        public string GroupMid
        {
            get;
            internal set;
        }
        public string UserMid
        {
            get;
            internal set;
        }
    }
    public class NotifiedInviteintoGroupEventArgs : EventArgs
    {
        public NotifiedInviteintoGroupEventArgs()
        {

        }
    }
    public class LeaveGroupEventArgs : EventArgs
    {
        public LeaveGroupEventArgs()
        {

        }
    }
    public class NotifiedLeaveGroupEventArgs : EventArgs
    {
        public NotifiedLeaveGroupEventArgs()
        {

        }
    }
    public class AcceptGroupInvitationEventArgs : EventArgs
    {
        public AcceptGroupInvitationEventArgs()
        {

        }
    }
    public class NotifiedAcceptGroupInvitationEventArgs : EventArgs
    {
        public NotifiedAcceptGroupInvitationEventArgs()
        {

        }
    }
    public class KickoutfromGroupEventArgs : EventArgs
    {
        public KickoutfromGroupEventArgs()
        {

        }
    }
    public class NotifiedKickoutfromGroupEventArgs : EventArgs
    {
        public NotifiedKickoutfromGroupEventArgs()
        {

        }
    }
    public class CreateRoomEventArgs : EventArgs
    {
        public CreateRoomEventArgs()
        {

        }
    }
    public class InviteintoRoomEventArgs : EventArgs
    {
        public InviteintoRoomEventArgs()
        {

        }
    }
    public class NotifiedInviteintoRoomEventArgs : EventArgs
    {
        public NotifiedInviteintoRoomEventArgs()
        {

        }
    }
    public class LeaveRoomEventArgs : EventArgs
    {
        public LeaveRoomEventArgs()
        {

        }
    }
    public class NotifiedLeaveRoomEventArgs : EventArgs
    {
        public NotifiedLeaveRoomEventArgs()
        {

        }
    }
    public class SendMessageEventArgs : EventArgs
    {
        public SendMessageEventArgs()
        {

        }
    }
    public class ReceiveMessageEventArgs : EventArgs
    {
        public Message Message
        {
            get; set;
        }
        public ReceiveMessageEventArgs(Message m)
        {
            Message = m;
        }
    }
    public class SendMessageReceiptEventArgs : EventArgs
    {
        public SendMessageReceiptEventArgs()
        {

        }
    }
    public class ReceiveMessageReceiptEventArgs : EventArgs
    {
        public ReceiveMessageReceiptEventArgs()
        {

        }
    }
    public class SendContentReceiptEventArgs : EventArgs
    {
        public SendContentReceiptEventArgs()
        {

        }
    }
    public class ReceiveAnnouncementEventArgs : EventArgs
    {
        public ReceiveAnnouncementEventArgs()
        {

        }
    }
    public class CancelInvitationGroupEventArgs : EventArgs
    {
        public CancelInvitationGroupEventArgs()
        {

        }
    }
    public class NotifiedCancelInvitationGroupEventArgs : EventArgs
    {
        public NotifiedCancelInvitationGroupEventArgs()
        {

        }
    }
    public class NotifiedUnregisterUserEventArgs : EventArgs
    {
        public NotifiedUnregisterUserEventArgs()
        {

        }
    }
    public class RejectGroupInvitationEventArgs : EventArgs
    {
        public RejectGroupInvitationEventArgs()
        {

        }
    }
    public class NotifiedRejectGroupInvitationEventArgs : EventArgs
    {
        public NotifiedRejectGroupInvitationEventArgs()
        {

        }
    }
    public class UpdateSettingsEventArgs : EventArgs
    {
        public UpdateSettingsEventArgs()
        {

        }
    }
    public class NotifiedRegisterUserEventArgs : EventArgs
    {
        public NotifiedRegisterUserEventArgs()
        {

        }
    }
    public class InviteviaEmailEventArgs : EventArgs
    {
        public InviteviaEmailEventArgs()
        {

        }
    }
    public class NotifiedRequestRecoveryEventArgs : EventArgs
    {
        public NotifiedRequestRecoveryEventArgs()
        {

        }
    }
    public class SendChatCheckedEventArgs : EventArgs
    {
        public SendChatCheckedEventArgs()
        {

        }
    }
    public class SendChatRemovedEventArgs : EventArgs
    {
        public SendChatRemovedEventArgs()
        {

        }
    }
    public class NotifiedForceSyncEventArgs : EventArgs
    {
        public NotifiedForceSyncEventArgs()
        {

        }
    }
    public class SendContentEventArgs : EventArgs
    {
        public SendContentEventArgs()
        {

        }
    }
    public class SendMessageMyhomeEventArgs : EventArgs
    {
        public SendMessageMyhomeEventArgs()
        {

        }
    }
    public class NotifiedUpdateContentPreviewEventArgs : EventArgs
    {
        public NotifiedUpdateContentPreviewEventArgs()
        {

        }
    }
    public class RemoveAllMessagesEventArgs : EventArgs
    {
        public RemoveAllMessagesEventArgs()
        {

        }
    }
    public class NotifiedUpdatePurchasesEventArgs : EventArgs
    {
        public NotifiedUpdatePurchasesEventArgs()
        {

        }
    }
    public class DummyEventArgs : EventArgs
    {
        public DummyEventArgs()
        {

        }
    }
    public class UpdateContactEventArgs : EventArgs
    {
        public UpdateContactEventArgs()
        {

        }
    }
    public class NotifiedReceivedCallEventArgs : EventArgs
    {
        public NotifiedReceivedCallEventArgs()
        {

        }
    }
    public class CancelCallEventArgs : EventArgs
    {
        public CancelCallEventArgs()
        {

        }
    }
    public class NotifiedRedirectEventArgs : EventArgs
    {
        public NotifiedRedirectEventArgs()
        {

        }
    }
    public class NotifiedChannelSyncEventArgs : EventArgs
    {
        public NotifiedChannelSyncEventArgs()
        {

        }
    }
    public class FailedSendMessageEventArgs : EventArgs
    {
        public FailedSendMessageEventArgs()
        {

        }
    }
    public class NotifiedReadMessageEventArgs : EventArgs
    {
        public NotifiedReadMessageEventArgs()
        {

        }
    }
    public class FailedEmailConfirmationEventArgs : EventArgs
    {
        public FailedEmailConfirmationEventArgs()
        {

        }
    }
    public class NotifiedChatContentEventArgs : EventArgs
    {
        public NotifiedChatContentEventArgs()
        {

        }
    }
    public class NotifiedPushNoticenterItemEventArgs : EventArgs
    {
        public NotifiedPushNoticenterItemEventArgs()
        {

        }
    }

}
