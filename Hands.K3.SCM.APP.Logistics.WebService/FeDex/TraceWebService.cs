// This code was built using Visual Studio 2005
using System;
using TrackWebServiceClient.TrackServiceWebReference;
using System.Web.Services.Protocols;
using Hands.K3.SCM.App.Core.SynchroService.SynDataObject.DeliveryNotice;
using System.Collections.Generic;
using System.IO;
using Hands.K3.SCM.App.Core.SynchroService;
using Hands.K3.SCM.App.Core.SynchroService.SynDataObject.CommonObject;
using Kingdee.BOS;

// Sample code to call the FedEx Track Web Service
// Tested with Microsoft Visual Studio 2017 Professional Edition
namespace TrackWebServiceClient
{
    public class TraceWebService
    {
        public static List<DeliveryNotice> GetLogisticsTraceDetail(Context ctx,List<DeliveryNotice> notices)
        {
            if (notices != null && notices.Count > 0)
            {
               
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
                                    TrackRequest request = CreateTrackRequest(trace.FCarryBillNo);
                                    LogXML("trackingNumber:["+ trace.FCarryBillNo + "]  Request",request,typeof(TrackRequest));
                                    //
                                    TrackService service = new TrackService();
                                    if (usePropertyFile())
                                    {
                                        service.Url = getProperty("endpoint");
                                    }
                                    //
                                    try
                                    {
                                        // Call the Track web service passing in a TrackRequest and returning a TrackReply
                                        TrackReply reply = service.track(request);

                                        LogXML("trackingNumber:[" + trace.FCarryBillNo + "]  Reply", reply, typeof(TrackReply)); 

                                        if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                                        {
                                            ShowTrackReply(reply);
                                            notice.LocusEntry = GetLocusDetail(reply);
                                        }
                                        ShowNotifications(reply);
                                    }
                                    catch (SoapException ex)
                                    {
                                        SynchroDataHelper.WriteSynchroLog(ctx, SynchroDataType.DeliveryNoticeBill, ex.Message + System.Environment.NewLine + ex.StackTrace);
                                    }
                                    catch (Exception ex)
                                    {
                                        SynchroDataHelper.WriteSynchroLog(ctx, SynchroDataType.DeliveryNoticeBill, ex.Message + System.Environment.NewLine + ex.StackTrace);
                                    }
                                    
                                }
                            }
                        }
                    }
                    
                }
            }
            return notices;
        }

        private static TrackRequest CreateTrackRequest(string trackingNumber)
        {
            // Build the TrackRequest
            TrackRequest request = new TrackRequest();
            //
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = "XXX"; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.UserCredential.Password = "XXX"; // Replace "XXX" with the Password
            request.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.ParentCredential.Key = "XXX"; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.ParentCredential.Password = "XXX"; // Replace "XXX"
            if (usePropertyFile()) //Set values from a file for testing purposes
            {
                request.WebAuthenticationDetail.UserCredential.Key = getProperty("key");
                request.WebAuthenticationDetail.UserCredential.Password = getProperty("password");
                request.WebAuthenticationDetail.ParentCredential.Key = getProperty("parentkey");
                request.WebAuthenticationDetail.ParentCredential.Password = getProperty("parentpassword");
            }
            //
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = "XXX"; // Replace "XXX" with the client's account number
            request.ClientDetail.MeterNumber = "XXX"; // Replace "XXX" with the client's meter number
            if (usePropertyFile()) //Set values from a file for testing purposes
            {
                request.ClientDetail.AccountNumber = getProperty("accountnumber");
                request.ClientDetail.MeterNumber = getProperty("meternumber");
            }
            //
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = "***Track Request using VC#***";  //This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId();
            //
            // Tracking information
            request.SelectionDetails = new TrackSelectionDetail[1] { new TrackSelectionDetail() };
            request.SelectionDetails[0].PackageIdentifier = new TrackPackageIdentifier();
            request.SelectionDetails[0].PackageIdentifier.Value = "449044304137821"; // Replace "XXX" with tracking number or door tag

            //if (!string.IsNullOrWhiteSpace(trackingNumber))
            //{
            //    request.SelectionDetails[0].PackageIdentifier.Value = trackingNumber;
            //}
            

            request.SelectionDetails[0].PackageIdentifier.Type = TrackIdentifierType.TRACKING_NUMBER_OR_DOORTAG;
            //
            // Date range is optional.
            // If omitted, set to false
            request.SelectionDetails[0].ShipDateRangeBegin = DateTime.Parse("2016-08-01"); //MM/DD/YYYY
            request.SelectionDetails[0].ShipDateRangeEnd = request.SelectionDetails[0].ShipDateRangeBegin.AddDays(0);
            request.SelectionDetails[0].ShipDateRangeBeginSpecified = false;
            request.SelectionDetails[0].ShipDateRangeEndSpecified = false;
            //
            // Include detailed scans is optional.
            // If omitted, set to false
            request.ProcessingOptions = new TrackRequestProcessingOptionType[1];
            request.ProcessingOptions[0] = TrackRequestProcessingOptionType.INCLUDE_DETAILED_SCANS;
            return request;
        }

        private static void ShowTrackReply(TrackReply reply)
        {
            // Track details for each package
            foreach (CompletedTrackDetail completedTrackDetail in reply.CompletedTrackDetails)
            {
                foreach (TrackDetail trackDetail in completedTrackDetail.TrackDetails)
                {
                    Console.WriteLine("Tracking details:");
                    Console.WriteLine("**************************************");
                    ShowNotification(trackDetail.Notification);
                    Console.WriteLine("Tracking number: {0}", trackDetail.TrackingNumber);
                    Console.WriteLine("Tracking number unique identifier: {0}", trackDetail.TrackingNumberUniqueIdentifier);
                    Console.WriteLine("Track Status: {0} ({1})", trackDetail.StatusDetail.Description, trackDetail.StatusDetail.Code);
                    Console.WriteLine("Carrier code: {0}", trackDetail.CarrierCode);

                    if (trackDetail.OtherIdentifiers != null)
                    {
                        foreach (TrackOtherIdentifierDetail identifier in trackDetail.OtherIdentifiers)
                        {
                            Console.WriteLine("Other Identifier: {0} {1}", identifier.PackageIdentifier.Type, identifier.PackageIdentifier.Value);
                        }
                    }
                    if (trackDetail.Service != null)
                    {
                        Console.WriteLine("ServiceInfo: {0}", trackDetail.Service.Description);
                    }
                    if (trackDetail.PackageWeight != null)
                    {
                        Console.WriteLine("Package weight: {0} {1}", trackDetail.PackageWeight.Value, trackDetail.PackageWeight.Units);
                    }
                    if (trackDetail.ShipmentWeight != null)
                    {
                        Console.WriteLine("Shipment weight: {0} {1}", trackDetail.ShipmentWeight.Value, trackDetail.ShipmentWeight.Units);
                    }
                    if (trackDetail.Packaging != null)
                    {
                        Console.WriteLine("Packaging: {0}", trackDetail.Packaging);
                    }
                    Console.WriteLine("Package Sequence Number: {0}", trackDetail.PackageSequenceNumber);
                    Console.WriteLine("Package Count: {0} ", trackDetail.PackageCount);
                    if (trackDetail.DatesOrTimes != null)
                    {
                        foreach (TrackingDateOrTimestamp timestamp in trackDetail.DatesOrTimes)
                        {
                            Console.WriteLine("{0}: {1}", timestamp.Type, timestamp.DateOrTimestamp);
                        }
                    }
                    if (trackDetail.DestinationAddress != null)
                    {
                        Console.WriteLine("Destination: {0}, {1}", trackDetail.DestinationAddress.City, trackDetail.DestinationAddress.StateOrProvinceCode);
                    }
                    if (trackDetail.AvailableImages != null)
                    {
                        foreach (AvailableImagesDetail ImageDetail in trackDetail.AvailableImages)
                        {
                            Console.WriteLine("Image availability: {0}", ImageDetail.Type);
                        }
                    }
                    if (trackDetail.NotificationEventsAvailable != null)
                    {
                        foreach (NotificationEventType notificationEventType in trackDetail.NotificationEventsAvailable)
                        {
                            Console.WriteLine("NotificationEvent type : {0}", notificationEventType);
                        }
                    }

                    //Events
                    Console.WriteLine();
                    if (trackDetail.Events != null)
                    {
                        Console.WriteLine("Track Events:");
                        foreach (TrackEvent trackevent in trackDetail.Events)
                        {
                            if (trackevent.TimestampSpecified)
                            {
                                Console.WriteLine("Timestamp: {0}", trackevent.Timestamp);
                            }
                            Console.WriteLine("Event: {0} ({1})", trackevent.EventDescription, trackevent.EventType);
                            Console.WriteLine("***");
                        }
                        Console.WriteLine();
                    }
                    Console.WriteLine("**************************************");
                }
            }

        }
        private static void ShowNotification(Notification notification)
        {
            Console.WriteLine(" Severity: {0}", notification.Severity);
            Console.WriteLine(" Code: {0}", notification.Code);
            Console.WriteLine(" Message: {0}", notification.Message);
            Console.WriteLine(" Source: {0}", notification.Source);
        }
        private static void ShowNotifications(TrackReply reply)
        {
            Console.WriteLine("Notifications");
            for (int i = 0; i < reply.Notifications.Length; i++)
            {
                Notification notification = reply.Notifications[i];
                Console.WriteLine("Notification no. {0}", i);
                ShowNotification(notification);
            }
        }
        private static bool usePropertyFile() //Set to true for common properties to be set with getProperty function.
        {
            return getProperty("usefile").Equals("True");
        }
        private static String getProperty(String propertyname) //Sets common properties for testing purposes.
        {
            try
            {
                String filename = "C:\\FedEx\\Information.txt";
                if (System.IO.File.Exists(filename))
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(filename);
                    do
                    {
                        String[] parts = sr.ReadLine().Split(',');
                        if (parts[0].Equals(propertyname) && parts.Length == 2)
                        {
                            return parts[1];
                        }
                    }
                    while (!sr.EndOfStream);
                }
                Console.WriteLine("Property {0} set to default 'XXX'", propertyname);
                return "XXX";
            }
            catch (Exception e)
            {
                Console.WriteLine("Property {0} set to default 'XXX'", propertyname);
                return "XXX";
            }
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

                                        locus.F_HS_AreaCode = trackDetail.ClearanceLocationCode;
                                        locus.F_HS_TarckStatus = trackDetail.StatusDetail.Description;
                                        locus.F_HS_AreaName = trackDetail.StatusDetail.Location.GeographicCoordinates;

                                        if (trackDetail.InformationNotes != null && trackDetail.InformationNotes.Length > 0)
                                        {
                                            foreach (var info in trackDetail.InformationNotes)
                                            {
                                                string note = info.Description;
                                            }
                                        }
                                        
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
    }
}