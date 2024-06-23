using System.IO.Ports;

class RS232Metrol
{
    static void Main(string[] args)
    {
        //GREG HANSON METROL RS232 DESIGN EXERCISE - PT1
        SerialPort _serialPort = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);
        var _continue = true;
        var _lastCommand = "";
        
        const string _queryBacklightStatusCommand = "DISPlay:BACKlight?";
        const string _backlightOnCommand = "DISPlay:BACKlight 1";
        const string _backlightOffCommand = "DISPlay:BACKlight 0";
        const string _sendBeepCommand = "SYSTem:BEEPer";
        const string _sendReadingsCommand = "READ?";
        const string _performSelfTestCommand = "*TST?";
        
        
        Thread readThread = new Thread(Read);
        readThread.Start();

        try
        {
            _serialPort.Open();
            Console.WriteLine("Port opened");

            while (_continue)
            {
                PrintInstructions();
                string? input = Console.ReadLine();
                _lastCommand = input;
                if (input?.ToLower() == "exit")
                    _continue = false;

                // Write the input to the device
                _serialPort.WriteLine(input);
                Console.WriteLine("Sent: " + input);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
        if (_serialPort.IsOpen) _serialPort.Close();
        
        void Read()
        {
            while (_continue)
            {
                var message = _serialPort.ReadLine();
                if (message is not null)
                {
                    switch (_lastCommand)
                    {
                        case(_performSelfTestCommand):
                            ShowResultsOfSelfTest(message);
                            break;
                        case(_queryBacklightStatusCommand):
                            ToggleBacklight(message);
                            break;
                        case(_sendBeepCommand):
                            _serialPort.WriteLine(_sendBeepCommand);
                            break;
                        case(_sendReadingsCommand):
                            var results = FormatResults(ExtractResultsFromString(message));
                            Console.WriteLine(results);
                            break;
                    }
                };
            }
        }

        List<decimal> ExtractResultsFromString(string message)
        {
            List<decimal> res = new List<decimal>();
            //Because I do not know the format of the string returned from the device, this method
            //is left blank, however when implemented what it would do is take the input message,
            //extract the readings from the string, and .Add them to a list
            return res;
        }
        
        List<string> FormatResults(List<decimal> list)
        {
            List<string> strList = new List<string>();
            
            for(var i = 1; i <= list.Count ; i++){
            
                var factorOf3 = FactorOf(list[i],3);
                var factorOf7 = FactorOf(list[i],7);
                var oneDP = string.Format("{0:F1}", list[i]);
            
                switch(factorOf3, factorOf7)
                {
                    case (false, false):
                        strList.Add(oneDP);
                        break;
                    case  (true, false):
                        strList.Add(oneDP + "<-");
                        break;
                    case  (false, true):
                        strList.Add(oneDP + "->");
                        break;
                    case  (true, true):
                        strList.Add(oneDP + "<->");
                        break;
                }
            }

            return strList;
        }

        bool FactorOf(decimal index, int modulus)
        {
            var roundDown = Math.Floor(index);
            if(roundDown % modulus == 0) return true;
            return false;
        }

        void PrintInstructions()
        {
            Console.WriteLine("Enter 1 to perform a self test");
            Console.WriteLine("Enter 2 to toggle the backlight");
            Console.WriteLine("Enter 3 to make the device beep once");
            Console.WriteLine("Enter 4 to take readings");
            Console.WriteLine("Enter 'exit' to quit the program");
        }

        void ShowResultsOfSelfTest(string results)
        {
            if (results == "0")
            {
                Console.WriteLine("Self test PASSED");
                return;
            }
            Console.WriteLine("Self test FAILED");
        }

        void ToggleBacklight(string message)
        {
            if(message == "0") _serialPort.WriteLine(_backlightOnCommand);
            if(message == "1") _serialPort.WriteLine(_backlightOffCommand);
        }
        
    }
}
