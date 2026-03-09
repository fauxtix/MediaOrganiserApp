using Dapper;
using MediaOrganizerApp.Application.Factories;
using Web.Blazor.Services.Recipes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Spinner;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Blazor.Pages.Recipes.TastyBaseComponents
{
    public class TastyBase : ComponentBase
    {
        [Inject] ITastyApiRecipesService recipeService { get; set; }
        [Inject] IJSRuntime JS { get; set; }
        [Inject]
        public IStringLocalizer<App> localizerApp { get; set; }


        public Root recipesList { get; set; } = new();
        public RecipeTags tagsData { get; set; }
        public List<TagItem> tagsList { get; set; }
        public int MaxResults { get; set; } = 40;
        public string searchQuery { get; set; }
        protected int TagId { get; set; }

        protected string spinnerLabel { get; set; }
        protected int recipesCount = 0;
        protected SfSpinner SpinnerObj { get; set; }
        protected DialogEffect Effects = DialogEffect.SlideTop;

        protected SfButton searchBtn;
        protected SfTextBox searchTxtBox;
        protected string text2Search { get; set; }

        protected string video_url;
        protected string video_title;
        public bool VideoVisibility { get; set; }

        protected int iCnt = 0;

        protected override async Task OnInitializedAsync()
        {
            tagsData = await GetTags_Data();

            tagsList = FillTagItems();
        }

        protected async Task onChangeTag(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, TagItem> args)
        {
            TagId = args.Value;
            if (TagId > 0)
            {
                text2Search = "";
                searchQuery = tagsData.results.SingleOrDefault(p => p.id == TagId).name;

                recipesList = await GetRecipes_Data(0);
            }
        }

        protected async Task<Root> GetRecipes_Data(int opcao, string label = "Lendo receitas...")
        {
            Root recipesList_Temp = new();
            spinnerLabel = label;
            await Task.Delay(100);
            await SpinnerObj.ShowAsync();

            recipesList_Temp = await recipeService.GetRecipes(searchQuery, MaxResults, opcao);

            recipesCount = recipesList_Temp.results.Count();

            await SpinnerObj.HideAsync();


            return recipesList_Temp;

        }

        protected async Task<RecipeTags> GetTags_Data()
        {
            // replace call to api call with get tags from database
            RecipeTags tagsData_Temp = new();
            tagsData_Temp = await recipeService.GetRecipesTags();
            StringBuilder sbData = new();
            foreach (var tag in tagsData_Temp.results)
            {
                sbData.Clear();
                sbData.Append("INSERT INTO [dbo].[ResultTag] ");
                sbData.Append("([Id],[type],[name],[display_name]) ");
                sbData.Append("VALUES (");
                sbData.Append($"{tag.id}, ");
                sbData.Append($"'{tag.type}', ");
                sbData.Append($"'{tag.name}', ");
                sbData.Append($"'{tag.display_name.Replace("'", "")}')");

                using (var connection = ConnectionManager.GetConnection("TastyDbConnection"))
                {
                    await connection.ExecuteAsync(sbData.ToString());
                }
            }

            StateHasChanged();


            return tagsData_Temp;
        }

        protected List<TagItem> FillTagItems()
        {
            var list = tagsData.results.ToList();

            List<TagItem> returnList = new();

            foreach (var item in list)
            {
                returnList.Add(new TagItem()
                {
                    Id = item.id,
                    DisplayName = item.display_name
                });
            }

            List<TagItem> SortedList = returnList.OrderBy(o => o.DisplayName).ToList();
            return SortedList;
        }

        protected void ShowVideo(string url, string title)
        {
            if (!string.IsNullOrEmpty(url))
            {
                video_url = url;
                video_title = title;
                VideoVisibility = true;
            }
        }

        protected void CloseVideoDialog()
        {
            video_url = "";
            VideoVisibility = false;
        }

        protected async Task SearchByRecipeName()
        {
            TagId = 0;
            searchQuery = searchTxtBox.Value;
            recipesList = await GetRecipes_Data(1, "Pesquisando texto nas receitas"); // search option = 'q' (1)
            await InvokeAsync(StateHasChanged);

        }
        public class TagItem
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
        }

    }
}
