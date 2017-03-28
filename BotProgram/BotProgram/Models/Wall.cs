﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotProgram.Models
{
   public class Wall
    {
        public int XPos { get; set; }
        public int XLength { get; set; }
        public int YPos { get; set; }
        public int YLength { get; set; }
        public bool Horizontal { get; set; }
        public bool Breakable { get; set; }
    }
}
