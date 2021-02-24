using System.Collections.Generic;
using Planner.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс отвечающий за представление группы категорий в каталоге объектов 
    /// </summary>
    public class CategoryGroupView : MonoBehaviour
    {
        /// <summary>
        /// Структура данных, отвечающая за настройки группы категории
        /// </summary>
        public struct Settings
        {
            public readonly Color ExpandedColor;
            public readonly Color CollapsedColor;

            public Settings (Color expandedColor, Color collapsedColor)
            {
                ExpandedColor = expandedColor;
                CollapsedColor = collapsedColor;
            }
        }

        /// <summary>
        /// Модель данных группы категорий
        /// </summary>
        public struct Model
        {
            public readonly int Id;
            public readonly string Name;
            public readonly CategoryView.Model[] Categories;
            public readonly BoolReactiveProperty Expanded;

            public Model (int id, string name, CategoryView.Model[] categories, bool expanded)
            {
                Id = id;
                Name = name;
                Categories = categories;
                Expanded = new BoolReactiveProperty (expanded);
            }

            public bool Equals (Model other)
            {
                return Id == other.Id;
            }

            public override bool Equals (object obj)
            {
                if (ReferenceEquals (null, obj)) return false;
                return obj is Model other && Equals (other);
            }

            public override int GetHashCode ()
            {
//                return (Name != null ? Name.GetHashCode () : 0);
                return Id;
            }
        }

        /// <summary>
        /// Пулл создающий и реинициализирующий представления группы категорий объектов
        /// </summary>
        public class Pool : MonoMemoryPool<Transform, Model, Subject<CategoryView.Model>, CategoryGroupView>
        {
            protected override void Reinitialize (
                Transform parent,
                Model room,
                Subject<CategoryView.Model> floor,
                CategoryGroupView view)
            {
                view.SetModel (room);
                view.ParentUnderAsLast (parent, false);
                view.categoryClickedSubject = floor;
            }
        }

        [SerializeField]
        private Transform categories;

        [SerializeField]
        private Toggle toggle;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private Image arrow;

        [Inject]
        private CategoryView.Pool categoryViewPool;

        [Inject]
        private Settings settings;

        private Model model;
        private readonly List<CategoryView> categoryViews = new List<CategoryView> ();
        private readonly CompositeDisposable disposables = new CompositeDisposable ();
        private readonly CompositeDisposable expandedDisposables = new CompositeDisposable ();
        private readonly CompositeDisposable hoveredDisposables = new CompositeDisposable();

        private Subject<CategoryView.Model> categoryClickedSubject;

        /// <summary>
        /// при уничтожении объекта отписываемся от всех подписок
        /// </summary>
        private void OnDestroy ()
        {
            disposables.Dispose ();
            expandedDisposables.Dispose ();
            hoveredDisposables.Dispose ();
        }

        /// <summary>
        /// Уничтожаем все представления категорий в группе и потом, если это необходимо создаем их заново и подписываемся на их событие клика по категории
        /// </summary>
        private void RefreshCategories ()
        {
            categoryViews.ForEach (categoryViewPool.Despawn);
            categoryViews.Clear ();
            expandedDisposables.Clear ();
            hoveredDisposables.Clear();

            if (!model.Expanded.Value)
                return;

            foreach (var category in model.Categories) {
                var categoryView = categoryViewPool.Spawn (categories, category);
                categoryViews.Add (categoryView);
            }

            categoryViews
                .ConvertAll (x => x.ClickObservable)
                .ToObservable ()
                .SelectMany (x => x)
                .Subscribe (categoryClickedSubject.OnNext)
                .AddTo (expandedDisposables);
        }

        /// <summary>
        /// Изменяем внешний вид представления и обновляем внутренние категории в зависимости от "развернутости" группы категорий
        /// </summary>
        private void ExpandedChanged ()
        {
            var expanded = model.Expanded.Value;
            arrow.transform.localRotation = expanded ? Quaternion.identity : Quaternion.Euler (0f, 0f, 90f);
            arrow.color = nameText.color = expanded ? settings.ExpandedColor : settings.CollapsedColor;

            toggle.isOn = expanded;
            categories.SetActive (expanded);

            RefreshCategories ();
        }

        /// <summary>
        /// Подписываемся на необходимые события и задаем наименование группы категорий на основе модели данных
        /// </summary>
        /// <param name="newModel">новая модель данных</param>
        private void SetModel (Model newModel)
        {
            if (newModel.Equals (model))
                return;

            disposables.Clear ();
            model = newModel;

            toggle
                .OnValueChangedAsObservable ()
                .Subscribe (x => model.Expanded.Value = x)
                .AddTo (disposables);

            model
                .Expanded
                .Subscribe (_ => ExpandedChanged ())
                .AddTo (disposables);

            nameText.text = model.Name;
            nameText.SetHoveredColor(settings.ExpandedColor, settings.CollapsedColor, hoveredDisposables);
        }
    }
}