using System;
using System.Collections.Generic;
using System.Text;

namespace Refiner
{
    //Wrapper class to make DB layer abstract
    public interface IDBFacade
    {
        bool SaveCollection()
        {
        }

        bool SaveEntry()
        {
        }
    }
}
