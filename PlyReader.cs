using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CourseworkWinforms
{
    public class PlyReader
    {
        private string path;

        public PlyReader(string path)
        {
            this.path = path;
        }

        public IEnumerable<Point<double>> ReadFile()
        {
            return File.ReadAllLines(path)
                .SkipWhile(x => !x.Contains("end_header"))
                .Skip(1)
                .Select(line =>
                {
                    var xyz = line
                        .Split(' ')
                        .Select(l => double.Parse(l, CultureInfo.InvariantCulture)).ToList();
                    return new Point<double>(xyz[0], xyz[1], xyz[2]);
                });
        }

        public IEnumerable<Point<double>> ReadFile(int len)
        {
            return ReadFile().Take(len);
        }
    }
}