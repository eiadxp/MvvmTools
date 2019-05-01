using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MvvmTools.Design
{
    public static class Data
    {
        static Data()
        {
            Configurations.Design.MapType(typeof(IEnumerable<>), typeof(List<>));
            Configurations.Design.MapType(typeof(ICollection<>), typeof(List<>));
            Configurations.Design.MapType(typeof(IList<>), typeof(List<>));
            Configurations.Design.MapType(typeof(IDictionary<,>), typeof(Dictionary<,>));
        }
        #region Create design time data
        public static object Create(Type type, int depth = 0, bool createList = false, int itemsCount = 3)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (depth < 0)
                throw new ArgumentOutOfRangeException($"Argument '{nameof(depth)}' can not be less than zero.");
            if (itemsCount < 1 && createList)
                throw new ArgumentOutOfRangeException($"Argument '{nameof(itemsCount)}' can not be less than one.");
            int level = 0;
            if (!createList) return CreateInternal(type, depth, level, itemsCount);
            var list = new ArrayList();
            for (int i = 0; i < itemsCount; i++)
            {
                list.Add(CreateInternal(type, depth, level, itemsCount));
            }
            return list;
        }
        static object CreateInternal(Type type, int depth, int level, int itemsCount)
        {
            if (level > depth) return null;
            if (type.GetConstructor(Type.EmptyTypes) == null) return null;
            object value = null;
            try
            {
                value = Activator.CreateInstance(type);
            }
            catch (Exception)
            {
            }
            if (value == null) return value;
            level += 1;
            if (value is IEnumerable collection)
            {
                FillCollection(value, type, depth, level, itemsCount);
                foreach (var item in collection)
                {
                    FillProperties(item, depth, level, itemsCount);
                }
            }
            else
            {
                FillProperties(value, depth, level, itemsCount);
            }
            return value;
        }
        static void FillCollection(object collection, Type type, int depth, int level, int itemsCount)
        {
            if (!type.IsGenericType) return;
            var itemType = type.GetGenericArguments()[0];
            var addMethod = type.GetMethod("Add", new[] { itemType });
            if (addMethod == null) return;
            for (int i = 0; i < itemsCount; i++)
            {
                var item = CreateInternal(itemType, depth, level, itemsCount);
                if (item != null) addMethod.Invoke(collection, new[] { item });
            }
        }
        static void FillProperties(object obj, int depth, int level, int itemsCount)
        {
            foreach (var item in ReflectionCash.GetProperties(obj.GetType()))
            {
                var pType = GetMappedType(item.PropertyType, item);
                if (pType == null) continue;
                if (pType.IsClass && item.CanWrite)
                {
                    if (pType == typeof(string))
                    {
                        item.SetValue(obj, item.Name);
                    }
                    else
                    {
                        try
                        {
                            if (item.GetValue(obj) == null) item.SetValue(obj, CreateInternal(pType, depth, level, itemsCount));
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }
        #endregion
        #region Map types
        internal static Dictionary<Type, Type> typesMaps = new Dictionary<Type, Type>();

        static Type GetMappedType(Type type, PropertyInfo property = null)
        {
            if (type.GetCustomAttribute<DesignIgnorAttribute>() != null) return null;
            if (property != null)
            {
                if (property.GetCustomAttribute<DesignIgnorAttribute>() != null) return null;
                var propertyAttribute = property.GetCustomAttribute<DesignTypeAttribute>();
                if (propertyAttribute != null) return propertyAttribute.Type;
            }
            var typeAttribute = type.GetCustomAttribute<DesignTypeAttribute>();
            if (typeAttribute != null) return typeAttribute.Type;
            if (typesMaps.TryGetValue(type, out Type mappedType)) return mappedType;
            if (type.IsGenericType && typesMaps.TryGetValue(type.GetGenericTypeDefinition(), out mappedType))
            {
                return mappedType.MakeGenericType(type.GenericTypeArguments) ?? type;
            }
            return type;
        }
        #endregion
    }
}
