using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlatBuffers;
using Planner.CommonEditor.Objects;
using Planner.CommonEditor.Tools;
using Planner.Launcher;
using Planner.ObjectsEditor.Serialization;
using Planner.Serialization;
using PlannerEditorAPI.Serialization;
using UnityEngine.Assertions;
using Zenject;

namespace Planner.MainEditor.ObjectsCatalogue
{
    /*!
        \ingroup  mainEditorObjectCatalog
    */
    /// <summary>
    /// Класс отвечающий за предоставление доступа к моделям данных объекта
    /// </summary>
    public class FilesystemObjectsProvider : IObjectsProvider
    {
        struct ObjectMetadata
        {
            public readonly int StateId;
            public readonly string Name;
            public readonly int[] Categories;

            public ObjectMetadata (int stateId, string name, int[] categories)
            {
                StateId = stateId;
                Name = name;
                Categories = categories;
            }
        }

        [Inject]
        private IPathsProvider pathsProvider;

        [Inject]
        private string metaFilename;

        [Inject]
        private FreeShapeObjectModel.Factory freeShapeObjectModelFactory;

        [Inject]
        private IObjectSerializer objectSerializer;

        private FreeShapeObjectModel freeShapeObjectModel;

        private readonly Dictionary<int, ObjectMetadata> IDMetadataMap = new Dictionary<int, ObjectMetadata> ();

        private readonly Dictionary<int, ObjectModel> cache = new Dictionary<int, ObjectModel> ();

        /// <summary>
        /// Возвращаем новый экземпляр объекта свободной формы
        /// </summary>
        /// <returns></returns>
        private FreeShapeObjectModel GetFreeShapeObjectModel ()
        {
            return freeShapeObjectModel ?? (freeShapeObjectModel = freeShapeObjectModelFactory.Create (true));
        }

        private FreeShapeObjectModel GetFreeShapeObjectModelInstance ()
        {
            return freeShapeObjectModelFactory.Create (true);
        }

        /// <summary>
        /// Загружаем объект с указанным идентификатором из файла
        /// </summary>
        /// <param name="id">идентификатор объекта</param>
        /// <returns></returns>
        /// <exception cref="???">Вызывает исключение десериализации объекта</exception>
        private ObjectModel Load (int id)
        {
            if (freeShapeObjectModel.Id == id)
                return freeShapeObjectModel;

            if (!IDMetadataMap.TryGetValue (id, out var metadata))
                return default;

            var filename = Path.Combine (pathsProvider.ObjectsPath, "Objects", metadata.StateId.ToString ());
            var bytes = File.ReadAllBytes (filename);

            if (!objectSerializer.TryDeserialize (bytes, out var objectModel, out var exception))
                throw exception;

            objectModel.StateId = metadata.StateId;
            return objectModel;
        }

        /// <summary>
        /// Обновляем список объектов
        /// </summary>
        public void Refresh ()
        {
            var metaBytes = File.ReadAllBytes (Path.Combine (pathsProvider.ObjectsPath, metaFilename));
            var buf = new ByteBuffer (metaBytes);
            var objectMetas = ObjectMetas.GetRootAsObjectMetas (buf);

            IDMetadataMap.Clear ();
            cache.Clear ();
            for (int i = 0; i < objectMetas.ObjectsLength; i++) {
                var objectMeta = objectMetas.Objects (i).Value;
                IDMetadataMap.Add (
                    objectMeta.Id,
                    new ObjectMetadata (objectMeta.State, objectMeta.Name, objectMeta.GetCategoriesArray ()));
            }

            var freeShapeObjectModel = GetFreeShapeObjectModel ();
            IDMetadataMap.Add (
                freeShapeObjectModel.Id,
                new ObjectMetadata (-1, "", freeShapeObjectModel.Categories.ToArray ()));
        }

        /// <summary>
        /// Наименование объекта с указанным идентифыикатором
        /// </summary>
        /// <param name="id">идентификатор объекта</param>
        /// <returns>Возвращает наименование объекта с указанным идентифыикатором</returns>
        public string GetObjectNameById (int id)
        {
            Assert.IsTrue (IDMetadataMap.TryGetValue (id < 0 ? -1 : id, out var metadata));
            return metadata.Name;
        }

        /// <summary>
        /// Проверяем существование объекта с таким идентификатором
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <returns>Возвращает <c>true</c>, если объект с таким идентификатором существует, и <c>false</c> в любом другом случае</returns>
        public bool ObjectExists (int id)
        {
            return IDMetadataMap.ContainsKey (id);
        }

        /// <summary>
        /// Находим объект по его идентификатору
        /// </summary>
        /// <param name="id">идентификатор объекта</param>
        /// <returns>Возвращает модель данных объекта</returns>
        public ObjectModel GetObjectById (int id)
        {
            if (!cache.TryGetValue (id, out var objectModel)) {
                objectModel = Load (id);
                cache.Add (id, objectModel);
            }

            if (objectModel.Id == freeShapeObjectModel.Id)
                return freeShapeObjectModel.Clone ();

            return objectModel;
        }

        /// <summary>
        /// Находим все объекты в данной категории
        /// </summary>
        /// <param name="categoryId">идентификатор категории</param>
        /// <returns>Возвращаем коллекцию объектов, соответствующих данной категории</returns>
        public ICollection<ObjectModel> GetObjectsFromCategory (int categoryId)
        {
            return IDMetadataMap
                .Where (x => Array.Exists (x.Value.Categories, y => y == categoryId))
                .Select (x => GetObjectById (x.Key))
                .ToArray ();
        }

        /// <summary>
        /// Находим все объекты в данных категориях
        /// </summary>
        /// <param name="categoriesId">идентификаторы категорий</param>
        /// <returns>Возвращаем коллекцию объектов, соответствующих данной категории</returns>
        public ICollection<ObjectModel> GetObjectsFromCategories(int[] categoriesId)
        {
            return IDMetadataMap
                .Where(x => Array.Exists(x.Value.Categories, y => categoriesId.Contains(y)))
                .Select(x => GetObjectById(x.Key))
                .ToArray();
        }

        /// <summary>
        /// Находим объект по шаблону имени
        /// </summary>
        /// <param name="filter">шаблон имени для поиска</param>
        /// <param name="excludedCategory">категории в которых не нужно искать</param>
        /// <returns>Возвращаем коллекцию объектов с наименованием, соотвествующим шаблону</returns>
        public ICollection<ObjectModel> GetObjectsByName (IFilter<string> filter, params int[] excludedCategory)
        {
            return IDMetadataMap
                .Where (
                    x => filter.Check (x.Value.Name) &&
                         (excludedCategory == null || !Array.Exists (x.Value.Categories, excludedCategory.Contains)))
                .Select (x => GetObjectById (x.Key))
                .ToArray ();
        }

        /// <summary>
        /// Получаем все объекты являющиеся валидными проемами
        /// </summary>
        /// <returns>Возвращает список объектов, являющихся валидными проемами</returns>
        public IList<ObjectModel> GetApertureObjects ()
        {
            var result = new List<ObjectModel> ();
            foreach (var metadata in IDMetadataMap) {
                var objectId = metadata.Key;

                // load without adding to cache
                if (!cache.TryGetValue (objectId, out var obj)) {
                    obj = Load (objectId);
                }

                if (IsValidApertureObject (obj)) {
                    result.Add (obj);
                }
            }

            return result;
        }

        /// <summary>
        /// Проверяем что объект является валидным для отображения на SVG-плане проемом
        /// </summary>
        /// <param name="obj">модель данных проверяемого объекта</param>
        /// <returns>Возвращает <c>true</c>, если объект является проемом не содержится в группе "Неопубликовано" и имеет SVG-миниатюру, и <c>false</c> в любом другом случае<</returns>
        private bool IsValidApertureObject (ObjectModel obj)
        {
            return IsApertureObject (obj) &&
                   !IsInOnlyOneCategory (obj, ObjectModel.ObjectUnpublishCategoryId) &&
                   obj.HasSvg ();
        }

        /// <summary>
        /// Проверяем, является ли объект проемом
        /// </summary>
        /// <param name="obj">модель данных проверяемого объекта </param>
        /// <returns>Возвращает <c>true</c>, если объект является проемом, и <c>false</c> в любом другом случае</returns>
        private static bool IsApertureObject (ObjectModel obj)
        {
            return obj.ObjectFlags.HasFlag (ObjectFlags.HoleProvider);
        }

        /// <summary>
        /// Проверяем , что данный объект содержится только в выбранной категории
        /// </summary>
        /// <param name="obj">модель данных проверяемого объекта</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <returns>Возвращает <c>true</c>, если объект содержится  только в этой категории, и <c>false</c> в любом другом случае</returns>
        private bool IsInOnlyOneCategory (ObjectModel obj, int categoryId)
        {
            if (!IDMetadataMap.ContainsKey (obj.Id)) return false;

            var meta = IDMetadataMap[obj.Id];

            return meta.Categories.Length == 1 && meta.Categories.Contains (categoryId);
        }

        /// <summary>
        /// Возвращаем количество объектов в категории
        /// </summary>
        /// <param name="categoryId">идентификатор категории</param>
        /// <returns>Возвращает количество объектов в категории </returns>
        public int GetObjectsFromCategoryCount (int categoryId)
        {
            return IDMetadataMap.Count (x => Array.Exists (x.Value.Categories, y => y == categoryId));
        }

        /// <summary>
        /// Возвращаем количество объектов в категориях
        /// </summary>
        /// <param name="categoryId">идентификаторы категорий</param>
        /// <returns>Возвращает количество объектов в категории </returns>
        public int GetObjectsFromCategoriesCount(int[] categoriesId)
        {
            return IDMetadataMap.Count(x => Array.Exists(x.Value.Categories, y => categoriesId.Contains(y)));
        }
    }
}