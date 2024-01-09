namespace TeamAssigner.Services
{
    using System.Net.Mail;
    using System.Net;
    public sealed class EmailService(string smtpServer, int smtpPort, string fromEmail, string fromEmailPwd)
    {
        readonly string smtpServer = smtpServer;
        readonly int smtpPort = smtpPort;
        readonly string fromEmail = fromEmail;
        readonly string fromEmailPwd = fromEmailPwd;

        internal void SendEmail(string toEmails, string subject, string body)
        {
            var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(fromEmail, fromEmailPwd),
                EnableSsl = true
            };
            try
            {
                client.Send(fromEmail, toEmails, subject, body);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not send email(s).The following exception occurred. {e}");
            }
        }
    }
}
