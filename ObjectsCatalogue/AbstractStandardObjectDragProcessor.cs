using Planner.CommonEditor.Objects;
using Planner.Geometry;
using Planner.MainEditor.View2D;
using Planner.ObjectsEditor;
using UniRx;
using UnityEngine.EventSystems;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Абстрактный класс, реализующий перетаскивание объекта из каталога на сцену
    /// </summary>
    public abstract class AbstractStandardObjectDragProcessor : IObjectDragProcessor
    {
        [Inject]
        protected ObjectPreview objectPreview;

        [Inject]
        protected UndoRedoStack undoRedoStack;

        [Inject]
        protected IApartmentModelProvider apartmentModelProvider;

        [Inject]
        protected ObjectInstanceModel.Pool objectInstanceModelPool;

        private const float pointerIndent = 5f;

        protected abstract void HandleObjectDragBegan (ObjectModel objectModel);

        protected abstract void HandleObjectDrag (PointerEventData pointerEventData, ObjectModel objectModel);

        protected abstract void HandleObjectDragCompleted ();

        /// <summary>
        /// При продолжении перетаскивания объекта перемещаем превью объекта вслед за курсором и вызываем обработчик продолжения перетаскивания 
        /// </summary>
        /// <param name="pointerEventData">данные о событии, формируемые курсором</param>
        /// <param name="objectModel">модель данных перетаскиваемого объекта</param>
        private void ProcessObjectDrag (PointerEventData pointerEventData, ObjectModel objectModel)
        {
            objectPreview.Move (pointerEventData.position);

            if (EventSystem.current.IsPointerOverGameObject ())
                return;

            HandleObjectDrag (pointerEventData, objectModel);
        }

        /// <summary>
        /// При завершении перетаскивания скрываем превью объекта и вызываем обработчик завершения перетаскивания
        /// </summary>
        private void ObjectDragCompleted ()
        {
            objectPreview.Hide ();

            HandleObjectDragCompleted ();
        }

        /// <summary>
        /// При старте перетаскивания отображаем превью объекта в месте начала перетаскивания и подписываемся на событие продолжения перетаскивания. Вызываем обработчик начала перетаскивания
        /// </summary>
        /// <param name="dragObservable">Событие  перетаскивания объекта</param>
        /// <param name="objectModel">Модель данных объекта, перетаскиваемого из каталога</param>
        public void ObjectDragBegan (ObjectView.DragObservable dragObservable, ObjectModel objectModel)
        {
            objectPreview.Display (objectModel, dragObservable.PointerEventData.position);
            dragObservable.Subscribe (y => ProcessObjectDrag (y, objectModel), ObjectDragCompleted);

            HandleObjectDragBegan (objectModel);
        }
    }
}