using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kudu.Contracts.SiteExtensions;
using NuGet;

namespace Kudu.Core.SiteExtensions
{
    public class SiteExtensionManager : ISiteExtensionManager
    {
        // TODO, suwatch: testing purpose
        static SiteExtensionInfo DummyInfo = new SiteExtensionInfo
        {
            Title = "Dummy Extension",
            Description = "Dummy stuff",
            Id = "Dummy",
            Update = new SiteExtensionInfo
            {
                Id = "Dummy",
            }
        };

        private static readonly Uri _remoteSource = new Uri("http://siteextensions.azurewebsites.net/api/v2/");
        private readonly IPackageRepository _sourceRepository = new DataServicePackageRepository(_remoteSource);

        public async Task<IEnumerable<SiteExtensionInfo>> GetRemoteExtensions(string filter, bool allowPrereleaseVersions = false)
        {
            if (String.IsNullOrEmpty(filter))
            {
                return await Task.Run(() => _sourceRepository.GetPackages()
                                        .Where(p => p.IsLatestVersion)
                                        .OrderByDescending(f => f.DownloadCount)
                                        .AsEnumerable()
                                        .Select(SiteExtensionInfo.ConvertFrom));
            }

            return await Task.Run(() => _sourceRepository.Search(filter, allowPrereleaseVersions)
                                                         .AsEnumerable()
                                                         .Select(SiteExtensionInfo.ConvertFrom));
            
        }

        public async Task<SiteExtensionInfo> GetRemoteExtension(string id, string version)
        {
            return await Task.FromResult(DummyInfo);
        }

        public async Task<IEnumerable<SiteExtensionInfo>> GetLocalExtensions(string filter, bool update_info)
        {
            return await Task.FromResult(new[] { DummyInfo });
        }

        public async Task<SiteExtensionInfo> GetLocalExtension(string id, bool update_info)
        {
            return await Task.FromResult(DummyInfo);
        }

        public async Task<SiteExtensionInfo> InstallExtension(SiteExtensionInfo info)
        {
            return await Task.FromResult(info);
        }

        public async Task<bool> UninstallExtension(string id)
        {
            return await Task.FromResult(true);
        }
    }
}
