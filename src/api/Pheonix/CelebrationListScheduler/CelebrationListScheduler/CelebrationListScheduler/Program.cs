using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using log4net;
using Newtonsoft.Json;

namespace CelebrationListScheduler
{
    class Program
    {
        private static readonly ILog Log4Net = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            Log4Net.Debug("Celebration List job started: =" + DateTime.Now);
            try
            {
                ContextDetails _ContextCredentialDetails = new ContextDetails();
                string userName = _ContextCredentialDetails.SPUserName;
                SecureString password = GetPassword();
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                using (var clientContext = new ClientContext(_ContextCredentialDetails.SPSiteUrl))
                {
                    clientContext.Credentials = new SharePointOnlineCredentials(userName, password);
                    
                    Web web = clientContext.Web;
                    clientContext.Load(web);
                    clientContext.ExecuteQuery();
                    
                    bool isYesterdayHoliday = false;
                    List holidayList = clientContext.Web.Lists.GetByTitle(Constants.holidayList);
                    clientContext.Load(holidayList);
                    clientContext.Load(holidayList.Fields);
                    clientContext.ExecuteQuery();
                    
                    var spTimeZone = clientContext.Web.RegionalSettings.TimeZone;
                    clientContext.Load(spTimeZone);
                    clientContext.ExecuteQuery();
                    
                    var fixedTimeZoneName = spTimeZone.Description.Replace("and", "&");
                    var timeZoneInfo = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(tz => tz.DisplayName == fixedTimeZoneName);
                    int yesterday = (int)System.DateTime.Now.DayOfWeek - 1;
                    int currentDay = (int)System.DateTime.Now.DayOfWeek;

                    DateTime today = DateTime.Today;
                    DateTime _yesterday = DateTime.Now.AddDays(-1);
                    string currentDate = today.ToString("MM/dd/yyyy");
                    string yesterdayDate = _yesterday.ToString("MM/dd/yyyy");
                    CamlQuery query = new CamlQuery();
                    query.ViewXml = @"<View>" +
                                         "<Query>" +
                                             "<Where>" +
                                             "<Neq>" +
                                                 "<FieldRef Name='Title'/>" +
                                                 "<Value Type='Text'>" + currentDate + "</Value>" +
                                             "</Neq>" +
                                             "</Where>" +
                                         "</Query>" +
                                         "</View>";

                    ListItemCollection allOpenItems = holidayList.GetItems(query);
                    clientContext.Load(allOpenItems);
                    clientContext.ExecuteQuery();

                    foreach (ListItem item in allOpenItems)
                    {
                        DateTime tzMitigationTragetDate = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item["H_Date"]), timeZoneInfo);
                        string dateText = tzMitigationTragetDate.ToString("MM/dd/yyyy");
                        string title = item["Title"].ToString();
                        if (yesterdayDate == dateText)
                        {
                            isYesterdayHoliday = true;
                            break;
                        }
                    }

                    //if ((currentDay != 6 || currentDay != 7) && isHoliday == false)
                    //    DeleteData(clientContext);

                    //if (isHoliday == true && (yesterday != 6 || yesterday != 7))
                    //{
                    //    DeleteData(clientContext);
                    //}
                    //else if (currentDay != 6 || currentDay != 7)
                    //{
                    //    DeleteData(clientContext);
                    //}
                    //if (currentDay != 7)
                    //{
                    //    if (isYesterdayHoliday == false && yesterday != 7 && yesterday != 0)
                    //    {
                    //        DeleteData(clientContext);
                    //    }
                    //}

                    if (System.DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
                        if (isYesterdayHoliday == false && (System.DateTime.Now.DayOfWeek - 1) != DayOfWeek.Sunday && (System.DateTime.Now.DayOfWeek - 1) != DayOfWeek.Saturday)
                            DeleteData(clientContext);

                    ///Add Data to List
                    AddData(clientContext);
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("Exception Message: " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry("CelebrationListJobScheduler : Funtion - Main() -", ex.Message, EventLogEntryType.Error);
            }
            Log4Net.Debug("Celebration List job finished: =" + DateTime.Now);
        }

        public class ContextDetails
        {
            public string SPUserName
            {
                get { return ConfigurationManager.AppSettings["UserName"]; }
            }

            public string SPPassword
            {
                get { return ConfigurationManager.AppSettings["Password"]; }
            }

            public string Domain
            {
                get { return "v2solutions"; }
            }

            public string SPSiteUrl
            {
                get { return ConfigurationManager.AppSettings["URL"]; }
            }

            public string VWRSiteUrl
            {
                get { return ConfigurationManager.AppSettings["VBRUrl"]; }
            }
        }

        private static SecureString GetPassword()
        {
            ContextDetails _ContextCredentialDetails = new ContextDetails();
            SecureString securePassword = new SecureString();
            foreach (char c in _ContextCredentialDetails.SPPassword)
                securePassword.AppendChar(c);
        
            return securePassword;
        }

        private static void DeleteData(ClientContext context)
        {
            try
            {
                List listCelebration = context.Web.Lists.GetByTitle(Constants.listName);
                context.Load(listCelebration);
                context.Load(listCelebration.Fields);
                context.ExecuteQuery();
                
                int itemCount = listCelebration.ItemCount;
                if (listCelebration != null && listCelebration.ItemCount > 0)
                {
                    CamlQuery allItems = new CamlQuery();
                    allItems.ViewXml = @"<View><ViewFields><FieldRef Name='Id'/></ViewFields></View>";
                    
                    ListItemCollection allOpenItems = listCelebration.GetItems(allItems);
                    context.Load(allOpenItems);
                    context.ExecuteQuery();
                    foreach (ListItem item in allOpenItems.ToList())
                        item.DeleteObject();

                    context.ExecuteQuery();
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("DeleteData Exception Message: " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry("DeleteData : ", ex.Message, EventLogEntryType.Error);
                AddErrorintoList(context, ex.Message, "DeleteData");
            }
        }

        private static void AddErrorintoList(ClientContext Context, string message, string ID)
        {
            List oList = Context.Web.Lists.GetByTitle("ErrorLog");
            if (oList != null)
            {
                try
                {
                    ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                    ListItem oListItem = oList.AddItem(itemCreateInfo);
                    oListItem["Title"] = "Error DateTime: " + DateTime.Now;
                    oListItem["ErrorType"] = "Scheduler -RiskID- " + ID;
                    oListItem["ErrorMessage"] = message;
                    oListItem.SystemUpdate();
                    Context.ExecuteQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private static void AddData(ClientContext context)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; 
                using (var client = new HttpClient())
                {
                    ContextDetails _ContextCredentialDetails = new ContextDetails();
                    client.BaseAddress = new Uri(_ContextCredentialDetails.VWRSiteUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // New code:
                    var responseTask = client.GetAsync("v1/sharepoint/GetCelebrationListData").Result;
                    string jsonStr = "";
                    using (HttpContent content = responseTask.Content)
                    {
                        Task<string> temp = content.ReadAsStringAsync();
                        jsonStr = temp.Result;
                    }
                    var listData = JsonConvert.DeserializeObject<List<CelebrationList>>(jsonStr);

                    List listCelebration = context.Web.Lists.GetByTitle(Constants.listName);
                    context.Load(listCelebration);
                    context.Load(listCelebration.Fields);
                    context.ExecuteQuery();
                    foreach (var item in listData)
                    {
                        ListItemCreationInformation listCreationInformation = new ListItemCreationInformation();
                        ListItem oListItem = listCelebration.AddItem(listCreationInformation);
                        oListItem[Constants.EmployeeID] = item.EmployeeID;
                        oListItem[Constants.EmployeeName] = item.EmployeeName;
                        oListItem[Constants.SpouseName] = item.SpouseName;
                        oListItem[Constants.DOB] = item.CelebrationDate;
                        oListItem[Constants.EmployeeEmailID] = item.OrganizationEmail;
                        oListItem[Constants.Category] = item.Category;
                        oListItem[Constants.OfficeLocation] = item.LocationName;
                        oListItem.Update();
                        context.ExecuteQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log4Net.Error("AddData Exception Message: " + ex.Message);
                System.Diagnostics.EventLog.WriteEntry("AddData : ", ex.Message, EventLogEntryType.Error);
                AddErrorintoList(context, ex.Message, "AddData");
            }
        }
    }
}