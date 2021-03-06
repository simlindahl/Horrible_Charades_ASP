﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Horrible_Charades_ASP.Database;
using Horrible_Charades_ASP.Models;
using System.Threading;

namespace Horrible_Charades_ASP
{
    public class GameHub : Hub
    {

        /// <summary>
        /// This function calls the method Hello on Client-Side, with something to write out
        /// </summary>
        /// <param name="textToWrite"></param>
        public void Hello(string textToWrite)
        {
            Clients.All.hello(textToWrite);
        }

        /// <summary>
        /// This function is called when a user clicks "New Game"
        /// </summary>
        public void CreateGame()
        {
            Game game = GameState.Instance.CreateGame();
            Clients.Caller.printGameCode(game); //Todo: skapa printGameCode på klientsidan
        }
        /// <summary>
        /// This function calls the method InsertCharadeHTML on Client-Side, which inserts provided string in a <div>-tag
        /// </summary>
        /// <param name="htmlToWrite"></param>
        public void InsertCharadeHTML(string htmlToWrite)
        {
            Clients.All.insertCharadeHTML(htmlToWrite);
        }

        /// <summary>
        /// Creates a new team if the device don't have a team. 
        /// </summary>
        /// <param name="teamName"></param>
        public void CreateTeam(string gameCode, string teamName) //To-do: validera team-name
        {
            Team team = GameState.Instance.GetTeam(teamName, gameCode);
            if (team != null)
            {
                Clients.Caller.displayMessage("There is already a team in this game with that name");
            }
            else
            {
                Game game = GameState.Instance.CreateTeam(teamName, gameCode, Context.ConnectionId);
                if (game.Teams.Count == 1)
                {

                    Clients.Caller.setTeamName(teamName);
                    Clients.Caller.redirectToView(game, "/#/LobbyHost");
                }
                else
                {
                    Clients.Group(game.GameCode).pushToTeamList(teamName);
                    Clients.Caller.setTeamName(teamName);
                    Clients.Caller.redirectToView(game, "/#/LobbyGuest");
                }
            }
        }

        /// <summary>
        /// Takes in a gameCode and TeamName from a joining team, looks for a Game with matching gameCode and adds the team into the game.
        /// </summary>
        /// <param name="gameCode"></param>
        /// <param name="teamName"></param>
        public void JoinGame(string gameCode, string teamName)
        {
            Game game = GameState.Instance.GetGame(gameCode);
            if (game == null)
                Clients.Caller.DisplayMessage("No such game exist. Revise your GameCode");
            else
                CreateTeam(gameCode, teamName);
        }

        /// <summary>
        /// Serverside activation of Getting RuleChangers. Called when leaving Waiting room.
        /// </summary>
        /// <param name="gameCode"></param>
        public void GetRuleChanger(string gameCode)
        {
            Game game = GameState.Instance.GiveAllTeamsRuleChanger(Context.ConnectionId, gameCode);
            StartCharade(game);
        }

        /// <summary>
        /// Shuffles and assigns which Team is going to do the charade and redirects the client
        /// </summary>
        /// <param name="gameCode"></param>
        public void StartCharade(Game game)
        {
            Clients.Client(game.WhosTurn.ConnectionID).redirectToView(game, "/#/WaitingRoomActor");
            Clients.Group(game.GameCode, game.WhosTurn.ConnectionID).redirectToView(game, "/#/WaitingRoomOpponent");

        }

        /// <summary>
        /// Redirects the client to PreCharadeActor and PreCharadeParticipant
        /// </summary>
        /// <param name="gameCode"></param>
        /// <param name="teamName"></param>
        /// 


        public void RedirectToPreCharade(string gameCode, string teamName)
        {
            Game game = GameState.Instance.GetGame(gameCode);
            Team myTeam = game.Teams.FirstOrDefault(t => t.Name == teamName);
            game.GameState = 4;

            //if (myTeam.ConnectionID == game.WhosTurn.ConnectionID)
            //{
            //    Clients.Caller.redirectToView("/#/PreCharadeActor");
            //    Clients.OthersInGroup(gameCode).redirectToView("/#/CharadeParticipant");
            //}
            //else
            //{
            Clients.Client(game.WhosTurn.ConnectionID).redirectToView(game, "/#/PreCharadeActor");
            Clients.Group(gameCode, game.WhosTurn.ConnectionID).redirectToView(game, "/#/PreCharadeParticipant");
            //}
        }

        /// <summary>
        /// Updates the current charade serverSide with either adjective or verb.
        /// Called upon when charade:Opponenet uses a the respective FunkUp
        /// </summary>
        /// <param name="typeOfWord"></param>
        /// <param name="gameCode"></param>
        public void UpdateCharade(string typeOfWord, string gameCode)
        {
            if (typeOfWord == "adjective")
            {
                Game game = GameState.Instance.GetAdjective(gameCode);
                Clients.Group(game.GameCode).InsertCharadeHTML(game, "adjective");
                Clients.Group(game.GameCode).resetTimer(10);
            }

            if (typeOfWord == "verb")
            {
                Game game = GameState.Instance.GetVerb(gameCode);
                Clients.Group(game.GameCode).InsertCharadeHTML(game, "verb");
                Clients.Group(game.GameCode).resetTimer(10);
            }
        }

        /// <summary>
        /// Shuffles the active Charades. Activated when Charade:Actor uses the respectrive PowerUp
        /// </summary>
        /// <param name="gameCode"></param>
        public void ShuffleCharade(string gameCode)
        {
            Game game = GameState.Instance.ShuffleCharade(gameCode);
            Clients.Caller.InsertCharadeHTML(game, "noun");
            Clients.Caller.InsertCharadeHTML(game, "adjective");
            Clients.Caller.InsertCharadeHTML(game, "verb");
            Clients.Group(gameCode).resetTimer();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameCode"></param>
        /// <param name="direction"></param>
        public void AffectCharadeTime(string gameCode, string direction)
        {
            Clients.Group(gameCode).debugMessage("AffectCharadeTime on serverSide");
            Clients.Group(gameCode).affectCharadeTime(direction);
            Clients.Group(gameCode).resetTimer(10);
        }


        public void RedirectToCharade(string gameCode, string teamName)
        {
            Game game = GameState.Instance.GetGame(gameCode);
            Team myTeam = game.Teams.FirstOrDefault(t => t.Name == teamName);
            game.GameState = 5;

            if (myTeam.ConnectionID == game.WhosTurn.ConnectionID)
            {
                Clients.Caller.redirectToView(game, "/#/CharadeActor");
            }
            else
            {
                Clients.Caller.redirectToView(game, "/#/CharadeParticipant");
            }
        }

        /// <summary>
        /// Gets a Noun from Database Table Nouns and Converts it into a Charade.
        ///  Pushes to Client-side
        /// </summary>
        public void GetNoun(string gameCode)
        {
            Game game = GameState.Instance.GetGame(gameCode);
            Clients.Group(gameCode).InsertCharadeHTML(game, "noun");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameCode"></param>
        public void GetIncorrectAnswers(string gameCode)
        {
            Game game = GameState.Instance.GetGame(gameCode);
            List<List<Word>> inCorrectAnswers = new List<List<Word>>();
            inCorrectAnswers = GameState.Instance.GetIncorrectAnswers(gameCode);
            Clients.Caller.DisplayAlternatives(inCorrectAnswers);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameCode"></param>
        /// <param name="timeLeft"></param>
        public void PointCounter(string gameCode, int timeLeft)
        {
            Game game = GameState.Instance.AssignPoints(gameCode, timeLeft, Context.ConnectionId);

            Clients.Group(game.GameCode).updateGameState(game);
            Clients.Group(game.GameCode).redirectToView(game, "/#/Score");
        }
        public void CalculateScoreP(string gameCode, int timeLeft, string guess)
        {
            Game game = GameState.Instance.AssignPoints(gameCode, timeLeft, Context.ConnectionId, guess);
        }

        public void RedirectToTotalScore(string gameCode)
        {
            Game game = GameState.Instance.GetGame(gameCode);
            game.GameState = 7;
            Clients.Group(gameCode).redirectToView(game, "/#/TotalScore");
        }

        public void StartNextCharade(string gameCode)
        {
            Game game = GameState.Instance.PrepareNewRound(gameCode, Context.ConnectionId);
            // TODO: Den knäppar ur totalt här! Behöver sitta ner ett par timmar och debugga, kanske bygga om "AssignWhosTurn"
            if (Context.ConnectionId == game.WhosTurn.ConnectionID)
            {
                game.Turn++;
                Clients.Group(gameCode).updateGameState(game);
                Clients.Caller.redirectToView(game, "/#/WaitingRoomActor");
            }
            else
            {
                Clients.Caller.redirectToView(game, "/#/WaitingRoomOpponent");
            }
        }
    }
}