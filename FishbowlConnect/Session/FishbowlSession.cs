using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FishbowlConnect.Helpers.Conversions;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.Threading;
using static FishbowlConnect.FBHelperClasses;
using System.Runtime.CompilerServices;
using System.Data;
using FishbowlConnect.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using FishbowlConnect.QueryClasses;
using FishbowlConnect.Json.CsvClassMaps;
using FishbowlConnect.Json.Imports;
using FishbowlConnect.Exceptions;
using FishbowlConnect.Json.Requests;
using Newtonsoft.Json.Serialization;
using FishbowlConnect.Json.APIObjects;
using FishbowlConnect.Json.QueryClasses;
using FishbowlConnect.MySQL;
using FishbowlConnect.Interfaces;
using FishbowlConnect.Logging;
using FishbowlConnect.Helpers;

namespace FishbowlConnect
{
    //on construction of the FBSession class, start the socket client to make sure there arent any errors before trying to connect
    public partial class FishbowlSession : IDisposable, INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogProvider.For<FishbowlSession>();


        public SessionConfig Config { get; private set; }



        //private const string _defaultIPAddress = "127.0.0.1";
        private const int _defaultPort = 28192;

        private LoginMethod RequestFormat;

        /// <summary>
        /// Set a flag for the receiveCallback to tell it to serialize the response of not. If we are just getting raw responses, skip the serialization process
        /// </summary>
        private bool RequestingRaw = false;

        Timer connectTimer;
        int timeoutFlag;

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string property = null)
        {
            var propChanged = PropertyChanged;
            if (propChanged != null)
            {
                propChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #region Connect/Login methods


        private IPEndPoint fishbowlServer { get; set; }


        #region Network_Socket

        private Socket _client;

        private AutoResetEvent connectDoneAuto =
            new AutoResetEvent(false);
        private AutoResetEvent sendDoneAuto =
            new AutoResetEvent(false);
        private AutoResetEvent receiveDoneAuto =
            new AutoResetEvent(false);


        /// <summary>
        /// The last raw response from the remote device.  
        /// </summary>
        public string serverResponse = string.Empty;

        private EndianBinaryWriteConverter _endianBinaryConverter;


        private class StateObject
        {
            // Client socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 4096;
            // Receive buffer.  
            public byte[] buffer = new byte[BufferSize];
            // Received data string.  
            public StringBuilder sb = new StringBuilder();
            //byte array to hold the Big endian message length
            public byte[] firstFourBytes = null;

            //public Type responseType;
        }

        //public async Task Reset()
        //{
        //    try
        //    {
        //        //reset the sesison to a new state and restart the client
        //        _disposed = false; //reset the socket state for reconnects
        //        Interlocked.Exchange(ref timeoutFlag, 0);//reset the timeout connect flag to 0 for the next round
        //        await StartClient();
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //        throw;
        //    }
        //}

        /// <summary>
        /// Creates a new socket to the server endpoint and calls BeginConnect. 
        /// Also starts a timer to EndConnect if it cant establish a connection in the time allotted.
        /// </summary>
        /// <returns>Nothing</returns>
        private void StartClient()
        {
            Logger.Debug("Starting Client...");
            //_disposed = false; //reset the socket state for reconnects

            // Connect to a remote device. 
            if (fishbowlServer == null)
            {
                throw new ArgumentNullException("Server Endpoint cannot be null");
            }
            try
            {
                //Establish the remote endpoint for the socket.  
                //IPEndPoint remoteEP = new IPEndPoint(ServerAddress, ServerPort);

                // Create a TCP/IP socket.  
                //Socket 
                _client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);


                // Connect to the remote endpoint.  
                //_client.BeginConnect(fishbowlServer,
                //    new AsyncCallback(ConnectCallback), _client);

                IAsyncResult result = _client.BeginConnect(fishbowlServer,
                    new AsyncCallback(ConnectCallback), _client);

                if (!result.IsCompleted)
                {
                    //still thinking, not immediate return so start the timer countdown
                    connectTimer = new Timer(connectTimeOut, null, 4000, Timeout.Infinite);

                }

            }
            catch (ArgumentNullException e)
            {
                Logger.Error(e.Message);
            }
            catch (Exception e)
            {

                IsConnected = false;
                IsAuthenticated = false;

                Logger.Error(e, "Unknown Error starting client");

                Disconnect();
                //Remove 3-15
                //Dispose();

                //throw new FishbowlException("Start Client Error", ex);
            }

        }

        private void ConnectCallback(IAsyncResult ar)
        {
            //this should fire as soon as we close the socket on the timeout 
            if (!_disposed) //if the connect timeout is hit before this is connected the socket is disposed and this is called
            {
                if (Interlocked.CompareExchange(ref timeoutFlag, 1, 0) != 0)
                {
                    // the flag was set by the timeout method, so return immediately.
                    return;
                }

                //the timeout timer wasnt hit so we are connected, dispose of the timer to keep it from firing

                connectTimer?.Dispose();
                Logger.Debug("Connect timer disposed in callback method");

                try
                {
                    // Retrieve the socket from the state object. 
                    Socket client = (Socket)ar.AsyncState;

                    // Complete the connection.  
                    client.EndConnect(ar);

                    IsConnected = true;

                    

                    Logger.Debug(string.Format("Socket connected to {0}",
                        client.RemoteEndPoint.ToString()));



                    //Debug.WriteLine("Disposed Status: " + _disposed.ToString());
                    // Create the state object.  
                    StateObject state = new StateObject();
                    state.workSocket = client;

                    _endianBinaryConverter = new EndianBinaryWriteConverter(EndianBitConverter.Big);

                    // Signal that the connection has been made.  


                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                    Logger.Debug("Begin Receive - Listening for incoming info");

                    connectDoneAuto.Set(); //done connecting

                }

                catch (Exception ex)
                {
                    connectDoneAuto.Set();
                    Logger.Debug(ex, "Unknown Error in Connect Callback");

                    //Debug.WriteLine(ex.Message);

                    Disconnect();

                    //TODO: need a way to signal the calling method
                }
            }
        }

        private void connectTimeOut(object obj)
        {
            if (Interlocked.CompareExchange(ref timeoutFlag, 2, 0) != 0)
            {
                //if it doesnt equal 0 then it has to equal 1 which means
                // the flag was set in the ConnectCallback so we got a response, so return immediately.
                return;
            }

            // we set the flag to 2, indicating a timeout was hit, so we need to dispose
            Logger.Debug("Connect Timed Out");

            connectTimer.Dispose();

            connectDoneAuto.Set(); //return the control back to the caller (the login method most likely)

            //Disconnect();
            //removed 3-15
            //Dispose();
        }

        //private void Receive(Socket client)
        //{
        //    try
        //    {
        //        // Create the state object.  
        //        StateObject state = new StateObject();
        //        state.workSocket = client;

        //        // Begin receiving the data from the remote device.  
        //        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        //            new AsyncCallback(ReceiveCallback), state);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new FishbowlException("Socket Receive Error", ex);
        //    }
        //}


        /// <summary>
        /// Async callback for the BeginReceive call on the socket. 
        /// Recieved data is serialized based on XML type or Json type
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                String content = String.Empty;
                ASCIIEncoding encoding = new ASCIIEncoding();

                if (!_disposed) //check to make sure socket is not disposed (happens on socket close)
                {
                    Logger.Debug("Receiving data");
                    // Retrieve the state object and the handler socket  
                    // from the asynchronous state object.  

                    StateObject state = (StateObject)ar.AsyncState;
                    Socket handler = state.workSocket;

                    //Debug.WriteLine("Receiving from: " + handler.RemoteEndPoint.ToString());

                    // Read data from the client socket.   
                    // start with the response length

                    int bytesRead = handler.EndReceive(ar);
                    //Debug.WriteLine(bytesRead);

                    if (bytesRead > 0)
                    {
                        if (state.firstFourBytes == null)
                        {
                            //havent gotten the stream length yet
                            state.firstFourBytes = new byte[4];
                            Buffer.BlockCopy(state.buffer, 0, state.firstFourBytes, 0, 4);
                            //Debug.WriteLine(EndianBitConverter.Big.ToInt32(state.firstFourBytes, 0));

                            if ((state.buffer.Length - 4) > 0)
                            {
                                //length is combined with the stream instead of a separate read
                                //copy buffer to new array minus the first four bytes
                                state.sb.Append(encoding.GetString(
                                    state.buffer, 4, bytesRead - 4));
                            }

                            else
                            {
                                Logger.Debug("Incoming FB Message");
                            }

                        }
                        else
                        {
                            // There  might be more data, so store the data received so far.  
                            state.sb.Append(encoding.GetString(
                                state.buffer, 0, bytesRead));
                        }

                        // Check for end-of-file tag. If it is not there, read   
                        // more data.  
                        content = state.sb.ToString();

                        bool IsJson = false;

                        //we need to check for both XML and Json now
                        if (content.Contains("</FbiXml>") || (IsJson = IsValidJson(content)))
                        {
                            //Logger.Debug("Full message received");
                            // All the data has been read from the   
                            // client. 
                            serverResponse = state.sb.ToString();

                            //deseralize the response and respond appropriately

                            Logger.Trace("Response received : {0}", serverResponse);

                            // Write the response to the console.  
                            Debug.WriteLine("Response received : {0}", serverResponse);


                            if (!RequestingRaw)
                            {

                                Logger.Debug("Processing response...");
                                //handle XML or JSON different
                                if (IsJson)
                                {
                                    LastResponseJson = serverResponse;
                                    FbiJson response = DeserializeFBServerResponseFromJsonString<FbiJson>(serverResponse);
                                    SessionKey = response.Ticket.Key;
                                    UserId = response.Ticket.UserID;
                                    ResponseJson = response.FbiMsgsRs;

                                }
                                else
                                {
                                    LastResponseXML = serverResponse;
                                    FbiXml response = DeserializeFromXMLString<FbiXml>(serverResponse);

                                    SessionKey = response.Ticket.Key; //update the key on the response. This is how we can use the loginRq and still get the key
                                    ResponseXML = (FbiMsgsRs)response.Item;

                                    //LastFbMsgRsStatusCode = _lastFbiMsgsRs.statusCode;
                                }


                                //we have the status, check for server disconnects and handle accordingly
                                if (LastFbMsgRsStatusCode == "1009" || LastFbMsgRsStatusCode == "1010")
                                {
                                    Logger.Debug("Server shutdown or you have been logged off");
                                    throw new FishbowlConnectionException(Utilities.StatusCodeMessage(LastFbMsgRsStatusCode), null, LastFbMsgRsStatusCode);

                                }

                            }

                            //receiveDoneAutoAsync.Set();
                            receiveDoneAuto.Set();
                            //receiveDone.Set();
                            state.sb = new StringBuilder();
                            state.firstFourBytes = null;
                            //receiveDone.Reset();
                            Logger.Debug("Receive Done");
                        }

                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                              new AsyncCallback(ReceiveCallback), state);
                        Logger.Debug("Begin receive restarted");
                    }
                    else
                    {
                        Logger.Debug("Receive reset event set");
                        //receiveDoneAutoAsync.Set();
                        receiveDoneAuto.Set();
                        //receiveDone.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is FishbowlConnectionException)
                {
                    LastFBException = (FishbowlConnectionException)ex; 
                }
                else if (ex is JsonSerializationException)
                {
                    LastFBException = new FishbowlSerializationException("Serialization Error", ex);
                                       
                }
                else
                {
                    //regular exception
                    if (ex.HResult == -2146232798)
                    {
                        //socket disposed exception
                        Debug.WriteLine("Socket disposed");
                        Logger.Debug("Socket Disposed, triggered in receive callback after socket disposed in disposing method");
                        //LastFBException = new FishbowlException("Socket Disposed, triggered in receive callback after socket disposed in disposing method", ex);
                        ((StateObject)ar.AsyncState).workSocket.Close(); //close the old socket


                    }
                    else
                    {
                        Debug.WriteLine("Unrecoverable error " + ex.Message);

                        LastFBException = new FishbowlException("Unknown Error in receive callback", ex);

                    }

                }


            }

        }

        /// <summary>
        /// Check if supplied string is a valid Json documnet
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns>true if valid, false if not</returns>
        private bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    Logger.Debug(jex, "Error parsing JSON");
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    Logger.Debug(ex, "Unknown error checking JSON");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method to send data to the connected server. Calls BeginSend on the socket
        /// </summary>
        /// <param name="client"></param>
        /// <param name="data"></param>
        private void Send(Socket client, String data)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();

            // Convert the string data to byte data using ASCII encoding.  
            //byte[] byteData = Encoding.UTF8.GetBytes(data); //changed to utf8
            byte[] byteData = encoding.GetBytes(data);

            //convert the length of the data before sending it first
            byte[] convertedLength = _endianBinaryConverter.GetByteArray(byteData.Length);

            //combine our arrays with the length at the beginning
            byte[] completeMessage = new byte[convertedLength.Length + byteData.Length];

            convertedLength.CopyTo(completeMessage, 0);
            byteData.CopyTo(completeMessage, convertedLength.Length);

            //send full data in one call now 2-18-19
            client.BeginSend(completeMessage, 0, completeMessage.Length, 0, new AsyncCallback(SendCallback), client);
        }


        /// <summary>
        /// Callback for the async BeginSend. Sets AutoReset event
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            Logger.Debug("Send Callback");
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Debug.WriteLine("Sent {0} bytes to server.", bytesSent);
                Logger.Debug("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDoneAuto.Set();
                //sendDone.Set();
            }
            catch (Exception ex)
            {
                LastFBException = new FishbowlException("Send Error", ex);
                //throw new FishbowlException("Send Error", ex);
            }
        }



        #endregion

        #region Session properties

        private string _debugMessage;
        /// <summary>
        /// Debug message
        /// </summary>
        public string DebugMessage
        {
            get { return _debugMessage; }
            set
            {
                _debugMessage = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Set the debug output level for this session
        /// </summary>
        public FishbowlConnectDebugLevel DebugLevel { get; set; }

        /// <summary>
        /// Returns the status code from the last response
        /// </summary>
        public string LastFbMsgRsStatusCode
        {
            get
            {
                return ResponseXML is null ? ResponseJson?.StatusCode : ResponseXML?.statusCode;
            }
        }
        /// <summary>
        /// Returns the status code from the last inner response. Eg, LoginRs, ImportRs, etc
        /// </summary>
        public string LastInnerRsStatusCode
        {
            get
            {

                if (ResponseXML != null)
                {
                    PropertyInfo pi = ResponseXML.Items[0].GetType().GetProperty("statusCode");
                    return Convert.ToString(pi.GetValue(ResponseXML.Items[0], null));

                }

                return ResponseJson?.Rs?.StatusCode;
            }
        }


        private FishbowlException _lastFBException;
        /// <summary>
        /// Fishbowl Last Internal Exception
        /// </summary>
        public FishbowlException LastFBException
        {
            get { return _lastFBException; }
            set
            {
                _lastFBException = value;
                if (_lastFBException != null)
                {
                    Logger.Error(_lastFBException, "Fishbowl Exception");
                }
                
                RaisePropertyChanged();
            }
        }


        private FbiMsgsRs ResponseXML;
        private Json.FbiMsgsRs ResponseJson;
        private bool _disposed;

        /// <summary>
        /// Returns the server connection status
        /// Not, Partial or fully connected
        /// </summary>
        public FishbowlConnectionStatus ConnectionStatus
        {
            get
            {
                if (!IsConnected)
                {
                    return FishbowlConnectionStatus.NotConnected;
                }
                else if (!IsAuthenticated)
                {
                    return FishbowlConnectionStatus.Partial;
                }
                else
                {
                    return FishbowlConnectionStatus.FullyConnected;
                }
            }
        }


        /// <summary>
        /// Fishbowl User Name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Fishbowl Unencrypted User Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Fishbowl Authentication Key
        /// </summary>
        public string SessionKey { get; private set; }

        /// <summary>
        /// the user Id logged in
        /// </summary>
        public int UserId { get; private set; }

        private bool _isConnected;
        /// <summary>
        /// Session Connection State ( Used 1 seat of your license )
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            private set
            {
                _isConnected = value;
                RaisePropertyChanged();
                RaisePropertyChanged("ConnectionStatus");
            }
        }


        private bool _isAuthenticated;
        /// <summary>
        /// Session Authentication State
        /// </summary>
        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
            private set
            {
                _isAuthenticated = value;
                RaisePropertyChanged();
                RaisePropertyChanged("ConnectionStatus");
            }
        }


        /// <summary>
        /// Fishbowl Last XML request made in raw format 
        /// </summary>
        private string LastRequestXML { get; set; }

        /// <summary>
        /// Fishbowl Last XML response received in raw format 
        /// </summary>
        private string LastResponseXML { get; set; }

        /// <summary>
        /// Fishbowl Last Json Request made in raw format
        /// </summary>
        private string LastRequestJson { get; set; }
        /// <summary>
        /// Fishbowl Last Json Response received in raw format
        /// </summary>
        private string LastResponseJson { get; set; }

        /// <summary>
        /// Get the Last request in raw format in either XML or Json
        /// </summary>
        public string LastRequestRaw
        {
            get
            {
                return LastRequestXML is null ? LastRequestJson : LastRequestXML;
            }
        }
        /// <summary>
        /// Get the Last response in raw format in either XML or Json
        /// </summary>
        public string LastResponseRaw
        {
            get
            {
                return LastResponseXML is null ? LastResponseJson : LastResponseXML;
            }
        }

        #endregion

        #region Session Constructors
        public FishbowlSession(SessionConfig sessionConfig)
            : this(sessionConfig.ServerAddress, sessionConfig.ServerPort, sessionConfig.APIUser, sessionConfig.APIPassword)
        {
            Config = sessionConfig;
            //CancellationToken = cancellationToken;
        }


        private FishbowlSession(string fishbowlServerAddress, int port, string user, string password)
            : this(IPAddress.Parse(fishbowlServerAddress), port)
        {
            this.UserName = user;
            this.Password = password;
            StartClient();
        }
        
        /// <summary>
        /// Fishbowl Inventory Helper
        /// </summary>
        /// <param name="fishbowlServerAddress">IPAddress of Server</param>
        /// <param name="tcpPort">TCP/IP Port</param>

        private FishbowlSession(IPAddress fishbowlServerAddress, int port)
        {
            //after all the constructors, here's where we end up
            fishbowlServer = new IPEndPoint(fishbowlServerAddress, port);
            //create connection to FB server

            //TODO: Remove this on 3-15 so that we can call the reset at the start of the IssueRequestAsync
            //StartClient();
        }


        #endregion


        /// <summary>
        /// Method to set the debug message depending on the level
        /// </summary>
        /// <param name="message"></param>
        public void DebugOutput(string message)
        {
            switch (DebugLevel)
            {
                case FishbowlConnectDebugLevel.Information:
                    break;
                case FishbowlConnectDebugLevel.Verbose:
                    DebugMessage = message;
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Login to Fishbowl and obtain a session key to make data requests.
        /// </summary>
        /// <param name="userName">Fishbowl User Name</param>
        /// <param name="password">Fishbowl Unencrypted User Password</param>
        /// <returns>Login Status Code</returns>
        public async Task<bool> Login(string userName, string password, LoginMethod loginMethod)
        {
            UserName = userName;
            Password = password;

            switch (loginMethod)
            {
                case LoginMethod.Json:
                    await LoginJson();
                    return true;
                case LoginMethod.Xml:
                    return await LoginXML();

            }
            return false;
        }

        public enum LoginMethod
        {
            Json,
            Xml
        }

        /// <summary>
        /// Login to Fishbowl and obtain a session key to make data requests. Set the username and password first
        /// </summary>
        /// <returns>Login Status Code</returns>
        public async Task<bool> LoginXML()
        {
            //if (IsUnmanaged)
            //{
            //await Reset(); 3-17 since we are creating new everytime we shouldnt need to reset
            connectDoneAuto.WaitOne(); //wait for the connection or error out before logging in
            //}

            if (String.IsNullOrEmpty(UserName))
            {
                throw new FishbowlAuthException("UserName was not assigned.");
            }

            if (String.IsNullOrEmpty(Password))
            {
                throw new FishbowlAuthException("Password was not assigned.");
            }

            //string resultCode = "9999";

            //test password for base64 first
            string md5HashString = null;

            if (Password.IsBase64String())
            {
                md5HashString = Password;
            }
            else
            {
                // MD5 Base64 Encode Password
                md5HashString = Utilities.EncryptPassword(Password);
            }


            // Build Login Request
            LoginRqType LoginRq = new LoginRqType();

            LoginRq.IADescription = "REEL mobile clients";
            LoginRq.IAID = "02361";
            LoginRq.IAName = "REEL";
            LoginRq.UserName = UserName;
            LoginRq.UserPassword = md5HashString;

            Debug.WriteLine("Logging in");
            DebugMessage = "Logging in";

            try
            {
                LoginRsType LoginRs = await IssueXMLRequestAsync<LoginRsType>(LoginRq);

                Logger.Debug("Authenticated");
                IsAuthenticated = true;

            }
            catch (Exception)
            {
                IsAuthenticated = false;
                throw;
            }
            return _isAuthenticated;
        }

        /// <summary>
        /// Login to Fishbowl and obtain a session key to make data requests. Set the username and password first
        /// </summary>
        /// <returns>Login Status Code</returns>
        public async Task<List<string>> LoginJson()
        {

            connectDoneAuto.WaitOne(); //wait for the connection or error out before logging in

            if (String.IsNullOrEmpty(UserName))
            {
                throw new FishbowlAuthException("UserName was not assigned.");
            }

            if (String.IsNullOrEmpty(Password))
            {
                throw new FishbowlAuthException("Password was not assigned.");
            }

            //if (!IsConnected) //this is checked in the issue request method
            //{
            //    throw new Exception("Not connected to the Fishbowl Server");
            //}



            // MD5 Base64 Encode Password
            string md5HashString = Utilities.EncryptPassword(Password);

            // Build Login Request
            LoginRq loginRq = new LoginRq();

            loginRq.IADescription = "REEL mobile clients";
            loginRq.IAID = "02361";
            loginRq.IAName = "REEL";
            loginRq.UserName = UserName;
            loginRq.UserPassword = md5HashString;

            Debug.WriteLine("Logging in");
            DebugMessage = "Logging in";

            try
            {
                LoginRs loginRs = await IssueJsonRequestAsync<LoginRs>(loginRq);

                Logger.Debug("Authenticated");
                IsAuthenticated = true;
                
                return loginRs.ModuleAccess.ModuleList;

            }
            catch (Exception)
            {
                IsAuthenticated = false;
                throw;
            }
            //return _isAuthenticated;
        }

        public async Task LogoutJson()
        {
            if (IsAuthenticated)
            {
                LogoutRq logoutRq = new LogoutRq();

                var response = await IssueJsonRequestAsync<LogoutRs>(logoutRq);

                IsConnected = false;
                IsAuthenticated = false;
                //Disconnect(); //Disconnecting again calls the issue request again which log us back in again....
            }
        }

        public async Task ForceLogoutJson()
        {
            LogoutRq logoutRq = new LogoutRq();

            string jsonRequest = "";

            ASCIIEncoding encoding = new ASCIIEncoding();

            //object[] requestObjects = new object[1];
            //requestObjects[0] = requestObject;

            FbiJson request = new FbiJson();
            request.Ticket = new Json.Ticket { Key = EmptyIfNull(SessionKey) };
            Json.FbiMsgsRq fbiMsgsRq = new Json.FbiMsgsRq { Rq = logoutRq };
            request.FbiMsgsRq = fbiMsgsRq;

            jsonRequest = await Task.Run(() => SerializeToJsonString(request));
            //Debug.WriteLine(jsonRequest);
            LastRequestJson = jsonRequest;


            Send(_client, jsonRequest); 

            IsConnected = false;
            IsAuthenticated = false;
        }


        /// <summary>
        /// Sets connected and authenticated properties to false and closes the socket
        /// </summary>
        public async Task Disconnect()
        {
            Logger.Debug("Disconnecting");
            if (RequestFormat == LoginMethod.Json)
            {
                try
                {
                    await LogoutJson();
                }
                catch(Exception)
                {
                    //swallow for now - possibly Invalid ticket on logout
                }
            }

            IsConnected = false;
            IsAuthenticated = false;

            if (_client != null && _client.Connected)
                _client.Shutdown(SocketShutdown.Both);
            _client?.Close();



            //_socket.Disconnect(false);

            //App.reelLogger.Verbose("FishbowlSession Disconnected");

        }

        /// <summary>
        /// Implements IDisposable
        /// </summary>
        protected virtual async void Dispose(bool Disposing)
        {

            //IsConnected = false;
            //IsAuthenticated = false;

            await Disconnect(); //dont shutdown, just close it

            _disposed = true;

            if (Disposing)
            {
                // stop the timer from firing.
                connectTimer?.Dispose();

                //Debug.WriteLine("Disposing Socket");// + _client?.LocalEndPoint?.ToString());
                //_client?.Close();
                //_client = null;

                _endianBinaryConverter?.Dispose();
                _client?.Close();
                _client?.Dispose();



                //_client = null;

                //}
                //_disposed = true;
            }

            //Reset();
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            //timeoutFlag = 0;
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);




        }
        #endregion


        #region Request Methods

        public static T DeserializeFromXMLString<T>(string xml)
        {
            try
            {
                //Debug.WriteLine("Deserializing: " + typeof(T).Name.ToString());


                //App.reelLogger.Debug("Deserializing: " + typeof(T).Name.ToString());

                //App.reelLogger.Debug("return XML string {xmlString}", xml);

                byte[] bytes = Encoding.UTF8.GetBytes(xml);
                MemoryStream mem = new MemoryStream(bytes);
                XmlSerializer ser = new XmlSerializer(typeof(T));

                Debug.WriteLine("Finished Deserializing");
                return (T)ser.Deserialize(mem);
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Serialize Object to RAW XML String
        /// ** No namespace, XML declaration, etc..
        /// </summary>
        /// <param name="objectToSerialize">Object to Serialize to XML</param>
        /// <returns>Serialized XML Object</returns>
        public static string SerializeToXMLString(object objectToSerialize)
        {
            try
            {
                Debug.WriteLine("Serializing: " + objectToSerialize.GetType().Name.ToString());
                //App.reelLogger.Debug("Serializing: " + objectToSerialize.GetType().Name.ToString());

                MemoryStream mem = new MemoryStream();

                XmlSerializer ser = new XmlSerializer(objectToSerialize.GetType());
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.OmitXmlDeclaration = true;
                writerSettings.Indent = true;

                StringWriter stringWriter = new StringWriter();
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, writerSettings))
                {
                    ser.Serialize(xmlWriter, objectToSerialize, ns);
                }

                mem.Dispose();
                Debug.WriteLine("Done Serializing");
                //App.reelLogger.Debug("Done Serializing");

                //App.reelLogger.Debug("XML string {xmlString}", stringWriter.ToString());

                return stringWriter.ToString();
            }
            catch (Exception)
            {

                throw;
            }


        }


        /// <summary>
        /// Send a raw string to the FB server and returns the raw response. Needs to be in the correct format, no checking is done
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<string> IssueRequest(string request, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IsConnected != true)
            {
                throw new FishbowlConnectionException("Connection Error, you are not connected to the fishbowl server.");
            }

            if (IsAuthenticated != true)
            {
                throw new FishbowlAuthException("Authentication Error, you are not authenticated with the fishbowl server.");
            }

            try
            {

                Debug.WriteLine("Writing to stream...");

                RequestingRaw = true;
                Logger.Trace(request);
                Send(_client, request); //send async using begin send and creates the response 
                sendDoneAuto.WaitOne();

                if (await receiveDoneAuto.WaitOneAsync(Config.RequestTimeout, cancellationToken))//set timeout
                {
                    //if true then its good, if false then it timed out
                    Logger.Debug("Issue Request Done");

                    return serverResponse;
                }
                else
                {
                    throw new FishbowlRequestException("Request Timed Out", null, "9999", null, LastRequestRaw);
                }



            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                throw;
            }
            //return null;

        }

        /// <summary>
        /// Issues the request to the FB server.
        /// </summary>
        /// <param name="requestObject"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>"T Response"</returns>
        /// <exception cref="FishbowlAuthException"></exception>
        /// <exception cref="FishbowlConnectionException"></exception>
        /// <exception cref="FishbowlRequestException"></exception>
        /// <exception cref="FishbowlException"></exception>
        public async Task<T> IssueXMLRequestAsync<T>(object requestObject,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            //check if disposed before sending to see if we need to reset
            //property to auto reset and re-login?
            try
            {
                if (!_disposed)
                {
                    RequestFormat = LoginMethod.Xml;
                    RequestingRaw = false;

                    //if (IsUnmanaged && requestObject.GetType() != typeof(LoginRqType))
                    if (!IsAuthenticated && requestObject.GetType() != typeof(LoginRqType))
                    {

                        await Login(Config.APIUser, Config.APIPassword, LoginMethod.Xml);
                    }


                    if (IsConnected != true)
                    {
                        throw new FishbowlConnectionException("Connection Error, you are not connected to the fishbowl server.");
                    }

                    if (requestObject.GetType() != typeof(LoginRqType) && IsAuthenticated != true)
                    {
                        throw new FishbowlAuthException("Authentication Error, you are not authenticated with the fishbowl server.");
                    }


                    ResetDebugProperties();
                    string xmlRequest = "";
                    //string xmlResponse = "";
                    //string statusCode = "";
                    string subStatusCode = "";
                    string subStatusMessage = "";


                    ASCIIEncoding encoding = new ASCIIEncoding();



                    object[] requestObjects = new object[1];
                    requestObjects[0] = requestObject;

                    FbiXml request = new FbiXml();
                    request.Ticket = new Ticket { Key = SessionKey };
                    FbiMsgsRq fbiMsgsRq = new FbiMsgsRq { Items = requestObjects };
                    request.Item = fbiMsgsRq;

                    xmlRequest = await Task.Run(() => SerializeToXMLString(request));
                    //Debug.WriteLine(xmlRequest);
                    LastRequestXML = xmlRequest;
                    Logger.Trace(xmlRequest);

                    Send(_client, xmlRequest); //send async using begin send and creates the response 
                    sendDoneAuto.WaitOne();
                    //sendDone.WaitOne();

                    Logger.Debug("Issue Sending Done");
                    //message sent , response is coming in the receive callback since the server is already listening when we started it

                    //Begin receive started in class construction, reset is done in the callback
                    if (await receiveDoneAuto.WaitOneAsync(Config.RequestTimeout, cancellationToken))//set timeout
                    {
                        //if true then its good, if false then it timed out

                        //receiveDone.WaitOne(); //wait for the response (sync)
                        Logger.Debug("Issue Receive Done");

                        if (LastFbMsgRsStatusCode != "1000")
                        {
                            if (LastFbMsgRsStatusCode == "1164")
                            {
                                //Logged off successfully
                                Logger.Debug("You have logged off");
                            }
                            else
                            {
                                if (requestObject.GetType() != typeof(LoginRqType))
                                {
                                    throw new FishbowlRequestException(Utilities.StatusCodeMessage(LastFbMsgRsStatusCode), null, LastFbMsgRsStatusCode, LastRequestRaw);
                                }
                                else
                                {
                                    throw new FishbowlAuthException(Utilities.StatusCodeMessage(LastFbMsgRsStatusCode), null, LastFbMsgRsStatusCode, LastRequestRaw);

                                }
                            }
                        }
                        else
                        {
                            PropertyInfo pi = ResponseXML.Items[0].GetType().GetProperty("statusCode");
                            subStatusCode = Convert.ToString(pi?.GetValue(ResponseXML.Items[0], null));

                            PropertyInfo piMessage = ResponseXML.Items[0].GetType().GetProperty("statusMessage");
                            subStatusMessage = Convert.ToString(piMessage?.GetValue(ResponseXML.Items[0], null));

                            //Main request was good but sub request might still not have worked
                            if (subStatusCode != "1000" && subStatusCode != "900") //900 for deprecated
                            {
                                //parent was good, request has error..
                                //DebugMessage = "1000 : " + Utilities.StatusCodeMessage(subStatusCode);
                                throw new FishbowlRequestException(subStatusMessage, null, subStatusCode, null, LastRequestRaw);

                                //App.reelLogger.Error("Request sub error {errorMessage}", Utilities.StatusCodeMessage(subStatusCode) + " - " + subStatusMessage);


                                //return default(T); //return null
                            }
                        }
                        //receiveDone.Reset(); //moved from receive callback //dont need anymore since we are using an autoreset

                        T tagResponse = (T)ResponseXML.Items[0];

                        Logger.Debug("Issue Request Done");

                        return tagResponse;
                    }
                    else
                    {
                        throw new FishbowlRequestException("Request Timed Out", null, "9999", null, LastRequestRaw);
                    }

                }
                else
                {
                    return default(T);
                }
            }
            catch (FishbowlException ex)
            {
                LastFBException = ex;
                throw;
            }
            catch (Exception e)
            {
                LastFBException = new FishbowlException("Request Error", e);
                throw;
            }

        }

        /// <summary>
        /// Reset all of the debug fields to null. Used before each IssueRequest Method
        /// </summary>
        private void ResetDebugProperties()
        {
            LastRequestXML = null;
            LastResponseXML = null;
            LastRequestJson = null;
            LastResponseJson = null;
            LastFBException = null;
            ResponseJson = null;
            ResponseXML = null;
        }


        /// <summary>
        /// This async method sends a Json request to the Fishbowl server. 
        /// If it is not connected the the server, it will login using the user and password supplied in the configuration class
        /// 
        /// </summary>
        /// <typeparam name="T">T is the response type</typeparam>
        /// <param name="requestObject"></param>
        /// <returns>Returns response object of type T</returns>
        /// <exception cref="FishbowlAuthException"></exception>
        /// <exception cref="FishbowlConnectionException"></exception>
        /// <exception cref="FishbowlRequestException"></exception>
        /// <exception cref="FishbowlException"></exception>
        public async Task<T> IssueJsonRequestAsync<T>(object requestObject, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (!_disposed)
                {
                    RequestFormat = LoginMethod.Json;
                    RequestingRaw = false;


                    if (!IsAuthenticated && requestObject.GetType() != typeof(LoginRq))
                    {
                        await Login(Config.APIUser, Config.APIPassword, LoginMethod.Json);
                    }

                    if (IsConnected != true)
                    {
                        throw new FishbowlConnectionException("Connection Error, you are not connected to the fishbowl server.");
                    }

                    if (requestObject.GetType() != typeof(LoginRq) && IsAuthenticated != true)
                    {
                        throw new FishbowlAuthException("Authentication Error, you are not authenticated with the fishbowl server.");
                    }


                    ResetDebugProperties();
                    string jsonRequest = "";
                    string subStatusCode = "";
                    //string subStatusMessage = "";

                    ASCIIEncoding encoding = new ASCIIEncoding();

                    //object[] requestObjects = new object[1];
                    //requestObjects[0] = requestObject;

                    FbiJson request = new FbiJson();
                    request.Ticket = new Json.Ticket { Key = EmptyIfNull(SessionKey) };
                    Json.FbiMsgsRq fbiMsgsRq = new Json.FbiMsgsRq { Rq = requestObject };
                    request.FbiMsgsRq = fbiMsgsRq;

                    jsonRequest = await Task.Run(() => SerializeToJsonString(request));
                    Debug.WriteLine(jsonRequest);
                    LastRequestJson = jsonRequest;

                    Logger.Trace(jsonRequest);
                    Send(_client, jsonRequest); //send async using begin send  
                    sendDoneAuto.WaitOne(); //wait for sending confirmation in callback, block thread

                    //cancellationToken.ThrowIfCancellationRequested(); //throw if cancelled


                    Logger.Debug("Issue Sending Done");
                    //message sent , response is coming in the receive callback since the server is already listening when we started it

                    //Begin receive started in class construction, reset is done in the callback

                    if (await receiveDoneAuto.WaitOneAsync(Config.RequestTimeout, cancellationToken))//set timeout
                    
                    {
                        //if true then its good, if false then it timed out

                        //receiveDone.WaitOne(); //wait for the response (sync)
                        Logger.Debug("Issue Receive Done");

                        if (LastFbMsgRsStatusCode != "1000")
                        {
                            if (LastFbMsgRsStatusCode == "1164")
                            {
                                //Logged off successfully
                                Logger.Debug("You have logged off");
                            } //TODO:add 1130 invalid ticket catch and ignore if logout type
                            else
                            {
                                if (requestObject.GetType() != typeof(LoginRq))
                                {
                                    if (String.IsNullOrEmpty(ResponseJson.StatusMessage))
                                    {
                                        throw new FishbowlRequestException(Utilities.StatusCodeMessage(LastFbMsgRsStatusCode), null, LastFbMsgRsStatusCode, LastRequestRaw);

                                    }
                                    else
                                    {
                                        throw new FishbowlRequestException(ResponseJson.StatusMessage, null, LastFbMsgRsStatusCode, null, LastRequestRaw);

                                    }

                                }
                                else
                                {
                                    if (String.IsNullOrEmpty(ResponseJson.StatusMessage))
                                    {
                                        throw new FishbowlAuthException(Utilities.StatusCodeMessage(LastFbMsgRsStatusCode), null, LastFbMsgRsStatusCode);

                                    }
                                    else
                                    {
                                        throw new FishbowlAuthException(ResponseJson.StatusMessage, null, LastFbMsgRsStatusCode);

                                    }


                                }

                            }
                        }
                        else
                        {
                            PropertyInfo pi = ResponseJson?.Rs?.GetType().GetProperty("StatusCode");
                            subStatusCode = Convert.ToString(pi?.GetValue(ResponseJson.Rs, null));


                            //PropertyInfo piMessage = _lastFbiMsgsRs.Items[0].GetType().GetProperty("statusMessage");
                            //subStatusMessage = Convert.ToString(pi.GetValue(_lastFbiMsgsRs.Items[0], null));

                            //Main request was good but sub request might still not have worked
                            if (subStatusCode != "1000" && subStatusCode != "900") //900 for deprecated
                            {
                                //parent was good, request has error..
                                if (String.IsNullOrEmpty(ResponseJson.Rs?.StatusMessage))
                                {
                                    throw new FishbowlRequestException(Utilities.StatusCodeMessage(subStatusCode), null, subStatusCode, null, LastRequestRaw);

                                }
                                else
                                {

                                    if (ResponseJson.Rs.StatusMessage.Contains("The following lines of the CSV import"))
                                    {
                                        string extractedError = StripImportError(ResponseJson.Rs?.StatusMessage);
                                        throw new FishbowlRequestException(extractedError, null, subStatusCode, null, LastRequestRaw);
                                    }

                                    throw new FishbowlRequestException(ResponseJson.Rs?.StatusMessage, null, subStatusCode, null, LastRequestRaw);

                                }
                            }
                        }


                        //T tagResponse = (T)ResponseJson.Rs;

                        //Debug.WriteLine("Done");
                        Logger.Debug("Issue Request Done");

                        return (T)ResponseJson.Rs;
                    }
                    else
                    {
                        throw new FishbowlRequestException("Request Timed Out", LastFBException, "9999", null, LastRequestRaw);

                    }

                }
                else
                {
                    return default(T);
                }
            }
            catch (FishbowlException ex)
            {
                LastFBException = ex;
                throw;
            }
            catch (Exception e)
            {
                LastFBException = new FishbowlException("Request Error", e);
                throw;
            }

        }

        private string StripImportError(string statusMessage)
        {
            string errorMessage = statusMessage.Replace(@"The following lines of the CSV import do not have the correct format or contain incompatible data. Due to these errors, only some information was imported. Please make the appropriate changes to these lines and re-import the file.", "");
            errorMessage = errorMessage.Replace(@"Line Number: 1", "");
            errorMessage = errorMessage.Trim();
            return errorMessage;
        }

        private string EmptyIfNull(string sessionKey)
        {
            return sessionKey.IsNull() ? "" : sessionKey;
        }

        /// <summary>
        /// Serialize request to Json. 
        /// Sets resolver to remane the generic Rq property to the actual type name of the request being passed in.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string SerializeToJsonString(FbiJson request)
        {
            //add the parent class
            //change the name to the type of request
            var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
            jsonResolver.RenameProperty(typeof(Json.FbiMsgsRq), "Rq", request.FbiMsgsRq.Rq.GetType().Name); //change Rq to LoginRq or ImportRq

            ITraceWriter traceWriter = new MemoryTraceWriter();

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = jsonResolver;
            serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            serializerSettings.TraceWriter = traceWriter;

            //wrap in class to get root class included in json
            Json.JsonWrapper wrapper = new JsonWrapper { FbiJson = request };


            string result = JsonConvert.SerializeObject(wrapper, serializerSettings);

            //Debug.WriteLine(traceWriter);
            return result;

        }

        /// <summary>
        /// Deserialized FB Server response Json into object of type T. Accounts for the tail they add to the json string 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T DeserializeFBServerResponseFromJsonString<T>(string json)
        {
            //try
            //{
            //Debug.WriteLine("Deserializing: " + typeof(T).Name.ToString());

            //strip the tail addon by FB : Response received: {0}

            JObject serverResponseRaw = JObject.Parse(json);
            JToken fbiJsonSegment = serverResponseRaw["FbiJson"];

            ITraceWriter traceWriter = new MemoryTraceWriter();

            T resultObject = fbiJsonSegment.ToObject<T>(new JsonSerializer
            {
                TraceWriter = traceWriter
            });

            Debug.WriteLine(traceWriter);

            return resultObject;

            //}
            //catch (Exception)
            //{

            //    throw;
            //}

        }

        /// <summary>
        /// Deserialized FB Server response Json into object of type T. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T DeserializeFromJsonString<T>(string json)
        {
            //try
            //{
            //Debug.WriteLine("Deserializing: " + typeof(T).Name.ToString());

            //strip the tail addon by FB : Response received: {0}

            //ITraceWriter traceWriter = new MemoryTraceWriter();

            //T resultObject = JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            //{
            //    TraceWriter = traceWriter
            //});

            //Debug.WriteLine(traceWriter);

            return JsonConvert.DeserializeObject<T>(json);

            //}
            //catch (Exception)
            //{

            //    throw;
            //}

        }

        #endregion

        #region API Methods

        public async Task SaveImageToFishbowl(SaveImageType ObjectType, string Number, string ImageBase64)
        {

            SaveImageRqType saveImageRq = new SaveImageRqType();

            saveImageRq.Type = ObjectType.ToString();
            saveImageRq.Number = Number;
            saveImageRq.Image = ImageBase64;
            saveImageRq.UpdateAssociations = "true";


            SaveImageRsType SaveImageRs = await this.IssueXMLRequestAsync<SaveImageRsType>(saveImageRq);


            //if (SaveImageRs.statusCode != "1000")
            //{
            //    throw new System.Exception(SaveImageRs.statusCode + " - " + Utilities.StatusCodeMessage(SaveImageRs.statusCode));
            //}

            

        }


        #region Inventory
        public async Task<Part> GetPartObject(string PartNum)
        {
            PartGetRqType PartRq = new PartGetRqType();

            PartRq.Number = PartNum;

            PartGetRsType PartRs = await IssueXMLRequestAsync<PartGetRsType>(PartRq);


            if (PartRs != null)
            {
                return PartRs.Part;
            }
            else
            {
                return null;
            }


        }

        
        public async Task<List<InvQty>> GetPartInventory(string PartNum)
        {
            //loads the part inventory and returns it in a list

            InvQtyRq InvRq = new InvQtyRq();
            InvRq.PartNum = PartNum;

            InvQtyRs InvRs = await IssueJsonRequestAsync<InvQtyRs>(InvRq);

            return InvRs.InvQty;


        }
        [Obsolete]
        public async Task<bool> MoveInventory(string PartNum, string Qty, int FromLocID, int ToLocTag)
        {
            Location1 SourceLocation1 = await GetLocationObject(FromLocID.ToString(), LocationLookupType.LocationID);
            if (ToLocTag.ToString() != SourceLocation1.TagNumber)
            {
                Location1 DestinationLocation1 = await GetLocationObject(ToLocTag.ToString(), LocationLookupType.TagNumber);

                Part part = await GetPartObject(PartNum);

                SourceLocation Source = new SourceLocation();
                DestinationLocation Destination = new DestinationLocation();

                if (part != null)
                {
                    //if (SourceLocation1 != null)
                    //{
                    if (DestinationLocation1 != null)
                    {
                        //both locations are filled in so make rq

                        MoveRqType MoveRq = new MoveRqType();

                        Source.Location = SourceLocation1;
                        Destination.Location = DestinationLocation1;

                        MoveRq.Part = part;
                        MoveRq.Quantity = Qty;
                        MoveRq.SourceLocation = Source;
                        MoveRq.DestinationLocation = Destination;

                        MoveRsType MoveRs = await this.IssueXMLRequestAsync<MoveRsType>(MoveRq);

                        if (MoveRs.statusCode != "1000")
                        {
                            throw new Exception(Utilities.StatusCodeMessage(MoveRs.statusCode));
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        throw new Exception("Destination Location not found");
                    }
                    //}
                    //else
                    //{
                    //    throw new Exception("Source Location not found");
                    //}
                }
                else
                {
                    throw new Exception("Error with Part");
                }

            }
            else
            {
                throw new Exception("Destination location is the same as the source");
            }
        }
        [Obsolete]
        public async Task<bool> MoveInventory(string PartNum, string Qty, Location1 FromLoc, int ToLocTag)
        {
            //Location1 SourceLocation1 = await GetLocationObject(FromLocTag.ToString());
            if (ToLocTag.ToString() != FromLoc.TagNumber)
            {
                Location1 DestinationLocation1 = await GetLocationObject(ToLocTag.ToString(), LocationLookupType.TagNumber);

                Part part = await GetPartObject(PartNum);

                SourceLocation Source = new SourceLocation();
                DestinationLocation Destination = new DestinationLocation();

                if (part != null)
                {
                    //if (SourceLocation1 != null)
                    //{
                    if (DestinationLocation1 != null)
                    {
                        //both locations are filled in so make rq

                        MoveRqType MoveRq = new MoveRqType();

                        Source.Location = FromLoc;
                        Destination.Location = DestinationLocation1;

                        MoveRq.Part = part;
                        MoveRq.Quantity = Qty;
                        MoveRq.SourceLocation = Source;
                        MoveRq.DestinationLocation = Destination;

                        MoveRsType MoveRs = await this.IssueXMLRequestAsync<MoveRsType>(MoveRq);

                        if (MoveRs.statusCode != "1000")
                        {
                            throw new Exception(Utilities.StatusCodeMessage(MoveRs.statusCode));
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        throw new Exception("Destination Location not found");
                    }
                    //}
                    //else
                    //{
                    //    throw new Exception("Source Location not found");
                    //}
                }
                else
                {
                    throw new Exception("Error with Part");
                }

            }
            else
            {
                throw new Exception("Destination location is the same as the source");
            }
        }
        [Obsolete]
        public async Task<bool> MoveInventory(Part Part, string Qty, Location1 FromLoc, int ToLocTag)
        {
            //Location1 SourceLocation1 = await GetLocationObject(FromLocTag.ToString());
            if (ToLocTag.ToString() != FromLoc.TagNumber)
            {
                Location1 DestinationLocation1 = await GetLocationObject(ToLocTag.ToString(), LocationLookupType.TagNumber);

                //Part part = await GetPartObject(PartNum);

                SourceLocation Source = new SourceLocation();
                DestinationLocation Destination = new DestinationLocation();

                if (Part != null)
                {
                    //if (SourceLocation1 != null)
                    //{
                    if (DestinationLocation1 != null)
                    {
                        //both locations are filled in so make rq

                        MoveRqType MoveRq = new MoveRqType();

                        Source.Location = FromLoc;
                        Destination.Location = DestinationLocation1;

                        MoveRq.Part = Part;
                        MoveRq.Quantity = Qty;
                        MoveRq.SourceLocation = Source;
                        MoveRq.DestinationLocation = Destination;

                        MoveRsType MoveRs = await this.IssueXMLRequestAsync<MoveRsType>(MoveRq);

                        if (MoveRs.statusCode != "1000")
                        {
                            throw new Exception(Utilities.StatusCodeMessage(MoveRs.statusCode));
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        throw new Exception("Destination Location not found");
                    }
                    //}
                    //else
                    //{
                    //    throw new Exception("Source Location not found");
                    //}
                }
                else
                {
                    throw new Exception("Error with Part");
                }

            }
            else
            {
                throw new Exception("Destination location is the same as the source");
            }
        }






        /// <summary>
        /// Add inventory to location by name with no tracking. Uses the last part cost in FB
        /// </summary>
        /// <param name="PartNum">Part number</param>
        /// <param name="Qty">Qty to add</param>
        /// <param name="LocationName">Location Name</param>
        /// <param name="LocationGroup">Location Group Name</param>
        /// <param name="Note">Note for add</param>
        /// <returns></returns>\
        [Obsolete]
        public async Task<bool> AddInventory(string PartNum, int Qty,
            string LocationName, string LocationGroup, string Note = "")
        {
            return await this.AddInventory(PartNum, Qty, LocationName,
                LocationGroup, null, Note);
        }

        /// <summary>
        /// Add Inventory to a location with tracking information. Uses the last part cost in FB
        /// </summary>
        /// <param name="PartNum">Part Number</param>
        /// <param name="Qty">Qty to add</param>
        /// <param name="LocationName">Location Name to add to</param>
        /// <param name="LocationGroup">Location Group Full Name</param>
        /// <param name="Tracking">List of tracking items for this part. Must include all tracking items that are used</param>
        /// <param name="Note">Optional Note</param>
        /// <returns></returns>
        [Obsolete]
        public async Task<bool> AddInventory(string PartNum, int Qty,
            string LocationName, string LocationGroup,
            List<TrackingItem> Tracking, string Note = "")
        {
            if (String.IsNullOrEmpty(PartNum))
            {
                throw new ArgumentNullException("Part number required");
            }
            if (String.IsNullOrEmpty(LocationGroup))
            {
                throw new ArgumentNullException("Location Group required");
            }
            if (String.IsNullOrEmpty(LocationName))
            {
                throw new ArgumentNullException("Location name required");
            }

            AddInventoryRq AddRq = new AddInventoryRq();

            //get location tag number from the location name and the location group
            string LocTag = (await GetLocationSimple(LocationGroup, LocationName)).LocationTagNum.ToString();


            AddRq.PartNum = PartNum;
            AddRq.Quantity = Qty.ToString();
            AddRq.LocationTagNum = LocTag;
            AddRq.Cost = await GetPartLastCost(PartNum);
            AddRq.TagNum = "0";
            AddRq.Note = "Mobile Add " + Note;
            AddRq.UOMID = "1";
            AddRq.Tracking = Tracking;

            AddInventoryRs AddRs = await IssueJsonRequestAsync<AddInventoryRs>(AddRq);

            return true;

        }






        [Obsolete]
        public async Task<bool> CycleInventory(string PartNum, string NewQty, string LocationID)
        {
            CycleCountRqType CycleRq = new CycleCountRqType();

            CycleRq.PartNum = PartNum;
            CycleRq.Quantity = NewQty;
            CycleRq.LocationID = LocationID;

            CycleCountRsType CycleRs = await this.IssueXMLRequestAsync<CycleCountRsType>(CycleRq);


            if (CycleRs.statusCode != "1000")
            {
                throw new Exception("Error: " + Utilities.StatusCodeMessage(CycleRs.statusCode));
            }
            else
            {
                return true;
            }
        }
        [Obsolete]
        public async Task<bool> CycleInventory(string PartNum, string NewQty, string LocationID, string tracking)
        {
            CycleCountRqType CycleRq = new CycleCountRqType();

            CycleRq.PartNum = PartNum;
            CycleRq.Quantity = NewQty;
            CycleRq.LocationID = LocationID;
            CycleRq.Tracking = tracking;

            CycleCountRsType CycleRs = await IssueXMLRequestAsync<CycleCountRsType>(CycleRq);


            if (CycleRs.statusCode != "1000")
            {
                throw new Exception("Error: " + Utilities.StatusCodeMessage(CycleRs.statusCode));
            }
            else
            {
                return true;
            }
        }

        #endregion
        [Obsolete]
        public async Task<Location1> GetLocationObject(string LookupNumber, LocationLookupType lookupType)
        {

            LocationQueryRqType LocRq = new LocationQueryRqType();

            switch (lookupType)
            {
                case LocationLookupType.LocationID:
                    LocRq.LocationID = LookupNumber;
                    break;
                case LocationLookupType.TagNumber:
                    LocRq.TagNum = LookupNumber;
                    break;
                    //default:
                    //    break;
            }


            LocationQueryRsType LocRs = await this.IssueXMLRequestAsync<LocationQueryRsType>(LocRq);

            if (LocRs != null)
            {
                if (LocRs.statusCode == "1000")
                {
                    return LocRs.Location;
                }
                else
                {
                    throw new Exception("Error: " + Utilities.StatusCodeMessage(LocRs.statusCode));
                }
            }
            else
            {
                throw new Exception("Location not found");
            }

        }


        #region Part 

        /// <summary>
        /// Returns the default location for the part using the default location group for the user requesting it
        /// </summary>
        /// <param name="partNum"></param>
        /// <returns>Location1 Object. Null if none set</returns>
        public async Task<Location1> GetPartDefaultLocationAsync(string partNum)
        {
            DefPartLocQueryRq DPLRq = new DefPartLocQueryRq();

            DPLRq.PartNum = partNum;

            DefPartLocQueryRs DPLRs = await IssueJsonRequestAsync<DefPartLocQueryRs>(DPLRq);

            return DPLRs.Location;

        }

        //public async Task<List<Location1>> GetAllPartDefaultLoc(string partID)
        //{
        //    List<Location1> setDefaultLocations = new List<Location1>();

        //    ExecuteQueryRqType EQRq = new ExecuteQueryRqType();

        //    EQRq.Query = @"select DEFAULTLOCATION.partid, DEFAULTLOCATION.LOCATIONGROUPID, Tag.num as TagNum 
        //                    from DEFAULTLOCATION join location on defaultlocation.locationid = location.id 
        //                    join tag on (tag.locationid = defaultlocation.locationid AND tag.typeid = 10) 
        //                    where DEFAULTLOCATION.partid = " + partID + " AND LOCATION.ACTIVEFLAG = 1";

        //    ExecuteQueryRsType EQRs = await IssueRequestAsync<ExecuteQueryRsType>(EQRq);

        //    if (EQRs != null)
        //    {

        //        if (EQRs.statusCode != "1000")
        //        {
        //            throw new Exception(EQRs.statusCode + " - " + Utilities.StatusCodeMessage(EQRs.statusCode));
        //        }
        //        else
        //        {
        //            for (int i = 1; i < EQRs.Rows.Count; i++)
        //            {
        //                //start at 1 index to skip csv headers
        //                string[] defaultlocation = EQRs.Rows[i].Split(',');
        //                //column 3 is the location tag number
        //                setDefaultLocations.Add(await this.GetLocationObject(defaultlocation[2].Replace("\"", ""), LocationLookupType.TagNumber));
        //            }

        //            //parse the csv,


        //            //get the tag numbers and get the corresponding location objects from them
        //            return setDefaultLocations;
        //        }
        //    }
        //    else
        //    {
        //        throw new Exception("Error retreiving default locations");
        //    }

        //}


        //public async Task<bool> SetDefaultPartLoc(string partNum, Location1 Location)
        //{
        //    SetDefPartLocRqType SDPRq = new SetDefPartLocRqType();

        //    SDPRq.PartNum = partNum;

        //    //Set the location object for the destination location
        //    SDPRq.Location = Location;

        //    SetDefPartLocRsType SDPRs = await IssueRequestAsync<SetDefPartLocRsType>(SDPRq);

        //    if (SDPRs.statusCode != "1000")
        //    {
        //        throw new System.Exception("Error: " + SDPRs.statusCode);
        //    }
        //    else
        //    {
        //        return true;
        //    }

        //}

        #endregion



        #region Picking

        /// <summary>
        /// Returns a tag object provided with 1 of the search terms
        /// </summary>
        /// <param name="TagID"></param>
        /// <param name="TagNum"></param>
        /// <param name="LocationID"></param>
        /// <returns>Tag object</returns>
        public async Task<Tag> GetTagObjectAsync(string TagID = null, string TagNum = null, string LocationID = null)
        {
            if (String.IsNullOrEmpty(TagID) && String.IsNullOrEmpty(TagNum) && String.IsNullOrEmpty(LocationID))
            {
                throw new ArgumentNullException("Must have at least one of the search parameters");
            }

            TagQueryRq tagQueryRq = new TagQueryRq
            {
                TagID = TagID,
                LocationID = LocationID,
                Num = TagNum
            };

            return (await IssueJsonRequestAsync<TagQueryRs>(tagQueryRq)).Tag;


        }

        #endregion







        /// <summary>
        /// Returns a list of the active tracking fields and values used with this TagId
        /// </summary>
        /// <param name="tagID">FB tagId, not tagNum</param>
        /// <returns></returns>
        public async Task<List<TagTrackingObject>> GetTrackingByTag(int tagID)
        {
            string query = String.Format(@"SELECT tag.id AS tagid, 
                        COALESCE (DATE_FORMAT(trackingdate.`info`,'%m/%d/%Y'), trackingdecimal.`info`, 
                            CASE WHEN parttracking.`typeId` = 80 THEN 
	                            CASE WHEN trackinginteger.`info` = 0 THEN 'false'
		                            ELSE 'true'
		                            END
	                            ELSE trackinginteger.`info`
	                            END, 

                            trackingtext.`info`) AS Info,
                            parttracking.`name` AS TrackingLabel
                            , parttracking.`typeId` as TrackingTypeID
                        FROM tag 
                        LEFT JOIN part ON tag.`partId` = part.id

                        LEFT JOIN parttotracking ON parttotracking.`partId` = part.id 
	                        LEFT JOIN parttracking ON parttotracking.`partTrackingId` = parttracking.`id`

                        LEFT JOIN trackingtext ON trackingtext.`partTrackingId` = parttotracking.`partTrackingId` AND trackingtext.`tagId` = tag.`id`
	
                        LEFT JOIN trackingdecimal ON trackingdecimal.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdecimal.`tagId` = tag.`id`
	
                        LEFT JOIN trackinginteger ON trackinginteger.`partTrackingId` = parttotracking.`partTrackingId` AND trackinginteger.`tagId` = tag.`id`
	
                        LEFT JOIN trackingdate ON trackingdate.`partTrackingId` = parttotracking.`partTrackingId` AND trackingdate.`tagId` = tag.`id`

                        WHERE tag.id = {0}", tagID);


            return await ExecuteQueryAsync<TagTrackingObject, TagTrackingItemClassMap>(query);

        }

        /// <summary>
        /// Executes provided query and returns the results in a List of type T. 
        /// TMap provides CsvHelper with the mapping to deserialize the returned csv into classes of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TMap"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown when no records found</exception>
        /// <exception cref="OperationCanceledException">Thrown when task cancelled</exception>
        public async Task<List<T>> ExecuteQueryAsync<T, TMap>(string query, CancellationToken cancellationToken = default(CancellationToken)) where TMap : ClassMap<T>
        {
            if (String.IsNullOrEmpty(query))
            {
                throw new ArgumentException("Query cannot be missing");
            }

            Logger.Trace(query);

            ExecuteQueryRq executeQueryRq = new ExecuteQueryRq()
            {
                Query = query
            };

            ExecuteQueryRs executeQueryRs = await IssueJsonRequestAsync<ExecuteQueryRs>(executeQueryRq, cancellationToken);

            if (executeQueryRs.Rows.Row.Count > 1)
            {

                List<T> returnedItems = new List<T>();

                var records = executeQueryRs.Rows.Row.Skip(1); //skip the header row. Fields should be mapped through indexes until FB fixes header import
                foreach (string row in records)
                {
                    //read csv in each row and convert to object
                    using (StringReader stringReader = new StringReader(row))
                    {
                        using (CsvReader csvReader = new CsvReader(stringReader))
                        {
                            csvReader.Configuration.HasHeaderRecord = false;
                            csvReader.Configuration.RegisterClassMap<TMap>();//(classMap);
                            csvReader.Read();
                            returnedItems.Add(csvReader.GetRecord<T>());
                        }
                    }


                }

                return returnedItems;

            }
            else
            {
                throw new KeyNotFoundException("No records returned.");

            }

        }

        /// <summary>
        /// Returns a list of type T without having to supply a Class map.
        /// Used for strings and simple types
        /// </summary>
        /// <typeparam name="T">String, int, etc</typeparam>
        /// <param name="query"></param>
        /// <returns>List of type T</returns>
        public async Task<List<T>> ExecuteQueryAsync<T>(string query)
        {
            if (String.IsNullOrEmpty(query))
            {
                throw new ArgumentException("Query cannot be missing");
            }

            ExecuteQueryRq executeQueryRq = new ExecuteQueryRq()
            {
                Query = query
            };

            ExecuteQueryRs executeQueryRs = await IssueJsonRequestAsync<ExecuteQueryRs>(executeQueryRq);

            if (executeQueryRs.Rows.Row.Count > 1)
            {

                List<T> returnedItems = new List<T>();

                var records = executeQueryRs.Rows.Row.Skip(1); //skip the header row. Fields should be mapped through indexes until FB fixes header import
                foreach (string row in records)
                {
                    //read csv in each row and convert to object
                    using (StringReader stringReader = new StringReader(row))
                    {
                        using (CsvReader csvReader = new CsvReader(stringReader))
                        {
                            csvReader.Configuration.HasHeaderRecord = false;
                            //csvReader.Configuration.RegisterClassMap<TMap>();//(classMap);
                            csvReader.Read();
                            returnedItems.Add(csvReader.GetField<T>(0));
                        }
                    }


                }

                return returnedItems;

            }
            else
            {
                throw new KeyNotFoundException("No records returned.");

            }

        }

        /// <summary>
        /// Execute Query Scalar, returns first value of results as string
        /// </summary>
        /// <param name="query"></param>
        /// <returns>String</returns>
        public async Task<string> ExecuteQueryAsync(string query)
        {
            if (String.IsNullOrEmpty(query))
            {
                throw new ArgumentException("Query cannot be missing");
            }

            ExecuteQueryRq executeQueryRq = new ExecuteQueryRq()
            {
                Query = query
            };

            ExecuteQueryRs executeQueryRs = await IssueJsonRequestAsync<ExecuteQueryRs>(executeQueryRq);

            if (executeQueryRs.Rows.Row.Count == 2)
            {
                var records = executeQueryRs.Rows.Row.Skip(1); //skip the header row. Fields should be mapped through indexes until FB fixes header import
                foreach (string row in records)
                {
                    //read csv in each row and convert to object
                    using (StringReader stringReader = new StringReader(row))
                    {
                        using (CsvReader csvReader = new CsvReader(stringReader))
                        {
                            csvReader.Configuration.HasHeaderRecord = false;
                            //csvReader.Configuration.RegisterClassMap<TMap>();//(classMap);
                            csvReader.Read();
                            return csvReader.GetField<string>(0);
                        }
                    }

                }
                throw new KeyNotFoundException("No fields returned.");
            }
            else
            {
                throw new KeyNotFoundException("No records returned.");

            }

        }




        #region Sales Orders

        public async Task<List<SalesOrder>> GetSalesOrderList(GetSOListRqType RequestFilters)
        {
            //send request filters to FB and get list returned
            GetSOListRsType SOListRs = await this.IssueXMLRequestAsync<GetSOListRsType>(RequestFilters);

            //if (SOListRs.statusCode != "1000")
            //{
            //    throw new System.Exception(SOListRs.statusCode + " - " + Utilities.StatusCodeMessage(SOListRs.statusCode));
            //}
            return SOListRs.SalesOrder;
        }

        public async Task<List<SalesOrderListInfo>> GetSalesOrderListInfo(SalesOrderListFilters RequestFilters)
        {
            //send request filters to FB and get list returned
            List<SalesOrderListInfo> SalesOrderList = new List<SalesOrderListInfo>();

            string WhereClause = "";

            if (RequestFilters.Status == "All")
            {
                WhereClause += @" AND (SOStatus.Name = 'Estimate' OR SOStatus.Name = 'Issued' OR SOStatus.Name = 'In Progress' 
                                        OR SOStatus.Name = 'Fulfilled' OR SOStatus.Name = 'Voided')";
            }
            else if (RequestFilters.Status == "All Open")
            {
                WhereClause += " AND (SOStatus.Name = 'Estimate' OR SOStatus.Name = 'Issued' OR SOStatus.Name = 'In Progress')";
            }
            else
            {
                WhereClause += " AND (SOStatus.Name = '" + RequestFilters.Status + "')";
            }

            if (RequestFilters.SearchField != null)
            {
                WhereClause += @" AND (Upper(SO.NUM) like '%" + RequestFilters.SearchField.ToUpper() + @"%' 
                                   OR Upper(SO.CUSTOMERCONTACT) like '%" + RequestFilters.SearchField.ToUpper() + @"%' 
                                   OR Upper(SO.BILLTONAME) like '%" + RequestFilters.SearchField.ToUpper() + @"%'
                                   OR Upper(SO.BILLTOADDRESS) like '%" + RequestFilters.SearchField.ToUpper() + @"%'
                                   OR Upper(SO.CUSTOMERPO) like '%" + RequestFilters.SearchField.ToUpper() + @"%'
                                   OR Upper(SO.VENDORPO) like '%" + RequestFilters.SearchField.ToUpper() + @"%'
                                   OR Coalesce((Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 50 Order By contact.defaultflag DESC)
, (Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 10 Order By contact.defaultflag DESC)
,(Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 20 Order By contact.defaultflag DESC)
) like '%" + RequestFilters.SearchField.ToUpper() + @"%' 
                                   OR Coalesce((Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 30 Order By contact.defaultflag DESC)
,(Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 10 Order By contact.defaultflag DESC)
,(Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 20 Order By contact.defaultflag DESC)) like '%" + RequestFilters.SearchField.ToUpper() + @"%')";
            }



            ExecuteQueryRq SOListRq = new ExecuteQueryRq();

            //number, contact, bill to name, address, contact main phone, contact mobile phone (ZCoalesce 2 numbers),
            //customer po, and vendor po

            SOListRq.Query = @"select sostatus.name as sostatusName, so.num, customer.name, Coalesce(so.CUSTOMERCONTACT,''), so.BILLTONAME, so.BILLTOADDRESS, 
                           Coalesce(so.CUSTOMERPO,''), Coalesce(so.VENDORPO,''),
Coalesce((Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 50 Order By contact.defaultflag DESC)
, (Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 10 Order By contact.defaultflag DESC)
,(Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 20 Order By contact.defaultflag DESC)
, '') as Phone1, 
Coalesce((Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 30 Order By contact.defaultflag DESC)
,(Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 10 Order By contact.defaultflag DESC)
,(Select First 1 contact.datus from contact where customer.accountid = Contact.accountid AND Contact.typeID = 20 Order By contact.defaultflag DESC)
, '') as Phone2

from SO
join customer on customer.id = so.customerid
JOIN SOStatus on SOStatus.id = SO.StatusID
                            Where " +
                            WhereClause.Substring(4);


            ExecuteQueryRs SOListRs = await this.IssueJsonRequestAsync<ExecuteQueryRs>(SOListRq);

            //if (SOListRs.statusCode != "1000")
            //{
            //    throw new System.Exception(SOListRs.statusCode + " - " + Utilities.StatusCodeMessage(SOListRs.statusCode));
            //}
            //else
            //{

                for (int i = 1; i < SOListRs.Rows.Row.Count; i++)
                {
                    MatchCollection FieldValues = Regex.Matches(SOListRs.Rows.Row[i], "(\"([^\"]*)\"|([^,]*))(,|$)");


                    //start at 1 index to skip csv headers

                    //CsvReader reader = new CsvReader(new StringReader(SOListRs.Rows[i]));
                    //reader.Configuration.HasHeaderRecord = false;
                    //foreach (Match item in FieldValues)
                    //{
                    //    Debug.WriteLine(item.Groups[2].Value);
                    //}
                    SalesOrderList.Add(new SalesOrderListInfo
                    {
                        status = FieldValues[0].Groups[2].Value,
                        orderNumber = FieldValues[1].Groups[2].Value,
                        CustomerName = FieldValues[2].Groups[2].Value,
                        contact = FieldValues[3].Groups[2].Value,
                        BillTo = FieldValues[4].Groups[2].Value,
                        street = FieldValues[5].Groups[2].Value,
                        CustomerPO = FieldValues[6].Groups[2].Value,
                        VendorPO = FieldValues[7].Groups[2].Value,
                        phone1 = FieldValues[8].Groups[2].Value,
                        phone2 = FieldValues[9].Groups[2].Value

                    });
                }

                //parse the csv,

                return SalesOrderList;
            //}

        }

        public async Task<SalesOrder> GetSalesOrder(string SalesOrderNumber)
        {
            LoadSORq LoadSORq = new LoadSORq();
            LoadSORq.Number = SalesOrderNumber;

            LoadSORs LoadSORs = await IssueJsonRequestAsync<LoadSORs>(LoadSORq);

            //if (LoadSORs.statusCode != "1000")
            //{
            //    throw new System.Exception(LoadSORs.statusCode + " - " + Utilities.StatusCodeMessage(LoadSORs.statusCode));
            //}
            return LoadSORs.SalesOrder;
        }

        public async Task SoPaymentPut(Payment payment)
        {
            MakePaymentRq PaymentRq = new MakePaymentRq();
            MakePaymentRs PaymentRs = new MakePaymentRs();
            PaymentRq.Payment = payment;
            PaymentRs = await IssueJsonRequestAsync<MakePaymentRs>(PaymentRq);

            //if (PaymentRs == null)
            //{
            //    throw new FishbowlException("Error adding new payment to Fishbowl");
            //}

        }

        [Obsolete("Doesnt use the new phone and email fields. Use ImportSalesOrderAsync Instead") ]
        public async Task<string> SalesOrderPut(SalesOrder salesOrder)
        {
            SOSaveRq SOSaveRq = new SOSaveRq();
            SOSaveRs SOSaveRs = new SOSaveRs();
            SOSaveRq.SalesOrder = salesOrder;
            SOSaveRq.IssueFlag = "true";
            SOSaveRs = await IssueJsonRequestAsync<SOSaveRs>(SOSaveRq);
            //if (SOSaveRs?.statusCode != "1000")
            //{
            //    throw new FishbowlRequestException("Error Saving Sales Order - " + SOSaveRs?.statusMessage, null, SOSaveRs?.statusCode);

            //}
            return SOSaveRs.SalesOrder.Number;
        }


        #endregion

        #region Customers
        [Obsolete]
        public async Task<Customer> GetCustomerObject(string CustomerName)
        {
            CustomerGetRqType CustomerRq = new CustomerGetRqType();

            CustomerRq.Name = CustomerName;

            CustomerGetRsType CustomerRs = await this.IssueXMLRequestAsync<CustomerGetRsType>(CustomerRq);


            if (CustomerRs != null)
            {
                return CustomerRs.Customer;
            }
            else
            {
                return null;
            }
        }

        public async Task<string> CustomerPut(Customer customer)
        {
            CustomerSaveRq CustSaveRq = new CustomerSaveRq();
            CustSaveRq.Customer = customer;

            CustomerSaveRs CustSaveRs = new CustomerSaveRs();
            CustSaveRs = await IssueJsonRequestAsync<CustomerSaveRs>(CustSaveRq);

            //if (CustSaveRs == null)
            //{
            //    throw new Exception("Error Saving Customer.");
            //}
            //else if (CustSaveRs.statusCode != "1000")
            //{
            //    throw new FishbowlRequestException("Error saving customer - " + CustSaveRs.statusMessage, null, CustSaveRs.statusCode);
            //}

            //int NewCustomerID;
            return CustSaveRs.Customer.AccountID;//, out NewCustomerID))

        }



        #endregion

        public static bool ValidateTracking(int TrackingTypeID, string TrackingValue)
        {
            switch (TrackingTypeID)
            {
                case 10:
                case 40:
                    //text and serial number
                    //just need to chck for invalid symbols like commas?
                    return true;

                //break;

                case 20:
                case 30:
                    //Date
                    DateTime parsed;

                    if (DateTime.TryParse(TrackingValue, out parsed))
                    {
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("Tracking Value must be a date");
                    }
                //break;

                case 50:
                case 60:
                case 70:
                    //double - count and quantity
                    //currency - decimal
                    double doubleOut;

                    if (double.TryParse(TrackingValue, out doubleOut))
                    {
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("Tracking Value must be a number");
                    }
                //break;

                case 80:
                    //boolean
                    if (TrackingValue.ToLower() == "true" || TrackingValue.ToLower() == "false")
                    {
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("Tracking Value must be true or false");
                    }
                //break;

                default:
                    return false;
            }
        }

    }



    /// <summary>
    /// Fishbowl Debug Output Level
    /// </summary>
    public enum FishbowlConnectDebugLevel
    {
        Information,
        Verbose
    }

    public enum FishbowlConnectionStatus
    {
        NotConnected,
        Partial,
        FullyConnected
    }

    public class SalesOrderListInfo
    {
        //status, number, contact, bill to name, address, contact main phone, contact mobile phone (ZCoalesce 2 numbers),
        //customer po, and vendor po

        public string status { get; set; }
        public string orderNumber { get; set; }
        public string CustomerName { get; set; }
        public string contact { get; set; }
        public string BillTo { get; set; }
        public string street { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public string CustomerPO { get; set; }
        public string VendorPO { get; set; }
    }

    public class SalesOrderListFilters
    {
        public string SearchField { get; set; }
        public string Status { get; set; }
    }

    public enum LocationLookupType
    {
        LocationID,
        TagNumber
    }

    #endregion
}
