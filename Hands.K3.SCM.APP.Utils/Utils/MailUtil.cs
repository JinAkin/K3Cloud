using Hands.K3.SCM.APP.Entity.K3WebApi;
using Hands.K3.SCM.APP.Entity.StructType;
using HS.K3.Common.Mike;
using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hands.K3.SCM.APP.Utils.Utils
{
    public class MailUtil
    {
        #region 邮件正文表体
        /// <summary>
        /// 邮件正文表体Part
        /// </summary>
        private static string partStr = "<tr height=\"18\" style=\"height:13.50pf;\">"
        + "<td class=\"et11\" colspan=\"12\" x:str=\"\" height=\"18\" width:auto style=\"color: rgb(0, 0, 0); font-size: 10pt; font-weight: 400; font-style: normal; text-decoration: none; text-align: left; vertical-align: top; white-space: normal; border: 0.5pt solid rgb(0, 0, 0); height: 13.53pt; width: 973.52pt; font-family: Arial; background: rgb(255, 255, 0);\">Part {0}</td>"
        + "</tr> ";

        /// <summary>
        /// 列开始
        /// </summary>
        private static string startStr = "<tr height=\"18\" style=\"height:13.50pf;\">";

        /// <summary>
        /// 列构建
        /// </summary>
        private static string columnStr = "<td class=\"et12\" x:str=\"\" height=\"18\" width:auto style=\"color: rgb(0, 0, 0); font-size: 10pt; font-weight: 400; font-style: normal; text-decoration: none; text-align: left; vertical-align: top; border: 0.5pt solid rgb(0, 0, 0); height: 13.53pt; width: 95.28pt; font-family: Arial;\">{0}</td>";

        /// <summary>
        /// 列结束
        /// </summary>
        private static string endStr = "</tr>";

        #endregion

        public static void SendEmail(Context ctx, HttpResponseResult result, string guidString)
        {
            #region 查询邮件内容

            #endregion

            #region 根据上传数据确认邮件拆分

            string retouchKey = "^@!";//修饰键(排除编码有相同部分的数据，避免造成数据错误)

            #endregion

            Queue<MailMessage> mailList = new Queue<MailMessage>();

            #region 新上传邮件
            string reFilePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\HS_TemplateFile\\DeliTemplate.txt";//邮件模板
            string reImgPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\HS_TemplateFile\\healthCabin-log.png";//公司log
            System.Net.Mail.Attachment att = new Attachment(reImgPath);
            string imgKey = "comLog";
            att.ContentId = imgKey;
            if (!File.Exists(reFilePath))
            {
                return;
            }
            var emailTemplate = File.ReadAllText(reFilePath, Encoding.Default);

            mailList = BuildBodyWithNo(ctx,result, emailTemplate, imgKey);//构建邮件主体内容
            #endregion

            #region 发送邮件

            if (mailList != null && mailList.Count > 0)
            {
                MailMsgService svc = new MailMsgService();
                var mailServer = svc.GetCurrUserMailInfo(ctx);
                svc.SrcBillType = "HS_SAL_ImportLogis";
                svc.SrcBillNo = DateTime.Now.ToLongDateString();
                if (mailServer == null)
                {
                    foreach (var m in mailList)
                    {
                        m.Attachments.Dispose();
                    }
                    return;
                }
                svc.SendMailSynchro(ctx, mailList, mailServer);
                Thread.Sleep(mailList.Count() * 100);
            }

            #endregion
        }

        /// <summary>
        /// 构建新上传运单的邮件内容
        /// </summary>
        /// <param name="mm">邮件类</param>
        /// <param name="deliInfos">邮件内容</param>
        /// <param name="emailTemplate">邮件模板</param>
        /// <param name="imgKey">logo</param>
        /// <Builder>Mike</Builder>
        private static Queue<MailMessage> BuildBodyWithNo(Context ctx, HttpResponseResult result, string emailTemplate, string imgKey)
        {
            
            Queue<MailMessage> mms = null;
            MailMessage mm = null;
            string allMsg = string.Empty;

            #region 构建内容

            StringBuilder head = new StringBuilder();//创建邮件表体数据
            StringBuilder body = null;

            head.Append("<html><head>");
            head.Append("<style class=\"fox_global_style\">div.fox_html_content { line-height: 1.5; }div.fox_html_content { font-size: 10.5pt; font-family: 'Microsoft YaHei UI'; color: rgb(0, 0, 0); line-height: 1.5; }</style>");
            head.Append("<style class=\"&quot;fox_global_style&quot;\">div.fox_html_content { line-height: 1.5; }div.fox_html_content { font-size: 10.5pt; font-family: 'Microsoft YaHei UI'; color: rgb(0, 0, 0); line-height: 1.5; }</style>");
            head.Append("</head><body><div><span id=\"&quot;_FoxCURSOR&quot;\"></span><br /></div>");
            head.Append(string.Format("An exception occurred in the {0} synchronization", result.DataType));
            head.Append("<div>");
            head.Append("<table border=\"1\" bordercolor=\"#000000\" cellpadding=\"2\" cellspacing=\"0\" style=\"font-size: 10pt; border-collapse:collapse;\" width=\"50%\">");
            head.Append("<tbody>");
            head.Append("<tr>");
            head.Append("<td width=\"12%\" nowrap=\"\"><font size=\"2\" face=\"Verdana\">Order ID</font></td>");
            head.Append("<td width=\"12%\" nowrap=\"\"><font size=\"2\" face=\"Verdana\">Error Message</font></td>");

            head.Append("</tr>");

            //if (result != null && !result.Success 
            //    && result.SpecifiedMsgs != null && result.SpecifiedMsgs.Count > 0)
            //{
            //    var group = result.SpecifiedMsgs.GroupBy(o => new { o.BillNo, o.SalerEmail });

            //    if (group != null && group.Count() > 0)
            //    {
            //        mms = new Queue<MailMessage>();

            //        foreach (var gro in group)
            //        {
            //            if (gro != null && gro.Count() > 0)
            //            {
            //                var sgro = gro.GroupBy(o => o.BillNo);
                            
            //                foreach (var g in sgro)
            //                {
            //                    if (g != null)
            //                    {
            //                        mm = new MailMessage();
            //                        body = new StringBuilder();

            //                        List<string> responses = g.Select(o => o.Msg).ToList();
            //                        string specificMsg = string.Empty;

            //                        if (responses != null && responses.Count > 0)
            //                        {
            //                            specificMsg = string.Join(Environment.NewLine, responses); 
            //                        }
            //                        mm.IsBodyHtml = true;
            //                        mm.SubjectEncoding = Encoding.UTF8;
            //                        mm.BodyEncoding = Encoding.UTF8;
            //                        mm.Subject = string.Format("An exception occurred in {0} synchronization", string.Join(",", result.DataType));//邮件标题

                                    
            //                        if (ctx.DBId.CompareTo(DataBaseConst.K3CloudDbId) == 0)
            //                        {
            //                            mm.To.Add(g.ElementAt(0).SalerEmail);
            //                            mm.From = new MailAddress("abby.lee@healthcabin.net");
            //                        }
                                    
            //                        body.Append("<tr>");
            //                        body.Append(string.Format("<td width=\"12%\" nowrap=\"\"><font size=\"2\" face=\"Verdana\">{0}</font></td>", gro.Key.BillNo));
            //                        body.Append(string.Format("<td width=\"12%\" nowrap=\"\"><font size=\"2\" face=\"Verdana\">{0}</font></td>", specificMsg));
            //                        body.Append("</tr>");

            //                        allMsg += body.ToString();

            //                        mm.Body = head.ToString() + body.ToString();
            //                        mm.Subject = string.Format("An exception occurred in the {0} synchronization", result.DataType);
            //                        mms.Enqueue(mm);
            //                    }
            //                } 
            //            } 
            //        }

            //        if (!string.IsNullOrWhiteSpace(allMsg))
            //        {
            //            mm = new MailMessage();
            //            mm.IsBodyHtml = true;
            //            mm.SubjectEncoding = Encoding.UTF8;
            //            mm.BodyEncoding = Encoding.UTF8;
            //            mm.Subject = string.Format("An exception occurred in {0} synchronization", string.Join(",", result.DataType));//邮件标题

            //            mm.To.Add("abby.lee@healthcabin.net");
            //            mm.To.Add("kevin.wu@handsgroups.com");
            //            mm.To.Add("abbott.zhang@healthcabin.net");
            //            mm.From = new MailAddress("abby.lee@healthcabin.net");

            //            mm.Body = head.ToString() + allMsg;
            //            mm.Subject = string.Format("An exception occurred in the {0} synchronization", result.DataType);
            //            mms.Enqueue(mm);
            //        }  
            //    }   
            //}
            return mms;
            #endregion
        }
    } 
}
