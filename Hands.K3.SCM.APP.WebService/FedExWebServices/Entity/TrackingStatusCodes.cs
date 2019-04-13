using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.WebService.FedExWebServices.Entity
{
    public class TrackingStatusCodes
    {
        public static string GetTrackingStatusDescription(string trackingStatusCode)
        {
            if (!string.IsNullOrWhiteSpace(trackingStatusCode))
            {
                switch (trackingStatusCode)
                {
                    case "AA":
                        return "At Airport";
                    case "AC":
                        return "At Canada Post facility";
                    case "AF":
                        return "At Delivery";
                    case "AP":
                        return "At Pickup";
                    case "AR":
                        return "Arrived at";
                    case "AX":
                        return "At USPS facility";
                    case "CA":
                        return "Shipment Cancelled";
                    case "CH":
                        return "Location Changed";
                    case "DD":
                        return "Delivery Delay";
                    case "DE":
                        return "Delivery Exception";
                    case "DL":
                        return "Delivered";
                    case "DP":
                        return "Departed";
                    case "DR":
                        return "Vehicle furnished but not used";
                    case "DS":
                        return "Vehicle Dispatched";
                    case "DY":
                        return "Delay";
                    case "EA":
                        return "Enroute to Airport";
                    case "ED":
                        return "Enroute to Delivery";
                    case "EO":
                        return "Enroute to Origin Airport";
                    case "EP":
                        return "Enroute to Pickup";
                    case "FD":
                        return "At FedEx Destination";
                    case "HL":
                        return "Hold at Location";
                    case "IT":
                        return "In Transit";
                    case "IX":
                        return "In transit (see Details)";
                    case "LO":
                        return "Left Origin";
                    case "OC":
                        return "Order Created";
                    case "OD":
                        return "Out for Delivery";
                    case "OF":
                        return "At FedEx origin facility";
                    case "OX":
                        return "Shipment information sent to USPS";
                    case "PD":
                        return "Pickup Delay";
                    case "PF":
                        return "Plane in Flight";
                    case "PL":
                        return "Plane Landed";
                    case "PM":
                        return "In Progress";
                    case "PU":
                        return "Picked Up";
                    case "PX":
                        return "Picked up (see Details)";
                    case "RR":
                        return "RR";
                    case "RM":
                        return "CDO Modified";
                    case "RC":
                        return "CDO Cancelled";
                    case "RS":
                        return "Return to Shipper";
                    case "RP":
                        return "Return label link emailed to return sender";
                    case "LP":
                        return "Return label link cancelled by shipment originator";
                    case "RG":
                        return "Return label link expiring soon";
                    case "RD":
                        return "Return label link expired";
                    case "SE":
                        return "Shipment Exception";
                    case "SF":
                        return "At Sort Facility";
                    case "SP":
                        return "Split Status";
                    case "TR":
                        return "Transfer";
                    case "CC":
                        return "Cleared Customs";
                    case "CD":
                        return "Clearance Delay";
                    case "CP":
                        return "Clearance in Progress";
                    case "SH":
                        return "Shipper";
                    case "CU":
                        return "Customs";
                    case "BR":
                        return "Broker";
                    case "TP":
                      return "Transfer Partner";
                }
            }
            return null;
        }
    }
}
