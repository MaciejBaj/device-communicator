
namespace programator
{
    class AvrdudeProgrammer : ConsoleProgram
    {
        public AvrdudeProgrammer(DataDisplayer dataDisplayer) : base(dataDisplayer, "avrdude")
        {
        }


        public void SendFile(string fileName)
        {
            launch("-p atmega328p -c usbasp -P usb -U flash:w:\"" + fileName + "\":i");
        }

        public void GetInfo()
        {
            launch("-p atmega328p -c usbasp -P usb -B 8");
        }
    }
}
