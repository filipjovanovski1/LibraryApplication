// Web/Mapping/BookCategoryMapper.cs
using System.Text.RegularExpressions;
using CoursesApplication.Domain.DomainModels;

namespace CoursesApplication.Web.Mapping
{
    public static class BookCategoryMapper
    {
        private static readonly Dictionary<string, BookCategory> Map =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // Core
                ["fiction"] = BookCategory.Fiction,
                ["nonfiction"] = BookCategory.NonFiction,
                ["non-fiction"] = BookCategory.NonFiction,
                ["classics"] = BookCategory.Classics,
                ["poetry"] = BookCategory.Poetry,
                ["drama"] = BookCategory.Drama,
                ["short stories"] = BookCategory.ShortStories,
                ["mystery"] = BookCategory.Mystery,
                ["thriller"] = BookCategory.Thriller,
                ["horror"] = BookCategory.Horror,
                ["romance"] = BookCategory.Romance,
                ["fantasy"] = BookCategory.Fantasy,
                ["science fiction"] = BookCategory.ScienceFiction,
                ["sci-fi"] = BookCategory.ScienceFiction,
                ["sci fi"] = BookCategory.ScienceFiction,
                ["graphic novel"] = BookCategory.GraphicNovelComics,
                ["comics"] = BookCategory.GraphicNovelComics,
                ["children"] = BookCategory.Children,
                ["kids"] = BookCategory.Children,
                ["young adult"] = BookCategory.YoungAdult,
                ["ya"] = BookCategory.YoungAdult,

                // Non-fiction families
                ["biography"] = BookCategory.BiographyMemoir,
                ["memoir"] = BookCategory.BiographyMemoir,
                ["biography & memoir"] = BookCategory.BiographyMemoir,
                ["popular science"] = BookCategory.PopularScience,
                ["history"] = BookCategory.History,
                ["politics"] = BookCategory.Politics,
                ["business"] = BookCategory.BusinessManagement,
                ["management"] = BookCategory.BusinessManagement,
                ["leadership"] = BookCategory.Leadership,
                ["philosophy"] = BookCategory.PhilosophyWorldview,
                ["worldview"] = BookCategory.PhilosophyWorldview,
                ["psychology"] = BookCategory.Psychology,
                ["self help"] = BookCategory.MotivationSelfHelp,
                ["self-help"] = BookCategory.MotivationSelfHelp,
                ["motivation"] = BookCategory.MotivationSelfHelp,
                ["religion"] = BookCategory.Religion,
                ["spirituality"] = BookCategory.Spirituality,
                ["it"] = BookCategory.IT,
                ["education"] = BookCategory.Education,
                ["technology"] = BookCategory.Technology,
                ["programming"] = BookCategory.Programming,
                ["data science"] = BookCategory.DataScienceAI,
                ["ai"] = BookCategory.DataScienceAI,
                ["economics"] = BookCategory.EconomicsFinance,
                ["finance"] = BookCategory.EconomicsFinance,
                ["law"] = BookCategory.Law,
                ["language learning"] = BookCategory.LanguageLearning,
                ["reference"] = BookCategory.Reference,
                ["health"] = BookCategory.HealthFitness,
                ["fitness"] = BookCategory.HealthFitness,
                ["cooking"] = BookCategory.CookingFood,
                ["food"] = BookCategory.CookingFood,
                ["travel"] = BookCategory.Travel,
                ["art"] = BookCategory.ArtDesign,
                ["design"] = BookCategory.ArtDesign,
                ["photography"] = BookCategory.Photography,
                ["music"] = BookCategory.Music,
                ["sports"] = BookCategory.Sports,
                ["home"] = BookCategory.HomeGarden,
                ["garden"] = BookCategory.HomeGarden,
                ["parenting"] = BookCategory.ParentingFamily,
                ["family"] = BookCategory.ParentingFamily,
                ["crafts"] = BookCategory.CraftsDIY,
                ["diy"] = BookCategory.CraftsDIY,
                ["true crime"] = BookCategory.TrueCrime,
                ["essays"] = BookCategory.Essays,
            };

        public static bool TryMap(string? input, out BookCategory value)
        {
            value = default;
            if (string.IsNullOrWhiteSpace(input)) return false;

            // quick hits
            if (Map.TryGetValue(input, out value)) return true;

            // light normalization: replace &, /, - with spaces; collapse whitespace
            var s = input.Replace('&', ' ')
                         .Replace('/', ' ')
                         .Replace('-', ' ');
            s = Regex.Replace(s, @"\s+", " ").Trim();

            return Map.TryGetValue(s, out value);
        }
    }
}
