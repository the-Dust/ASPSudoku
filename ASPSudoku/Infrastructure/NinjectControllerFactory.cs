using Services.SudokuGame;
using Services.SudokuGame.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;

namespace ASPSudoku.Infrastructure
{
    public class NinjectControllerFactory : DefaultControllerFactory
    {
        public IKernel Kernel { get; private set; }

        public NinjectControllerFactory()
        {
            Kernel = new StandardKernel();
            AddBindings();
        }

        public void AddBindings()
        {
            Kernel.Bind<ISudoku>().To<Sudoku>();
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            return controllerType != null ? (IController)Kernel.Get(controllerType) : null;
        }
    }
}