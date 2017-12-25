using Services.SudokuGame.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services.SudokuGame
{
    public class SudokuCell
    {
        public int? Value { get; set; }

        public bool IsRed { get; set; }

        public bool Hidden { get; set; }
    }

    public class Sudoku : ISudoku
    {
        public int CurrentLevel { get { return levelPath[levelOfGame]; } private set { } }

        private static int[] levelPath;

        private SudokuCell[,] game = new SudokuCell[9, 9];

        private Action[] shakeStrategy = new Action[5];

        private int levelOfGame = 0;

        public Sudoku()
        {
            SetShaker();
        }

        static Sudoku()
        {
            GetNewLevelPath();
        }

        public SudokuCell[] NewGame(HttpRequestBase request, HttpResponseBase response)
        {
            HttpCookie cookieLevel = request.Cookies["SudokuLevel"];
            if (cookieLevel != null)
            {
                levelOfGame = int.Parse(cookieLevel.Value);
            }
            else
            {
                levelOfGame = 0;
            }

            LevelIncrease();

            SetVariantOfGame();

            return GetLineArray(request, response);
        }

        //We don't need to check sudoku rules here, because our reference array
        //was generated according sudoku rules 
        public bool CheckSolution(SudokuCell[] array, HttpRequestBase request)
        {
            bool failed = false;

            HttpCookie cookieGame = request.Cookies["SudokuGame"];

            if (cookieGame != null)
            {
                int[] reference = cookieGame.Value.Split(',').Select(int.Parse).ToArray();
                if (reference.Length != 81)
                    return true;
                for (int i = 0; i < 81; i++)
                {
                    if (array[i].Value != reference[i])
                        failed = true;
                }
            }
            else
                return true;

            return failed;
        }

        public int OpenCell(string cellName, HttpRequestBase request)
        {
            int cellNumber = 0;
                
            int.TryParse(cellName.Replace("].Value", "").TrimStart('['), out cellNumber);

            HttpCookie cookieGame = request.Cookies["SudokuGame"];

            int[] reference;

            if (cookieGame != null)
            {
                reference = cookieGame.Value.Split(',').Select(int.Parse).ToArray();
                if (reference.Length != 81)
                    return reference[0];
            }
            else
                return 0;

            return reference[cellNumber];
        }

        private SudokuCell[] GetLineArray(HttpRequestBase request, HttpResponseBase response)
        {
            SudokuCell[] array = GetLineArray();

            SetCookie(array, request, response);

            return array.Select(x => { if (x.Hidden) x.Value = null; return x; }).ToArray();
        }

        //Transforms 2d array into line array, because line array is easier to binding in MVC View
        private SudokuCell[] GetLineArray()
        {
            SudokuCell[] array = new SudokuCell[81];
            int counter = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    array[counter] = game[i, j];
                    counter++;
                }
            }

            return array;
        }

        private void SetCookie(SudokuCell[] array, HttpRequestBase request, HttpResponseBase response)
        {
            HttpCookie cookieLevel = request.Cookies["SudokuLevel"] ?? new HttpCookie("SudokuLevel");

            HttpCookie cookieGame = request.Cookies["SudokuGame"] ?? new HttpCookie("SudokuGame");

            cookieLevel.Value = levelOfGame.ToString();
            cookieGame.Value = String.Join(",", array.Select(x => x.Value));

            cookieLevel.Expires = DateTime.Now.AddDays(365);
            cookieGame.Expires = DateTime.Now.AddDays(365);

            response.Cookies.Add(cookieLevel);
            response.Cookies.Add(cookieGame);

        }

        //This method set static levels of game. Random method seems better for me, 
        //but the task strongly requires only five static states
        private void SetVariantOfGame()
        {
            GetInitialDistribution();

            //17 is just empirical digit
            SetHiddenCells(CurrentLevel + 17);

            //higher level - more shakes)
            for (int i = 0; i <= CurrentLevel; i++)
            {
                shakeStrategy[i]();
            }

            PaintingCells();
        }

        private void LevelIncrease()
        {
            if (++levelOfGame > 4)
            {
                levelOfGame = 0;
            }
        }

        private static void GetNewLevelPath()
        {
            levelPath = new int[] { 0, 1, 2, 3, 4 };

            Random rnd = new Random();

            for (int i = 4; i > 0; i--)
            {
                int a = rnd.Next(0, i);

                Swap(levelPath, a, i);
            }
        }

        private static void Swap(int[] arr, int a, int b)
        {
            int t = arr[a];

            arr[a] = arr[b];

            arr[b] = t;
        }

        //This hides each 2nd and k-th cell in the game
        private void SetHiddenCells(int k)
        {
            int counter = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (counter % 2 == 0 || counter % k == 0)
                        game[i, j].Hidden = true;
                    counter++;
                }
            }
        }

        //This provides some static shaker strategies. Numbers are just empirical digits
        private void SetShaker()
        {
            shakeStrategy[0] = () => { SwapRows(1, 2); SwapColumns(3, 5); };
            shakeStrategy[1] = () => { SwapRows(6, 7); SwapAreaColumns(0, 1); };
            shakeStrategy[2] = () => { SwapColumns(6, 7); SwapAreaRows(0, 1); };
            shakeStrategy[3] = () => { SwapRows(0, 1); SwapAreaColumns(1, 2); };
            shakeStrategy[4] = () => { SwapColumns(0, 1); SwapAreaRows(1, 2); };
        }

        private void GetInitialDistribution()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    SudokuCell cell = new SudokuCell();
                    //this formula provides array according sudoku rules
                    cell.Value = (i * 3 + i / 3 + j) % 9 + 1;
                    cell.IsRed = false;
                    cell.Hidden = false;
                    game[i, j] = cell;
                }
            }
        }

        private void PaintingCells()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int a = i / 3;
                    int b = j / 3;
                    game[i, j].IsRed = (a != 1 && b == 1 || a == 1 && b != 1);
                }
            }
        }

        private void SwapRows(int a, int b)
        {
            for (int i = 0; i < 9; i++)
            {
                var t = game[a, i];
                game[a, i] = game[b, i];
                game[b, i] = t;
            }
        }

        private void SwapColumns(int a, int b)
        {
            for (int i = 0; i < 9; i++)
            {
                var t = game[i, a];
                game[i, a] = game[i, b];
                game[i, b] = t;
            }
        }

        private void SwapAreaColumns(int a, int b)
        {
            for (int i = 0; i < 3; i++)
            {
                SwapColumns(a * 3 + i, b * 3 + i);
            }
        }

        private void SwapAreaRows(int a, int b)
        {
            for (int i = 0; i < 3; i++)
            {
                SwapRows(a * 3 + i, b * 3 + i);
            }
        }
    }
}
