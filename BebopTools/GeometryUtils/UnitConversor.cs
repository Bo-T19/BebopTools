using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BebopTools.GeometryUtils
{
    internal class UnitConversor
    {
        //A little class that has the tools for converting between the Imperial and the Metric system
        public static double ConvertFeetToMeters(double dimension)
        {
            return dimension * 0.3048;
        }

        public static double ConvertSquareFeetToSquareMeters(double dimension)
        {
            return dimension * 0.092903;
        }

        public static double ConvertCubicFeetToCubicMeters(double dimension)
        {
            return dimension * 0.0283168;
        }

    }
}
