using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resources
{
    public struct CommandEvent
    {
        public readonly string command;
        public readonly string[] parameters;

        public CommandEvent(string command, string[] parameters)
        {
            this.command = command;
            this.parameters = parameters;
        }
    }
}



