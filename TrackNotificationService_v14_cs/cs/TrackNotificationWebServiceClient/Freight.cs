// This code was built using Visual Studio 2005
using System;
using System.Web.Services.Protocols;
using TrackNotificationWebServiceClient.TrackNotificationWebReference;

// Sample code to call the FedEx Track Notification Web Service
// Tested with Microsoft Visual Studio 2017 Professional Edition
namespace TrackNotificationWebServiceClient
{
    class Program
    {
        static void Main(string[] args)
        {
            SendNotificationsRequest request = CreateSendNotificationsRequest();
            //
            TrackService service = new TrackService();
			if (usePropertyFile())
            {
                service.Url = getProperty("endpoint");
            }
            try
            {
                // Call the Track web service passing in a TrackNotificationRequest and returning a TrackNotificationReply
                SendNotificationsReply reply = service.sendNotifications(request);
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    ShowSendNotificationsReply(reply);
                }
                ShowNotifications(reply);
            }
            catch (SoapException e)
            {
                Console.WriteLine(e.Detail.InnerText);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Press any key to quit!");
            Console.ReadKey();
        }

        private static SendNotificationsRequest CreateSendNotificationsRequest()
        {
            // Build the TrackNotificationRequest
            SendNotificationsRequest request = new SendNotificationsRequest();
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
            request.TransactionDetail.CustomerTransactionId = "***TrackNotification Request using VC#***"; //This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId();
            //
            request.TrackingNumber = "XXX"; // Replace "XXX" with the tracking number
            if (usePropertyFile()) //Set values from a file for testing purposes
            {
                request.TrackingNumber = getProperty("trackingnumber");
            }
            //
            // Date range is optional.
            // If omitted, set to false
            request.ShipDateRangeBegin = DateTime.Parse("3/25/2012"); //MM/DD/YYYY
            request.ShipDateRangeEnd = DateTime.Parse("6/19/2012"); //MM/DD/YYYY       
            request.ShipDateRangeBeginSpecified = false;
            request.ShipDateRangeEndSpecified = false;
            //
            request.SenderEMailAddress = "test@test.com";
            request.SenderContactName = "Sender";
            request.EventNotificationDetail = new ShipmentEventNotificationDetail();
            request.EventNotificationDetail.AggregationType = ShipmentNotificationAggregationType.PER_SHIPMENT;
            request.EventNotificationDetail.AggregationTypeSpecified = true;
            request.EventNotificationDetail.PersonalMessage = "Personal Message";
            request.EventNotificationDetail.EventNotifications = new ShipmentEventNotificationSpecification[1]{
                new ShipmentEventNotificationSpecification()};
            request.EventNotificationDetail.EventNotifications[0].Role = ShipmentNotificationRoleType.THIRD_PARTY;
            request.EventNotificationDetail.EventNotifications[0].RoleSpecified = true;
            request.EventNotificationDetail.EventNotifications[0].FormatSpecification = new ShipmentNotificationFormatSpecification();
            request.EventNotificationDetail.EventNotifications[0].FormatSpecification.Type = NotificationFormatType.HTML;
            request.EventNotificationDetail.EventNotifications[0].FormatSpecification.TypeSpecified = true;
            request.EventNotificationDetail.EventNotifications[0].NotificationDetail = new NotificationDetail();
            request.EventNotificationDetail.EventNotifications[0].NotificationDetail.EmailDetail = new EMailDetail();
            request.EventNotificationDetail.EventNotifications[0].NotificationDetail.EmailDetail.EmailAddress = "recipient@acme.com";
            request.EventNotificationDetail.EventNotifications[0].NotificationDetail.EmailDetail.Name = "recipient name";
            request.EventNotificationDetail.EventNotifications[0].NotificationDetail.NotificationType = NotificationType.EMAIL;
            request.EventNotificationDetail.EventNotifications[0].NotificationDetail.NotificationTypeSpecified = true;
            NotificationEventType[] EventTypes = new NotificationEventType[5];
                EventTypes[0] = NotificationEventType.ON_DELIVERY;
                EventTypes[1] = NotificationEventType.ON_ESTIMATED_DELIVERY;
                EventTypes[2] = NotificationEventType.ON_EXCEPTION;
                EventTypes[3] = NotificationEventType.ON_SHIPMENT;
                EventTypes[4] = NotificationEventType.ON_TENDER;
            request.EventNotificationDetail.EventNotifications[0].Events = EventTypes;
            //
            return request;
        }

        private static void ShowSendNotificationsReply(SendNotificationsReply reply)
        {
            Console.WriteLine("SendNotificationsReply details:");
            Console.WriteLine();
            foreach (TrackNotificationPackage package in reply.Packages)
            {
                Console.WriteLine("Tracking Number: {0} ", package.TrackingNumber);
                Console.WriteLine("Carrier Code: {0} ", package.CarrierCode);
                if (null != package.ShipDate) { Console.WriteLine("ShipDate: {0}", package.ShipDate.ToShortDateString()); }
                Console.WriteLine("Destination Address: {0}, {1}", package.Destination.City, package.Destination.StateOrProvinceCode);
                //
                if (package.RecipientDetails != null)
                {
                    foreach (NotificationEventType notificationEventType in package.RecipientDetails)
                    {
                        Console.WriteLine("Recipient Email Notification Event type: {0}", notificationEventType);
                    }
                }
                Console.WriteLine("************************************************");
            }
        }

        private static void ShowNotifications(SendNotificationsReply reply)
        {
            Console.WriteLine("Notifications");
            for (int i = 0; i < reply.Notifications.Length; i++)
            {
                Notification notification = reply.Notifications[i];
                Console.WriteLine("Notification no. {0}", i);
                Console.WriteLine(" Severity: {0}", notification.Severity);
                Console.WriteLine(" Code: {0}", notification.Code);
                Console.WriteLine(" Message: {0}", notification.Message);
                Console.WriteLine(" Source: {0}", notification.Source);
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
                String filename = "C:\\filepath\\filename.txt";
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
    }
}