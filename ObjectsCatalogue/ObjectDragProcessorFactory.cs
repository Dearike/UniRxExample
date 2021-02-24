using Planner.CommonEditor.Objects;
using Planner.Launcher.Notification;
using UnityEngine;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс, реализующий фабрику, создающую классы-обработчики событий перетаскивания объекта из каталога на сцену
    /// </summary>
    public class ObjectDragProcessorFactory : PlaceholderFactory<ObjectModel, IObjectDragProcessor>
    {
    }

    /// <summary>
    /// Интерфейс классов реализующих фабрику классов-обработчиков событий перетаскивания объекта из каталога на сцену
    /// </summary>
    public interface IObjectDragProcessorFactory : IFactory<ObjectModel, IObjectDragProcessor>
    {
    }

    /// <summary>
    /// Класс-обертка, реализующий универсальный метод создания класса-обработчика событий перетаскивания объекта для всех фабрик 
    /// </summary>
    public class ObjectDragProcessorFactoryProxy : IObjectDragProcessorFactory
    {
        private IObjectDragProcessorFactory subject;

        /// <summary>
        /// Сохраняем фабрику, которая будет создавать классы-обработчики событий 
        /// </summary>
        /// <param name="subject">фабрика классов-обработчиков событий перетаскивания объекта из каталога на сцену</param>
        public void SetSubject (IObjectDragProcessorFactory subject)
        {
            this.subject = subject;
        }

        /// <summary>
        /// Создаем класс-обработчик событий с помощью сохраненной фабрики
        /// </summary>
        /// <param name="objectModel">Модель данных объекта, который будет перетаскиваться</param>
        /// <returns>Возвращает класс-обработчик событий перетаскивания объекта из каталога на сцену</returns>
        public IObjectDragProcessor Create (ObjectModel objectModel)
        {
            return subject?.Create (objectModel);
        }
    }

    /// <summary>
    /// Класс, реализующий фабрику, создающую классы-обработчики событий перетаскивания объекта из каталога на сцену в режиме 2D
    /// </summary>
    public class ObjectDragProcessor2DFactory : IObjectDragProcessorFactory
    {
        private readonly DiContainer diContainer;

        public ObjectDragProcessor2DFactory (DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }

        /// <summary>
        /// Проверяем, является ли перетаскиваемый объект объектом свободной формы, объектом с проемом или просто объектом и возвращаем подходящий класс
        /// </summary>
        /// <param name="objectModel">модель данных перетаскиваемого объекта</param>
        /// <returns>Возвращает класс-обработчик событий перетаскивания объекта из каталога на сцену в режиме 2D</returns>
        public IObjectDragProcessor Create (ObjectModel objectModel)
        {
            if (objectModel.ObjectFlags.HasFlag (ObjectFlags.HoleProvider))
                return diContainer.Resolve<HoleProviderObjectDragProcessor> ();

            if (objectModel is FreeShapeObjectModel)
                return diContainer.Resolve<FreeShapeObjectDragProcessor2D> ();

            return diContainer.Resolve<StandardObjectDragProcessor2D> ();
        }
    }

    /// <summary>
    /// Класс, реализующий фабрику, создающую классы-обработчики событий перетаскивания объекта из каталога на сцену в режиме 3D
    /// </summary>
    public class ObjectDragProcessor3DFactory : IObjectDragProcessorFactory
    {
        [Inject]
        private NotificationController notificationController;

        private readonly DiContainer diContainer;

        public ObjectDragProcessor3DFactory (DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }

        /// <summary>
        /// Проверяем, что перетаскиваемый объект не является объектом свободной формы и не имеет проема и возвращаем соответсвующий класс-обработчик событий
        /// </summary>
        /// <param name="objectModel">модель данных перетаскиваемого объекта</param>
        /// <returns>Возвращает класс-обработчик событий перетаскивания объекта из каталога на сцену в режиме 3D</returns>
        public IObjectDragProcessor Create (ObjectModel objectModel)
        {
            if (objectModel.ObjectFlags.HasFlag (ObjectFlags.HoleProvider) ||
                objectModel.ObjectFlags.HasFlag (ObjectFlags.FreeShape)) {
                //Debug.LogError ("This object couldn't be added in 3D mode");
                notificationController.Show (new NotificationModel.Memento ("Невозможно добавить в режиме 3D"));
                return null;
            }

            return diContainer.Resolve<StandardObjectDragProcessor3D> ();
        }
    }
}