using DatafeedClient.DataModels;
using Newtonsoft.Json;
using QuickFix;
using QuickFix.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace DatafeedClient
{
    public class MyFixApplication : QuickFix.MessageCracker, IApplication //more details here: http://quickfixn.org/tutorial/creating-an-application.html
    {
        private Session _hscSession;
        #region FIX Application Event Handler need to be implement
        public void OnCreate(SessionID sessionID) // -> this event handler will be called when start the FIX Application
        {            
            _hscSession = Session.LookupSession(sessionID); // we store HSC's session to use later, this Session will be use to send message to HSC
        }
        public void OnLogon(SessionID sessionID)
        {
            // -> QUICKFIX will call this event handler before send logon Message to HSC Datafeed APP , you can use this event handler to do logging 
            Console.WriteLine("Send logon message to HSC:" + sessionID.ToString());
        }
        public void FromAdmin(Message msg, SessionID sessionID) // -> every received admin level message will pass through this method, such as heartbeats, logons, and logouts.
        {
            if (msg.GetType() == typeof(QuickFix.FIX44.Logon))
            {
                //received logon ack from HSC
                Console.WriteLine("Logon Success");
                //send security list request
                SendSecurityListRequest();
            }
        }
        public void FromApp(Message msg, SessionID sessionID) // -> this event handler will be called when it received a FIX message
        {

            Console.WriteLine("REVEIVED FIX MESSAGE:  " + msg.ToString()); //do logging
            try
            {
                Crack(msg, sessionID); //base on type of message, this function will call the appropriate OnMessage() medthod, please implement more OnMessage handler for your application requirement
                //on this example, when received Security List response from HSC, the Crack Function will call the OnMessage(QuickFix.FIX44.SecurityList msg,SessionID sessionID) below

                //more details: http://quickfixn.org/tutorial/receiving-messages.html
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }
       
        public void OnLogout(SessionID sessionID) { // -> this event handler will be called when Logout
            
        }
        
        
        public void ToAdmin(Message msg, SessionID sessionID) {
            
        }
        public void ToApp(Message msg, SessionID sessionID) { //-> this event handler will be called before message sent out
            
        }

        #endregion
        public void OnMessage(QuickFix.FIX44.SecurityList msg,SessionID sessionID) // -> this method get called by the Crack(msg,SessionID)
        {
            //received Security List message from HSC
            //parse this message to get Instrument's info like symbol, ref price 
            //please view HSC Datafeeed document for detail info about Security List message            
            Console.WriteLine("ON SecurityList Response:" + msg.ToString());

            int result = msg.SecurityRequestResult.getValue(); //0 = success , 1 = reject
            if (result == 0)
            {
                int noRelatedSym = msg.GetInt(Tags.NoRelatedSym);
                //parse repeating group, more details: http://quickfixn.org/tutorial/repeating-groups.html
                if (noRelatedSym > 0)
                {
                    for (int i=0; i < noRelatedSym; i++)
                    {
                        var noRelatedSymGroup = new QuickFix.FIX44.SecurityList.NoRelatedSymGroup();
                        
                        msg.GetGroup(i+1, noRelatedSymGroup);
                        if (noRelatedSymGroup != null)
                        {
                            var StockSymbol = noRelatedSymGroup.GetString(QuickFix.Fields.Symbol.TAG);
                            Stock stock = new Stock { Symbol = StockSymbol };
                            var RefPrice = noRelatedSymGroup.GetDecimal(Tags.TradingReferencePrice);
                            stock.RefPrice = RefPrice;
                            Console.WriteLine("StockInfo:" + JsonConvert.SerializeObject(stock));
                        }
                    }
                    

                }
            }
            else
            {
                Console.WriteLine("Security list request was rejected");
            }
        }

        

        private void SendSecurityListRequest()
        {
            QuickFix.FIX44.SecurityListRequest msg = new QuickFix.FIX44.SecurityListRequest(new QuickFix.Fields.SecurityReqID("IdOfSecListRequest2"), new QuickFix.Fields.SecurityListRequestType(4));
            msg.SubscriptionRequestType = new SubscriptionRequestType('0');
            //use HSC Session to send message to HSC Datafeed
            _hscSession.Send(msg);
        }
    }
}
