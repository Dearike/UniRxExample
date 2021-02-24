using Planner.CommonEditor.Tools;
using UniRx;

namespace Planner.MainEditor.ObjectsCatalogue
{
    public class SearchCatalogModel : IFilter<string>
    {
        public readonly StringReactiveProperty SearchStringAsObservable = new StringReactiveProperty(string.Empty);

        public bool NeedSearch => SearchStringAsObservable.Value != string.Empty;

        public bool Check(string checkedValue)
        {
            return !checkedValue.IsNullOrEmpty() 
                   && checkedValue.ToLowerInvariant().Contains(SearchStringAsObservable.Value.ToLowerInvariant());
        }
    }
}