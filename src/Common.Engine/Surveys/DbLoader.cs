using Entities.DB;
using Microsoft.EntityFrameworkCore;

namespace Common.Engine.Surveys;

public class DbLoader
{
    public static async Task LoadSurveyPageQuestions(DataContext context)
    {
        await LoadSurveyPageQuestions(context, 0);
    }

    public static async Task<SurveyPage?> LoadSurveyPageQuestions(DataContext context, int pageNumber)
    {
        // Load survey questions from the database
        var publishedPages = await context.SurveyPages
            .Where(p => p.IsPublished)
            .Include(p => p.Questions)
            .OrderBy(p => p.PageIndex)
            .ToListAsync();

        if (publishedPages.Count > pageNumber)
            return new SurveyPage(publishedPages[pageNumber]);
        else
            return null;
    }
}
