namespace Common.Engine.Surveys;


public class SurveyEngineLogicException : Exception
{
}
public class SurveyEngineDataException : Exception
{
    public SurveyEngineDataException(string? message) : base(message)
    {
    }
}
