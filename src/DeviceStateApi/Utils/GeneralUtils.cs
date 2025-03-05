using System;

namespace DeviceStateApi.Utils;

public static class GeneralUtils
{
    public static void PrintMessageToConsoleWithSpecialChar(string messageWithSpecialChar)
    {
        Console.OutputEncoding = System.Text.Encoding.Unicode;
        Console.Write(messageWithSpecialChar);
    }
}
