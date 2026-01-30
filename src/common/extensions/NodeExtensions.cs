using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public static class NodeExtensions
{
    public static T GetComponentInParent<T>(this Node node) where T : class
    {
        Node current = node;
        while (current != null)
        {
            if (current is T found)
            {
                return found;
            }
            current = current.GetParent();
        }
        return null;
    }
}