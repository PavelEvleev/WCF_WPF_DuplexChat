using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WCF_Duplex_Chat_Svc
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    [ServiceBehavior (InstanceContextMode = InstanceContextMode.Single,
                            ConcurrencyMode = ConcurrencyMode.Multiple,
                                      UseSynchronizationContext = false)]
    public class Duplex_ChatService : IDuplex_ChatService
    {
        Dictionary<ChatUser, IChatCallBack> clients =
            new Dictionary<ChatUser,IChatCallBack>();
        List<ChatUser> clientList = new List<ChatUser>();

        public IChatCallBack CurrentCallback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<IChatCallBack>();
            }

        }
        object syncObj = new object();

        private bool SearchClientByName(string name)
        {
            foreach(ChatUser c in clients.Keys)
            {
                if (c.UserName == name)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Connect(ChatUser client)
        {
            if (!clients.ContainsValue(CurrentCallback) && !SearchClientByName(client.UserName))
            {
                lock (syncObj)
                {
                    clients.Add(client, CurrentCallback);
                    clientList.Add(client);

                    foreach (ChatUser key in clients.Keys)
                    {
                        IChatCallBack callback = clients[key];
                        try
                        {
                            callback.RefreshConnectedClient(clientList);
                            callback.UserJoin(client);
                        }
                        catch
                        {
                            clients.Remove(key);
                            return false;
                        }

                    }

                }
                return true;
            }
            return false;
        }

        public void Disconnect(ChatUser client)
        {
            foreach(ChatUser c in clients.Keys)
            {
                if (client.UserName == c.UserName)
                {
                    lock (syncObj)
                    {
                        this.clients.Remove(c);
                        this.clientList.Remove(c);
                        foreach(IChatCallBack callback in clients.Values)
                        {
                            callback.RefreshConnectedClient(clientList);
                            callback.UserLeave(client);
                        }
                    }
                    return;
                }
            }
        }

        public void Say(ChatMessage msg)
        {
            lock (syncObj)
            {
                foreach(IChatCallBack callback in clients.Values)
                {
                    callback.Receive(msg);
                }
            }
        }
    }
}
