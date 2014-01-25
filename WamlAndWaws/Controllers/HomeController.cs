using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.WebSites;
using Microsoft.WindowsAzure.Management.WebSites.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WamlAndWaws.Models;

namespace WamlAndWaws.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var cred = GetCredentials();
            var client = CloudContext.Clients.CreateWebSiteManagementClient(cred);
            var viewModel = GetHomePageViewModel(client);

            return View(viewModel);
        }

        public ActionResult CertInfo()
        {
            var certificate = GetCertificate();

            ViewBag.friendlyName = certificate.FriendlyName;
            ViewBag.issuer = certificate.Issuer;
            ViewBag.issuerName = certificate.IssuerName.Name;
            ViewBag.subject = certificate.Subject;
            ViewBag.thumbprint = certificate.Thumbprint;

            return View();
        }

        [HttpPost]
        public ActionResult NewSite(string SiteName, string WebSpaceName)
        {
            var cred = GetCredentials();
            var client = new WebSiteManagementClient(cred);

            var parameters = new WebSiteCreateParameters
            {
                Name = SiteName,
                SiteMode = WebSiteMode.Limited,
                ComputeMode = WebSiteComputeMode.Shared,
                WebSpaceName = WebSpaceName
            };

            parameters.HostNames.Add(
                string.Format("{0}.azurewebsites.net", SiteName)
                );

            var result = client.WebSites.Create(WebSpaceName, parameters);

            var viewModel = GetHomePageViewModel(client);

            return View("Index", viewModel);
        }

        private static HomePageViewModel GetHomePageViewModel(WebSiteManagementClient client)
        {
            var viewModel = new HomePageViewModel();

            var webSpaceFunc = new Action<WebSpacesListResponse.WebSpace>((x) =>
            {
                viewModel.WebSpaces.Add(new WebSpaceListItem
                {
                    GeoRegion = x.GeoRegion,
                    WebSpaceName = x.Name
                });
            });

            var webSiteListFunc = new Action<WebSpacesListResponse.WebSpace>((webSpace) =>
            {
                var webSitesInWebSpace = client.WebSpaces.ListWebSites(
                    webSpace.Name,
                    new WebSiteListParameters
                    {
                        PropertiesToInclude = new List<string>()
                    });

                viewModel.WebSites = webSitesInWebSpace.WebSites.Select(s => new WebSiteRowItem
                {
                    WebSpace = viewModel.WebSpaces.First(a => a.WebSpaceName == webSpace.Name),
                    WebSiteName = s.Name,
                    DomainNames = s.HostNames.ToList()
                }).ToList();
            });

            var listWebSpacesResponse = client.WebSpaces.List();

            listWebSpacesResponse.WebSpaces.ToList().ForEach(x =>
            {
                webSpaceFunc(x);
                webSiteListFunc(x);
            });

            return viewModel;
        }

        private X509Certificate2 GetCertificate()
        {
            string certPath = Server.MapPath(
                ConfigurationManager.AppSettings["CERTIFICATE-PATH"]
                );

            var x509Cert = new X509Certificate2(certPath, 
                ConfigurationManager.AppSettings["CERTIFICATE-PASSWORD"]
                );

            return x509Cert;
        }

        [AllowAnonymous]
        private SubscriptionCloudCredentials GetCredentials()
        {
            var logPath = Server.MapPath("~/App_Data/log.txt");
            CloudContext.Configuration.Tracing.AddTracingInterceptor(
                new LogFileTracingInterceptor(logPath)
                );

            var subscriptionId = ConfigurationManager.AppSettings["AZURE-SUBSCRIPTION-ID"];

            return new CertificateCloudCredentials(subscriptionId, GetCertificate());
        }
    }

    public class LogFileTracingInterceptor : Microsoft.WindowsAzure.ICloudTracingInterceptor
    {
        string _logFile;

        public LogFileTracingInterceptor(string logFile)
        {
            _logFile = logFile;
        }

        private void Write(string message, params object[] arguments)
        {
            using (FileStream filestream = new FileStream(_logFile, FileMode.Append))
            {
                var line = string.Format(message, arguments);
                var writer = new StreamWriter(filestream);
                writer.Write(line);
                writer.Flush();
                filestream.Close();
            }
        }

        public void Information(string message)
        {
            Write(message);
        }

        public void Configuration(string source, string name, string value)
        {
            Write("Configuration(" + source + "): " + name + " = " + value);
        }

        public void Enter(string invocationId, object instance, string method, IDictionary<string, object> parameters)
        {
            Write("{0}: Enter {1}({4}) on 0x{3:X}:{2}",
                invocationId,
                method,
                instance,
                instance.GetHashCode(),
                string.Join(
                    ", ",
                    parameters.Select(p => p.Key + "=" + p.Value.ToString())));
        }

        public void SendRequest(string invocationId, HttpRequestMessage request)
        {
            Write("{0}: SendRequst {1}", invocationId, request.ToString());
        }

        public void ReceiveResponse(string invocationId, HttpResponseMessage response)
        {
            Write("{0}: ReceiveResponse {1}", invocationId, response.ToString());
        }

        public void Error(string invocationId, Exception ex)
        {
            Write("{0}: Error {1}", invocationId, ex.ToString());
        }

        public void Exit(string invocationId, object result)
        {
            Write("{0}: Exit {1}", invocationId, result);
        }
    }
}