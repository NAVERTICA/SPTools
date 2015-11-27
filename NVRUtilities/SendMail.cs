/*  Copyright (C) 2015 NAVERTICA a.s. http://www.navertica.com 

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.  */
	
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.SharePoint.Administration;
using Navertica.SharePoint.Interfaces;
using Navertica.SharePoint.Logging;
using Newtonsoft.Json.Linq;

namespace Navertica.SharePoint
{
    /// <summary>
    /// Way to define attachments for Tools.SendMail
    /// </summary>
    public class MailAttachment
    {
        public Stream FileStream;
        public string FileName;
        public string ContentType;
    }
}

namespace Navertica.SharePoint.Utilities
{
    public partial class Tools
    {
        public static bool HasProperty(dynamic json, string property)
        {
            List<string> propertyNames = ( (JObject) json ).Properties().Select(p => p.Name).ToList();
            return propertyNames.Contains(property);
        }

        public static bool SendMail(string to, string subject, string body, IEnumerable<MailAttachment> attachments = null, bool isHtml = true, List<string> cc = null)
        {
            ILogging log = NaverticaLog.GetInstance();

            SPOutboundMailServiceInstance smtpInstance = SPAdministrationWebApplication.Local.OutboundMailServiceInstance;

            if (smtpInstance == null)
            {
                log.LogWarning("OutboundMailService is not configured!");
                return false;
            }

            string smtpServer = smtpInstance.Server.Address;
            //string smtpFrom = from;

            //from nepouzivame a bereme vzdy to co je v Outgiong E-Mail configuration
            //if (string.IsNullOrEmpty(smtpFrom))
            //{
            string smtpFrom = SPAdministrationWebApplication.Local.OutboundMailSenderAddress;
            //}

            System.Net.Mail.MailMessage mailMessage;
            try
            {
                mailMessage = new System.Net.Mail.MailMessage(smtpFrom, to, subject, body);
                mailMessage.IsBodyHtml = isHtml;

                if (cc != null)
                {
                    foreach (string copy in cc)
                    {
                        mailMessage.CC.Add(copy);

                    }
                }
            }
            catch (Exception exc)
            {
                log.LogException("SendMail Failed from: '" + smtpFrom + " to: '" + to, exc);
                return false;
            }

            if (attachments != null)
            {
                foreach (MailAttachment att in attachments)
                {
                    mailMessage.Attachments.Add(new System.Net.Mail.Attachment(att.FileStream, att.FileName, att.ContentType));
                }
            }

            int timeout = -1;
            try
            {
                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient(smtpServer);
                timeout = smtpClient.Timeout;
                smtpClient.Send(mailMessage);
            }
            catch (Exception exc)
            {
                log.LogError("SendMail Failed with timeout " + timeout + " from: '" + smtpFrom + " to: '" + to, exc);

                try
                {
                    System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient(smtpServer);
                    timeout = timeout * 2;
                    smtpClient.Timeout = timeout;
                    smtpClient.Send(mailMessage);
                }
                catch (Exception exc2)
                {
                    log.LogError("Second SendMail Failed with timeout " + timeout + " from: '" + smtpFrom + " to: '" + to, exc2);

                    return false;
                }

                return false;
            }

            return true;
        }
    }
}