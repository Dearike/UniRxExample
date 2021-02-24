using Planner.CommonEditor.Objects;
using Planner.CommonEditor.Tools;
using Planner.Geometry;
using Planner.Launcher.Notification;
using Planner.MainEditor.Commands;
using Planner.MainEditor.Tools;
using Planner.MainEditor.Tools.Selection2D;
using Planner.MainEditor.Tools.Shared;
using Planner.MainEditor.View2D;
using Planner.ObjectsEditor;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс, реализующий перетаскивание объекта с проемом из каталога на сцену в режиме 2D
    /// </summary>
    public class HoleProviderObjectDragProcessor : IObjectDragProcessor
    {
        [Inject]
        private ObjectPreview objectPreview;

        [Inject]
        private HighlightNearestWallTool highlightNearestWallTool;

        [Inject]
        private ToolsManager toolsManager;

        [Inject]
        private SelectionTool selectionTool;

        [Inject (Id = ID.DotsWallsPlane)]
        private Plane dotsWallsPlane;

        [Inject]
        private IApartmentModelProvider apartmentModelProvider;

        [Inject]
        private UndoRedoStack undoRedoStack;

        [Inject]
        private ObjectInstanceModel.Pool objectInstanceModelPool;

        [Inject]
        private NotificationController notificationController;

        private Vector2 pointerPosition;

        /// <summary>
        /// При продолжении перетаскивания объекта перемещаем превью объекта вслед за курсором 
        /// </summary>
        /// <param name="pointerEventData">данные о событии, формируемые курсором</param>
        private void ProcessObjectDrag (PointerEventData pointerEventData)
        {
            objectPreview.Move (pointerPosition = pointerEventData.position);
        }

        /// <summary>
        /// При завершении перетаскивания скрываем превью объекта, проверяем, что есть стена, на которую можно добавить проем, добавляем объект и привязываем его к выбранной стене 
        /// </summary>
        /// <param name="objectModel">Модель данных объекта</param>
        private void ObjectDragCompleted (ObjectModel objectModel)
        {
            objectPreview.Hide ();

            toolsManager.Add (selectionTool);

            dotsWallsPlane.Raycast (pointerPosition, out var planePosition);
            var wall = apartmentModelProvider
                .Apartment
                .Floor
                .GetNearestWall (planePosition);

            if (wall == null) {
                notificationController.Show (new NotificationModel.Memento ("Невозможно добавить. Нет доступных стен"));
                return;
            }

            //Attention! Should be performed strictly after getting nearest wall.
            planePosition = planePosition
                .FlipYZ ()
                .WithY (0f);

            var addObjectCommand = new AddObjectCommand (
                objectModel,
                Vector3.zero,
                objectInstanceModelPool,
                apartmentModelProvider,
                apartmentModelProvider.Apartment.FloorIndex.Value);

            var bindObjectCommand = new BindObjectToWallCommand (addObjectCommand, planePosition, wall);
            var compositeCommand = new CompositeCommand ();
            compositeCommand.AddSubCommand (addObjectCommand, bindObjectCommand);

            undoRedoStack.Execute (compositeCommand);
        }

        /// <summary>
        /// При старте перетаскивания отображаем превью объекта в месте начала перетаскивания и подписываемся на событие продолжения перетаскивания
        /// </summary>
        /// <param name="dragObservable">Событие  перетаскивания объекта</param>
        /// <param name="objectModel">Модель данных объекта, перетаскиваемого из каталога</param>
        public void ObjectDragBegan (ObjectView.DragObservable dragObservable, ObjectModel objectModel)
        {
            toolsManager.Add (highlightNearestWallTool);

            objectPreview.Display (objectModel, dragObservable.PointerEventData.position);
            dragObservable.Subscribe (ProcessObjectDrag, () => ObjectDragCompleted (objectModel));
        }
    }
}