using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AA.Pango.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {

            while (true)
            {
                var info = string.Empty;


                var outputData = Console.ReadLine();
                if (!string.IsNullOrEmpty(outputData))
                {
                    info = outputData;
                }

                Console.WriteLine("Info: " + info);
            }

        }

    }
}
