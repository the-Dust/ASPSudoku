using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Services.SudokuGame;
using Services.SudokuGame.Base;

namespace ASPSudoku.Controllers
{
    public class HomeController : Controller
    {
        ISudoku game = null;

        public HomeController(ISudoku game)
        {
            this.game = game;
        }

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult NewGame()
        {
            var array = game.NewGame(Request, Response);

            ViewBag.Level = $"Текущий вариант игры: {game.CurrentLevel}";

            return PartialView("_GamePartial", array);
        }

        public PartialViewResult CheckGame(SudokuCell[] array)
        {
            bool failed = game.CheckSolution(array, Request);

            string result = failed ? "В решении есть ошибки" : 
                "Поздравляю вас! Вы правильно решили Судоку.";

            if (!failed)
                NewGame();

            return PartialView("_ResultPartial", result);
        }

        public int OpenCell(string cellName)
        {
            return game.OpenCell(cellName, Request);
        }
    }
}