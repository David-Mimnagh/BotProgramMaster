using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotProgram.Models
{
    public class Level
    {
        public int BoardWidth { get; set; }
        public int BoardHeight{ get; set; }
        public List<Wall> Walls { get; set; }
        public Exit Exit { get; set; }
        public List<Gate> Gates { get; set; }
        public List<Key> Keys { get; set; }
    }
}
