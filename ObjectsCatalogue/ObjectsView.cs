using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Класс, отвечающий за представление группы объектов в каталоге
    /// </summary>
    public class ObjectsView : MonoBehaviour
    {
        /// <summary>
        /// Модель данных категории
        /// </summary>
        public struct Model
        {
            public readonly string Name;
            public readonly ObjectView.Model[] Objects;

            public Model (string name, ObjectView.Model[] objects)
            {
                Name = name;
                Objects = objects;
            }

            public bool Equals (Model other)
            {
                return string.Equals (Name, other.Name) && ArrayComparer.Equals (Objects, other.Objects);
            }

            public override bool Equals (object obj)
            {
                if (ReferenceEquals (null, obj)) return false;
                return obj is Model other && Equals (other);
            }

            public override int GetHashCode ()
            {
                unchecked {
                    return ((Name != null ? Name.GetHashCode () : 0) * 397) ^
                           (Objects != null ? Objects.GetHashCode () : 0);
                }
            }
        }

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private UIBehaviour backClickZone;

        [SerializeField]
        private Transform objectsContainer;

        [Inject]
        private ObjectView.Pool objectViewPool;

        public IObservable<Unit> BackClickAsObservable { private set; get; }

        private readonly List<ObjectView> objectViews = new List<ObjectView> ();
        private Model model;

        public IObservable<ObjectView.DragObservable> OnDragObservable { private set; get; }

        /// <summary>
        /// При загрузке экземпляра объекта подписываемся на событие клика по кнопке "назад"
        /// </summary>
        private void Awake ()
        {
            BackClickAsObservable = backClickZone
                .OnPointerClickAsObservable ()
                .Select (_ => Unit.Default);
        }

        /// <summary>
        /// Инициализируем модель данных, обновляем наименование категории и подписываемся на событие начала перетаскивания объекта из каталога на сцену
        /// </summary>
        /// <param name="newModel">модель данных категории объектов</param>
        public void SetModel (Model newModel)
        {
            if (model.Equals (newModel))
                return;

            model = newModel;

            nameText.text = model.Name;
            objectViews.ForEach (objectViewPool.Despawn);
            objectViews.Clear ();

            foreach (var obj in model.Objects) {
                var objectView = objectViewPool.Spawn (objectsContainer, obj);
                objectViews.Add (objectView);
            }

            OnDragObservable = objectViews
                .Select (x => x.OnDragObservable)
                .ToObservable ()
                .SelectMany (x => x);
        }
    }
}