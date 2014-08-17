var app = angular.module('TfsDashboardApp', []);

function getBuildColorClass(lastBuild) {
    if (lastBuild != undefined) {
        switch (lastBuild.Status) {
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
    }
    return 'unkownBuild';
}

app.filter("timeago", function () {
    //time: the time
    //local: compared to what time? default: now
    //raw: wheter you want in a format of "5 minutes ago", or "5 minutes"
    return function (time, local, raw) {
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

function calculateDiagrams($scope) {


    $.each($scope.Data, function (idx, summary) {

        var maxDuration;
        $.each(summary.LastBuilds, function (idx, item) {
            if (item.Duration > maxDuration || maxDuration == undefined)
                maxDuration = item.Duration;
        });
        $.each(summary.LastBuilds, function (idx, item) {
            item.PercentageDurationLastBuilds = item.Duration / maxDuration;
            item.bgClass = getBuildColorClass(item);
        });

        var maxCheckins;
        $.each(summary.CheckinStatistic, function (idx, item) {
            if (item.Count > maxCheckins || maxCheckins == undefined)
                maxCheckins = item.Count;
        });
        $.each(summary.CheckinStatistic, function (idx, item) {
            item.PercentageCheckins = item.Count / maxCheckins;
        });


        summary.bgClass = getBuildColorClass(summary.LastBuild);
        summary.StatusText = getBuildText(summary.LastBuild);
    });

}

function getBuildText(lastBuild) {
    if (lastBuild != undefined) {
        switch (lastBuild.Status) {
            case "Failed":
                return 'Build fehlgeschlagen';
            case "InProgress":
                return 'In Bearbeitung';
            case "None":
                return 'None';
            case "NotStarted":
                return 'Noch nicht gestartet';
            case "PartiallySucceeded":
                return 'Teilweise erfolgreich';
            case "Stopped":
                return 'Buildvorgang abgebrochen';
            case "Succeeded":
                return 'Alles in Ordnung - Successfull';
        }
    }
    return "Unbekannt";
}

var currentIdx = 0;
function refresh($scope, $http) {
    $http({ method: 'GET', url: '/api/TfsDashboard' }).
            success(function (summaries, status, headers, config) {
                $scope.Data = summaries;

                $scope.SelectedTfs = summaries[currentIdx];

                $scope.selectSummary = function (idx) {
                    currentIdx = idx;
                    $scope.SelectedTfs = summaries[currentIdx];
                    calculateDiagrams($scope);
                };

                calculateDiagrams($scope);

                $("#loader").hide();
                $("#wrapper").show();
            });
}

app.controller('GreetingController', ['$scope', '$http', '$interval',
    function ($scope, $http, $interval) {
        refresh($scope, $http);
        $interval(function () { refresh($scope, $http); }, 5000);
        $interval(function () { document.location.reload(true); }, 1000 * 60 * 60);
    }
]);

