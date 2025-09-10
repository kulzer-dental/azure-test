using System.Reflection;

namespace KDC.Main.Helpers
{
    public static class CustomTemplateEmailSender
    {
        private static string _templateDirectory;

        static CustomTemplateEmailSender()
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            if (assemblyLocation != null)
            {
                _templateDirectory = Path.Combine(assemblyLocation, "Templates");
            }
            else
            {
                throw new InvalidOperationException("Assembly location could not be determined.");
            }
        }

        public static string GetEmailBody(string htmlTemplate, string reportName = "")
        {
            var layoutPath = Path.Combine(_templateDirectory, "Layout.html");
            var layoutHtml = File.ReadAllText(layoutPath);

            var templatePath = Path.Combine(_templateDirectory, htmlTemplate);
            var templateHtml = File.ReadAllText(templatePath);

            var body = layoutHtml.Replace("{htmlContent}", templateHtml);

            body = body.Replace("{ReportName}", reportName);
            return body;
        }
    }
}
