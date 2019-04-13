// This code was built using Visual Studio 2017
using System;
using System.IO;
using System.Web.Services.Protocols;
using ShipWebServiceClient.ShipServiceWebReference;
using Hands.K3.SCM.App.Core.SynchroService.SynDataObject.DeliveryNotice;
using Hands.K3.SCM.App.Core.SynchroService.SynDataObject.SaleOrder;
using System.Collections.Generic;
using Hands.K3.SCM.App.Core.SynchroService.SynDataObject.CommonObject;
using Hands.K3.SCM.App.Core.Utils;
using Hands.K3.SCM.App.Core.K3WebApi;
using Kingdee.BOS;
using Hands.K3.SCM.App.Core.SynchroService;
using System.Linq;

// Sample code to call the FedEx Ship Service - Express International Shipment
// Tested with Microsoft Visual Studio 2017 Professional Edition

namespace ShipWebServiceClient
{
    public class ShipWebService
    {
        /// <summary>
        /// 获取物流细节
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="notices"></param>
        /// <returns></returns>
        public static Dictionary<string, T> GetLogisticsDetail<T>(Context ctx, List<DeliveryNotice> notices)
        {
            string messages = "";
            Dictionary<string, T> dict = new Dictionary<string, T>();

            // Set this to true to process a COD shipment and print a COD return Label
            
            if (notices != null && notices.Count > 0)
            {
                foreach (var notice in notices)
                {
                    ProcessShipmentRequest request = CreateShipmentRequest(notice);

                    LogXML("DeliveryNoitceNo:" + notice.FBillNo + " Request ", request, typeof(ProcessShipmentRequest));

                    ShipService service = new ShipService();
                    if (usePropertyFile())
                    {
                        service.Url = getProperty("endpoint");
                    }
                    //
                    try
                    {
                        // Call the ship web service passing in a ProcessShipmentRequest and returning a ProcessShipmentReply
                        ProcessShipmentReply reply = service.processShipment(request);

                        LogXML("DeliveryNoitceNo:" + notice.FBillNo + " Reply ", reply, typeof(ProcessShipmentReply));
                        messages += GetNotifications(reply);

                        if ((reply.HighestSeverity != NotificationSeverityType.ERROR) && (reply.HighestSeverity != NotificationSeverityType.FAILURE))
                        {
                            notice.TraceEntry = GetShipmentReply(reply);
                            //LogXML(reply, typeof(ProcessShipmentReply));
                        }
                        RecordNotifications(ctx, reply);

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

            HttpResponseResult result = new HttpResponseResult();
            result.Message = messages;

            dict.Add("result", (T)(Object)result);
            dict.Add("notices", (T)(Object)notices);

            return dict;
        }

        private static WebAuthenticationDetail SetWebAuthenticationDetail()
        {
            WebAuthenticationDetail wad = new WebAuthenticationDetail();
            wad.UserCredential = new WebAuthenticationCredential();
            wad.ParentCredential = new WebAuthenticationCredential();
            wad.UserCredential.Key = "XXX"; // Replace "XXX" with the Key
            wad.UserCredential.Password = "XXX"; // Replace "XXX" with the Password
            wad.ParentCredential = new WebAuthenticationCredential();
            wad.ParentCredential.Key = "XXX"; // Replace "XXX" with the Key
            wad.ParentCredential.Password = "XXX"; // Replace "XXX"
            if (usePropertyFile()) //Set values from a file for testing purposes
            {
                wad.UserCredential.Key = getProperty("key");
                wad.UserCredential.Password = getProperty("password");
                wad.ParentCredential.Key = getProperty("parentkey");
                wad.ParentCredential.Password = getProperty("parentpassword");
            }
            return wad;
        }

        private static ProcessShipmentRequest CreateShipmentRequest(DeliveryNotice notice)
        {
            // Build the ShipmentRequest
            ProcessShipmentRequest request = new ProcessShipmentRequest();
            //
            request.WebAuthenticationDetail = SetWebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = "XXX"; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.UserCredential.Password = "XXX"; // Replace "XXX" with the Password
            if (usePropertyFile()) //Set values from a file for testing purposes
            {
                request.WebAuthenticationDetail.UserCredential.Key = getProperty("key");
                request.WebAuthenticationDetail.UserCredential.Password = getProperty("password");
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
            request.TransactionDetail.CustomerTransactionId = "***Express International Shipment Request using VC#***"; // The client will get the same value back in the response
            //
            request.Version = new VersionId();
            //
            SetShipmentDetails(request);
            //
            SetSender(request);
            //
            SetRecipient(request,notice);
            //
            SetPayment(request);
            //
            SetLabelDetails(request);
            //
            SetPackageLineItems(request,notice);
            //
            SetCustomsClearanceDetails(request,notice);
            //
            return request;
        }
       
        private static void SetShipmentDetails(ProcessShipmentRequest request)
        {
            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipTimestamp = DateTime.Now; // Ship date and time
            //
            request.RequestedShipment.DropoffType = DropoffType.REGULAR_PICKUP;
            request.RequestedShipment.ServiceType = ServiceType.INTERNATIONAL_PRIORITY;
            request.RequestedShipment.PackagingType = PackagingType.YOUR_PACKAGING; // Packaging type YOUR_PACKAGING, ...
            //
            request.RequestedShipment.PackageCount = "1";
        }

        private static void SetSender(ProcessShipmentRequest request)
        {
            request.RequestedShipment.Shipper = new Party();
            request.RequestedShipment.Shipper.Contact = new Contact();
            request.RequestedShipment.Shipper.Contact.PersonName = HSDeliveryNoticeConst.SenderName;
            request.RequestedShipment.Shipper.Contact.CompanyName = HSDeliveryNoticeConst.SenderCompanyName;
            request.RequestedShipment.Shipper.Contact.PhoneNumber = HSDeliveryNoticeConst.SenderPhoneNumber;
            request.RequestedShipment.Shipper.Address = new Address();
            request.RequestedShipment.Shipper.Address.StreetLines = new string[2] { HSDeliveryNoticeConst.SenderAddress_1, HSDeliveryNoticeConst.SenderAddrss_2 };
            request.RequestedShipment.Shipper.Address.City = HSDeliveryNoticeConst.SenderCity;
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = StateOrProvinceCode.GetStateOrProvinceCode(HSDeliveryNoticeConst.SenderStateOrProvinceCode);
            request.RequestedShipment.Shipper.Address.PostalCode = HSDeliveryNoticeConst.SenderPostalCode;
            request.RequestedShipment.Shipper.Address.CountryCode = HSDeliveryNoticeConst.SenderCountryCode;
        }

        private static void SetRecipient(ProcessShipmentRequest request, DeliveryNotice notice)
        {
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Contact = new Contact();
            request.RequestedShipment.Recipient.Contact.PersonName = notice.F_HS_DeliveryName;
            request.RequestedShipment.Recipient.Contact.CompanyName = notice.FCustCompany;
            request.RequestedShipment.Recipient.Contact.PhoneNumber = notice.F_HS_MobilePhone;
            request.RequestedShipment.Recipient.Address = new Address();
            request.RequestedShipment.Recipient.Address.StreetLines = GetDeliveryAddress(notice.F_HS_DeliveryAddress);
            request.RequestedShipment.Recipient.Address.City = notice.F_HS_DeliveryCity;
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = StateOrProvinceCode.GetStateOrProvinceCode(notice.F_HS_DeliveryProvinces);
            request.RequestedShipment.Recipient.Address.PostalCode = notice.F_HS_PostCode;
            request.RequestedShipment.Recipient.Address.CountryCode = notice.F_HS_RecipientCountry;
            request.RequestedShipment.Recipient.Address.Residential = true;
        }

        private static void SetPayment(ProcessShipmentRequest request)
        {
            request.RequestedShipment.ShippingChargesPayment = new Payment();
            request.RequestedShipment.ShippingChargesPayment.PaymentType = PaymentType.SENDER;
            request.RequestedShipment.ShippingChargesPayment.Payor = new Payor();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty = new Party();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.AccountNumber = "XXX"; // Replace "XXX" with client's account number
            if (usePropertyFile()) //Set values from a file for testing purposes
            {
                request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.AccountNumber = getProperty("payoraccount");
            }
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.Contact = new Contact();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.Address = new Address();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.Address.CountryCode = "US";
        }

        private static void SetLabelDetails(ProcessShipmentRequest request)
        {
            request.RequestedShipment.LabelSpecification = new LabelSpecification();
            request.RequestedShipment.LabelSpecification.ImageType = ShippingDocumentImageType.PDF; // Image types PDF, PNG, DPL, ...
            request.RequestedShipment.LabelSpecification.ImageTypeSpecified = true;
            request.RequestedShipment.LabelSpecification.LabelFormatType = LabelFormatType.COMMON2D;
        }

        private static void SetPackageLineItems(ProcessShipmentRequest request,DeliveryNotice notice)
        {
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[1];
            request.RequestedShipment.RequestedPackageLineItems[0] = new RequestedPackageLineItem();
            request.RequestedShipment.RequestedPackageLineItems[0].SequenceNumber = "1";
            // Package weight information
            request.RequestedShipment.RequestedPackageLineItems[0].Weight = new Weight();
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Value = 20.0M;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Units = WeightUnits.KG;
            // package dimensions
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions = new Dimensions();
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Length = "12";
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Width = "13";
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Height = "14";
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Units = LinearUnits.CM;
            // insured value
            //request.RequestedShipment.RequestedPackageLineItems[0].InsuredValue = new Money();
            //request.RequestedShipment.RequestedPackageLineItems[0].InsuredValue.Amount = 100;
            //request.RequestedShipment.RequestedPackageLineItems[0].InsuredValue.Currency = notice.orderFin.FSettleCurrID;
            // Reference details
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences = new CustomerReference[1] { new CustomerReference() };
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[0].CustomerReferenceType = CustomerReferenceType.CUSTOMER_REFERENCE;
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[0].Value = "testreference";

            //if (notice != null)
            //{
            //    if (notice.Packages != null && notice.Packages.Count > 0)
            //    {
            //        request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[notice.Packages.Count];

            //        for (int i = 0; i < notice.Packages.Count; i++)
            //        {
            //            if (notice.Packages[i] != null)
            //            {
            //                Package package = notice.Packages[i];

            //                request.RequestedShipment.RequestedPackageLineItems[i] = new RequestedPackageLineItem();
            //                //
            //                request.RequestedShipment.RequestedPackageLineItems[i].SequenceNumber = package.SequenceNumber;
            //                // Package weight information
            //                request.RequestedShipment.RequestedPackageLineItems[i].Weight = new Weight();
            //                request.RequestedShipment.RequestedPackageLineItems[i].Weight.Value = package.Weight;
            //                request.RequestedShipment.RequestedPackageLineItems[i].Weight.Units = WeightUnits.KG;
            //                //
            //                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions = new Dimensions();
            //                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Length = package.Dimension.Length;
            //                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Width = package.Dimension.Width;
            //                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Height =package.Dimension.Height;
            //                request.RequestedShipment.RequestedPackageLineItems[i].Dimensions.Units = LinearUnits.CM;
            //            }
            //        }
            //    }
            //}
        }

        private static void SetCustomsClearanceDetails(ProcessShipmentRequest request,DeliveryNotice notice)
        {
            request.RequestedShipment.CustomsClearanceDetail = new CustomsClearanceDetail();
            request.RequestedShipment.CustomsClearanceDetail.DutiesPayment = new Payment();
            request.RequestedShipment.CustomsClearanceDetail.DutiesPayment.PaymentType = PaymentType.SENDER;
            request.RequestedShipment.CustomsClearanceDetail.DutiesPayment.Payor = new Payor();
            request.RequestedShipment.CustomsClearanceDetail.DutiesPayment.Payor.ResponsibleParty = new Party();
            request.RequestedShipment.CustomsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.AccountNumber = "XXX"; // Replace "XXX" with the payor account number
            if (usePropertyFile()) //Set values from a file for testing purposes
            {
                request.RequestedShipment.CustomsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.AccountNumber = getProperty("dutiesaccount");
            }
            request.RequestedShipment.CustomsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.Contact = new Contact();
            request.RequestedShipment.CustomsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.Address = new Address();
            request.RequestedShipment.CustomsClearanceDetail.DutiesPayment.Payor.ResponsibleParty.Address.CountryCode = notice.F_HS_RecipientCountry;
            request.RequestedShipment.CustomsClearanceDetail.DocumentContent = InternationalDocumentContentType.NON_DOCUMENTS;
            //
            request.RequestedShipment.CustomsClearanceDetail.CustomsValue = new Money();
            request.RequestedShipment.CustomsClearanceDetail.CustomsValue.Amount = notice.orderFin.FBillAmount;
            request.RequestedShipment.CustomsClearanceDetail.CustomsValue.Currency = notice.orderFin.FSettleCurrID;
            //
            SetCommodityDetails(request,notice);
        }

        private static void SetCommodityDetails(ProcessShipmentRequest request, DeliveryNotice notice)
        {
            if (notice != null)
            {
                if (notice.OrderEntry != null && notice.OrderEntry.Count > 0)
                {
                    request.RequestedShipment.CustomsClearanceDetail.Commodities = new Commodity[notice.OrderEntry.Count];
                    for (int i = 0; i < notice.OrderEntry.Count; i++)
                    {
                        if (notice.OrderEntry[i] != null)
                        {
                            K3SalOrderEntryInfo entry = notice.OrderEntry[i];

                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i] = new Commodity();
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].Name = entry.FMaterialName;
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].Description = GetDescription(entry.F_HS_IsOil);
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].NumberOfPieces = "1";
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].CountryOfManufacture = "US";

                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].UnitPrice = new Money();
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].UnitPrice.Currency = notice.orderFin.FSettleCurrID;
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].UnitPrice.Amount = entry.FTAXPRICE;

                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].Weight = new Weight();
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].Weight.Units = WeightUnits.KG;
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].Weight.Value = 0.1M/*entry.F_HS_TotalWeight / 100*/;

                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].Quantity = entry.FQTY;
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].QuantitySpecified = true;
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].QuantityUnits = entry.FUnitId;

                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].CustomsValue = new Money();
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].CustomsValue.Amount = entry.FTAXPRICE;
                            request.RequestedShipment.CustomsClearanceDetail.Commodities[i].CustomsValue.Currency = notice.orderFin.FSettleCurrID;

                        }

                    }
                }
            }
        }

        private static void ShowShipmentReply(ProcessShipmentReply reply)
        {
            Console.WriteLine("Shipment Reply details:");
            // Details for each package
            foreach (CompletedPackageDetail packageDetail in reply.CompletedShipmentDetail.CompletedPackageDetails)
            {
                ShowTrackingDetails(packageDetail.TrackingIds);
                if (null != reply.CompletedShipmentDetail.ShipmentRating)
                {
                    ShowPackageRateDetails(reply.CompletedShipmentDetail.ShipmentRating);
                }
                else
                {
                    Console.WriteLine("Shipment Rates not returned/");
                }
                if (null != packageDetail.Label.Parts[0].Image)
                {
                    // Save outbound shipping label
                    string LabelPath = "c:\\";
                    if (usePropertyFile())
                    {
                        LabelPath = getProperty("labelpath");
                    }
                    string LabelFileName = LabelPath + packageDetail.TrackingIds[0].TrackingNumber + ".pdf";
                    SaveLabel(LabelFileName, packageDetail.Label.Parts[0].Image);
                }
                ShowBarcodeDetails(packageDetail.OperationalDetail.Barcodes);
            }
            ShowPackageRouteDetails(reply.CompletedShipmentDetail.OperationalDetail);
        }

        private static void ShowTrackingDetails(TrackingId[] TrackingIds)
        {
            // Tracking information for each package
            Console.WriteLine("Tracking details");
            if (TrackingIds != null)
            {
                for (int i = 0; i < TrackingIds.Length; i++)
                {
                    Console.WriteLine("Tracking # {0} Form ID {1}", TrackingIds[i].TrackingNumber, TrackingIds[i].FormId);
                }
            }
        }

        private static void ShowPackageRateDetails(ShipmentRating ShipmentRating)
        {
            Console.WriteLine("\nRate details");
            for (int i = 0; i < ShipmentRating.ShipmentRateDetails.Length; i++)
            {
                Console.WriteLine("RateType: " + ShipmentRating.ActualRateType);
                Console.WriteLine("Total Billing Weight: " + ShipmentRating.ShipmentRateDetails[i].TotalBillingWeight.Value);
                Console.WriteLine("Total Base Charge: " + ShipmentRating.ShipmentRateDetails[i].TotalBaseCharge.Amount);
                Console.WriteLine("Total Freight Discount: " + ShipmentRating.ShipmentRateDetails[i].TotalFreightDiscounts.Amount);
                Console.WriteLine("Total Surcharges: " + ShipmentRating.ShipmentRateDetails[i].TotalSurcharges.Amount);
                Console.WriteLine("Total Net Charge: " + ShipmentRating.ShipmentRateDetails[i].TotalNetCharge.Amount);
                Console.WriteLine("**********************************************************");
            }
        }

        private static void ShowBarcodeDetails(PackageBarcodes barcodes)
        {
            // Barcode information for each package
            Console.WriteLine("\nBarcode details");
            if (barcodes != null)
            {
                if (barcodes.StringBarcodes != null)
                {
                    for (int i = 0; i < barcodes.StringBarcodes.Length; i++)
                    {
                        Console.WriteLine("String barcode {0} Type {1}", barcodes.StringBarcodes[i].Value, barcodes.StringBarcodes[i].Type);
                    }
                }
                if (barcodes.BinaryBarcodes != null)
                {
                    for (int i = 0; i < barcodes.BinaryBarcodes.Length; i++)
                    {
                        Console.WriteLine("Binary barcode Type {0}", barcodes.BinaryBarcodes[i].Type);
                    }
                }
            }
        }

        private static void ShowPackageRouteDetails(ShipmentOperationalDetail routingDetail)
        {
            Console.WriteLine("\nRouting details");
            Console.WriteLine("URSA prefix {0} suffix {1}", routingDetail.UrsaPrefixCode, routingDetail.UrsaSuffixCode);
            Console.WriteLine("Service commitment {0} Airport ID {1}", routingDetail.DestinationLocationId, routingDetail.AirportId);

            if (routingDetail.DeliveryDaySpecified)
            {
                Console.WriteLine("Delivery day " + routingDetail.DeliveryDay);
            }
            if (routingDetail.DeliveryDateSpecified)
            {
                Console.WriteLine("Delivery date " + routingDetail.DeliveryDate.ToShortDateString());
            }
            if (routingDetail.TransitTimeSpecified)
            {
                Console.WriteLine("Transit time " + routingDetail.TransitTime);
            }
        }

        private static void SaveLabel(string labelFileName, byte[] labelBuffer)
        {
            // Save label buffer to file
            FileStream LabelFile = new FileStream(labelFileName, FileMode.Create);
            LabelFile.Write(labelBuffer, 0, labelBuffer.Length);
            LabelFile.Close();
            // Display label in Acrobat
            DisplayLabel(labelFileName);
        }

        private static void DisplayLabel(string labelFileName)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(labelFileName);
            info.UseShellExecute = true;
            info.Verb = "open";
            System.Diagnostics.Process.Start(info);
        }

        private static void RecordNotifications(Context ctx, ProcessShipmentReply reply)
        {
            if (reply != null)
            {
                SynchroDataHelper.WriteSynchroLog(ctx, SynchroDataType.DeliveryNoticeBill, GetNotifications(reply));
            }
            else
            {
                SynchroDataHelper.WriteSynchroLog(ctx, SynchroDataType.DeliveryNoticeBill, "Fedex无响应");
            }
        }
        private static string GetNotifications(ProcessShipmentReply reply)
        {
            string messages = "";

            if (reply != null)
            {
                if (reply.Notifications != null && reply.Notifications.Length > 0)
                {
                    for (int i = 0; i < reply.Notifications.Length; i++)
                    {
                        Notification notification = reply.Notifications[i];

                        string message = string.Format("Notification no. {0}", i) + System.Environment.NewLine
                                        + string.Format("Severity: {0}", notification.Severity) + System.Environment.NewLine
                                        + string.Format("Code: {0}", notification.Code) + System.Environment.NewLine
                                        + string.Format("Message: {0}", notification.Message) + System.Environment.NewLine
                                        + string.Format("Source: {0}", notification.Source);

                        messages += message;
                    }
                }
            }
            return messages;
        }
        private static bool usePropertyFile() //Set to true for common properties to be set with getProperty function.
        {
            return getProperty("usefile").Equals("True");
        }
        private static String getProperty(String propertyname) //Sets common properties for testing purposes.
        {
            try
            {
                String filename = "C:\\Fedex\\Information.txt";
                if (System.IO.File.Exists(filename))
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(filename);
                    do
                    {
                        String[] parts = sr.ReadLine().Split(',');
                        if (parts[0].Equals(propertyname) && parts.Length == 2)
                        {
                            return parts[1].Trim();
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
        private static void LogXML(string operationType, Object obj, Type type)
        {
            System.Xml.Serialization.XmlSerializer serializer =
                new System.Xml.Serialization.XmlSerializer(type);
            TextWriter writer = new StreamWriter("E:\\ship.log", true);
            writer.WriteLine("========================================" + operationType + DateTime.Now.ToString() + "=========================================");
            serializer.Serialize(writer, obj);
            writer.WriteLine();
            writer.WriteLine("_________________________________________________________________________________________________________________________________");
            writer.WriteLine();
            writer.Close();
        }

        /// <summary>
        /// 分割发货地址
        /// </summary>
        /// <param name="deliveryAddress"></param>
        /// <returns></returns>
        private static string[] GetDeliveryAddress(string deliveryAddress)
        {
            string[] address = null;
            if (!string.IsNullOrWhiteSpace(deliveryAddress))
            {
                if (deliveryAddress.Contains(System.Environment.NewLine))
                {
                    address = deliveryAddress.Split(System.Environment.NewLine.ToCharArray());
                    address = address.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                    return address;
                }
                else
                {
                    return new string[1] { deliveryAddress };
                }
            }
            return null;
        }

        /// <summary>
        /// 商品固体或液体属性描述
        /// </summary>
        /// <param name="liquidProperty"></param>
        /// <returns></returns>
        private static string GetDescription(string liquidProperty)
        {
            if (!string.IsNullOrWhiteSpace(liquidProperty))
            {
                if (liquidProperty.CompareTo("1") == 0 || liquidProperty.CompareTo("2") == 0)
                {
                    return HSDeliveryNoticeConst.LIQUID;
                }
                else if (liquidProperty.CompareTo("3") == 0)
                {
                    return HSDeliveryNoticeConst.SOLID;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取物流轨迹明细
        /// </summary>
        /// <param name="routingDetail"></param>
        /// <param name="traces"></param>
        /// <returns></returns>
        private static List<DeliveryNoticeTraceEntry> GetPackageRouteDetails(ShipmentOperationalDetail routingDetail, List<DeliveryNoticeTraceEntry> traces)
        {
            if (traces != null && traces.Count > 0)
            {
                foreach (var trace in traces)
                {
                    routingDetail.DeliveryDateSpecified = true;
                    trace.F_HS_DeliDate = routingDetail.DeliveryDate;
                }
            }
            return traces;
        }

        /// <summary>
        /// 获取装运响应
        /// </summary>
        /// <param name="isCodShipment"></param>
        /// <param name="reply"></param>
        /// <returns></returns>
        private static List<DeliveryNoticeTraceEntry> GetShipmentReply(ProcessShipmentReply reply)
        {
            List<DeliveryNoticeTraceEntry> traces = null;
            // Details for each package
            foreach (CompletedPackageDetail packageDetail in reply.CompletedShipmentDetail.CompletedPackageDetails)
            {
                traces = GetTrackingDetails(packageDetail.TrackingIds);
            }
            return GetPackageRouteDetails(reply.CompletedShipmentDetail.OperationalDetail, traces);
        }

        /// <summary>
        /// 获取跟踪细节
        /// </summary>
        /// <param name="TrackingIds"></param>
        /// <returns></returns>
        private static List<DeliveryNoticeTraceEntry> GetTrackingDetails(TrackingId[] TrackingIds)
        {
            DeliveryNoticeTraceEntry trace = null;
            List<DeliveryNoticeTraceEntry> traces = null;

            // Tracking information for each package

            if (TrackingIds != null)
            {
                traces = new List<DeliveryNoticeTraceEntry>();

                for (int i = 0; i < TrackingIds.Length; i++)
                {
                    trace = new DeliveryNoticeTraceEntry();
                    trace.F_HS_CarryBillNO = TrackingIds[i].TrackingNumber;
                    trace.FLogComId = TrackingIds[i].TrackingIdType.ToString();
                    trace.F_HS_Channel = TrackingIds[i].TrackingIdType.ToString();
                    traces.Add(trace);
                }
            }
            return traces;
        }
    }
}