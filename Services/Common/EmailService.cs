using AppSettings;
using System.Net.Mail;

namespace CommonServices
{
    public class EmailTemplates
    {
        public static string NewRegistration = "new_registration.html";
        public static string NewActivation = "new_activation.html";
        public static string AccountRegistration = "account_registration.html";
        public static string AccountVerification = "account_verification.html";
        public static string AccountRejection = "account_rejection.html";
        public static string AccountConfirmation = "account_confirmation.html";
        public static string PasswordReset = "password_reset.html";
    }
    public class EmailSubjects
    {
        public static string NewRegistration = "BROS ONE TECH - BOTPOS | New registration";
        public static string NewActivation = "BROS ONE TECH - BOTPOS | New activation";
        public static string AccountRegistration = "BROS ONE TECH - BOTPOS | Registration success";
        public static string AccountVerification = "BROS ONE TECH - BOTPOS | Verify your email";
        public static string AccountRejection = "BROS ONE TECH - BOTPOS | Registration not approved";
        public static string AccountConfirmation = "BROS ONE TECH - BOTPOS | Welcome ";
        public static string PasswordReset = "BROS ONE TECH - BOTPOS | Password Reset";
    }
    public class EmailService
    {
        public static string allposFooterContent = @"<hr/>
                                <p style='font-size:16px'> Thanks & Regards,</p>
                                <div Style='width:100px; background:#008080; color:white; font-size:18px; padding:5px; text-align:center'> BROS ONE TECH - BOTPOS </div>
                                <p style='font-size:14px;><a target='_blank' href='https://www.allpos.software'>www.allpos.software</a></p>
                                <p style='font-size:14px'><b>Note: Please do not reply to this mail as this is an automated mail service.</b></p>";

        public static string allposEmailVerificationSubject = "BROS ONE TECH - BOTPOS | Verify your email";
        public static string allposEmailVerificationEmail = @"<!DOCTYPE html>" +
            "<html>" +
            "<head>" +
            "<title>BROS ONE TECH - BOTPOS Billing Software</title>" +
            "</head>" +
            "<body>" +
            "<div>" +
            "   <div style=\"font-family:Verdana,Arial,Helvetica,sans-serif;font-size:10pt\">" +
            "       <p style=\"margin:0px\"><b><span style=\"color:black\">Hi&nbsp; @user@,</span></b><br></p>" +
            "       <div><br></div>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">Thanks for signing up with&nbsp;ALLPOS!&nbsp;(The most Versatile Point-of-Sale Software)</span><br></p>" +
            "       <div><br></div>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">Just one quick security measure before we get going. <span style=\"background:white\">Please take a second to confirm your email address and finish your account set up.</span></span><br></p>" +
            "       <div><br></div>" +
            "       <p style=\"margin:0px\"><span style=\"color:black;background:white\">Verify this is the email address you used for @restaurant@</span><br></p>" +
            "       <div><span style=\"color:black;background:white\"></span><br></div>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\"><a target = '_blank' href = '@activationurl@'> Click here to verify...</a></span><br></p>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">&nbsp;</span><br></p>" +
            "       <p style=\"margin-top:0px\"><span style=\"font-family:&quot;Calibri&quot;,sans-serif;color:black\">To protect the security of your account, this link will expire in 5 days.</span><br></p>" +
            "       <p style=\"margin-top:0px\"><span style=\"font-family:&quot;Calibri&quot;,sans-serif;color:black\">Please&nbsp;ignore this email if you didn’t make this request.</span><br></p>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">&nbsp;</span><br></p>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">Thanks,</span><br></p>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">ALLPOS Team</span><br></p>" +
            "   </div>" +
            "</div>" +
            "</body>" +
            "</html>";

        public static string allposAccountActivationSubject = "ALLPOS | Welcome ";
        public static string allposAccountActivationEmail = @"<!DOCTYPE html>" +
            "<html>" +
            "<head>" +
            "<title>ALLPOS Restaurant Billing Software</title>" +
            "</head>" +
            "<body>" +
            "<div>" +
            "   <div style=\"font-family:Verdana,Arial,Helvetica,sans-serif;font-size:10pt\">" +
            "       <p style=\"text-align:center;\"><img src=\"https://commonresource.s3.ap-south-1.amazonaws.com/logo/allpos-logo.png\" alt=\"allpos\" width=\"200\" height=\"30\"></p>" +
            "       <p style=\"text-align:center;margin:0px\" align=\"center\"><b><span style=\"font-size:20.0pt;color:black\">@user@, Welcome to your journey with ALLPOS</span></b><br></p>" +
            "       <p style=\"text-align:center;margin:0px\" align=\"center\"><b><span style=\"font-size:20.0pt;color:black\">We’re excited to have you onboard</span></b><br></p>" +
            "       <p style=\"margin:0px\"><b><span style=\"color:black\">&nbsp;</span></b><br></p>" +
            "       <div><br></div>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">Thanks for verifying your email, <span>Your Account has been activated successfully</span>.</span><br></p>" +
            "       <div><br></div>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">Below are your login credentials;</span><br></p>" +
            "       <div><br></div>" +
            "       <p style=\"margin:0px\"><b><span style=\"color:black\">Account Id: @accountid@</span></b><br></p>" +
            "       <div><br></div>" +
            "       <p style=\"margin:0px\"><b><span style=\"color:black\">Organization Id: @organizationid@</span></b><br></p>" +
            "       <div><br></div>" +
            "       <p style=\"margin:0px\"><b><span style=\"color:black\">Username: @username@</span></b><br></p>" +
            "       <div><br></div>" +
            "       <p style=\"margin:0px\"><span style=\"color:#222222;background:white\">If you need help getting started, reach out to our customer success team support@allpos.software</span><br></p>" +
            "       <p style=\"margin:0px\"><span style=\"color:#222222\"><br><span style=\"background:white\"><span style=\"font-variant-caps:normal;text-align:start;float:none;word-spacing:0px\">If you have any other issues, feel free to contact us +91 955-165-8565.</span></span><br style=\"font-variant-caps:normal;text-align:start;box-sizing:border-box;word-spacing:0px\"> <br style=\"font-variant-caps:normal;text-align:start;box-sizing:border-box;word-spacing:0px\"> <span style=\"background:white\"><span style=\"font-variant-caps:normal;text-align:start;float:none;word-spacing:0px\">Happy Selling :)</span></span></span></p>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">&nbsp;</span><br></p>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">Thanks,</span><br></p>" +
            "       <p style=\"margin:0px\"><span style=\"color:black\">ALLPOS Team</span><br></p>" +
            "   </div>" +
            "</div>" +
            "</body>" +
            "</html>";

        #region Allpos Email Service
        public static string SendEmail(List<string> emailReceivers, string subject, string htmlContent)
        {
            string result = "Email Sent";

            try
            {
                if (!string.IsNullOrWhiteSpace(AppSetting.MailConfig.FromMailID) && !string.IsNullOrWhiteSpace(AppSetting.MailConfig.Username) && !string.IsNullOrWhiteSpace(AppSetting.MailConfig.Password) && emailReceivers != null && emailReceivers.Count > 0)
                {

                    MailMessage mail = new MailMessage
                    {
                        Sender = new MailAddress(AppSetting.MailConfig.SenderMailID, AppSetting.MailConfig.FriendlyName),
                        From = new MailAddress(AppSetting.MailConfig.FromMailID, AppSetting.MailConfig.FriendlyName),
                        Subject = subject,
                        IsBodyHtml = true,
                        Body = htmlContent
                    };

                    mail.ReplyToList.Add(AppSetting.MailConfig.ReplyToMailID);

                    foreach (var data in emailReceivers)
                    {
                        mail.To.Add(data);
                    }

                    //System.Net.Mail.Attachment attachment;
                    //attachment = new System.Net.Mail.Attachment(Server.MapPath("~/TempFiles/") + filename);
                    //// attachment = new System.Net.Mail.Attachment(@"E:\ClientFiles\" + filename);
                    //mail.Attachments.Add(attachment);

                    //// SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");                    
                    //SmtpClient SmtpServer = new SmtpClient("smtpout.secureserver.net")
                    //{
                    //    Port = 587,
                    //    Credentials = new System.Net.NetworkCredential(SenderUserName, SenderPassWord),
                    //    EnableSsl = true
                    //};

                    SmtpClient SmtpServer = new SmtpClient(AppSetting.MailConfig.SmtpClient)
                    {
                        Port = AppSetting.MailConfig.SmtpPort,
                        Credentials = new System.Net.NetworkCredential(AppSetting.MailConfig.Username, AppSetting.MailConfig.Password),
                        EnableSsl = AppSetting.MailConfig.SmtpEnableSsl
                    };

                    SmtpServer.Send(mail);

                    result = "Email Sent";
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            return result;
        }
        public static string SendEmail(List<string> emailReceivers, string subject, string bodyContent, bool addFooter = true)
        {
            string result = "Email Sent";

            try
            {
                if (!string.IsNullOrWhiteSpace(AppSetting.MailConfig.FromMailID) && !string.IsNullOrWhiteSpace(AppSetting.MailConfig.Username) && !string.IsNullOrWhiteSpace(AppSetting.MailConfig.Password) && emailReceivers != null && emailReceivers.Count > 0)
                {
                    string bodyOfMail = "<html>";
                    bodyOfMail += "<head></head>";
                    bodyOfMail += "<body style = 'font-family: Arial, Helvetica, sans-serif; font-size:18px' >";
                    bodyOfMail += bodyContent;
                    if (addFooter)
                    {
                        bodyOfMail += allposFooterContent;
                    }
                    bodyOfMail += "</body>";
                    bodyOfMail += "</html>";

                    MailMessage mail = new MailMessage
                    {
                        Sender = new MailAddress(AppSetting.MailConfig.SenderMailID, AppSetting.MailConfig.FriendlyName),
                        From = new MailAddress(AppSetting.MailConfig.FromMailID, AppSetting.MailConfig.FriendlyName),
                        Subject = subject,
                        IsBodyHtml = true,
                        Body = bodyOfMail
                    };

                    mail.ReplyToList.Add(AppSetting.MailConfig.ReplyToMailID);

                    foreach (var data in emailReceivers)
                    {
                        mail.To.Add(data);
                    }

                    //System.Net.Mail.Attachment attachment;
                    //attachment = new System.Net.Mail.Attachment(Server.MapPath("~/TempFiles/") + filename);
                    //// attachment = new System.Net.Mail.Attachment(@"E:\ClientFiles\" + filename);
                    //mail.Attachments.Add(attachment);

                    //// SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");                    
                    //SmtpClient SmtpServer = new SmtpClient("smtpout.secureserver.net")
                    //{
                    //    Port = 587,
                    //    Credentials = new System.Net.NetworkCredential(SenderUserName, SenderPassWord),
                    //    EnableSsl = true
                    //};

                    SmtpClient SmtpServer = new SmtpClient(AppSetting.MailConfig.SmtpClient)
                    {
                        Port = 587,
                        Credentials = new System.Net.NetworkCredential(AppSetting.MailConfig.Username, AppSetting.MailConfig.Password),
                        EnableSsl = true
                    };

                    SmtpServer.Send(mail);

                    result = "Email Sent";
                }
            }
            catch (SmtpException ex)
            {
                result = ex.ToString();
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            return result;
        }
        #endregion Allpos Email Service

    }
}
