using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AtlasViewer.Services;

public static class AlertService
{
    public static void Success(ITempDataDictionary tempData, string message)
    {
        tempData["SuccessMessage"] = message;
    }

    public static void Error(ITempDataDictionary tempData, string message)
    {
        tempData["ErrorMessage"] = message;
    }

    public static void Info(ITempDataDictionary tempData, string message)
    {
        tempData["InfoMessage"] = message;
    }

    public static void Warning(ITempDataDictionary tempData, string message)
    {
        tempData["WarningMessage"] = message;
    }
}
