﻿(function () {
    "use strict";

    //Obtains the existing module
    angular.module("mainContent")
        .controller("contentController", contentController);

    function contentController(gameService, signalRService, $interval) {
        var vm = this;
        var hub = $.connection.gameHub; //Saves connection in "hub"-variable

        // Updates contentController to fit the locally persisted data in gameService. 
        vm.gameData = gameService.game;

        vm.timeLeft = 10;

        //Starts timer on CharadeActor
        vm.startTimer = function () {
            $interval(function () {
                vm.timeLeft--;
                if (vm.timeLeft <= 0) {
                    vm.pointCounter(0);

                }
            }, 1000);
        };

        //Calls CreateGame function on Server-Side when CreateTeamHost is loaded
        vm.createGame = function () {
            hub.server.createGame();
        };

        //Calls JoinGame function on Server-Side when a teamName and GameCode is submitted in CreateTeamGuest
        vm.joinGame = function () {
            hub.server.joinGame($("#GameCode").val(), $("#TeamName").val());
        };

        //Calls StartCharade on Server-Side when a the host presses start
        vm.startCharade = function () {
            hub.server.startCharade(vm.gameData.GameCode);
        };
        //Calls CreateTeam function on Server-Side when a teamName in CreateTeamHost is submitted
        vm.createTeam = function () {
            gameService.game.GameCode = $("#GameCode").text();
            hub.server.createTeam(gameService.game.GameCode, $("#TeamName").val());
        };

        //Redirects to nextView
        vm.redirectFromWaitingRoom = function () {
            console.log("Redirecting to PreCharade");
            hub.server.redirectFromWaitingRoom(gameService.game.GameCode);
        };

        //Calls GetCharade function on Server-Side when PreCharadeActor is loaded
        vm.getNoun = function () {
            console.log("initiating getNoun");
            hub.server.getNoun(gameService.game.GameCode);
        };

        //Calls UpdateCharade function on Server-Side when "Get Adjective"-button is pressed
        vm.getAdjective = function () {
            console.log("initiating getAdjective");
            hub.server.updateCharade("adjective", gameService.game.GameCode);
        };

        //Calls UpdateCharade function on Server-Side when "Get Verb"-button is pressed
        vm.getVerb = function () {
            console.log("initiating getVerb");
            hub.server.updateCharade("verb", gameService.game.GameCode);
        };

        vm.getRuleChanger = function () {
            console.log("initiating getRuleChanger");
            hub.server.getRuleChanger(gameService.game.GameCode);
        };

        vm.getIncorrectAnswers = function () {
            hub.server.getIncorrectAnswers(gameService.game.GameCode);
        };

        vm.pointCounter = function (timeLeft) {
            console.log("inside Pointcounter");
            console.log(timeLeft);
            hub.server.pointCounter(gameService.game.GameCode, timeLeft);
        }
    }
})();