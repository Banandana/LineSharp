//Author MID: #REDACTED
using System;
using System.Data.Odbc;
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
                    //Everything worked
                if (loginResult == Result.OK)
                {
                    Console.WriteLine("Authed successfully!");
                }
                    //Phone verification needed
                else if (loginResult == Result.REQUIRES_PIN_VERIFICATION)
                {
                    //The user then is required to enter the pin (retrieved from calling
                    string PIN = line.Pin;
                    //)

                    //Then call this function, then enter the pin on the mobile device.
                    line.VerifyPin();
                    //WARNING: This function will hang until the pin verifies.
                }
                else
                {
                    Console.WriteLine("Did not auth successfully. Paused.");
                    Console.Read();
                    Environment.Exit(0);
                }

                
            });

            line.OnPinVerified += new LineClient.PinVerifiedEvent((Result pinVerifiedResult) =>
            {
                //The pin was verified, or it had timed out???


            });

            line.OnReceiveMessage += (o, eventArgs) => Console.WriteLine(eventArgs.Message.Text);
            
            line.Login("USERNAME", "PASSWORD");
            while (true) line.Update();
            //Console.Read();
            //Line.Logout();
        }
    }
}