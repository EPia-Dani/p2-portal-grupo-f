using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Extensions
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        return component ? component : gameObject.AddComponent<T>();
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject, string targetObjectName) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component) return component;
        return gameObject.GetAllChildren()
            .Find(go => go.name == targetObjectName)
            .GetOrAddComponent<T>();
    }

    public static T GetOrAddComponentRecursive<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponentRecursive<T>();
        return component ? component : gameObject.AddComponent<T>();
    }

    public static T GetOrAddComponentRecursive<T>(this GameObject gameObject, string targetObjectName) where T : Component
    {
        var component = gameObject.GetComponentRecursive<T>();
        if (component) return component;
        return gameObject.GetAllChildren()
            .Find(go => go.name == targetObjectName)
            .GetOrAddComponent<T>();
    }

    public static T GetComponentRecursive<T>(this GameObject gameObject) where T : Component
    {
        return gameObject.GetComponentRecursive_Internal<T>();
    }

    public static bool TryGetComponentRecursive<T>(this GameObject gameObject, out T component) where T : Component
    {
        component = gameObject.GetComponentRecursive<T>();
        if (component) return true;
        return false;
    }

    public static T[] GetAllComponentsRecursive<T>(this GameObject gameObject) where T : Component
    {
        var components = gameObject.GetComponents<T>();
        var componentsToReturn = new List<T>(components);
        var allChildren = gameObject.GetAllChildren();
        foreach (var child in allChildren)
        {
            var childComponents = child.GetComponents<T>();
            componentsToReturn.AddRange(childComponents);
        }
        return componentsToReturn.ToArray();
    }

    private static T GetComponentRecursive_Internal<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component) return component;

        var allChildern = gameObject.GetChildren();
        foreach (var child in allChildern)
        {
            var foundComponent = child.GetComponentRecursive_Internal<T>();
            if (foundComponent)
            {
                return foundComponent;
            }
        }
        return null;
    }

    public static List<GameObject> GetChildren(this GameObject gameObject)
    {
        var children = new List<GameObject>();
        for (var i = 0; i < gameObject.transform.childCount; i++)
        {
            children.Add(gameObject.transform.GetChild(i).gameObject);
        }
        return children;
    }

    public static List<GameObject> GetAllChildren(this GameObject gameObject)
    {
        return gameObject.GetChildrenRecursive_Internal();
    }

    private static List<GameObject> GetChildrenRecursive_Internal(this GameObject gameObject)
    {
        var children = gameObject.GetChildren();
        var foundChildren = new List<GameObject>();
        foreach (var child in children)
        {
            foundChildren.AddRange(child.GetChildrenRecursive_Internal());
        }
        return children.Concat(foundChildren).ToList();
    }

    public static T GetComponentInChildren<T>(this GameObject gameObject, string gameObjectName) where T : Component
    {
        var allChildren = gameObject.GetComponentsInChildren<T>();
        foreach (var child in allChildren)
        {
            if (child.gameObject.name == gameObjectName)
            {
                return child;
            }
        }
        return default;
    }

    public static bool TryGetComponentInChildren<T>(this GameObject gameObject, string gameObjectName, out T component) where T : Component
    {
        component = gameObject.GetComponentInChildren<T>(gameObjectName);
        if (component) return true;
        return false;
    }

    public static GameObject GetChild(this GameObject gameObject, string gameObjectName)
    {
        var allChildren = gameObject.GetChildren();
        foreach (var child in allChildren)
        {
            if (child.gameObject.name == gameObjectName)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public static GameObject GetChildRecursive(this GameObject gameObject, string gameObjectName)
    {
        var allChildren = gameObject.GetAllChildren();
        foreach (var child in allChildren)
        {
            if (child.gameObject.name == gameObjectName)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public static bool TryGetChild(this GameObject gameObject, string gameObjectName, out GameObject gameObjectOut)
    {
        gameObjectOut = gameObject.GetChild(gameObjectName);
        if (gameObjectOut) return true;
        return false;
    }

    public static GameObject GetRootGameObject(this GameObject gameObject)
    {
        var parent = gameObject.transform.parent;
        var rootGameObject = gameObject;
        while (parent)
        {
            rootGameObject = parent.gameObject;
            parent = rootGameObject.transform.parent;
        }
        return rootGameObject;
    }
}