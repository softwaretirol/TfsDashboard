var app = angular.module('TfsDashboardApp', []);

function getBuildColorClass(status) {
    switch (status) {
        case "Failed":
            return 'failedBuild';
        case "InProgress":
            return 'unkownBuild';
        case "None":
            return 'unkownBuild';
        case "NotStarted":
            return 'unkownBuild';
        case "PartiallySucceeded":
            return 'partiallyBuild';
        case "Stopped":
            return 'failedBuild';
        case "Succeeded":
            return 'successBuild';
    }
    return undefined;
}

app.filter("timeago", function() {
    //time: the time
    //local: compared to what time? default: now
    //raw: wheter you want in a format of "5 minutes ago", or "5 minutes"
    return function(time, local, raw) {
        if (!time) return "never";

        if (!local) {
            (local = Date.now())
        }

        if (angular.isDate(time)) {
            time = time.getTime();
        } else if (typeof time === "string") {
            time = new Date(time).getTime();
        }

        if (angular.isDate(local)) {
            local = local.getTime();
        } else if (typeof local === "string") {
            local = new Date(local).getTime();
        }

        if (typeof time !== 'number' || typeof local !== 'number') {
            return;
        }

        var
            offset = Math.abs((local - time) / 1000),
            span = [],
            MINUTE = 60,
            HOUR = 3600,
            DAY = 86400,
            WEEK = 604800,
            MONTH = 2629744,
            YEAR = 31556926,
            DECADE = 315569260;

        if (offset <= MINUTE) span = ['', raw ? 'now' : 'weniger als einer Minute'];
        else if (offset < (MINUTE * 60)) span = [Math.round(Math.abs(offset / MINUTE)), 'Minuten'];
        else if (offset < (HOUR * 24)) span = [Math.round(Math.abs(offset / HOUR)), 'Stunden'];
        else if (offset < (DAY * 7)) span = [Math.round(Math.abs(offset / DAY)), 'Tage'];
        else if (offset < (WEEK * 52)) span = [Math.round(Math.abs(offset / WEEK)), 'Wochen'];
        else if (offset < (YEAR * 10)) span = [Math.round(Math.abs(offset / YEAR)), 'Jahren'];
        else span = ['', 'a long time'];

        span = span.join(' ');

        if (raw === true) {
            return span;
        }
        return (time <= local) ? span : 'in ' + span;
    }
});

app.directive('timeago', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            attrs.$observe("timeago", function () {
                element.text(moment(attrs.timeago).fromNow());
            });
        }
    };
});

function refresh($scope, $http) {
    $http({ method: 'GET', url: '/api/TfsDashboard' }).
            success(function (data, status, headers, config) {
                $scope.Data = data;

                var maxDuration;
                $.each(data.LastBuilds, function(idx, item) {
                    if (item.Duration > maxDuration || maxDuration == undefined)
                        maxDuration = item.Duration;
                });
                $.each(data.LastBuilds, function (idx, item) {
                    item.PercentageDurationLastBuilds = item.Duration / maxDuration;
                    item.bgClass = getBuildColorClass(item.Status);
                });
                data.LastBuilds.reverse();

                var maxCheckins;
                $.each(data.CheckinStatistic, function (idx, item) {
                    if (item.Count > maxCheckins || maxCheckins == undefined)
                        maxCheckins = item.Count;
                });
                $.each(data.CheckinStatistic, function (idx, item) {
                    item.PercentageCheckins = item.Count / maxCheckins;
                });
                data.CheckinStatistic.reverse();

                $scope.bgClass = getBuildColorClass(data.LastBuild.Status);
                switch (data.LastBuild.Status) {
                    case "Failed":
                        $scope.StatusText = 'Build fehlgeschlagen';
                        break;
                    case "InProgress":
                        $scope.StatusText = 'In Bearbeitung';
                        break;
                    case "None":
                        $scope.StatusText = 'None';
                        break;
                    case "NotStarted":
                        $scope.StatusText = 'Noch nicht gestartet';
                        break;
                    case "PartiallySucceeded":
                        $scope.StatusText = 'Teilweise erfolgreich';
                        break;
                    case "Stopped":
                        $scope.StatusText = 'Buildvorgang abgebrochen';
                        break;
                    case "Succeeded":
                        $scope.StatusText = 'Alles in Ordnung - Successfull';
                        break;
                }

                $("#loader").hide();
                $("#wrapper").show();
            }).
            error(function (data, status, headers, config) {

            });
}

app.controller('GreetingController', ['$scope', '$http', '$interval',
    function ($scope, $http, $interval) {
        refresh($scope, $http);
        $interval(function () { refresh($scope, $http); }, 5000);
        $interval(function () { document.location.reload(true); }, 1000 * 60 * 60);
    }
]);

