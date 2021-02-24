using System;
using System.Collections.Generic;
using Planner.Utils;
using UniRx;
using UnityEngine;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс отвечающий за представление каталога объектов 
    /// </summary>
    public class CategoriesView : MonoBehaviour
    {
        /// <summary>
        /// Модель данных каталога объектов 
        /// </summary>
        public struct Model
        {
            public readonly CategoryGroupView.Model[] Groups;

            public Model (CategoryGroupView.Model[] groups)
            {
                Groups = groups;
            }

            public bool Equals (Model other)
            {
                return ArrayComparer.Equals (Groups, other.Groups);
            }

            public override bool Equals (object obj)
            {
                if (ReferenceEquals (null, obj)) return false;
                return obj is Model other && Equals (other);
            }

            public override int GetHashCode ()
            {
                return (Groups != null ? Groups.GetHashCode () : 0);
            }
        }

        [SerializeField]
        private Transform groupsContainer;

        [Inject]
        private CategoryGroupView.Pool categoryGroupViewPool;

        private readonly List<CategoryGroupView> groupViews = new List<CategoryGroupView> ();
        private Model model;
        private Subject<CategoryView.Model> categoryClickedSubject;

        public IObservable<CategoryView.Model> CategoryClickedObservable => categoryClickedSubject;

        /// <summary>
        /// При загрузке экземпляра объекта данного класса создаем событие клика по категории объектов 
        /// </summary>
        private void Awake ()
        {
            categoryClickedSubject = new Subject<CategoryView.Model> ();
        }

        /// <summary>
        /// Обновляем модель данных, очищаем список представлений групп категорий объектов и для каждой группы в модели создаем новое представление
        /// </summary>
        /// <param name="newModel">новая модель данных каталога</param>
        public void SetModel (Model newModel)
        {
            if (newModel.Equals (model))
                return;

            model = newModel;

            groupViews.ForEach (categoryGroupViewPool.Despawn);
            groupViews.Clear ();

            foreach (var group in model.Groups) {
                var groupView = categoryGroupViewPool.Spawn (groupsContainer, group, categoryClickedSubject);
                groupViews.Add (groupView);
            }
        }
    }
}