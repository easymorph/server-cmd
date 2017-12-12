using Morph.Server.Sdk.Model;
using MorphCmd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphCmd.Interfaces
{

    internal interface ICommand
    {
        Task Execute(Parameters parameters);
        
    }
}
