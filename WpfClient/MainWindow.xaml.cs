using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceModel;
using WpfClient.ServiceReference1;

namespace WpfClient
{
    
    public partial class MainWindow : Window, ServiceReference1.IDuplex_ChatServiceCallback
    {
        ServiceReference1.Duplex_ChatServiceClient proxy = null;
        ServiceReference1.ChatUser receiver = null;
        ServiceReference1.ChatUser localClient = null;

        private delegate void FaultedInvoker();

        Dictionary<ListBoxItem, ServiceReference1.ChatUser> OnlineClients =
            new Dictionary<ListBoxItem, ChatUser>();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Window1_Loaded);
            chatListBoxNames.SelectionChanged += new SelectionChangedEventHandler(chatListBoxNames_SelectionChanged);
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            ShowChat(false);
            ShowLogin(true);
        }

        private void HandleProxy()
        {
            if (proxy != null)
            {
                switch (this.proxy.State)
                {
                    case CommunicationState.Closed:
                        {
                            proxy = null;
                            chatListBoxMsgs.Items.Clear();
                            chatListBoxNames.Items.Clear();
                            loginLabelStatus.Content = "Disconnected";
                            ShowChat(false);
                            ShowLogin(true);
                            loginButtonConnect.IsEnabled = true;
                            break;
                        }
                    case CommunicationState.Closing:
                        break;
                    case CommunicationState.Created:
                        break;
                    case CommunicationState.Faulted:
                        {
                            proxy.Abort();
                            proxy = null;
                            chatListBoxMsgs.Items.Clear();
                            chatListBoxNames.Items.Clear();
                            loginLabelStatus.Content = "Disconnected";
                            ShowChat(false);
                            ShowLogin(true);
                            loginButtonConnect.IsEnabled = true;
                            break;
                        }
                    case CommunicationState.Opened:
                        {
                            ShowChat(true);
                            ShowLogin(false);

                            chatLabelCurrentStatus.Content = "Online";
                            chatLabelCurrentUName.Content = this.localClient.UserName;
                            break;
                        }
                    case CommunicationState.Opening:
                        break;
                    default:
                        break;
                }
            }
        }

        private void Connect()
        {
            if (proxy == null)
            {
                try
                {
                    this.localClient = new ChatUser();
                    this.localClient.UserName = loginTxtBoxUName.Text.ToString();
                    InstanceContext context = new InstanceContext(this);
                    proxy = new Duplex_ChatServiceClient(context);

                    proxy.Open();

                    proxy.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
                    proxy.InnerDuplexChannel.Opened += new EventHandler(InnerDuplexChannel_Opened);
                    proxy.InnerDuplexChannel.Closed += new EventHandler(InnerDuplexChannel_Closed);
                    proxy.ConnectAsync(this.localClient);
                    proxy.ConnectCompleted += new EventHandler<ConnectCompletedEventArgs>(proxy_ConnectCopleted);
                }
                catch (Exception exception)
                {
                    loginTxtBoxUName.Text = exception.Message.ToString();
                    loginLabelStatus.Content = "Offline";
                    loginButtonConnect.IsEnabled = true;
                }

            }
            else
            {
                HandleProxy();
            }
        }

        private void Send()
        {
            if (proxy != null&& chatTxtBoxType.Text!="")
            {
                if (proxy.State == CommunicationState.Faulted)
                {
                    HandleProxy();
                }
                else
                {
                    ChatMessage msg = new ChatMessage();
                    msg.User = this.localClient;
                    msg.Message = chatTxtBoxType.Text.ToString();
                    msg.Date = DateTime.Now;

                    proxy.SayAsync(msg);
                    chatTxtBoxType.Text = "";
                    chatTxtBoxType.Focus();
                }
            }
        }
       
        #region InnerDuplexChannel
        private void InnerDuplexChannel_Closed(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }

        private void InnerDuplexChannel_Opened(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }

        private void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new FaultedInvoker(HandleProxy));
                return;
            }
            HandleProxy();
        }
        #endregion

        #region RefreshConnectedClient
        public void RefreshConnectedClient(List<ChatUser> clients)
        {
            chatListBoxNames.Items.Clear();
            OnlineClients.Clear();
            foreach (ChatUser c in clients)
            {
                ListBoxItem item = MakeItem(c.UserName);
                chatListBoxNames.Items.Add(item);
                OnlineClients.Add(item, c);
            }
        }

        public IAsyncResult BeginRefreshConnectedClient(List<ChatUser> clients, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndRefreshConnectedClient(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Receive
        public void Receive(ChatMessage msg)
        {
            foreach (ChatUser c in this.OnlineClients.Values)
            {
                if (c.UserName == msg.User.UserName)
                {
                    ListBoxItem item = MakeItem(msg.Date.ToLongTimeString() + " "+ msg.User.UserName + " -> " + msg.Message);
                    chatListBoxMsgs.Items.Add(item);
                }
            }
            ScrollViewer sv = FindVisualChild(chatListBoxMsgs);
            sv.LineDown();
        }

        public IAsyncResult BeginReceive(ChatMessage msg, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndReceive(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region UserJoin
        public void UserJoin(ChatUser client)
        {
            ListBoxItem item = MakeItem("------------ " + client.UserName + " joined chat ------------");
            chatListBoxMsgs.Items.Add(item);
            ScrollViewer sv = FindVisualChild(chatListBoxMsgs);
            sv.LineDown();
        }

        public IAsyncResult BeginUserJoin(ChatUser client, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndUserJoin(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region UserLeave
        public void UserLeave(ChatUser client)
        {
            ListBoxItem item = MakeItem("------------ " + client.UserName + " left chat ------------");
            chatListBoxMsgs.Items.Add(item);
            ScrollViewer sv = FindVisualChild(chatListBoxMsgs);
            sv.LineDown();
        }

        public IAsyncResult BeginUserLeave(ChatUser client, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndUserLeave(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
        #endregion

        private void ShowLogin(bool show)
        {
            if (show)
            {
                loginButtonConnect.Visibility = Visibility.Visible;
                loginLabelStatus.Visibility = Visibility.Visible;
                loginLabelTitle.Visibility = Visibility.Visible;
                loginLabelUName.Visibility = Visibility.Visible;
                loginPolyLine.Visibility = Visibility.Visible;
                loginTxtBoxUName.Visibility = Visibility.Visible;
            }
            else
            {
                loginButtonConnect.Visibility = Visibility.Collapsed;
                loginLabelStatus.Visibility = Visibility.Collapsed;
                loginLabelTitle.Visibility = Visibility.Collapsed;
                loginLabelUName.Visibility = Visibility.Collapsed;
                loginPolyLine.Visibility = Visibility.Collapsed;
                loginTxtBoxUName.Visibility = Visibility.Collapsed;
            }
        }
       
        private void ShowChat(bool show)
        {
            if (show)
            {
                chatButtonDisconnect.Visibility = Visibility.Visible;
                chatButtonSend.Visibility = Visibility.Visible;
                chatCurrentImage.Visibility = Visibility.Visible;
                chatLabelCurrentStatus.Visibility = Visibility.Visible;
                chatLabelCurrentUName.Visibility = Visibility.Visible;
                chatListBoxMsgs.Visibility = Visibility.Visible;
                chatListBoxNames.Visibility = Visibility.Visible;
                chatTxtBoxType.Visibility = Visibility.Visible;
            }
            else
            {
                chatButtonDisconnect.Visibility = Visibility.Collapsed;
                chatButtonSend.Visibility = Visibility.Collapsed;
                chatCurrentImage.Visibility = Visibility.Collapsed;
                chatLabelCurrentStatus.Visibility = Visibility.Collapsed;
                chatLabelCurrentUName.Visibility = Visibility.Collapsed;
                chatListBoxMsgs.Visibility = Visibility.Collapsed;
                chatListBoxNames.Visibility = Visibility.Collapsed;
                chatTxtBoxType.Visibility = Visibility.Collapsed;
            }
        }

        void proxy_ConnectCopleted(object sender, ConnectCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                loginLabelStatus.Foreground = new SolidColorBrush(Colors.Red);
                loginTxtBoxUName.Text = e.Error.Message.ToString();
                loginButtonConnect.IsEnabled = true;
            }
            else if (e.Result)
            {
                HandleProxy();
            }
            else if (!e.Result)
            {
                loginLabelStatus.Content = "Name found";
                loginButtonConnect.IsEnabled = true;
            }
        }

        private ScrollViewer FindVisualChild(DependencyObject obj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is ScrollViewer)
                {
                    return (ScrollViewer)child;
                }
                else
                {
                    ScrollViewer childOfChild = FindVisualChild(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        private ListBoxItem MakeItem(string text)
        {
            ListBoxItem item = new ListBoxItem();

            TextBlock txtblock = new TextBlock();
            txtblock.Text = text;
            txtblock.VerticalAlignment = VerticalAlignment.Center;

            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.Children.Add(item);
            panel.Children.Add(txtblock);

            ListBoxItem bigItem = new ListBoxItem();
            bigItem.Content = panel;

            return bigItem;
        }

        #region UI_Event

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (proxy != null)
            {
                if (proxy.State == CommunicationState.Opened)
                {
                    proxy.Disconnect(this.localClient);
                }
                else
                {
                    HandleProxy();
                }
            }
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            loginButtonConnect.IsEnabled = false;
            loginLabelStatus.Content = "Connecting..";
            proxy = null;
            Connect();

        }
       
        private void chatButtonSend_Click(object sender, RoutedEventArgs e)
        {
            Send();
        }

        private void chatButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (proxy != null)
            {
                if (proxy.State == CommunicationState.Faulted)
                {
                    HandleProxy();
                }
                else
                {
                    proxy.DisconnectAsync(this.localClient);
                }
            }
        }

        void chatListBoxNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem item = chatListBoxNames.SelectedItem as ListBoxItem;
            if (item != null)
            {
                this.receiver = this.OnlineClients[item];
            }
        }
        #endregion

        #region IChatCallback Members
        public void RefreshClients(List<ChatUser> clients)
        {
            chatListBoxNames.Items.Clear();
            OnlineClients.Clear();
            foreach (ChatUser c in clients)
            {
                ListBoxItem item = MakeItem(c.UserName);
                chatListBoxNames.Items.Add(item);
                OnlineClients.Add(item, c);
            }
        }
        #endregion
    }

}
