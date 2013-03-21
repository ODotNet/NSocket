using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketLib
{
    public class ChangePosition
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string ID { get; set; }

        private static Random ran = new Random();

        public static ChangePosition GetPoint()
        {
            ChangePosition cp = new ChangePosition();
            cp.X = ran.Next(1, 399);
            cp.Y = ran.Next(1, 399);
            return cp;
        }
    }
}
