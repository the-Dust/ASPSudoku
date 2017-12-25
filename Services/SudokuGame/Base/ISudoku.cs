using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services.SudokuGame.Base
{
    public interface ISudoku
    {
        int CurrentLevel { get; }

        SudokuCell[] NewGame(HttpRequestBase request, HttpResponseBase response);

        bool CheckSolution(SudokuCell[] array, HttpRequestBase request);

        int OpenCell(string cellName, HttpRequestBase request);
    }
}
