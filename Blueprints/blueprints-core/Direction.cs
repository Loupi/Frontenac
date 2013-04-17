using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public enum Direction
    {
        OUT,
        IN,
        BOTH
    }

    public static class Directions
    {
        public static readonly Direction[] Proper = new Direction[]
        {
            Direction.OUT,
            Direction.IN
        };

        public static Direction Opposite(this Direction direction)
        {
            if (direction == Direction.OUT)
                return Direction.IN;
            else if (direction == Direction.IN)
                return Direction.OUT;
            else
                return Direction.BOTH;
        }
    }
}
