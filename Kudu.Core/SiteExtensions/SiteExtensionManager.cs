using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kudu.Contracts.SiteExtensions;
using Kudu.Core.Infrastructure;
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
        private readonly IPackageRepository _remoteRepository = new DataServicePackageRepository(_remoteSource);
        private readonly IPackageRepository _localRepository;
        private readonly IEnvironment _environment;

        public SiteExtensionManager(IEnvironment environment)
        {
            _environment = environment;
            _localRepository = new LocalPackageRepository(_environment.RootPath + "\\SiteExtensions");
        }

        public async Task<IEnumerable<SiteExtensionInfo>> GetRemoteExtensions(string filter, bool allowPrereleaseVersions = false)
        {
            if (String.IsNullOrEmpty(filter))
            {
                return await Task.Run(() => _remoteRepository.GetPackages()
                                        .Where(p => p.IsLatestVersion)
                                        .OrderByDescending(f => f.DownloadCount)
                                        .AsEnumerable()
                                        .Select(SiteExtensionInfo.ConvertFrom));
            }

            return await Task.Run(() => _remoteRepository.Search(filter, allowPrereleaseVersions)
                                                         .AsEnumerable()
                                                         .Select(SiteExtensionInfo.ConvertFrom));
        }

        public async Task<SiteExtensionInfo> GetRemoteExtension(string id, string version)
        {
            return await Task.FromResult(DummyInfo);
        }

        public async Task<IEnumerable<SiteExtensionInfo>> GetLocalExtensions(string filter, bool update_info)
        {
            return await Task.Run(() =>
                _localRepository.Search(filter, true).AsEnumerable()
                                                     .Select(SiteExtensionInfo.ConvertFrom)
            );
        }

        public async Task<SiteExtensionInfo> GetLocalExtension(string id, bool update_info)
        {
            return await Task.FromResult(DummyInfo);
        }

        public async Task<SiteExtensionInfo> InstallExtension(string id)
        {
            return await Task.Run(() =>
            {
                IPackage package = _remoteRepository.FindPackage(id);
                var installationDirectory = _localRepository.Source + package.GetFullName();

                foreach (var file in package.GetFiles())
                {
                    var fullPath = Path.Combine(installationDirectory, file.Path);
                    FileSystemHelpers.CreateDirectory(Path.GetDirectoryName(fullPath));
                    using (Stream writeStream = File.OpenWrite(fullPath),
                        readStream = file.GetStream())
                    {
                        readStream.CopyTo(writeStream);
                    }
                }

                return SiteExtensionInfo.ConvertFrom(package);
            });
        }

        public async Task<bool> UninstallExtension(string id)
        {
            return await Task.Run(() =>
            {
                IPackage package = _localRepository.FindPackage(id);
                FileSystemHelpers.DeleteDirectoryContentsSafe(_localRepository.Source + package.GetFullName());
                return true;
            });
        }
    }
}
