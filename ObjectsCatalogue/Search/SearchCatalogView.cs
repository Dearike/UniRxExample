using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    public class SearchCatalogView : MonoBehaviour
    {
        [SerializeField]
        private InputField _inputField;

        [Inject]
        private SearchCatalogModel _model;

        private void Awake()
        {
            _inputField.onValueChanged.RemoveAllListeners();
            _inputField
                .OnValueChangedAsObservable()
                .Subscribe(search => _model.SearchStringAsObservable.Value = search)
                .AddTo(this);
        }

        public void Clear()
        {
            _inputField.text = string.Empty;
        }
    }
}