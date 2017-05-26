using System;
using System.Collections.Generic;
using Abarsoft.Helpers;
using System.Linq;

namespace Helpers
{
    class Program
    {
        static void Main(string[] args)
        {
            //Convertidor vs ExpresionSerializar multiple expressions
            /* for (var i = 0; i < 5; i++)
             {
                 Console.WriteLine($@"Convertidor: {UnitTest.Convertidor()} milliseconds");
                 Console.WriteLine($@"ExpressionSerializer: {UnitTest.ExpressionSerializer()} milliseconds");
             }
             */
            var c = new ColeccionSet<int>(new List<int> { 1, 2, 3 }).Where(a => a == 1).FirstOrDefault();

            Console.WriteLine($@"must be 1 = {c}");
        }
    }
}
