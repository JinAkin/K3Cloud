// This code was built using Visual Studio 2017
using System;
using System.Web.Services.Protocols;
using RateWebServiceClient.RateServiceWebReference;

// Sample code to call the FedEx Rate Web Service
// Tested with Microsoft Visual Studio 2017 Professional Edition

namespace RateWebServiceClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RateRequest request = CreateRateRequest();
            //
            RateService service = new RateService();
			if (usePropertyFile())
            {
                service.Url = getProperty("endpoint");
            }
            try
            {
                // Call the web service passing in a RateRequest and returning a RateReply
                RateReply reply = service.getRates(request);

                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    ShowRateReply(reply);
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

        private static RateRequest CreateRateRequest()
        {
            // Build a RateRequest
            RateRequest request = new RateRequest();
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
            request.TransactionDetail.CustomerTransactionId = "***Rate Request using VC#***"; // This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId();
            //
            request.ReturnTransitAndCommit = true;
            request.ReturnTransitAndCommitSpecified = true;
            //
            SetShipmentDetails(request);
            //
            return request;
        }

        private static void SetShipmentDetails(RateRequest request)
        {
            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipTimestamp = DateTime.Now; // Shipping date and time
            request.RequestedShipment.ShipTimestampSpecified = true;
            request.RequestedShipment.DropoffType = DropoffType.REGULAR_PICKUP; //Drop off types are BUSINESS_SERVICE_CENTER, DROP_BOX, REGULAR_PICKUP, REQUEST_COURIER, STATION
            request.RequestedShipment.ServiceType = ServiceType.INTERNATIONAL_PRIORITY; // Service types are STANDARD_OVERNIGHT, PRIORITY_OVERNIGHT, FEDEX_GROUND ...
            request.RequestedShipment.ServiceTypeSpecified = true;
            request.RequestedShipment.PackagingType = PackagingType.YOUR_PACKAGING; // Packaging type FEDEX_BOK, FEDEX_PAK, FEDEX_TUBE, YOUR_PACKAGING, ...
            request.RequestedShipment.PackagingTypeSpecified = true;
            //
            SetOrigin(request);
            //
            SetDestination(request);
            //
            SetPackageLineItems(request);
            //
            request.RequestedShipment.TotalInsuredValue = new Money();
            request.RequestedShipment.TotalInsuredValue.Amount = 100;
            request.RequestedShipment.TotalInsuredValue.Currency = "USD";
            //
            request.RequestedShipment.PackageCount = "2";
        }

        private static void SetOrigin(RateRequest request)
        {
            request.RequestedShipment.Shipper = new Party();
            request.RequestedShipment.Shipper.Address = new Address();
            request.RequestedShipment.Shipper.Address.StreetLines = new string[1] { "SHIPPER ADDRESS LINE 1" };
            request.RequestedShipment.Shipper.Address.City = "COLLIERVILLE";
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = "TN";
            request.RequestedShipment.Shipper.Address.PostalCode = "38017";
            request.RequestedShipment.Shipper.Address.CountryCode = "US";
        }

        private static void SetDestination(RateRequest request)
        {
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Address = new Address();
            request.RequestedShipment.Recipient.Address.StreetLines = new string[1] { "RECIPIENT ADDRESS LINE 1" };
            request.RequestedShipment.Recipient.Address.City = "Montreal";
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = "PQ";
            request.RequestedShipment.Recipient.Address.PostalCode = "H1E1A1";
            request.RequestedShipment.Recipient.Address.CountryCode = "CA";
        }

        private static void SetPackageLineItems(RateRequest request)
        {
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[2];
            request.RequestedShipment.RequestedPackageLineItems[0] = new RequestedPackageLineItem();
            request.RequestedShipment.RequestedPackageLineItems[0].SequenceNumber = "1"; // package sequence number
            request.RequestedShipment.RequestedPackageLineItems[0].GroupPackageCount = "1";
            // package weight
            request.RequestedShipment.RequestedPackageLineItems[0].Weight = new Weight();
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Units = WeightUnits.LB;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.UnitsSpecified = true;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Value = 15.0M;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.ValueSpecified = true;
            // package dimensions
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions = new Dimensions();
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Length = "10";
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Width = "13";
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Height = "4";
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Units = LinearUnits.IN;
            request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.UnitsSpecified = true;
            // insured value
            request.RequestedShipment.RequestedPackageLineItems[0].InsuredValue = new Money();
            request.RequestedShipment.RequestedPackageLineItems[0].InsuredValue.Amount = 100;
            request.RequestedShipment.RequestedPackageLineItems[0].InsuredValue.Currency = "USD";
            //
            request.RequestedShipment.RequestedPackageLineItems[1] = new RequestedPackageLineItem();
            request.RequestedShipment.RequestedPackageLineItems[1].SequenceNumber = "2"; // package sequence number
            request.RequestedShipment.RequestedPackageLineItems[1].GroupPackageCount = "1";
            // package weight
            request.RequestedShipment.RequestedPackageLineItems[1].Weight = new Weight();
            request.RequestedShipment.RequestedPackageLineItems[1].Weight.Units = WeightUnits.LB;
            request.RequestedShipment.RequestedPackageLineItems[1].Weight.UnitsSpecified = true;
            request.RequestedShipment.RequestedPackageLineItems[1].Weight.Value = 25.0M;
            request.RequestedShipment.RequestedPackageLineItems[1].Weight.ValueSpecified = true;
            // package dimensions
            request.RequestedShipment.RequestedPackageLineItems[1].Dimensions = new Dimensions();
            request.RequestedShipment.RequestedPackageLineItems[1].Dimensions.Length = "20";
            request.RequestedShipment.RequestedPackageLineItems[1].Dimensions.Width = "13";
            request.RequestedShipment.RequestedPackageLineItems[1].Dimensions.Height = "4";
            request.RequestedShipment.RequestedPackageLineItems[1].Dimensions.Units = LinearUnits.IN;
            request.RequestedShipment.RequestedPackageLineItems[1].Dimensions.UnitsSpecified = true;
            // insured value
            request.RequestedShipment.RequestedPackageLineItems[1].InsuredValue = new Money();
            request.RequestedShipment.RequestedPackageLineItems[1].InsuredValue.Amount = 500;
            request.RequestedShipment.RequestedPackageLineItems[1].InsuredValue.Currency = "USD";
        }

        private static void ShowRateReply(RateReply reply)
        {
            Console.WriteLine("RateReply details:");
            foreach (RateReplyDetail rateReplyDetail in reply.RateReplyDetails)
            {
                if (rateReplyDetail.ServiceTypeSpecified)
                    Console.WriteLine("Service Type: {0}", rateReplyDetail.ServiceType);
                if (rateReplyDetail.PackagingTypeSpecified)
                    Console.WriteLine("Packaging Type: {0}", rateReplyDetail.PackagingType);
                Console.WriteLine();
                foreach (RatedShipmentDetail shipmentDetail in rateReplyDetail.RatedShipmentDetails)
                {
                    ShowShipmentRateDetails(shipmentDetail);
                    Console.WriteLine();
                }
                ShowDeliveryDetails(rateReplyDetail);
                Console.WriteLine("**********************************************************");
            }
        }

        private static void ShowShipmentRateDetails(RatedShipmentDetail shipmentDetail)
        {
            if (shipmentDetail == null) return;
            if (shipmentDetail.ShipmentRateDetail == null) return;
            ShipmentRateDetail rateDetail = shipmentDetail.ShipmentRateDetail;
            Console.WriteLine("--- Shipment Rate Detail ---");
            //
            Console.WriteLine("RateType: {0} ", rateDetail.RateType);
            if (rateDetail.TotalBillingWeight != null) Console.WriteLine("Total Billing Weight: {0} {1}", rateDetail.TotalBillingWeight.Value, shipmentDetail.ShipmentRateDetail.TotalBillingWeight.Units);
            if (rateDetail.TotalBaseCharge != null) Console.WriteLine("Total Base Charge: {0} {1}", rateDetail.TotalBaseCharge.Amount, rateDetail.TotalBaseCharge.Currency);
            if (rateDetail.TotalFreightDiscounts != null) Console.WriteLine("Total Freight Discounts: {0} {1}", rateDetail.TotalFreightDiscounts.Amount, rateDetail.TotalFreightDiscounts.Currency);
            if (rateDetail.TotalSurcharges != null) Console.WriteLine("Total Surcharges: {0} {1}", rateDetail.TotalSurcharges.Amount, rateDetail.TotalSurcharges.Currency);
            if (rateDetail.Surcharges != null)
            {
                // Individual surcharge for each package
                foreach (Surcharge surcharge in rateDetail.Surcharges)
                    Console.WriteLine(" {0} surcharge {1} {2}", surcharge.SurchargeType, surcharge.Amount.Amount, surcharge.Amount.Currency);
            }
            if (rateDetail.TotalNetCharge != null) Console.WriteLine("Total Net Charge: {0} {1}", rateDetail.TotalNetCharge.Amount, rateDetail.TotalNetCharge.Currency);
        }

        private static void ShowDeliveryDetails(RateReplyDetail rateDetail)
        {
            if (rateDetail.DeliveryTimestampSpecified)
                Console.WriteLine("Delivery timestamp: " + rateDetail.DeliveryTimestamp.ToString());
            if (rateDetail.TransitTimeSpecified)
                Console.WriteLine("Transit time: " + rateDetail.TransitTime);
        }

        private static void ShowNotifications(RateReply reply)
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