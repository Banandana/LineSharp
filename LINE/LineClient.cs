using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Text;
using LineSharp.Datatypes;
using LineSharp.Functions;
using LineSharp.Globals;
using LineSharp.Net;
using Thrift.Protocol;
using Contact = LineSharp.Common.Contact;
using Group = LineSharp.Common.Group;
using Profile = LineSharp.Common.Profile;
using Room = LineSharp.Common.Room;

namespace LineSharp
{
    internal class LineClient
    {
        private readonly OperationHandler _operationHandler;
        private readonly LineTransport _thriftTransport;

        internal TalkService.Client Client;
        //This is internal because other classes will use this client to make
        //thrift calls to get their required information
        //this is hugely important

        public LineClient()
        {
            //Queue of operations for dispatch
            Operations = new Queue<Operation>();
            
            //This shit is for serialization
            _thriftTransport = new LineTransport(Protocol.UserAgent, Protocol.LineApplication);
            TProtocol thriftProtocol = new TCompactProtocol(_thriftTransport);

            //This is the heart and soul of all the function calls here.
            Client = new TalkService.Client(thriftProtocol);

            //This reads for operations/events and reports them where they belong
            _operationHandler = new OperationHandler(this);
        }

        #region Authcode

        public delegate void LoggedInEvent(Result result);

        private string _accesskey;

        private string _email;

        public string Email
        {
            get { return _email; }
        }

        //If the access key is nothing or if it's null, the user is, according to this library
        //not logged in. Don't even bother with haing a logged in boolean or some faggy shit like that.

        public string AccessKey
        {
            get { return _accesskey; }
        }

        public event LoggedInEvent OnLogin;

        public void Login(string Email, string Password)
        {
            _email = Email;
            //Sets the email var for possible future reference, not required atm

            //Create the request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL.Key);

            //It's a post and we're going to send data
            request.Method = "POST";

            //We have no content length because we're not sending data,
            request.ContentLength = 0;

            //Identifies the client
            request.Headers.Add("X-Line-Application: " + Protocol.LineApplication);
            ((HttpWebRequest)request).UserAgent = Protocol.UserAgent;
            HttpWebResponse webResponse = null;

            //Honestly not the best implementation but to be honest, this function itself has never failed.
            try
            {
                //Get the response, which is the key and whatnot to encrypt user auth data
                webResponse = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                if (OnLogin != null) OnLogin.Invoke(Result.UNKNOWN_ERROR);
                return; //Result.UNKNOWN_ERROR
            }


            var reader = new BinaryReader(webResponse.GetResponseStream());
            //Create the stream for the body data

            //The buffer size should be the same as the actual size of the body
            var buffer = new byte[webResponse.ContentLength];

            //Reads all the data from the stream
            reader.Read(buffer, 0, Convert.ToInt32(webResponse.ContentLength));

            //And closes the stream
            webResponse.Close();

            //Converts it to text
            string response = Encoding.UTF8.GetString(buffer);

            //Crude attempt to parse the json because it's likely not to change
            //and if it does, it's a huge problem anyways
            //Don't try to read it, it's really bad

            //Remove brackets
            var fields = new List<string>();
            response = response.Replace("{", "").Replace("}", "");

            for (int i = 0; i < response.Length - 1; i++)
            {
                if (response.Substring(i, 1) == "\"")
                {
                    //Gets the next quote location
                    int nextquote = 0;
                    for (int j = response.Length - 1; j > i; j--)
                    {
                        if (response.Substring(j, 1) == "\"")
                        {
                            nextquote = j;
                        }
                    }
                    string nextfield = (response.Substring(i + 1, nextquote - i - 1));
                    if (nextfield != "," && nextfield != ":") fields.Add(nextfield);
                }
            }

            //If it breaks, fuck it. I'll switch to Newtonsoft's JSON parser but until then, this is one less dependency.

            //The format of the response is:
            //convoluted.

            //It has a session key which uniquely identifies each session, obviously
            //And an RSA key, mod, and exponent used to crypt usernames and passwords before sending it over
            //the net.

            //This is industry standard and in other web services, is done via Javascript.

            string sessionKey = fields[fields.IndexOf("session_key") + 1];
            string rsaData = fields[fields.IndexOf("rsa_key") + 1];

            string keyName = rsaData.Split(',')[0];
            string modulus = rsaData.Split(',')[1]; //Public key
            string exponent = rsaData.Split(',')[2];

            var rsa = new RSACryptoServiceProvider();
            var parameters = new RSAParameters();
            parameters.Exponent = Bytes.GetExponentFromString(exponent);
            parameters.Modulus = Bytes.HexStringToByteArray(modulus);
            rsa.ImportParameters(parameters);

            //The Auth.GenerateAuthCode function simply takes all the required elements and mashes them together in the
            //terrible format required to authenticate to the server.

            //The format is as follows:

            //Session ID length (1 byte)
            //Session ID
            //Username length (1 byte)
            //Username
            //Password length (1 byte)
            //Password

            //This entire payload is sent after being RSA encrypted by the client,
            //and is sent over an SSL connection

            byte[] cypher = rsa.Encrypt(Auth.GenerateAuthCode(sessionKey, Email, Password), false);


            //The talkservice URL is the one responsible for logging users in among other things
            //P4 is for polling, and I don't know what S4 is for. Ask Lumpio.
            _thriftTransport.TargetUrl = URL.TalkService;


            //This is the magic of actually logging in.
            //I honestly have no idea why some of this data is needed.
            LoginResult authResponse = null;
            try
            {
                authResponse =
                    Client.loginWithIdentityCredentialForCertificate(
                        IdentityProvider.LINE, //We're using LineSharp
                        keyName, //This is the username
                        Bytes.GetHexStringFromByteArray(cypher), //Hashed password
                        true, //Keep us logged in
                        Functions.Net.GetLocalIpAddress(), //Location of login
                        Environment.MachineName, //Idenfitier
                        null //Certificate, but who knows what that does
                        );
            }
            catch (TalkException talkException)
            {
                //Auth issues -BESIDES FAILURE-
                Exceptions.Display(talkException);
            }

            //The actual authresponse contains info about wether or not the user authed successfully!
            if (authResponse != null && authResponse.AuthToken != null && authResponse.AuthToken != "")
            {
                //Successfully retrieved an access key
                _accesskey = authResponse.AuthToken;
                _thriftTransport.AccessKey = authResponse.AuthToken;
                _thriftTransport.TargetUrl = URL.TalkService;


                //Don't know which should come first.
                _operationHandler.Start();
                //Starts listening to events, then calls the OnLogin function.
                if (OnLogin != null) OnLogin.Invoke(Result.OK);
                return;
            }

            if (OnLogin != null) OnLogin.Invoke(Result.UNKNOWN_ERROR);
        }

        public void Logout()
        {
            //Literally kill the method of authenticating
            _accesskey = "";

            //Don't listen to events.
            _operationHandler.Stop();

            Client.logout();
            //No fucking idea what this does but it seems cool
        }

        #endregion

        #region Operations/Updating

        internal Queue<Operation> Operations;

        //Here is where the operation bullshit occurs

        internal void RegisterOperation(Operation operation)
        {
            //VERY IMPORTANT
            //THIS IS USED BY THE PUSH CLIENT EXTERNALLY TO SEND THE
            //OPERATIONS TO THIS CLASS

            //nah fuck this is just a proxy
            OperationRecieved(operation);

            //Yeah that's about it, bawss
        }

        internal Operation GetNextOperation()
        {
            Operation ret = null;

            //var opMutex = new Mutex();
            //opMutex.WaitOne();

            if (Operations.Count > 0)
            {
                ret = Operations.Dequeue();
            }

           // opMutex.ReleaseMutex();

            return ret;
        }

        //This is where update code goes, aka dispatching delegates
        public void Update()
        {
            Operation operation = GetNextOperation();
            if (operation == null) return;

#if DEBUG
            
            Console.WriteLine("Operation recieved.");
            Console.WriteLine(">>Type: " + operation.Type);
            Console.WriteLine(">>Oprev: " + operation.Revision);
            Console.WriteLine(">>Param1: " + operation.Param1);
            Console.WriteLine(">>Param2: " + operation.Param2);
            Console.WriteLine(">>Param3: " + operation.Param3);
            if (operation.Message != null)
            {
                Console.WriteLine(">>Message: ");
                Console.WriteLine(">>>Text: " + operation.Message.Text);
                if (operation.Message.ContentMetadata != null)
                {
                    Console.WriteLine(">>>Content-Metadata:");
                    foreach (KeyValuePair<string, string> kvp in operation.Message.ContentMetadata)
                    {
                        Console.WriteLine(">>>>Key = {0}, Value = {1}",
                            kvp.Key, kvp.Value);
                    }
                }
                Console.WriteLine(">>>>CreatedTime: " + operation.Message.CreatedTime);
                Console.WriteLine(">>>>DeliveredTime: " + operation.Message.DeliveredTime);
                Console.WriteLine(">>>ID: " + operation.Message.Id);
                Console.WriteLine(">>>To: " + operation.Message.To);
                Console.WriteLine(">>>From: " + operation.Message.From);
                Console.WriteLine(">>>Type: " + operation.Message.ContentType);
            }

           

            Console.WriteLine();
#endif

            switch (operation.Type)
            {
                case OpType.END_OF_OPERATION:
                    //if (OnEndofOperation != null) OnEndofOperation.Invoke(this, new EndofOperationEventArgs());
                    break;
                case OpType.UPDATE_PROFILE:
                    if (OnUpdateProfile != null) OnUpdateProfile.Invoke(this, new UpdateProfileEventArgs());
                    break;
                case OpType.NOTIFIED_UPDATE_PROFILE:
                    if (OnNotifiedUpdateProfile != null)
                        OnNotifiedUpdateProfile.Invoke(this, new NotifiedUpdateProfileEventArgs(operation.Param1));
                    break;
                case OpType.REGISTER_USERID:
                    if (OnRegisterUserid != null) OnRegisterUserid.Invoke(this, new RegisterUseridEventArgs());
                    break;
                case OpType.ADD_CONTACT:
                    if (OnAddContact != null)
                        OnAddContact.Invoke(this, new UserMidArgs(operation.Param1));
                    break;
                case OpType.NOTIFIED_ADD_CONTACT:
                    if (OnNotifiedAddContact != null)
                        OnNotifiedAddContact.Invoke(this, new UserMidArgs(operation.Param1));
                    break;
                case OpType.BLOCK_CONTACT:
                    if (OnBlockContact != null) OnBlockContact.Invoke(this, new UserMidArgs(operation.Param1));
                    break;
                case OpType.UNBLOCK_CONTACT:
                    if (OnUnblockContact != null) OnUnblockContact.Invoke(this, new UserMidArgs(operation.Param1));
                    break;
                case OpType.NOTIFIED_RECOMMEND_CONTACT:
                    if (OnNotifiedRecommendContact != null)
                        OnNotifiedRecommendContact.Invoke(this, new NotifiedRecommendContactEventArgs());
                    break;
                case OpType.CREATE_GROUP:
                    if (OnCreateGroup != null) OnCreateGroup.Invoke(this, new CreateGroupEventArgs());
                    break;
                case OpType.UPDATE_GROUP:
                    if (OnUpdateGroup != null) OnUpdateGroup.Invoke(this, new UserMidArgs(operation.Param1));
                    break;
                case OpType.NOTIFIED_UPDATE_GROUP:
                    if (OnNotifiedUpdateGroup != null)
                        OnNotifiedUpdateGroup.Invoke(this, new ItemUserMidArgs(operation.Param1, operation.Param2));
                    break;
                case OpType.INVITE_INTO_GROUP:
                    if (OnInviteintoGroup != null) OnInviteintoGroup.Invoke(this, new InviteintoGroupEventArgs(operation.Param1, operation.Param2));
                    break;
                case OpType.NOTIFIED_INVITE_INTO_GROUP:
                    if (OnNotifiedInviteintoGroup != null) OnNotifiedInviteintoGroup.Invoke(this, new NotifiedInviteintoGroupEventArgs());
                    break;
                case OpType.LEAVE_GROUP:
                    if (OnLeaveGroup != null) OnLeaveGroup.Invoke(this, new LeaveGroupEventArgs());
                    break;
                case OpType.NOTIFIED_LEAVE_GROUP:
                    if (OnNotifiedLeaveGroup != null) OnNotifiedLeaveGroup.Invoke(this, new NotifiedLeaveGroupEventArgs());
                    break;
                case OpType.ACCEPT_GROUP_INVITATION:
                    if (OnAcceptGroupInvitation != null) OnAcceptGroupInvitation.Invoke(this, new AcceptGroupInvitationEventArgs());
                    break;
                case OpType.NOTIFIED_ACCEPT_GROUP_INVITATION:
                    if (OnNotifiedAcceptGroupInvitation != null) OnNotifiedAcceptGroupInvitation.Invoke(this, new NotifiedAcceptGroupInvitationEventArgs());
                    break;
                case OpType.KICKOUT_FROM_GROUP:
                    if (OnKickoutfromGroup != null) OnKickoutfromGroup.Invoke(this, new KickoutfromGroupEventArgs());
                    break;
                case OpType.NOTIFIED_KICKOUT_FROM_GROUP:
                    if (OnNotifiedKickoutfromGroup != null) OnNotifiedKickoutfromGroup.Invoke(this, new NotifiedKickoutfromGroupEventArgs());
                    break;
                case OpType.CREATE_ROOM:
                    if (OnCreateRoom != null) OnCreateRoom.Invoke(this, new CreateRoomEventArgs());
                    break;
                case OpType.INVITE_INTO_ROOM:
                    if (OnInviteintoRoom != null) OnInviteintoRoom.Invoke(this, new InviteintoRoomEventArgs());
                    break;
                case OpType.NOTIFIED_INVITE_INTO_ROOM:
                    if (OnNotifiedInviteintoRoom != null) OnNotifiedInviteintoRoom.Invoke(this, new NotifiedInviteintoRoomEventArgs());
                    break;
                case OpType.LEAVE_ROOM:
                    if (OnLeaveRoom != null) OnLeaveRoom.Invoke(this, new LeaveRoomEventArgs());
                    break;
                case OpType.NOTIFIED_LEAVE_ROOM:
                    if (OnNotifiedLeaveRoom != null) OnNotifiedLeaveRoom.Invoke(this, new NotifiedLeaveRoomEventArgs());
                    break;
                case OpType.SEND_MESSAGE:
                    if (OnSendMessage != null) OnSendMessage.Invoke(this, new SendMessageEventArgs());
                    break;
                case OpType.RECEIVE_MESSAGE:
                    if (OnReceiveMessage != null) OnReceiveMessage.Invoke(this, new ReceiveMessageEventArgs(operation.Message));
                    break;
                case OpType.SEND_MESSAGE_RECEIPT:
                    if (OnSendMessageReceipt != null) OnSendMessageReceipt.Invoke(this, new SendMessageReceiptEventArgs());
                    break;
                case OpType.RECEIVE_MESSAGE_RECEIPT:
                    if (OnReceiveMessageReceipt != null) OnReceiveMessageReceipt.Invoke(this, new ReceiveMessageReceiptEventArgs());
                    break;
                case OpType.SEND_CONTENT_RECEIPT:
                    if (OnSendContentReceipt != null) OnSendContentReceipt.Invoke(this, new SendContentReceiptEventArgs());
                    break;
                case OpType.RECEIVE_ANNOUNCEMENT:
                    if (OnReceiveAnnouncement != null) OnReceiveAnnouncement.Invoke(this, new ReceiveAnnouncementEventArgs());
                    break;
                case OpType.CANCEL_INVITATION_GROUP:
                    if (OnCancelInvitationGroup != null) OnCancelInvitationGroup.Invoke(this, new CancelInvitationGroupEventArgs());
                    break;
                case OpType.NOTIFIED_CANCEL_INVITATION_GROUP:
                    if (OnNotifiedCancelInvitationGroup != null) OnNotifiedCancelInvitationGroup.Invoke(this, new NotifiedCancelInvitationGroupEventArgs());
                    break;
                case OpType.NOTIFIED_UNREGISTER_USER:
                    if (OnNotifiedUnregisterUser != null) OnNotifiedUnregisterUser.Invoke(this, new NotifiedUnregisterUserEventArgs());
                    break;
                case OpType.REJECT_GROUP_INVITATION:
                    if (OnRejectGroupInvitation != null) OnRejectGroupInvitation.Invoke(this, new RejectGroupInvitationEventArgs());
                    break;
                case OpType.NOTIFIED_REJECT_GROUP_INVITATION:
                    if (OnNotifiedRejectGroupInvitation != null) OnNotifiedRejectGroupInvitation.Invoke(this, new NotifiedRejectGroupInvitationEventArgs());
                    break;
                case OpType.UPDATE_SETTINGS:
                    if (OnUpdateSettings != null) OnUpdateSettings.Invoke(this, new UpdateSettingsEventArgs());
                    break;
                case OpType.NOTIFIED_REGISTER_USER:
                    if (OnNotifiedRegisterUser != null) OnNotifiedRegisterUser.Invoke(this, new NotifiedRegisterUserEventArgs());
                    break;
                case OpType.INVITE_VIA_EMAIL:
                    if (OnInviteviaEmail != null) OnInviteviaEmail.Invoke(this, new InviteviaEmailEventArgs());
                    break;
                case OpType.NOTIFIED_REQUEST_RECOVERY:
                    if (OnNotifiedRequestRecovery != null) OnNotifiedRequestRecovery.Invoke(this, new NotifiedRequestRecoveryEventArgs());
                    break;
                case OpType.SEND_CHAT_CHECKED:
                    if (OnSendChatChecked != null) OnSendChatChecked.Invoke(this, new SendChatCheckedEventArgs());
                    break;
                case OpType.SEND_CHAT_REMOVED:
                    if (OnSendChatRemoved != null) OnSendChatRemoved.Invoke(this, new SendChatRemovedEventArgs());
                    break;
                case OpType.NOTIFIED_FORCE_SYNC:
                    if (OnNotifiedForceSync != null) OnNotifiedForceSync.Invoke(this, new NotifiedForceSyncEventArgs());
                    break;
                case OpType.SEND_CONTENT:
                    if (OnSendContent != null) OnSendContent.Invoke(this, new SendContentEventArgs());
                    break;
                case OpType.SEND_MESSAGE_MYHOME:
                    if (OnSendMessageMyhome != null) OnSendMessageMyhome.Invoke(this, new SendMessageMyhomeEventArgs());
                    break;
                case OpType.NOTIFIED_UPDATE_CONTENT_PREVIEW:
                    if (OnNotifiedUpdateContentPreview != null) OnNotifiedUpdateContentPreview.Invoke(this, new NotifiedUpdateContentPreviewEventArgs());
                    break;
                case OpType.REMOVE_ALL_MESSAGES:
                    if (OnRemoveAllMessages != null) OnRemoveAllMessages.Invoke(this, new RemoveAllMessagesEventArgs());
                    break;
                case OpType.NOTIFIED_UPDATE_PURCHASES:
                    if (OnNotifiedUpdatePurchases != null) OnNotifiedUpdatePurchases.Invoke(this, new NotifiedUpdatePurchasesEventArgs());
                    break;
                case OpType.DUMMY:
                    if (OnDummy != null) OnDummy.Invoke(this, new DummyEventArgs());
                    break;
                case OpType.UPDATE_CONTACT:
                    if (OnUpdateContact != null) OnUpdateContact.Invoke(this, new UpdateContactEventArgs());
                    break;
                case OpType.NOTIFIED_RECEIVED_CALL:
                    if (OnNotifiedReceivedCall != null) OnNotifiedReceivedCall.Invoke(this, new NotifiedReceivedCallEventArgs());
                    break;
                case OpType.CANCEL_CALL:
                    if (OnCancelCall != null) OnCancelCall.Invoke(this, new CancelCallEventArgs());
                    break;
                case OpType.NOTIFIED_REDIRECT:
                    if (OnNotifiedRedirect != null) OnNotifiedRedirect.Invoke(this, new NotifiedRedirectEventArgs());
                    break;
                case OpType.NOTIFIED_CHANNEL_SYNC:
                    if (OnNotifiedChannelSync != null) OnNotifiedChannelSync.Invoke(this, new NotifiedChannelSyncEventArgs());
                    break;
                case OpType.FAILED_SEND_MESSAGE:
                    if (OnFailedSendMessage != null) OnFailedSendMessage.Invoke(this, new FailedSendMessageEventArgs());
                    break;
                case OpType.NOTIFIED_READ_MESSAGE:
                    if (OnNotifiedReadMessage != null) OnNotifiedReadMessage.Invoke(this, new NotifiedReadMessageEventArgs());
                    break;
                case OpType.FAILED_EMAIL_CONFIRMATION:
                    if (OnFailedEmailConfirmation != null) OnFailedEmailConfirmation.Invoke(this, new FailedEmailConfirmationEventArgs());
                    break;
                case OpType.NOTIFIED_CHAT_CONTENT:
                    if (OnNotifiedChatContent != null) OnNotifiedChatContent.Invoke(this, new NotifiedChatContentEventArgs());
                    break;
                case OpType.NOTIFIED_PUSH_NOTICENTER_ITEM:
                    if (OnNotifiedPushNoticenterItem != null) OnNotifiedPushNoticenterItem.Invoke(this, new NotifiedPushNoticenterItemEventArgs());
                    break;

                default:
                    break;
            }
        }

        internal void OperationRecieved(Operation operation)
        {
           // Mutex opMutex = new Mutex();
           // opMutex.WaitOne();
            Operations.Enqueue(operation);
            //opMutex.ReleaseMutex();
        }

        #endregion

        #region Delegates

        #region Delegate Definitions

        public delegate void AcceptGroupInvitationEvent(object o, AcceptGroupInvitationEventArgs e);

        public delegate void AddContactEvent(object o, UserMidArgs e);

        public delegate void BlockContactEvent(object o, UserMidArgs e);

        public delegate void CancelCallEvent(object o, CancelCallEventArgs e);

        public delegate void CancelInvitationGroupEvent(object o, CancelInvitationGroupEventArgs e);

        public delegate void CreateGroupEvent(object o, CreateGroupEventArgs e);

        public delegate void CreateRoomEvent(object o, CreateRoomEventArgs e);

        public delegate void DummyEvent(object o, DummyEventArgs e);

        public delegate void EndofOperationEvent(object o, EndofOperationEventArgs e);

        public delegate void FailedEmailConfirmationEvent(object o, FailedEmailConfirmationEventArgs e);

        public delegate void FailedSendMessageEvent(object o, FailedSendMessageEventArgs e);

        public delegate void InviteintoGroupEvent(object o, InviteintoGroupEventArgs e);

        public delegate void InviteintoRoomEvent(object o, InviteintoRoomEventArgs e);

        public delegate void InviteviaEmailEvent(object o, InviteviaEmailEventArgs e);

        public delegate void KickoutfromGroupEvent(object o, KickoutfromGroupEventArgs e);

        public delegate void LeaveGroupEvent(object o, LeaveGroupEventArgs e);

        public delegate void LeaveRoomEvent(object o, LeaveRoomEventArgs e);

        public delegate void NotifiedAcceptGroupInvitationEvent(object o, NotifiedAcceptGroupInvitationEventArgs e);

        public delegate void NotifiedAddContactEvent(object o, UserMidArgs e);

        public delegate void NotifiedCancelInvitationGroupEvent(object o, NotifiedCancelInvitationGroupEventArgs e);

        public delegate void NotifiedChannelSyncEvent(object o, NotifiedChannelSyncEventArgs e);

        public delegate void NotifiedChatContentEvent(object o, NotifiedChatContentEventArgs e);

        public delegate void NotifiedForceSyncEvent(object o, NotifiedForceSyncEventArgs e);

        public delegate void NotifiedInviteintoGroupEvent(object o, NotifiedInviteintoGroupEventArgs e);

        public delegate void NotifiedInviteintoRoomEvent(object o, NotifiedInviteintoRoomEventArgs e);

        public delegate void NotifiedKickoutfromGroupEvent(object o, NotifiedKickoutfromGroupEventArgs e);

        public delegate void NotifiedLeaveGroupEvent(object o, NotifiedLeaveGroupEventArgs e);

        public delegate void NotifiedLeaveRoomEvent(object o, NotifiedLeaveRoomEventArgs e);

        public delegate void NotifiedPushNoticenterItemEvent(object o, NotifiedPushNoticenterItemEventArgs e);

        public delegate void NotifiedReadMessageEvent(object o, NotifiedReadMessageEventArgs e);

        public delegate void NotifiedReceivedCallEvent(object o, NotifiedReceivedCallEventArgs e);

        public delegate void NotifiedRecommendContactEvent(object o, NotifiedRecommendContactEventArgs e);

        public delegate void NotifiedRedirectEvent(object o, NotifiedRedirectEventArgs e);

        public delegate void NotifiedRegisterUserEvent(object o, NotifiedRegisterUserEventArgs e);

        public delegate void NotifiedRejectGroupInvitationEvent(object o, NotifiedRejectGroupInvitationEventArgs e);

        public delegate void NotifiedRequestRecoveryEvent(object o, NotifiedRequestRecoveryEventArgs e);

        public delegate void NotifiedUnregisterUserEvent(object o, NotifiedUnregisterUserEventArgs e);

        public delegate void NotifiedUpdateContentPreviewEvent(object o, NotifiedUpdateContentPreviewEventArgs e);

        public delegate void NotifiedUpdateGroupEvent(object o, ItemUserMidArgs e);

        public delegate void NotifiedUpdateProfileEvent(object o, NotifiedUpdateProfileEventArgs e);

        public delegate void NotifiedUpdatePurchasesEvent(object o, NotifiedUpdatePurchasesEventArgs e);

        public delegate void ReceiveAnnouncementEvent(object o, ReceiveAnnouncementEventArgs e);

        public delegate void ReceiveMessageEvent(object o, ReceiveMessageEventArgs e);

        public delegate void ReceiveMessageReceiptEvent(object o, ReceiveMessageReceiptEventArgs e);

        public delegate void RegisterUseridEvent(object o, RegisterUseridEventArgs e);

        public delegate void RejectGroupInvitationEvent(object o, RejectGroupInvitationEventArgs e);

        public delegate void RemoveAllMessagesEvent(object o, RemoveAllMessagesEventArgs e);

        public delegate void SendChatCheckedEvent(object o, SendChatCheckedEventArgs e);

        public delegate void SendChatRemovedEvent(object o, SendChatRemovedEventArgs e);

        public delegate void SendContentEvent(object o, SendContentEventArgs e);

        public delegate void SendContentReceiptEvent(object o, SendContentReceiptEventArgs e);

        public delegate void SendMessageEvent(object o, SendMessageEventArgs e);

        public delegate void SendMessageMyhomeEvent(object o, SendMessageMyhomeEventArgs e);

        public delegate void SendMessageReceiptEvent(object o, SendMessageReceiptEventArgs e);

        public delegate void UnblockContactEvent(object o, UserMidArgs e);

        public delegate void UpdateContactEvent(object o, UpdateContactEventArgs e);

        public delegate void UpdateGroupEvent(object o, UserMidArgs e);

        public delegate void UpdateProfileEvent(object o, UpdateProfileEventArgs e);

        public delegate void UpdateSettingsEvent(object o, UpdateSettingsEventArgs e);

        #endregion

        #region Events
        //Notified Events notify the logged-in user of other users' actions!
        public event EndofOperationEvent OnEndofOperation;
        public event UpdateProfileEvent OnUpdateProfile;
        public event NotifiedUpdateProfileEvent OnNotifiedUpdateProfile;
        public event RegisterUseridEvent OnRegisterUserid;
        public event AddContactEvent OnAddContact;
        public event NotifiedAddContactEvent OnNotifiedAddContact;
        public event BlockContactEvent OnBlockContact;
        public event UnblockContactEvent OnUnblockContact;
        public event NotifiedRecommendContactEvent OnNotifiedRecommendContact;
        public event CreateGroupEvent OnCreateGroup;
        public event UpdateGroupEvent OnUpdateGroup;
        public event NotifiedUpdateGroupEvent OnNotifiedUpdateGroup;
        public event InviteintoGroupEvent OnInviteintoGroup;
        public event NotifiedInviteintoGroupEvent OnNotifiedInviteintoGroup;
        public event LeaveGroupEvent OnLeaveGroup;
        public event NotifiedLeaveGroupEvent OnNotifiedLeaveGroup;
        public event AcceptGroupInvitationEvent OnAcceptGroupInvitation;
        public event NotifiedAcceptGroupInvitationEvent OnNotifiedAcceptGroupInvitation;
        public event KickoutfromGroupEvent OnKickoutfromGroup;
        public event NotifiedKickoutfromGroupEvent OnNotifiedKickoutfromGroup;
        public event CreateRoomEvent OnCreateRoom;
        public event InviteintoRoomEvent OnInviteintoRoom;
        public event NotifiedInviteintoRoomEvent OnNotifiedInviteintoRoom;
        public event LeaveRoomEvent OnLeaveRoom;
        public event NotifiedLeaveRoomEvent OnNotifiedLeaveRoom;
        public event SendMessageEvent OnSendMessage;
        public event ReceiveMessageEvent OnReceiveMessage;
        public event SendMessageReceiptEvent OnSendMessageReceipt;
        public event ReceiveMessageReceiptEvent OnReceiveMessageReceipt;
        public event SendContentReceiptEvent OnSendContentReceipt;
        public event ReceiveAnnouncementEvent OnReceiveAnnouncement;
        public event CancelInvitationGroupEvent OnCancelInvitationGroup;
        public event NotifiedCancelInvitationGroupEvent OnNotifiedCancelInvitationGroup;
        public event NotifiedUnregisterUserEvent OnNotifiedUnregisterUser;
        public event RejectGroupInvitationEvent OnRejectGroupInvitation;
        public event NotifiedRejectGroupInvitationEvent OnNotifiedRejectGroupInvitation;
        public event UpdateSettingsEvent OnUpdateSettings;
        public event NotifiedRegisterUserEvent OnNotifiedRegisterUser;
        public event InviteviaEmailEvent OnInviteviaEmail;
        public event NotifiedRequestRecoveryEvent OnNotifiedRequestRecovery;
        public event SendChatCheckedEvent OnSendChatChecked;
        public event SendChatRemovedEvent OnSendChatRemoved;
        public event NotifiedForceSyncEvent OnNotifiedForceSync;
        public event SendContentEvent OnSendContent;
        public event SendMessageMyhomeEvent OnSendMessageMyhome;
        public event NotifiedUpdateContentPreviewEvent OnNotifiedUpdateContentPreview;
        public event RemoveAllMessagesEvent OnRemoveAllMessages;
        public event NotifiedUpdatePurchasesEvent OnNotifiedUpdatePurchases;
        public event DummyEvent OnDummy;
        public event UpdateContactEvent OnUpdateContact;
        public event NotifiedReceivedCallEvent OnNotifiedReceivedCall;
        public event CancelCallEvent OnCancelCall;
        public event NotifiedRedirectEvent OnNotifiedRedirect;
        public event NotifiedChannelSyncEvent OnNotifiedChannelSync;
        public event FailedSendMessageEvent OnFailedSendMessage;
        public event NotifiedReadMessageEvent OnNotifiedReadMessage;
        public event FailedEmailConfirmationEvent OnFailedEmailConfirmation;
        public event NotifiedChatContentEvent OnNotifiedChatContent;
        public event NotifiedPushNoticenterItemEvent OnNotifiedPushNoticenterItem;

        #endregion

        #endregion

        #region Functions

        public Profile GetProfile()
        {
            return new Profile(Client.getProfile());
        }

        #region Contacts

        public List<string> GetContactIDs()
        {
            return Client.getAllContactIds();
        }

        public List<string> GetBlockedContactIDs()
        {
            return Client.getBlockedContactIds();
        }

        public Contact GetContact(string id)
        {
            return new Contact(Client.getContact(id));
        }

        public List<Contact> GetContacts(List<string> id)
        {
            List<Datatypes.Contact> contacts = Client.getContacts(id);
            var newContacts = new List<Contact>();
            for (int i = 0; i < contacts.Count; i++)
            {
                newContacts.Add(new Contact(contacts[i]));
            }
            return newContacts;
        }

        public void BlockContact(string id)
        {
            Client.blockContact(0, id);
        }

        public void UnblockContact(string id)
        {
            Client.unblockContact(0, id);
        }

        #endregion

        #region Groups

        public void InviteToGroup(string groupId, List<string> contactIDs)
        {
            Client.inviteIntoGroup(0, groupId, contactIDs);
        }

        public List<string> GetGroupIDs()
        {
            return Client.getGroupIdsJoined();
        }

        public List<string> GetGroupInvitedIDs()
        {
            return Client.getGroupIdsInvited();
        }

        public void RejectGroupInvitation(string groupId)
        {
            Client.rejectGroupInvitation(0, groupId);
        }

        public void AcceptGroupInvitation(string groupId)
        {
            Client.acceptGroupInvitation(0, groupId);
        }

        public List<Group> GetGroups(List<string> groupIDs)
        {
            if (groupIDs == null)
            {
                return new List<Group>();
            }
            List<Datatypes.Group> groups = Client.getGroups(groupIDs);

            var newGroups = new List<Group>();


            for (int i = 0; i < groups.Count; i++)
            {
                newGroups.Add(new Group(groups[i]));
            }

            return newGroups;
        }

        public Group CreateGroup(string name, List<string> contactIDs)
        {
            return new Group(Client.createGroup(0, name, contactIDs));
        }

        public void KickFromGroup(string groupId, List<string> contactIDs)
        {
            Client.kickoutFromGroup(0, groupId, contactIDs);
        }

        public void LeaveGroup(string groupId)
        {
            Client.leaveGroup(0, groupId);
        }

        #endregion

        #region Rooms

        public Room GetRoom(string RoomID)
        {
            return new Room(Client.getRoom(RoomID));
        }

        public Room CreateRoom(List<string> ContactIDs)
        {
            return new Room(Client.createRoom(0, ContactIDs));
        }

        public void LeaveRoom(string RoomID)
        {
            Client.leaveRoom(0, RoomID);
        }

        #endregion

        #endregion
    }
}