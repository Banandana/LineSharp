//Author MID: #REDACTED
using System;
using LineSharp;

namespace LineSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            LineClient line = new LineClient();

            line.OnLogin += new LineClient.LoggedInEvent((Result loginResult) =>
            {
                if (loginResult == Result.OK)
                {
                    Console.WriteLine("Authed successfully!");
                }
                else
                {
                    Console.WriteLine("Did not auth successfully. Paused.");
                    Console.Read();
                    Environment.Exit(0);
                }

                
            });

            line.OnReceiveMessage += (o, eventArgs) => Console.WriteLine(eventArgs.Message.Text);
            
            line.Login("USERNAME", "PASSWORD");
            while (true) line.Update();
            //Console.Read();
            //Line.Logout();
        }
    }
}