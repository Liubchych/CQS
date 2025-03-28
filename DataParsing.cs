using System;
using System.Text.RegularExpressions;

class ParsedData
{
    public string RobotID { get; set; }
    public string OkQty { get; set; }
    public string NokQty { get; set; }
    public string ErrorCode { get; set; }
}

class DataParsing
{
    public static ParsedData ParseData(string input)
    {
        // Регулярні вирази
        string robotIDPattern = @"@I(\d+)";
        string okQtyPattern = @"@O(\d+)";
        string nokQtyPattern = @"@N(\d+)";
        string errorCodePattern = @"@E(\d+)";

        // Створюємо об'єкт для збереження даних
        ParsedData parsedData = new ParsedData();

        // Розділяємо рядок
        string[] parts = input.Split('\t');

        foreach (string part in parts)
        {
            if (Regex.Match(part, robotIDPattern) is Match m && m.Success)
                parsedData.RobotID = m.Groups[1].Value;

            if (Regex.Match(part, okQtyPattern) is Match o && o.Success)
                parsedData.OkQty = o.Groups[1].Value;

            if (Regex.Match(part, nokQtyPattern) is Match n && n.Success)
                parsedData.NokQty = n.Groups[1].Value;

            if (Regex.Match(part, errorCodePattern) is Match e && e.Success)
                parsedData.ErrorCode = e.Groups[1].Value;
        }

        return parsedData;
    }

}

