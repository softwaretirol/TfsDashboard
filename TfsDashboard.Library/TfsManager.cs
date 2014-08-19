using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Web;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TfsDashboard.Library
{
    public class TfsManager
    {
        private readonly TfsTeamProjectCollection _projectCollection;
        private readonly TfsDashboardSettings _settings;

        public TfsManager(TfsDashboardSettings settings)
        {
            var uri = new Uri(settings.Url + settings.Collection);
            var credentials = new NetworkCredential(settings.User, settings.Password);
            _projectCollection = new TfsTeamProjectCollection(uri, credentials);
            _settings = settings;
        }

        public byte[] GetImage(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;
            try
            {
                _projectCollection.EnsureAuthenticated();
                var identityService = _projectCollection.GetService<IIdentityManagementService2>();
                TeamFoundationIdentity i = identityService.ReadIdentity(IdentitySearchFactor.AccountName, username,
                    MembershipQuery.Direct, ReadIdentityOptions.ExtendedProperties);
                var img = i.GetProperty("Microsoft.TeamFoundation.Identity.Image.Data");
                return img as byte[];
            }
            catch
            {
                return null;
            }
        }

        private static VersionSpec GetDateVSpec(DateTime date)
        {
            string dateSpec = string.Format("D{0:yyy}-{0:MM}-{0:dd}T{0:HH}:{0:mm}", date);
            return VersionSpec.ParseSingleSpec(dateSpec, "");
        }

        private void GetChangeset(TfsDashboardSummary summary)
        {
            var watch = Stopwatch.StartNew();
            _projectCollection.EnsureAuthenticated();

            var versionControl = _projectCollection.GetService<VersionControlServer>();
            var queryParams = new QueryHistoryParameters(_settings.VersionControlPath, RecursionType.Full)
            {
                Item = _settings.VersionControlPath,
                IncludeDownloadInfo = false,
                IncludeChanges = false,
                VersionStart = GetDateVSpec(DateTime.Today.AddDays(-75))
            };
            var query = versionControl.QueryHistory(queryParams);
            var results = query.ToList();
            summary.LastCheckins = results.Take(15).Select(x => new TfsCheckinSummary
            {
                Comment = x.Comment,
                Committer = x.CommitterDisplayName,
                Username = x.Committer,
                CreationDate = x.CreationDate,
                TimeElapsed = x.CreationDate
            }).ToList();

            var allDays = Enumerable.Range(0, 75).Select(x => new TfsCheckinStatistic
            {
                Day = DateTime.Today.AddDays(-x),
                Count = 0
            });
            var checkins = results.GroupBy(x => x.CreationDate.Date).Select(x => new TfsCheckinStatistic
            {
                Day = x.Key,
                Count = x.Count()
            }).ToList();

            checkins = checkins.Union(allDays.Where(x => checkins.All(y => y.Day != x.Day))).ToList();
            summary.CheckinStatistic = checkins.OrderBy(x => x.Day).ToList();

            summary.CheckinsToday = results.Count(x => x.CreationDate.Date == DateTime.Today);
            Trace.WriteLine("GetChangeset: " + watch.Elapsed);
        }

        private void GetBuildInformation(TfsDashboardSummary summary)
        {
            var watch = Stopwatch.StartNew();
            _projectCollection.EnsureAuthenticated();

            var buildServer = _projectCollection.GetService<IBuildServer>();
            var buildDetailSpec = buildServer.CreateBuildDetailSpec(_settings.Project, _settings.BuildDefinition);
            buildDetailSpec.MaxBuildsPerDefinition = 150;
            buildDetailSpec.QueryOptions = QueryOptions.All;
            buildDetailSpec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
            buildDetailSpec.InformationTypes = null;

            var results = buildServer.QueryBuilds(buildDetailSpec);
            if (!results.Failures.Any())
            {
                var builds = results.Builds.Select(buildDetail => new TfsBuildSummary
                {
                    Uri = buildDetail.Uri,
                    Duration = (int)(buildDetail.FinishTime - buildDetail.StartTime).TotalSeconds,
                    Who = buildDetail.Requests[0].RequestedFor,
                    Username = buildDetail.Requests[0].RequestedForDisplayName,
                    BuildNumber = buildDetail.BuildNumber,
                    DropLocation = buildDetail.DropLocation,
                    StartTime = buildDetail.StartTime,
                    TestStatus = buildDetail.TestStatus,
                    CompilationStatus = buildDetail.CompilationStatus.ToString(),
                    Status = buildDetail.Status.ToString(),
                    SourceGetVersion = buildDetail.SourceGetVersion,
                }).ToList();
                summary.LastBuilds = builds.OrderBy(x => x.StartTime);

                var lastBuild = builds.OrderByDescending(x => x.StartTime).FirstOrDefault();
                if (lastBuild != null)
                {
                    lastBuild.TestCoverage = GetTestcoverage(_projectCollection, lastBuild.Uri);
                    summary.LastBuild = lastBuild;
                    summary.LastWarningCount = InformationNodeConverters.GetBuildWarnings(results.Builds.OrderByDescending(x => x.StartTime).FirstOrDefault()).Count;
                }

            }
            Trace.WriteLine("GetBuildInformation: " + watch.Elapsed);
        }

        private dynamic GetTestcoverage(TfsTeamProjectCollection projectCollection, Uri uri)
        {
            var watch = Stopwatch.StartNew();
            projectCollection.EnsureAuthenticated();
            var tcm = projectCollection.GetService<ITestManagementService>();
            var testManagementTeamProject = tcm.GetTeamProject(_settings.Project);
            var runs = testManagementTeamProject.TestRuns.ByBuild(uri);

            var result = new
            {
                Statistics = runs.Select(x => new
                {
                    x.Statistics.TotalTests,
                    x.Statistics.FailedTests,
                    x.Statistics.PassedTests
                }).ToList()
            };
            Trace.WriteLine("GetTestCoverage: " + watch.Elapsed);
            return result;
        }

        public TfsDashboardSummary CreateSummary()
        {
            var summary = new TfsDashboardSummary();
            GetBuildInformation(summary);
            GetChangeset(summary);
            return summary;
        }
    }
}