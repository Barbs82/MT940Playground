using Raptorious.SharpMt940Lib;
using System;
using System.IO;
using System.Text;

namespace Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            Auszug a = new Auszug();           
            a.readCustMsg();       
        }
    }  
}
