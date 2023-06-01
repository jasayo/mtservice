#region imports
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manager;
#endregion
namespace MTService
{
    class API_Interface : IDisposable
    {
        MeTatraderManager MT4 = new MeTatraderManager();
        public List<UserRecord> lstUser = new List<UserRecord>();
        public List<TradeRecord> lstTradeRecord = new List<TradeRecord>();
        
        public  API_Interface()
        {

        }
        public bool Login(string Server, int Login, string Password)
        {
            if(MT4.Connect(Server) != RetValues.RET_OK)
            {
                Console.WriteLine("Cannot Connect -check ip address");
                return false;
            }
            var res = MT4.Login(Login, Password);
            Console.WriteLine(res);
            if(res==RetValues.RET_OK)
            {
                return true;
            }
            return false;
        }
        public ConGroup GroupRecordGet(string group)
        {
            return MT4.GroupRecordGet(group);
        }
        public List<ConGroup> GroupsRequest()
        {
            return MT4.GroupsRequest();
        }
        public List<UserRecord> UserTrades(out uint serverTime)
        {
            serverTime = MT4.GetServerTime();
            var result = MT4.ManagerCommon(out var conCommon);
            return MT4.UsersRequest();
        }
        
        public List<TradeRecord> UserTradesRecords(int login, out uint serverTime)
        {
            serverTime = MT4.GetServerTime();
            int result = System.DateTime.Now.Year * 10000 + System.DateTime.Now.AddMonths(-1).Month * 100
             + System.DateTime.Now.Day + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second;
            return MT4.TradesUserHistory(login, result, (int)serverTime);
        }

        #region loaddispose
        public bool Initialize()
        {
            MT4 = new MeTatraderManager();
            return true;
        }

        public Double GetAgentCommissions(int TradingAccoutLogin, int days)
        {
            double TotalCommision = 0;
            try {
                var listadeTrades = MT4.TradesUserHistory(TradingAccoutLogin, UnixTime(days), UnixTime(1));
                if (listadeTrades.Count > 0)
                {
                    foreach (var Trade in listadeTrades)
                    {
                        TotalCommision += Trade.commission_agent;
                    }
                }
                if (TotalCommision != 0)
                {
                    Console.WriteLine("Trader :" + TradingAccoutLogin + " Total Commission " + TotalCommision);
                }
            }
            catch (Exception ex) {
                return 0;
            }
            return TotalCommision;
        }
        public static int UnixTime(int offset)
        {
            DateTime Foo = DateTime.Today.AddDays(offset);
            int unix = (int)((DateTimeOffset)Foo).ToUnixTimeSeconds();
            return unix;
        }

        public void Dispose()
        {
            try
            {
                MT4.Dispose();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message+ " - Cannot dispose");
            }
           // throw new NotImplementedException();
        }
        #endregion
    }
}
