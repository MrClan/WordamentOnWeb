/// <reference path="timer.js" />
$().ready(function () {
    var audio = new Audio("Scripts/d.mp3");
    var moveShapeHub = $.connection.moveShapeHub;
    var scored = false;    
    moveShapeHub.client.updateScore = function (model) {
        //alert(model.Name);
        //$('#divPlayerInfo').html($('#divPlayerInfo').html() + "<br/>Name: <b>" + model.Name + "</b> Score: <i>" + model.Score + "</i>");
        //if (model.Name.length > 0) {
        $('#trp' + model.Name).remove();
        $('#tPlayerInfo').append("<tr id='trp" + model.Name + "'><td><br/>Name: <b>" + model.Name + "</b> Score: <i>" + model.Score + "</i></td></tr>");
        //}
    };
    $.connection.hub.start().done(function () {
        console.log('live update mode ready');
    });
    console.log(dict.wordlist.length);


    function DrawGrid(grid)
    {
        $('div#AllWords').html(''); // clear results
        $('#hdnGridId').val(grid.GUID);
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


    function InitTimer(time)
    {
        var $example = $(".example--night"),
		$ceMinutes = $example.find('.ce-minutes'),
		$ceSeconds = $example.find('.ce-seconds'),
		now = new Date(),
		then = new Date(now.getTime() + (time*1000));

        $example.find(".countdown").countEverest({
            second: (then.getSeconds()),
            minute: then.getMinutes(),
            hour: then.getHours(),
            day: then.getDate(),
            month: (then.getMonth() + 1),
            year: then.getFullYear(),
            onComplete: function () {
                alert("TIME UP...pls wait while we fetch results");
                $.get('/Home/GetSolution/' + $('#hdnGridId').val()).success(function (results) {
                    //alert(results);
                    //debugger;
                    
                    for (x = 0; x < results.solution.length; x++)
                    {
                        $('div#AllWords').append('<b>'+ results.solution[x] + '</b><br/>');
                    }
                    //setTimeout(function () {
                    //    alert('drawing new grid');
                    //    DrawGrid(results.Grid);
                    //    InitTimer(results.Life);
                    //}, 40000);
                });
            }
        });
    }

    var allChars = ['A', 'A', 'B', 'C', 'D', 'E', 'E', 'F', 'I', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'M', 'N', 'O', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];
    var displayedChars = [];

    var filledBox = [];
    $.get('/Home/GetGrid').success(function (data) {
        DrawGrid(data.Grid);

        // Initialize countdown timer
        //alert((data.LifeTimeLeft));
        InitTimer(data.Life);
        
    });

    //alert(allChars.length);
    //for (i = 0; i < 16; i++) {
    //    var rndIndex = Math.floor(Math.random() * 16) + 1;
    //    while (_.contains(filledBox, rndIndex)) {
    //        rndIndex = Math.floor(Math.random() * 16) + 1;
    //    }
    //    filledBox[i] = rndIndex;
    //    var rndChar = Math.floor(Math.random() * 32);
    //    //console.info('@ '+ rndChar + ' ' + allChars[rndChar]);
    //    var temp = $('#d' + rndIndex);
    //    var orderString = '<sub>' + $(temp).attr('order') + '</sub>';
    //    temp.append('<span class="text">' + allChars[rndChar] + '</span>');
    //}

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

    $('span.text').bind('mouseover', function () {
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
    }).bind('mouseout', function () {
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
	.bind('mousedown', function () {
	    selectedGrids.push($(this).parent());
	    selectedValue += $(this).text();
	    $(this).parent().addClass('mousedown');
	});

    $('div.box').bind('mouseover', function () {
        if (mousedown) {
            if ($('#cbHighlightGrids').is(':checked')) {
                ShowConnectedGrid($(this));
            }
        }
    }).bind('mouseout', function () {
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

    var curWordScore = 0;
    var playerScore = 0;
    CheckValidWord = function (item) {
        if (_.contains(dict.wordlist, item)) {
            console.info(item.toLowerCase() + ' is a valid word');
            // check if this word has already been made
            var rowToHighlight = $('#tr' + item, $('#tMadeWords'));
            if (rowToHighlight.length == 0) {
                $('#tMadeWords').append('<tr id="tr' + item + '"><td>' + item + '</td></tr>');
                $(selectedGrids).each(function () {
                    curWordScore += parseInt($('.score', $(this)).text());
                });
                playerScore += curWordScore;
                $('#sPlayerScore').text(playerScore);
                moveShapeHub.server.updateScore({ Name: playerName, Score: playerScore });
                audio.play();
            }
            else {
                $(rowToHighlight).addClass('mousedown');
                setTimeout(function () { $(rowToHighlight).removeClass('mousedown') },1000);
            }
        }
        curWordScore = 0;
    };
});