﻿using MorphCmd.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.BusinessLogic
{
    public class ConsoleInput : IInputEndpoint
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
