using System;
using Planner.Commands;
using Planner.Launcher;
using Planner.Launcher.Alert;
using Planner.Utils;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс, реализующий представление объекта в каталоге 
    /// </summary>
    public class ObjectView : UIBehaviour
    {
        /// <summary>
        /// Класс события перетаскивания объекта из каталога на сцену 
        /// </summary>
        public class DragObservable : IObservable<PointerEventData>
        {
            private IObservable<PointerEventData> observable;

            public readonly PointerEventData PointerEventData;
            public readonly Model Model;

            public DragObservable (
                PointerEventData pointerEventData,
                Model model,
                IObservable<PointerEventData> observable)
            {
                PointerEventData = pointerEventData;
                Model = model;
                this.observable = observable;
            }

            /// <summary>
            /// Реализуем возможность подписаться на событие
            /// </summary>
            /// <param name="observer"></param>
            /// <returns>Возвращает подписку на событие перетаскивания объекта</returns>
            public IDisposable Subscribe (IObserver<PointerEventData> observer)
            {
                return observable.Subscribe (observer);
            }
        }

        /// <summary>
        /// Модель данных представления объекта в каталоге
        /// </summary>
        public struct Model
        {
            public readonly int Id;
            public readonly string Name;
            public readonly Sprite Preview;

            public Model (int id, string name, Sprite preview)
            {
                Id = id;
                Name = name;
                Preview = preview;
            }

            public bool Equals (Model other)
            {
                return string.Equals (Name, other.Name) && Equals (Preview, other.Preview);
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
                           (Preview != null ? Preview.GetHashCode () : 0);
                }
            }
        }

        /// <summary>
        /// Пул создающий и реинициализирующий представления объектов в каталоге
        /// </summary>
        public class Pool : MonoMemoryPool<Transform, Model, ObjectView>
        {
            protected override void Reinitialize (Transform parent, Model model, ObjectView view)
            {
                view.SetModel (model);
                view.ParentUnderAsLast (parent, false);
            }
        }

        [SerializeField]
        private Image previewImage;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private LayoutElement layoutElement;

        [Inject]
        private ObjectProjectRequester objectProjectRequester;
        
        [Inject]
        private ExitEditorCommand.Pool _exitEditorCommander;

        [Inject]
        private AlertController _alertController;

        private Model model;

        public IObservable<DragObservable> OnDragObservable { private set; get; }

        /// <summary>
        /// При активации представления подписываемся на событие перетаскивания объекта
        /// </summary>
        protected override void OnEnable ()
        {
            OnDragObservable = this
                .OnBeginDragAsObservable ()
                .TakeUntilDisable (this)
                .Select (
                    x => new DragObservable (
                        x,
                        model,
                        this
                            .OnDragAsObservable ()
                            .TakeUntil (this.OnEndDragAsObservable ())));

            //IBeginDragHandler works only with IDragHandler, without this fake observable begin drag will never emit data.
            this
                .OnDragAsObservable ()
                .TakeUntilDisable (this);

            previewImage
                .OnPointerClickAsObservable()
                .TakeUntilDisable(this)
                .Subscribe(_ => _alertController.ShowAlert(AlertType.ExitFromEditor, LoadObject));
        }

        /// <summary>
        /// Обновляем наименование объекта и его миниатюру и подравниваем текст наименования объекта под миниатюрой
        /// </summary>
        /// <param name="newModel">модель данных представления объекта в каталоге</param>
        private void SetModel (Model newModel)
        {
            if (newModel.Equals (model))
                return;

            model = newModel;
            nameText.text = model.Name;
            previewImage.overrideSprite = model.Preview;
        }

        private void LoadObject()
        {
            _exitEditorCommander.Spawn(Unit.Default);
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            Resources.UnloadUnusedAssets();
            objectProjectRequester.Run(model.Id);
        }
    }
}