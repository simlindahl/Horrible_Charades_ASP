﻿(function () {
    "use strict";

    //Obtains the existing module
    angular.module("mainContent")
        .controller("contentController", contentController);

    function contentController(gameService, signalRService) {
        var vm = this;
        var hub = $.connection.gameHub; //Saves connection in "hub"-variable
        vm.gameData = gameService.game;

        timeLeft = 60;
        counter = setInterval(timer, 1000);
        function timer() {
            timeLeft -= 1;
            if (timeLeft <= 0) {
                clearInterval(counter);
                document.getElementById("timer").innerHTML = "Time is up!" //Här ska vi skicka vidare användaren till nästa view
                window.location.href = '/Play/Score';
                return;
            }
            document.getElementById("timer").innerHTML = "00:" + timeLeft < 10 ? "0" + timeLeft : timeLeft;
        }
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
        vm.redirectToView = function () {
            hub.server.redirectFromWaitingRoom(gameService.game.GameCode);
        }

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

        vm.getModifier = function () {
            console.log("initiating getModifier");
            hub.server.getModifier(gameService.game.GameCode, 1);
        };

    }
})();