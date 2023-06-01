using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Manager;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>
/// METATRADER4
/// </summary>
/// ESPECIFICACIONES DE LA HERRAMIENTA
/// Metatrader ofrece a traves de la dll informacion de las cuotas de divisas BID-ASK por cada SYMBOL
/// TradeUserHistory(login, )
/// 
/// Explicacion de las entidades
/// ActivationType -SL=Stop Loss TP= Take Profit,Pending= pendiente activacion, Stopout fuera,SLRollback=fuera,TPRollback=pendiente reverso, StopoutRollback=parada de regreso
/// BackupInfo - File=archivo nombre backup, size=tamaño archivo, Time=creado.
/// BackupMode - All backup Todo backup; Periodical= periodo, Startup = inicia, Delete borrado
/// BalanceDiff - Login = numero cuenta agemte, Diff= defencia del balance
/// ChartInfo(ChartPeriod(period), ChartRequestMode(Mode)) - Symbol=divisa, Period= periodo visualiza, Start =valor inicio, End valorfinal, 
/// TimeSign - fecha del chart
/// ChartRequestMode - modo de peticion
/// ChartPeriod - marca el periodo de refrescar el chart
/// ChartRequestMode - RangeIn Rango entrada, RangeOut rango de salida, RangeLast ultimo rango
/// DailyGroupRequest -  la solicitud grupal diaria -  Name , From, to desde hasta
/// DailyReport - Objeto que representa el informe diario; Login , Ctm=tiempo, Group= grupo, Bank= banco, BalancePrev = balance previo; Balance,Deposit,Credit, ProfitClosed,    Profit,Equity,  Margin,MarginFree 
/// FeedDescription(FeederModes(modes)) = configuracion del servidor mt4
/// GroupCommandInfo(GroupCommands(command))  - seguimiento al grupo
/// GroupCommands - usuarios borrados, activos, deshabilitados, inicializado grupo
/// PumpingNotificationCodes - notificaciones enviadas a MT4
/// mailbox - configuracion email mt4
/// MarginLevel(MarginLevelType(MarginType, LevelType)) - niveles de margenes para parar cuando este por debajo o por encima con un limite
/// MetaTraderException - errores de la plataforma mt4
/// NewsTopic - representa noticias en mt4
/// OnlineRecord - representa operacion en linea por agente
/// RateInfo - representa la barra velero
/// ReportGroupRequest - Name= grupos nombres son agrupados por tendencia o equipo de trabajo
/// RequestInfo(TradeTransInfo(type). TradeRequestStatus(status) ) - TradeRequestStatus=estado req, Login=agent , Group= nombre grupo, Balance = saldo, Credit= tiene credito, list<Prices> Bid ask</Prices>   - TradeTransInfo trade  
/// ServerFeed(FeedDescription(feed)) = registra la conf de la cuenta mt4
/// ServerLog - registra todo el log de mt4
/// SymbolInfo(SymbolPriceDirection(Direction)) - toda la informacion del stock apertura, cierre, volumen transacciones, etc
/// SymbolPriceDirection - direccion predecida arriba, bajada none
/// SymbolProperties - propagacion , suavizado, 
/// SymbolSummary -resumen del estado dela divisa, o empresa APPL o commodities, etc, nro ordenes OPV, cantidad, lotes de compra, lotes de venta,utilidad precio de compra, precio d eventa
/// TickInfo - cuadrante de estado, tick compra bidm venta, 
/// TickRecord(TickRecordFlags(Flags)) registris del cuadrante 
/// TickRequest - muestra los diferentes estados historicos del cuadrante de un simbolo determinado
/// TradeTransInfo(tradecommand(cmd), TradeTransactionType(Type), )
/// TradeCommand - clasificacion de los tipos de comando de trade 0, compra, 1 venta, limites, etc
/// TradeReason - medio desde se genero la OPV
/// TradeRecord(TradeCommand(Cmd), TradeState(State), TradeReason(Reason), ActivationType(Activation)) - representa los registros de OPV
/// TradeRequestStatus - Estado del trade, vacio, bloqueado, respondido, reseteadoi,m cancelado, etc
/// TradeState - Open normal,  Close normal,  Closed by, Deleted
/// TradeTransactionType - Prices requets. Open order (Instant Execution), Open order (Market Execution)

namespace MTService
{
    enum ErrorCode : ushort
    {
        None = 0,
        Unknown = 1,
        ConnectionLost = 100,
        OutlierReading = 200,
        CmdExecutedOK = 9000,
        CmdExecutedFail = 6000,
        AccessDenied = 6301,
        PermisionRevoked = 6302,
        PlanDown = 6303,
        PlanBlock = 6304,
    }
    public class Configmt4
    {
        public int Mlm { get; set; }
        public int User { get; set; }
        public string Password { get; set; }
        public string Ip { get; set; }
        public int Days { get; set; }
    }
    
    //int User = 100;
    //string Password = "reports4321";
    //string Ip = "213.175.208.159:443";
    class Program
    {
        //public static WSSms.Service wsms = new WSSms.Service();
        //TODO: agregar servicio web de validar licencias. ejecutar la validacion de la licencia
        //crear el servicio windows que funcione con la sincronizacion
        //realizar las interafaces para los otros brokers xm, etc
        static void Main(string[] args)
        {
            #region tables
            //List<ConGroup> cg = new List<ConGroup>();
            //empresaVO = wsms.ObtenerPersonas();
            #endregion
            WSSms.Service wsms = new WSSms.Service();
            List<UserRecord> lstUser = new List<UserRecord>();
            WSSms.PersonasVO _user = new WSSms.PersonasVO();
            WSSms.TraderecordsVO _userbdTrades = new WSSms.TraderecordsVO();
            List<TradeRecord> lstTradeRecord = new List<TradeRecord>();
            wsms.Credentials = System.Net.CredentialCache.DefaultCredentials;
            Configmt4 cfg = new Configmt4();
            ReadBrokerConfig(cfg);
            List<WSSms.TraderecordsVO> _traderecordsVO;
            List<WSSms.PersonasVO> _personasVO;
            WSSms.ParametrosVO _parametrosVO;
            
            MTService.API_Interface API_mt4 = new MTService.API_Interface();
            if (!API_mt4.Initialize())
            {
                Console.WriteLine("Fallo la inicialización");
            }
            else
            {
                _personasVO = new List<WSSms.PersonasVO>();
                _traderecordsVO  = new List<WSSms.TraderecordsVO>();
                _parametrosVO = new WSSms.ParametrosVO();

                if (wsms == null)
                {
                    wsms = new WSSms.Service();
                    wsms.Credentials = System.Net.CredentialCache.DefaultCredentials;
                }
                string ServerName = System.Environment.MachineName;
                string IpAddress = Dns.GetHostAddresses(Environment.MachineName)
                                            .First(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
             
                if (cfg.Mlm.ToString() == "0")
                {
                    ReadBrokerConfig(cfg);
                }
                _parametrosVO = wsms.ObtenerParametrosBroker(cfg.Mlm);// , ServerName, IpAddress);
                if (API_mt4.Login(cfg.Ip, cfg.User, cfg.Password))
                {
                    Console.WriteLine("Connected");
                }
                else { Console.WriteLine("Failed Connected"); return; }
                if (wsms.EliminarTraderecordsAll() == -1)
                {
                    Console.WriteLine("Registros Trade Eliminados");
                }
                // listado de usuarios de la plataforma MT4 de todas las cuentas.
                lstUser = API_mt4.UserTrades(out uint servertime);
                //////////////////////grupos//////////////////////
                // borra los grupos del broker
            // if (wsms.EliminarGruposmoviles(cfg.Mlm.ToString()) == -1)
            //    {
            //        Console.WriteLine("Grupos Eliminados.");
            //    }
                WSSms.GruposmovilesVO _grupoVO = new WSSms.GruposmovilesVO();
                _grupoVO.pnit = cfg.Mlm;
                _grupoVO.pgrupo = 0;
                _grupoVO.pnombregrupo = "None";
                _grupoVO.pestado = true;
                _grupoVO.preportes = true;
                //_grupoVO.Fecha = System.DateTime.Now.ToString("yyyy/MM/dd");
                //_grupoVO.Hora = System.DateTime.Now.ToString("hh:mm:ss");
                // inserta los grupos detectados en la sincronizacion
                if (wsms.InsertarGruposmoviles(_grupoVO) == -1)
                {
                    Console.WriteLine("Registro no insertado: " + _user.Login.ToString());
                }
                _grupoVO = null;
                var Grupos = (from r in lstUser.AsEnumerable()
                              select new
                              {
                                  grupo = r.@group
                              }).Distinct();
                for(int i=0;i<=Grupos.Count()-1;i++)
                {
                    _grupoVO = new WSSms.GruposmovilesVO();
                    _grupoVO.pnit = cfg.Mlm;
                    _grupoVO.pnombregrupo = Grupos.ElementAt(i).grupo.ToString();
                    _grupoVO.pgrupo = i+1;
                    _grupoVO.pestado = true;
                    _grupoVO.preportes = true;
                    //_grupoVO.Fecha = System.DateTime.Now.ToString("yyyy/MM/dd");
                    //_grupoVO.Hora = System.DateTime.Now.ToString("hh:mm:ss");
                    if (wsms.InsertarGruposmoviles(_grupoVO) == -1)
                    {
                        Console.WriteLine("Registro grupo no insertado: "+ _grupoVO.pnombregrupo.ToString());
                    }
                    _grupoVO = null;
                }
                //////////////////end grupos////////////////
                foreach (var userT in lstUser)
                {
                    if (_user == null)
                    { _user = new WSSms.PersonasVO(); }
                    _user.pcedula = userT.login;
                    _user.pnombre = userT.name == string.Empty ? "-" : userT.name;
                    _user.pcelular = userT.phone == string.Empty ? "-" : userT.phone;
                    _user.pdireccion = userT.address == string.Empty ? "-" : userT.address;
                    _user.ptelefono = userT.phone == string.Empty ? "-" : userT.phone;
                    _user.pcodpais = 0;
                    _user.pcodciudad = 0;
                    _user.pmarcacel = "-";
                    _user.pmodelocel = "-";
                    _user.pperfil = userT.id == string.Empty ? "-" : userT.id;
                    _user.pedad = 0;
                    _user.pemail = userT.email == string.Empty ? "-" : userT.email;
                    if (userT.group == "MT-U-X-001")
                        _user.pgrupo = 1;
                    if (userT.group == "Agent-MT-USD")
                        _user.pgrupo = 2;
                    _user.poperador = userT.id == string.Empty ? "-" : userT.id;
                    _user.pnit = 10001;
                    _user.pis_master = 0;
                    _user.pis_ib_master = 0;
                    _user.pis_ib_normal = 0;
                    _user.Login = userT.login;
                    _user.Group = userT.group;
                    _user.Password = userT.login.ToString();
                    _user.Enable = userT.enable == 0 ? 0 : userT.enable;
                    _user.EnableChangePassword = userT.enable_change_password == 0 ? 0 : userT.enable_change_password;
                    _user.EnableReadOnly = userT.enable_read_only == 0 ? 0 : userT.enable_read_only;
                    _user.EnableOTP = userT.enable_read_only == 0 ? 0 : userT.enable_read_only;
                    _user.PasswordInvestor = "-";
                    _user.PasswordPhone = "-";
                    _user.Name = userT.name == string.Empty ? "-" : userT.name;
                    _user.Country = userT.country == string.Empty ? "-" : userT.country;
                    _user.City = userT.city == string.Empty ? "-" : userT.city;
                    _user.State = userT.state == string.Empty ? "-" : userT.state;
                    _user.ZipCode = userT.zipcode == string.Empty ? "-" : userT.zipcode;
                    _user.Address = userT.address == string.Empty ? "-" : userT.address;
                    _user.LeadSource = userT.lead_source == string.Empty ? "-" : userT.lead_source;
                    _user.Phone = userT.phone == string.Empty ? "-" : userT.phone;
                    _user.Comment = userT.comment == string.Empty ? "-" : userT.comment;
                    _user.Id = userT.id == string.Empty ? "-" : userT.id;
                    _user.Status = userT.status == string.Empty ? "-" : userT.status;
                    _user.Regdate = userT.regdate == 0 ? 0 : userT.regdate;
                    _user.LastDate = userT.lastdate == 0 ? 0 : userT.lastdate;
                    _user.Leverage = userT.leverage == 0 ? 0 : userT.leverage;
                    _user.AgentAccount = userT.agent_account == 0 ? 0 : userT.agent_account;
                    _user.Timestamp = userT.timestamp == 0 ? 0 : userT.timestamp;
                    _user.LastIp = 0;
                    //_user.Balance = userT.balance == 0 ? 0 : userT.balance;
                    _user.Balance = API_mt4.GetAgentCommissions(userT.login, cfg.Days);
                    _user.PrevMonthBalance = userT.prevmonthbalance == 0 ? 0 : userT.prevmonthbalance;
                    _user.PrevBalance = userT.prevbalance == 0 ? 0 : userT.prevbalance;
                    _user.Credit = userT.credit == 0 ? 0 : userT.credit;
                    _user.InterestRate = userT.interestrate == 0 ? 0 : userT.interestrate;
                    _user.Taxes = userT.taxes == 0 ? 0 : userT.taxes;
                    _user.PrevMonthEquity = userT.prevmonthequity == 0 ? 0 : userT.prevmonthequity;
                    _user.PrevEquity = userT.prevmonthequity == 0 ? 0 : userT.prevmonthequity;
                    _user.PublicKey = "-";
                    _user.SendReports = userT.send_reports == 0 ? 0 : userT.send_reports;
                    _user.OTPSecret = userT.otp_secret == String.Empty ? "-" : userT.otp_secret;
                    _user.Mqid = userT.mqid == 0 ? 0 : userT.mqid;
                    _user.UserColor = userT.user_color == 0 ? 0 : userT.user_color;
                    if (wsms == null)
                    {
                        wsms = new WSSms.Service();
                        wsms.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    }
                    if (wsms.EliminarPersonaLogin(_user.Login) == -1)
                    {
                        Console.WriteLine("Registro no eliminado: " + _user.Login.ToString());
                    }
                    if (wsms.InsertarPersona(_user) == -1)
                    {
                        Console.WriteLine("Registro no insertado: " + _user.Login.ToString());
                    }
                    _personasVO.Add(_user);
                    _user = null;
                }
                foreach (var userT in lstUser)
                {
                    lstTradeRecord = API_mt4.UserTradesRecords(userT.login, out uint time);
                    foreach (var userTrades in lstTradeRecord)
                    { 
                        if (_userbdTrades == null)
                        { _userbdTrades = new WSSms.TraderecordsVO(); }
                        _userbdTrades.pnit = cfg.Mlm;
                       // _userbdTrades.plogin = userTrades.login == 0 ? 0 : userTrades.login;
                      //  _userbdTrades.pnitafiliation = long.Parse(cfg.Mlm.ToString() + _userbdTrades.plogin.ToString());
                        _userbdTrades.porder = userTrades.order == 0 ? 0 : userTrades.order;
                        _userbdTrades.psymbol = userTrades.symbol == string.Empty ? "NONE" : userTrades.symbol;
                        _userbdTrades.pcmd = userTrades.cmd == 0 ? 0 : userTrades.cmd;
                        _userbdTrades.pvolume = userTrades.volume == 0 ? 0 : userTrades.volume;
                        _userbdTrades.popen_time = userTrades.open_time== 0 ? 0 : userTrades.open_time;
                        _userbdTrades.pstate = userTrades.state== 0 ? 0 : userTrades.state;
                        _userbdTrades.popen_price = userTrades.open_price== 0 ? 0 : userTrades.open_price;
                        _userbdTrades.psl = userTrades.sl == 0 ? 0 : userTrades.sl;
                        _userbdTrades.ptp = userTrades.tp== 0 ? 0 : userTrades.tp;
                        _userbdTrades.pclose_time= userTrades.close_time== 0 ? 0 : userTrades.close_time;
                        _userbdTrades.pgw_volume = userTrades.gw_volume== 0 ? 0 : userTrades.gw_volume;
                        _userbdTrades.pexpiration = userTrades.expiration == 0 ? 0 : userTrades.expiration;
                        _userbdTrades.pconv_rates= userTrades.conv_rate1 == 0 ? 0 : userTrades.conv_rate1;
                        _userbdTrades.pcommision = userTrades.commission == 0 ? 0 : userTrades.commission;
                        _userbdTrades.pcommision_agent = userTrades.commission_agent== 0 ? 0 : userTrades.commission_agent;
                        _userbdTrades.pstorage = userTrades.storage== 0 ? 0 : userTrades.storage;
                        _userbdTrades.pclose_price = userTrades.close_price== 0 ? 0 : userTrades.close_price;
                        _userbdTrades.pprofit = userTrades.profit== 0 ? 0 : userTrades.profit;
                        _userbdTrades.ptaxes = userTrades.taxes== 0 ? 0 : userTrades.taxes;
                        _userbdTrades.pcomment= userTrades.comment== string.Empty ? "NONE" : userTrades.comment;
                        _userbdTrades.pgw_order = userTrades.gw_order== 0 ? 0 : userTrades.gw_order;
                        _userbdTrades.pgw_open_price = userTrades.gw_open_price== 0 ? 0 : userTrades.gw_open_price;
                        _userbdTrades.pgw_close_price = userTrades.gw_close_price== 0 ? 0 : userTrades.gw_close_price;
                        _userbdTrades.pmargin_rate = userTrades.margin_rate== 0 ? 0 : userTrades.margin_rate;
                        _userbdTrades.ptimestamp= userTrades.timestamp== 0 ? 0 : userTrades.timestamp;
                        _userbdTrades.psync = 0;
                        _userbdTrades.pidusuario = "10001";
                        _userbdTrades.pfcha_act = System.DateTime.Now.ToShortDateString();
                        _userbdTrades.phora_act = System.DateTime.Now.ToString("HH:MM:SS");
                        if (wsms == null)
                        {
                            wsms = new WSSms.Service();
                            wsms.Credentials = System.Net.CredentialCache.DefaultCredentials;
                        }
                        
                        //System.Threading.Thread.Sleep(3000);
                        if (wsms.InsertarTraderecords(_userbdTrades) == -1)
                        {
                            Console.WriteLine("Registro no insertado: " + _user.Login.ToString());
                        }
                        _traderecordsVO.Add(_userbdTrades);
                        _userbdTrades = null;
                    }
                }
              
            }
            Console.WriteLine("Disposing");
            API_mt4.Dispose();
            Console.ReadKey();
        }

        public static string ReadBrokerConfig(Configmt4 cfg)
        {
            cfg.Mlm = 10001;
            cfg.User = 100;
            cfg.Password = "reports4321";
            cfg.Ip = "213.175.208.159:443";
            return "OK";
            //int counter = 0;
            //string line = string.Empty;
            //try {
            //    System.IO.StreamReader file =
            //    new System.IO.StreamReader(@"C:\tmp\config.txt");
            //    while ((line = file.ReadLine()) != null)
            //    {
            //        if (counter == 0)
            //        {
            //            cfg.Mlm = int.Parse(line.ToString());
            //        }
            //        if (counter == 1)
            //        {
            //            cfg.User = int.Parse(line.ToString());
            //        }
            //        if (counter == 2)
            //        {
            //            cfg.Password = line.ToString();
            //        }
            //        if (counter == 3)
            //        {
            //            cfg.Ip = line.ToString();
            //        }
            //        if (counter == 4)
            //        {
            //            cfg.Days = int.Parse(line.ToString());
            //        }
            //        System.Console.WriteLine(line);
            //        counter++;
            //    }
            //    line = "OK";
            //    file.Close();
            //}
            //catch (Exception ex)
            //{
            //    line = "FAIL: "+ex.Message.ToString();
            //}
            //finally {
            //    GC.Collect();                   
            //}
            //return line;
        }

        #region LiquidacionImpuestos
        public DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
        #endregion

        private bool ILog(int tipo, int op, int prio, string msg)
        {
            WSSms.LogsVO logsVO = new WSSms.LogsVO();
            logsVO.Datelog = System.DateTime.Now.ToString("dd/MM/yyyy");
            logsVO.Hourlog = System.DateTime.Now.ToString("hh:mm:ss");
            logsVO.Idoperation = op;
            logsVO.Iduser = 0;  //user
            logsVO.Ip = "";
            logsVO.Menu = "Equipo Remoto: " + ""+ " Menu: frmAutentica";
            logsVO.Nit = 0;
            logsVO.Observation = msg;
            logsVO.Prioridad = prio;
            logsVO.Typelog = tipo;
            //wsms.InsertarLogs(logsVO);
            return true;
        }
    }
}