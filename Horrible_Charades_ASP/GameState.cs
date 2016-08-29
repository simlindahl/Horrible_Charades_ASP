﻿using Horrible_Charades_ASP.Database;
using Horrible_Charades_ASP.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Horrible_Charades_ASP
{
    public class GameState
    {
        private static readonly Lazy<GameState> _instance = new Lazy<GameState>(            //Lazy = Skapas inte förän klassen accessas
            () => new GameState(GlobalHost.ConnectionManager.GetHubContext<GameHub>()));    //Skickar in vår Gamehub till konstruktorn

        private readonly ConcurrentDictionary<string, Team> _teams =
            new ConcurrentDictionary<string, Team>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, Game> _games =
            new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);

        public IHubConnectionContext<dynamic> Clients { get; set; }         //Todo: Funkar detta?
        public IGroupManager Groups { get; set; }                           //Används för att hålla koll på SignalR grupper

        DatabaseUtils _dbUtils = new DatabaseUtils(new CharadeContext());

        public GameState(IHubContext context)
        {
            Clients = context.Clients;
            Groups = context.Groups;
        }

        public static GameState Instance
        {
            get { return _instance.Value; }
        }
        /// <summary>
        /// Checks if a game with the speicific gamecode already has a team with the requested name
        /// </summary>
        /// <param name="teamName"></param>
        /// <returns></returns>
        internal Team GetTeam(string teamName, string gameCode)
        {
            return _teams.Values.FirstOrDefault(t => t.Name == teamName && t.GameCode == gameCode);
        }
        /// <summary>
        /// Skapar ett team som kopplas till ConnectionId
        /// </summary>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public Game CreateTeam(string teamName, string gameCode, string conId) //Todo: koppla till connectionstring istället för Teamname
        {
            Game game = GetGame(gameCode);
            Team team = new Team(teamName);
            team.ConnectionID = conId;
            team.GameCode = gameCode;

            _teams[team.Name] = team;//Todo: Fundera på vad vi ska koppla Team till, GetMD5Hash för att göra en safe connectionId
            game.Teams.Add(team);
            _games[game.GameCode] = game;

            Task add = Groups.Add(team.ConnectionID, team.GameCode);
            add.Wait();
            return game; //Todo: Ta in gameCode. Lägga till laget i game och i en grupp
        }

        internal void AssignWhosTurn(Game game)
        {
            if (game.Turn == 0)
            {
                game.TurnOrder = game.Teams.OrderBy(t => RandomUtils.rnd.Next()).Select(o => o.Id).ToArray();
            }

            foreach (Team team in game.Teams)
            {
                if (team.Id == game.TurnOrder[game.Turn])
                {
                    game.WhosTurn = team;
                }
            }
        }

        internal Game GetGame(string gameCode)
        {
            var game = _games.FirstOrDefault(g => g.Key == gameCode);
            return game.Value;//.Values.FirstOrDefault(t => t.Name == teamName);
        }
        /// <summary>
        /// Skapar ett team som kopplas till ConnectionId
        /// </summary>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public Game CreateGame() //Todo: koppla till connectionstring istället för Teamname
        {
            Game game = new Game();
            _games[game.GameCode] = game; //Todo: Fundera på vad vi ska koppla Team till, GetMD5Hash för att göra en safe connectionId
            return game;
        }

        // Todo: Se över hur vi ska hämta ut och lämna över listorna med felaktiga gissningar
        internal Game GetNoun(string gameCode)
        {
            Word noun = _dbUtils.GetNoun();
            //List<string> tmpList = _dbUtils.GetIncorrectAnswers(noun);
            Game game = GetGame(gameCode);
            game.CurrentCharade.Noun = noun.Description;

            return game;
        }

        internal Game GetModifier(string gameCode, int categoryID)
        {
            Game game = GetGame(gameCode);
            if (categoryID == 1)
            {
                List<RuleChanger> modifier = _dbUtils.GetAllModifiers(1);
               
                game.FunkUps.Add(modifier[0]);
                return game;
            }
            else
            {
                List<RuleChanger> modifier = _dbUtils.GetAllModifiers(2);
                game.PowerUps.Add(modifier[0]);
                return game;
            }

        }
        internal Game GetAdjective(string gameCode)
        {
            var adjective = _dbUtils.GetAdjective();
            //List<string> tmpList = _dbUtils.GetIncorrectAnswers(adjective);
            Game game = GetGame(gameCode);
            game.CurrentCharade.Adjective.Add(_dbUtils.GetAdjective().Description);
            return game;
        }

        internal Game GetVerb(string gameCode)
        {
            //var verb = _dbUtils.GetVerb();
            Game game = GetGame(gameCode);
            game.CurrentCharade.Verb.Add(_dbUtils.GetVerb().Description);
            //List<string> tmpList = _dbUtils.GetIncorrectAnswers(verb);

            return game;
        }
    }
}