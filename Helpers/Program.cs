using System;

namespace Helpers
{
    class Program
    {
        static void Main(string[] args)
        {
            //Convertidor vs ExpresionSerializar multiple expressions
            for (var i = 0; i < 5; i++)
            {
                Console.WriteLine($@"Convertidor: {UnitTest.Convertidor()} milliseconds");
                Console.WriteLine($@"ExpressionSerializer: {UnitTest.ExpressionSerializer()} milliseconds");
            }
        }
    }
}
