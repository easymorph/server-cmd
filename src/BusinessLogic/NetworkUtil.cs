using Morph.Server.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
#if NETCOREAPP2_0
using System.Net.Http;
#endif
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic
{
    /// <summary>
    /// NetworkUtil class - contains miscellaneous helper functions related to network.
    /// </summary>
    public class NetworkUtil
    {

        private static SslPolicyErrors[] sslFatalErrors = Enum.GetValues(typeof(SslPolicyErrors)).Cast<SslPolicyErrors>().Where(x=>x != SslPolicyErrors.None).ToArray();
        /// <summary>
        /// Configures ServicePointManager static class 
        /// </summary>        
        public static void ConfigureServicePointManager(bool suppressSslErrors)
        {
            // Allow self-signed certificates
            ServicePointManager.ServerCertificateValidationCallback += (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                if (suppressSslErrors)
                {
                    return (!sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable));
                }
                else
                {
                    return !(sslFatalErrors.Any(x => sslPolicyErrors.HasFlag(x)));
                }        

            };
            // Allow SSL3. Default value is: Tls, Tls11, Tls12
#if NET45
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#else
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
        }

#if NETCOREAPP2_0
        internal static void ConfigureServerCertificateCustomValidationCallback(bool suppressSslErrors)
        {

            MorphServerApiClientGlobalConfig.ServerCertificateCustomValidationCallback = (request, certificate, chain, sslPolicyErrors) =>
             {
                 if (suppressSslErrors)
                 {
                     return (!sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable));
                 }
                 else
                 {
                     return !(sslFatalErrors.Any(x => sslPolicyErrors.HasFlag(x)));
                 }
             };
            
        }
#endif
    }
}