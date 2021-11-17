using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CourseworkWinforms
{
    public class Point <T>
    {
        public T X { get; }
        public T Y { get; }
        public T Z { get; }

        public Point(T x, T y, T z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}