using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WCF_Duplex_Chat_Svc
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract (CallbackContract = typeof(IChatCallBack),
                            SessionMode = SessionMode.Required)]
    public interface IDuplex_ChatService
    {
        [OperationContract(IsInitiating = true)]
        bool Connect(ChatUser client);

        [OperationContract(IsOneWay = true)]
        void Say(ChatMessage msg);

        [OperationContract(IsOneWay = true, IsTerminating = true)]
        void Disconnect(ChatUser client);
    }

    public interface IChatCallBack
    {
        [OperationContract(IsOneWay = true)]
        void RefreshConnectedClient(List<ChatUser> clients);

        [OperationContract(IsOneWay = true)]
        void Receive(ChatMessage msg);

        [OperationContract(IsOneWay = true)]
        void UserJoin(ChatUser client);

        [OperationContract(IsOneWay = true)]
        void UserLeave(ChatUser client);

    }

    [DataContract]
    public class ChatMessage
    {
        private ChatUser user;
        [DataMember]
        public ChatUser User
        {
            get { return user; }
            set { user = value; }
        }
        private string message;
        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        private DateTime date;
        [DataMember]
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }
    }

    [DataContract]
    public class ChatUser
    {
        private string userName;
        
        [DataMember]
        public string UserName
        {
            get { return userName; }
            set { userName = value; }
        }
        private DateTime time;
        [DataMember]
        public DateTime Time
        {
            get { return time; }
            set { time = value; }
        }

        public override string ToString()
        {
            return this.UserName;
        }
    }
}
