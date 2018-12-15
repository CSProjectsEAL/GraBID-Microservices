using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Refiner
{
    public interface IDataProcessor
    {
        string Process(string data);
    }
}
