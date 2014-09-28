$().ready(function () {
    // StartClock(10);
    var audio = new Audio("Scripts/d.mp3");
    var moveShapeHub = $.connection.moveShapeHub;
    var scored = false;
    moveShapeHub.client.updateScore = function (model) {
        $('#trp' + model.Name).remove();
        $('#tPlayerInfo tbody').append("<tr id='trp" + model.Name + "'><td><br/><b>" + model.Name + " :</b> " + model.Score + "</td></tr>");
    };
    var isGameOn = false;
    var isResultsDisplayed = false;
    moveShapeHub.client.StartNewGame = function (model) {
        StartNewGame(model.Grid, model.Grid.LifeLeft, model.TotalWords);
    };
    moveShapeHub.client.DisableGamePlay = function (model) {
        DisableGamePlay(model.Solution, (30 - model.Status.TimeLeftInSeconds));
    }

    function StartNewGame(grid, life, totalWordCount) {
        alert('starting new game');
        wordCount = 0;
        playerScore = 0;
        isResultsDisplayed = false;
        if (!isGameOn) {
            DrawGrid(grid);
            StartClock(life);
            $('#sTotWords').text(' / ' + totalWordCount);
            isGameOn = true;
        }
    }

    function DisableGamePlay(solution, life) {
        isGameOn = false;
        if (!isResultsDisplayed) {
            DisplayResults(solution);
            StopCountDown();
            $('div#txt').html('<b> PLEASE WAIT ' + life + ' seconds.');
            isResultsDisplayed = true;
        }
    }


    $.connection.hub.start().done(function () {
        console.log('live update mode ready');
    });

    //console.log(dict.wordlist.length);


    function DrawGrid(grid) {
        $('div#AllWords').html(''); // clear results
        $('#tMadeWords tbody').html('');
        //$('#hdnGridId').val(grid.GUID);
        for (i = 0; i < 16; i++) {
            var ct = grid.Tiles[i];
            console.log('i: ' + i);
            console.log(ct);
            var temp = $('#d' + i);
            temp.html('');
            var scoreString = '<sup class="score">' + ct.Weight + '</sup>';
            temp.append('<span class="text">' + ct.Letter + '</span>' + scoreString);
        }
    }

    function DisplayResults(solution) {
        if (solution) {
            for (x = 0; x < solution.length; x++) {
                $('div#AllWords').append('<b>' + solution[x] + '</b><br/>');
            }
        }
    }

    function TimeCompleteCallback() {
        $('#sPlayerScore').append("TIME UP...pls wait while we fetch results... DISABLE THE GRID, and WAIT 30 SECONDS");
        $.get('/Home/GetSolution').success(function (results) {
            if (results.isResultsFine) {
                DisplayResults(results.solution);
                $('#txt').text('Refresh the page to start next round');
            }
        });
    }

    function StartClock(time) {
        CountDown(time);
    }

    $.get('/Home/FirstLoad').success(function (data) {
        console.log(data.Status.Status);
        if (data.Status.Status == "0")// status is GAMEON
        {
            StartNewGame(data.Grid, data.Grid.LifeLeft, data.TotalWords);
        }
        else {
            DisableGamePlay(data.Solution, (30 - data.Status.TimeLeftInSeconds));
        }
    });
    //$.get('/Home/GetGrid').success(function (data) {
    //    console.log(data.GameStatus);
    //    if (data.GameStatus != 'GAMEON') {
    //        console.log('PLEASE WAIT WHILE THE GAME BOOTS UP...');
    //        return;
    //    }
    //    else {
    //        DrawGrid(data.Grid);
    //    }

    //    // Initialize countdown timer
    //    //StartClock(data.Life);
    //});

    var playerName = prompt("Enter your name please ?");

    var selectedGrids = [];
    var selectedValue = '';
    var firstGrid = null;
    var pathTrack = {};
    var prevGrid = '';
    var curGrid = '';

    var mousedown = false;
    $(document).on('mousedown', function () {
        mousedown = true;
    }).on('mouseup', function () {
        mousedown = false;
        //console.info(selectedGrids.join(''));
        if (selectedValue.length > 2) {
            CheckValidWord(selectedValue);
        }
        selectedGrids = []; firstGrid = null; selectedValue = ''; pathTrack = {}; prevGrid = '';
        $('div.box').removeClass('mousedown').removeClass('conDiv');
    });

    $('div.box').on('mouseover', 'span.text', function () {
        var cId = $(this).parent().attr('id');

        if (_.isEmpty(selectedGrids)) {
            firstGrid = cId;
            if (prevGrid.length == 0) {
                prevGrid = cId;
            }
        }
        //console.info('mousedown');
        if (mousedown && cId != firstGrid) {
            curGrid = cId;
            pathTrack[cId] = prevGrid;
            if (!_.contains(selectedGrids, cId) /*&& IsConnectedGrid(cId)*/) {
                pathTrack[cId] = prevGrid;
                selectedGrids.push($('div#' + cId));
                selectedValue += $(this).text();
                $(this).parent().addClass('mousedown');
            }
        }
    }).on('mouseout', 'span.text', function () {
        //console.info('mousedown');
        var cId = $(this).parent().attr('id');
        prevGrid = cId;
        if (mousedown && cId != firstGrid) {
            //console.log(prevGrid);
            if (!_.contains(selectedGrids, cId) && pathTrack[prevGrid] == curGrid) {
                selectedGrids.pop(cId);
                //selectedValue += $(this).text();
                $(this).parent().removeClass('mousedown');
            }
        }
    })
	.on('mousedown', 'span.text', function () {
	    selectedGrids.push($(this).parent());
	    selectedValue += $(this).text();
	    $(this).parent().addClass('mousedown');
	});

    $('div.box').on('mouseover', function () {
        if (mousedown) {
            if ($('#cbHighlightGrids').is(':checked')) {
                ShowConnectedGrid($(this));
            }
        }
    }).on('mouseout', function () {
        $('div.box').removeClass('conDiv');
    });


    function ShowConnectedGrid(currentGrid) {
        _this = $(currentGrid);
        var curOrder = _this.attr('order');
        $('div.box').each(function () {
            if (IsConnectedGrid(_this.attr('order'), $(this).attr('order'))) {
                if (!$(this).hasClass('mousedown')) {
                    $(this).addClass('conDiv');
                }
            }
        });
    }

    function IsConnectedGrid(curOrder, testOrder) {
        var curX = parseInt(curOrder[0]);
        var curY = parseInt(curOrder[1]);
        var testX = parseInt(testOrder[0]);
        var testY = parseInt(testOrder[1]);
        var validX = (Math.abs(testX - curX) <= 1);
        var validY = (Math.abs(testY - curY) <= 1);
        //console.log('xValid: ' + validX + 'yValid: ' + validY);
        if (validX && validY) { return true; }
        //console.log('curX, curY = ' + curX + ', ' + curY);
    }

    var wordCount = 0;
    var curWordScore = 0;
    var playerScore = 0;
    CheckValidWord = function (item) {
        if (_.contains(dict.wordlist, item)) {
            console.info(item.toLowerCase() + ' is a valid word');
            // check if this word has already been made
            var rowToHighlight = $('#tr' + item, $('#tMadeWords'));
            if (rowToHighlight.length == 0) {
                $('#tMadeWords tbody').append('<tr id="tr' + item + '"><td>' + item + '</td></tr>');
                $(selectedGrids).each(function () {
                    curWordScore += parseInt($('.score', $(this)).text());
                });
                playerScore += curWordScore;
                wordCount++;
                $('#sPlayerScore').text(playerScore);
                $('#sWordCount').text(wordCount);
                moveShapeHub.server.updateScore({ Name: playerName, Score: playerScore });
                audio.play();
            }
            else {
                $(rowToHighlight).addClass('mousedown');
                setTimeout(function () { $(rowToHighlight).removeClass('mousedown') }, 1000);
            }
        }
        curWordScore = 0;
    };
});