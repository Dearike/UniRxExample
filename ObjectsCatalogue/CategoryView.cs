using System;
using Planner.Utils;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс отвечающий за представление  категории в каталоге объектов 
    /// </summary>
    public class CategoryView : UIBehaviour
    {
        public struct Settings
        {
            public readonly Color HoveredTextColor;
            public readonly Color NormalTextColor;

            public Settings(Color hoveredTextColor, Color normalTextColor)
            {
                HoveredTextColor = hoveredTextColor;
                NormalTextColor = normalTextColor;
            }
        }

        /// <summary>
        /// Модель данных категории
        /// </summary>
        public struct Model
        {
            public readonly int Id;
            public readonly string Name;
            public readonly int ObjectsCount;
            public readonly bool Selected;
            public int ObjectId;

            public Model (int id, string name, int objectsCount, bool selected)
            {
                Id = id;
                Name = name;
                ObjectsCount = objectsCount;
                Selected = selected;
                ObjectId = -1;
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
                return Id;
            }
        }

        /// <summary>
        /// Пул создающий и реинициализирующий представления категории объектов
        /// </summary>
        public class Pool : MonoMemoryPool<Transform, Model, CategoryView>
        {
            protected override void Reinitialize (Transform parent, Model model, CategoryView view)
            {
                view.SetModel (model);
                view.ParentUnderAsLast (parent, false);
            }
        }

        [SerializeField]
        private TextMeshProUGUI nameText;

        [Inject]
        private Settings settings;

        private Model model;

        public IObservable<Model> ClickObservable { private set; get; }

        /// <summary>
        /// На этапе загрузки экземпляра объекта создаем событие клика по категории
        /// </summary>
        protected override void Awake ()
        {
            ClickObservable = this
                .OnPointerClickAsObservable ()
                .Select (_ => model);
        }

        /// <summary>
        /// Задаем наименование категории и его внешний вид на основе модели данных  
        /// </summary>
        /// <param name="newModel">новая модель данных</param>
        private void SetModel (Model newModel)
        {
            if (newModel.Equals (model))
                return;

            model = newModel;

            nameText.text = $"{model.Name} ({model.ObjectsCount})";
            nameText.fontStyle = model.Selected ? FontStyles.Bold : FontStyles.Normal;
            nameText.SetHoveredColor(settings.HoveredTextColor, settings.NormalTextColor, this);
        }
    }
}