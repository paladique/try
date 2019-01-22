﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Clockwise;
using MLS.Repositories;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    public class NugetToolPackageDiscoveryStrategy : IPackageDiscoveryStrategy
    {
        private readonly DirectoryInfo _workingDirectory;
        ToolPackageLocator _locator;
        private readonly PackageLocator _packageLocator;
        private readonly string _addSource;

        public NugetToolPackageDiscoveryStrategy(DirectoryInfo workingDirectory, string addSource)
        {
            _workingDirectory = workingDirectory;
            _locator = new ToolPackageLocator(workingDirectory.FullName);
            _packageLocator = new PackageLocator();
            _addSource = addSource;
        }

        public async Task<PackageBuilder> Locate(PackageDescriptor packageDesciptor, Budget budget = null)
        {
            var dotnet = new Dotnet();
            var installationResult = await dotnet.ToolInstall(
                packageDesciptor.Name,
                _workingDirectory.FullName,
                _addSource);

            if (installationResult.ExitCode != 0)
            {
                return null;
            }

            var tool = await _locator.LocatePackageAsync(packageDesciptor.Name, budget);
            if (tool != null)
            {
                var pb = new PackageBuilder(packageDesciptor.Name,
                    new PackageToolInitializer(Path.Combine(_workingDirectory.FullName, packageDesciptor.Name), _workingDirectory));
                pb.Directory = tool.Directory;
                return pb;
            }

            return null;
        }
    }
}
