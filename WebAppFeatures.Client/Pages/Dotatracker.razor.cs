using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io.Network;
using MudBlazor;
using Models.DTOs;

namespace WebAppFeatures.Client.Pages
{
    public partial class Dotatracker
    {
        public async Task<List<DotaTrackerDTO>> Parser(string pickedHero, string againstHero)
        {
            var config = Configuration.Default.WithRequesters().WithDefaultLoader();

            var context = BrowsingContext.New(config);
            var pickedHeroLink = pickedHero.Replace("_", "%20").Replace("'", "%27");
            var document = await context.OpenAsync($"https://dota2protracker.com/hero/{pickedHeroLink}");

            var inputs = document.QuerySelectorAll(".pros-stats a").ToList();

            string regex = @"^row-\d+";

            var rows = new List<IElement>();
            List<DotaTrackerDTO> _dtos = new();
            int i = 1;

            var trElements = document.QuerySelectorAll("tr");
            foreach (var tr in trElements)
            {
                var className = tr.GetAttribute("class");
                var items = tr.GetAttribute("items");
                if (className == null) continue;
                try
                {
                    if (Regex.IsMatch(className, regex))
                    {
                        var tdDraft = tr.QuerySelector("td.td-draft");
                        var imgPickedHero = tdDraft.QuerySelector($"img[src='/static/icons/{pickedHero}_minimap_icon.png']");
                        if (imgPickedHero != null)
                        {
                            // Определяем, в каком div находится img
                            var teamDiv = imgPickedHero.Closest("div.team-radiant") ?? imgPickedHero.Closest("div.team-dire");
                            var teamName = teamDiv.ClassList.Contains("team-radiant") ? "team-radiant" : "team-dire";

                            // Ищем противоположный div и ищем там img с src="/static/icons/Puck_minimap_icon.png" и классом img_rf
                            var oppositeTeamDiv = teamDiv.ClassList.Contains("team-radiant")
                                ? tr.QuerySelector("div.team-dire")
                                : tr.QuerySelector("div.team-radiant");
                            var imgHeroAgainst =
                                oppositeTeamDiv.QuerySelector($"img[src='/static/icons/{againstHero}_minimap_icon.png']");

                            if (imgHeroAgainst != null)
                            {
                                var dotabuff = tr.QuerySelector("a.info.dotabuff");
                                if (dotabuff != null)
                                {
                                    DotaTrackerDTO obj = new();
                                    obj.Id = i;
                                    obj.Link = dotabuff.GetAttribute("href");
                                    obj.Items = new List<string>();
                                    obj.Items.AddRange(items.Split(','));
                                    _dtos.Add(obj);
                                    i++;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.HResult != -2147467261) Console.WriteLine(ex.Message); // Код value == 0
                }
            }
            return _dtos;
        }
    }
}
