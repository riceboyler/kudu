﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kudu.Client.Infrastructure;
using Kudu.Contracts.SiteExtensions;

namespace Kudu.Client.SiteExtensions
{
    public class RemoteSiteExtensionManager : KuduRemoteClientBase, ISiteExtensionManager
    {
        public RemoteSiteExtensionManager(string serviceUrl, ICredentials credentials = null, HttpMessageHandler handler = null)
            : base(serviceUrl, credentials, handler)
        {
        }

        public async Task<IEnumerable<SiteExtensionInfo>> GetRemoteExtensions(string filter = null, bool allowPrereleaseVersions = false)
        {
            var url = new StringBuilder(ServiceUrl);
            url.Append("remote");

            var separator = '?';
            if (!String.IsNullOrEmpty(filter))
            {
                url.Append(separator);
                url.Append("filter=");
                url.Append(filter);
                separator = '&';
            }

            if (allowPrereleaseVersions)
            {
                url.Append(separator);
                url.Append("allowPrereleaseVersions=");
                url.Append(true);
            }

            return await Client.GetJsonAsync<IEnumerable<SiteExtensionInfo>>(url.ToString());
        }

        public async Task<SiteExtensionInfo> GetRemoteExtension(string id, string version = null)
        {
            var url = new StringBuilder(ServiceUrl);
            url.Append("remote/");
            url.Append(id);

            if (!String.IsNullOrEmpty(version))
            {
                url.Append("?version=");
                url.Append(version);
            }

            return await Client.GetJsonAsync<SiteExtensionInfo>(url.ToString());
        }

        public async Task<IEnumerable<SiteExtensionInfo>> GetLocalExtensions(string filter = null, bool update_info = true)
        {
            var url = new StringBuilder(ServiceUrl);
            url.Append("local");

            var separator = '?';
            if (!String.IsNullOrEmpty(filter))
            {
                url.Append(separator);
                url.Append("filter=");
                url.Append(filter);
                separator = '&';
            }

            if (!update_info)
            {
                url.Append(separator);
                url.Append("update_info=");
                url.Append(update_info);
                separator = '&';
            }

            return await Client.GetJsonAsync<IEnumerable<SiteExtensionInfo>>(url.ToString());
        }

        public async Task<SiteExtensionInfo> GetLocalExtension(string id, bool update_info = true)
        {
            var url = new StringBuilder(ServiceUrl);
            url.Append("local/");
            url.Append(id);

            if (!update_info)
            {
                url.Append("?update_info=");
                url.Append(update_info);
            }
            
            return await Client.GetJsonAsync<SiteExtensionInfo>(url.ToString());
        }

        public async Task<SiteExtensionInfo> InstallExtension(string id)
        {
            return await Client.PostJsonAsync<string, SiteExtensionInfo>(String.Empty, id);
        }

        public async Task<bool> UninstallExtension(string id)
        {
            var url = new StringBuilder(ServiceUrl);
            url.Append("local/");
            url.Append(id);

            HttpResponseMessage result = await Client.DeleteAsync(new Uri(url.ToString()));
            return await result.EnsureSuccessful().Content.ReadAsAsync<bool>();
        }
    }
}