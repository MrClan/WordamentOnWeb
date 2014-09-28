var timerId = null;
function CountDown(time, callback) {
    if (time < 0) {
        clearTimeout(timerId);
        if (callback) {
            callback();
        }
        return;
    }
    else {
        var mins = Math.floor(time / 60);
        if (mins < 10)
            mins = '0' + mins;
        var secs = Math.floor(time % 60);
        if (secs < 10)
            secs = '0' + secs;
        document.getElementById('txt').innerHTML = mins + ":" + secs;
        timerId = setTimeout(function () { CountDown(time - 1,callback) }, 1000);
    }
}

function StopCountDown()
{
    clearTimeout(timerId);
    document.getElementById('txt').innerHTML = "00:00";
}

function InitTimer(time) {
    var $example = $(".example--night"),
    $ceMinutes = $example.find('.ce-minutes'),
    $ceSeconds = $example.find('.ce-seconds'),
    now = new Date(),
    then = new Date(now.getTime() + (time * 1000));

    $example.find(".countdown").countEverest({
        second: (then.getSeconds()),
        minute: then.getMinutes(),
        hour: then.getHours(),
        day: then.getDate(),
        month: (then.getMonth() + 1),
        year: then.getFullYear(),
        onComplete: function () {
            $('#sPlayerScore').append("TIME UP...pls wait while we fetch results... DISABLE THE GRID, and WAIT 30 SECONDS");
            $.get('/Home/GetSolution').success(function (results) {
                //alert(results);
                //debugger;
                if (results.isResultsFine) {
                    for (x = 0; x < results.solution.length; x++) {
                        $('div#AllWords').append('<b>' + results.solution[x] + '</b><br/>');
                    }
                    setTimeout(function () {
                        DrawGrid(results.Grid);
                        StartClock(results.Life);
                    }, 30000);
                    $('#txt').text('Refresh the page to start next round');
                }
            });
        }
    });
}