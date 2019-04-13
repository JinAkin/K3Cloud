// This code was built using Visual Studio 2005
using System;
using TrackWebServiceClient.TrackServiceWebReference;
using System.Web.Services.Protocols;
using System.Collections.Generic;
using System.IO;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using System.Linq;
using Hands.K3.SCM.APP.WebService.FedExWebServices.Entity;

using Hands.K3.SCM.APP.Entity.SynDataObject.DeliveryNotice;

using Hands.K3.SCM.APP.Entity.StructType;
using Hands.K3.SCM.APP.Utils.Utils;
using HS.K3.Common.Abbott;

// Sample code to call the FedEx Track Web Service
// Tested with Microsoft Visual Studio 2017 Professional Edition
namespace Hands.K3.SCM.APP.WebService
{
    public class TraceWebService
    {
        public static List<DeliveryNotice> GetLogisticsTraceDetail(Context ctx)
        {
            #region
            //if (notices != null && notices.Count > 0)
            //{

            //    foreach (var notice in notices)
            //    {
            //        if (notice != null)
            //        {
            //            if (notice.TraceEntry != null && notice.TraceEntry.Count > 0)
            //            {
            //                foreach (var trace in notice.TraceEntry)
            //                {
            //                    if (trace != null)
            //                    {
            //                        TrackRequest request = CreateTrackRequest(trace.FCarryBillNo);
            //                        LogXML("trackingNumber:["+ trace.FCarryBillNo + "]  Request",request,typeof(TrackRequest));
            //                        //
            //                        TrackService service = new TrackService();
            //                        if (usePropertyFile())
            //                        {
            //                            service.Url = getProperty("endpoint");
            //                        }
            //                        //
            //                        try
            //                        {
            //                            // Call the Track web service passing in a TrackRequest and returning a TrackReply
            //                            TrackReply reply = service.track(request);

            //                            LogXML("trackingNumber:[" + trace.FCarryBillNo + "]  Reply", reply, typeof(TrackReply)); 

            //                            if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
            //                            {
            //                                ShowTrackReply(reply);
            //                                notice.LocusEntry = GetLocusDetail(reply);
            //                            }
            //                            ShowNotifications(reply);
            //                        }
            //                        catch (SoapException ex)
            //                        {
            //                            (ctx, SynchroDataType.DeliveryNoticeBill, ex.Message + System.Environment.NewLine + ex.StackTrace);
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            (ctx, SynchroDataType.DeliveryNoticeBill, ex.Message + System.Environment.NewLine + ex.StackTrace);
            //                        }

            //                    }
            //                }
            //            }
            //        }

            //    }
            //}
            //return notices;
            #endregion

            List<DeliveryNotice> notices = null;
            Dictionary<DeliveryNotice, TrackRequest> dict = CreateTrackRequest(GetTrackingNumbers(ctx));

            if (dict != null && dict.Count > 0)
            {
                notices = new List<DeliveryNotice>();

                foreach (var item in dict)
                {
                    if (item.Key != null && item.Value != null)
                    {
                        LogXML("trackingNumber:["+item.Key.FBillNo+"]  Request", item.Value, typeof(TrackRequest));
                        
                        TrackService service = new TrackService();
                        
                        try
                        {
                            // Call the Track web service passing in a TrackRequest and returning a TrackReply
                            TrackReply reply = service.track(item.Value);

                            LogXML("trackingNumber:["+item.Key.FBillNo+"]  Reply", reply, typeof(TrackReply));

                            if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                            {
                               
                                item.Key.LocusEntry = GetLocusDetail(reply);
                                notices.Add(item.Key);
                            }
                           
                        }
                        catch (SoapException ex)
                        {
                            LogUtils.WriteSynchroLog(ctx, SynchroDataType.DeliveryNoticeBill, ex.Message + System.Environment.NewLine + ex.StackTrace);
                        }
                        catch (Exception ex)
                        {
                            LogUtils.WriteSynchroLog(ctx, SynchroDataType.DeliveryNoticeBill, ex.Message + System.Environment.NewLine + ex.StackTrace);
                        }
                    }
                }
            }

            return notices;
        }

        private static Dictionary<DeliveryNotice,TrackRequest> CreateTrackRequest(List<DeliveryNotice> notices)
        {
            List<TrackRequest> requests = null;
            Dictionary<DeliveryNotice, TrackRequest> dict = null;

            if (notices != null && notices.Count > 0)
            {
                dict = new Dictionary<DeliveryNotice, TrackRequest>();
                requests = new List<TrackRequest>();

                foreach (var notice in notices)
                {
                    if (notice != null)
                    {
                        if (notice.TraceEntry != null && notice.TraceEntry.Count > 0)
                        {
                            foreach (var trace in notice.TraceEntry)
                            {
                                if (trace != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(trace.F_HS_CarryBillNO))
                                    {
                                        // Build the TrackRequest
                                        TrackRequest request = new TrackRequest();
                                        //
                                        request.WebAuthenticationDetail = new WebAuthenticationDetail();
                                        request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
                                        //request.WebAuthenticationDetail.UserCredential.Key = ConfigurationUtil.GetAppSetting("key"); // Replace "XXX" with the Key
                                        //request.WebAuthenticationDetail.UserCredential.Password = ConfigurationUtil.GetAppSetting("password"); // Replace "XXX" with the Password
                                        //request.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential();
                                        //request.WebAuthenticationDetail.ParentCredential.Key = ConfigurationUtil.GetAppSetting("parentkey"); // Replace "XXX" with the Key
                                        //request.WebAuthenticationDetail.ParentCredential.Password = ConfigurationUtil.GetAppSetting("parentpassword"); // Replace "XXX"
                                        
                                        //request.ClientDetail = new ClientDetail();
                                        //request.ClientDetail.AccountNumber = ConfigurationUtil.GetAppSetting("accountnumber"); // Replace "XXX" with the client's account number
                                        //request.ClientDetail.MeterNumber = ConfigurationUtil.GetAppSetting("meternumber"); // Replace "XXX" with the client's meter number
                                        
                                        request.TransactionDetail = new TransactionDetail();
                                        request.TransactionDetail.CustomerTransactionId = "***Track Request using VC#***";  //This is a reference field for the customer.  Any value can be used and will be provided in the response.
                                                                                                                            //
                                        request.Version = new VersionId();
                                        
                                        // Tracking information
                                        request.SelectionDetails = new TrackSelectionDetail[1] { new TrackSelectionDetail() };
                                        request.SelectionDetails[0].PackageIdentifier = new TrackPackageIdentifier();
                                       
                                        request.SelectionDetails[0].PackageIdentifier.Value = trace.F_HS_CarryBillNO;
                                        request.SelectionDetails[0].PackageIdentifier.Type = TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG;
                                        
                                        // Date range is optional.
                                        // If omitted, set to false
                                        request.SelectionDetails[0].ShipDateRangeBegin = DateTime.Parse("2016-08-01"); //MM/DD/YYYY
                                        request.SelectionDetails[0].ShipDateRangeEnd = request.SelectionDetails[0].ShipDateRangeBegin.AddDays(0);
                                        request.SelectionDetails[0].ShipDateRangeBeginSpecified = false;
                                        request.SelectionDetails[0].ShipDateRangeEndSpecified = false;
                                        
                                        // Include detailed scans is optional.
                                        // If omitted, set to false
                                        request.ProcessingOptions = new TrackRequestProcessingOptionType[1];
                                        request.ProcessingOptions[0] = TrackRequestProcessingOptionType.INCLUDE_DETAILED_SCANS;

                                        if (!dict.ContainsKey(notice))
                                        {
                                            dict.Add(notice, request);
                                        }
                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return dict;
        }

        private static List<DeliveryNoticeLocusEntry> GetLocusDetail(TrackReply reply)
        {
            List<DeliveryNoticeLocusEntry> locuses = null;
            DeliveryNoticeLocusEntry locus = null;

            if (reply != null)
            {
                locuses = new List<DeliveryNoticeLocusEntry>();

                if (reply.CompletedTrackDetails != null && reply.CompletedTrackDetails.Length > 0)
                {
                    foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
                    {
                        if (completedTrackDetail != null)
                        {
                            if (completedTrackDetail.TrackDetails != null && completedTrackDetail.TrackDetails.Length > 0)
                            {
                                foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                                {
                                    if (trackDetail != null)
                                    {
                                        locus = new DeliveryNoticeLocusEntry();

                                        locus.F_HS_Signtime = trackDetail.StatusDetail.CreationTime;
                                        locus.F_HS_AreaCode = trackDetail.StatusDetail.Location.CountryCode + trackDetail.StatusDetail.Location.StateOrProvinceCode;
                                        locus.F_HS_TrackInfo = trackDetail.StatusDetail.Description;
                                        locus.F_HS_AreaName = trackDetail.StatusDetail.Location.CountryName + "," + trackDetail.StatusDetail.Location.City  ;
                                        locus.F_HS_TarckStatus = TrackingStatusCodes.GetTrackingStatusDescription(trackDetail.StatusDetail.Code);

                                        locuses.Add(locus);
                                    }
                                }
                            }
                        }
                    }
                }

            }
            return locuses;
        }
        private static void LogXML(string operationType, Object obj, Type type)
        {
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(type);
            TextWriter writer = new StreamWriter("E:\\trace.log", true);
            writer.WriteLine("========================================" + operationType + DateTime.Now.ToString() + "=========================================");
            serializer.Serialize(writer, obj);
            writer.WriteLine();
            writer.WriteLine("_________________________________________________________________________________________________________________________________");
            writer.WriteLine();
            writer.Close();
        }

        private static List<DeliveryNotice> GetTrackingNumbers(Context ctx)
        {     
            List<DeliveryNotice> noitces = null;
            DeliveryNotice notice = null;
            string sql = string.Format(@"/*dialect*/ select FBILLNO,F_HS_CARRYBILLNO from {0} where F_HS_DOCUMENTSTATUS = '{1}'", HSTableConst.HS_T_LogisticsInfo, LogisticsQuery.Query);
            DynamicObjectCollection coll = SQLUtils.GetObjects(ctx, sql);

            var group = from l in coll
                        group l by l["FBILLNO"]
                        into g
                        select g;

            if (group != null && group.Count() > 0)
            {
                noitces = new List<DeliveryNotice>();

                foreach (var g in group)
                {
                    notice = new DeliveryNotice();

                    if (g != null)
                    {
                        notice.FBillNo = JsonUtils.ConvertObjectToString(g.ElementAt(0)["FBILLNO"]);

                        foreach (var item in g)
                        {
                            DeliveryNoticeTraceEntry trace = new DeliveryNoticeTraceEntry();
                            trace.F_HS_CarryBillNO = JsonUtils.ConvertObjectToString(item["F_HS_CARRYBILLNO"]);
                            notice.TraceEntry.Add(trace);
                            noitces.Add(notice);
                        }
                    }
                }
            }

            return noitces;
        }


    }
}