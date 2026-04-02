using Godot;
using KitchenDesigner.Features.Kitchen.Interfaces;
using System.Collections.Generic;

public static class KitchenExporter
{
    public static void ExportScene(Node currentScene, string fileName = "navrh_kuchyne.glb")
    {
        List<IKitchenComponent> componentsToExport = new List<IKitchenComponent>();
        FindKitchenComponents(currentScene, componentsToExport);

        if (componentsToExport.Count == 0)
        {
            GD.Print("Nebyly nalezeny žádné kuchyňské komponenty k exportu.");
            return;
        }

        Node3D exportRoot = new Node3D();
        exportRoot.Name = "KitchenExport";

        // 1. EXTRAKCE ČISTÝCH MESHŮ
        foreach (var component in componentsToExport)
        {
            if (component is Node3D componentNode && componentNode.IsVisibleInTree())
            {
                ExtractCleanMeshes(componentNode, exportRoot);
            }
        }

        // 2. VÝPOČET STŘEDU A NEJNIŽŠÍHO BODU
        Aabb totalBounds = new Aabb();
        bool isFirst = true;

        foreach (Node child in exportRoot.GetChildren())
        {
            if (child is MeshInstance3D mesh)
            {
                Aabb localAabb = mesh.GetAabb();
                if (localAabb.Size == Vector3.Zero) continue;

                Aabb worldAabb = mesh.Transform * localAabb;

                if (isFirst)
                {
                    totalBounds = worldAabb;
                    isFirst = false;
                }
                else
                {
                    totalBounds = totalBounds.Merge(worldAabb);
                }
            }
        }

        // 3. POSUNUTÍ MODELU NA NULU (Vycentrování a přichycení k podlaze)
        if (totalBounds.Size != Vector3.Zero)
        {
            Vector3 centerOffset = new Vector3(
                totalBounds.GetCenter().X,
                totalBounds.Position.Y,
                totalBounds.GetCenter().Z
            );

            foreach (Node child in exportRoot.GetChildren())
            {
                if (child is Node3D node3D)
                {
                    node3D.Position -= centerOffset;
                }
            }
        }

        // 4. EXPORT DO GLTF
        GltfDocument gltfDoc = new GltfDocument();
        GltfState gltfState = new GltfState();

        Error err = gltfDoc.AppendFromScene(exportRoot, gltfState);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Chyba při generování GLTF: {err}");
            exportRoot.QueueFree();
            return;
        }

        // 5. ULOŽENÍ
        string downloadsPath = OS.GetSystemDir(OS.SystemDir.Downloads);
        if (string.IsNullOrEmpty(downloadsPath)) downloadsPath = "user://";

        string fullPath = $"{downloadsPath}/{fileName}";
        err = gltfDoc.WriteToFilesystem(gltfState, fullPath);

        if (err == Error.Ok)
        {
            GD.Print($"Export byl úspěšný! Uloženo v: {fullPath}");
        }
        else
        {
            GD.PrintErr($"Chyba při zápisu souboru: {err}");
        }

        exportRoot.QueueFree();
    }

    /// <summary>
    /// Rekurzivně projde komponentu a vytvoří "hloupé" kopie pouze pro její viditelné 3D modely.
    /// Ignoruje veškeré skripty, kolize, markery a oblasti.
    /// </summary>
    private static void ExtractCleanMeshes(Node node, Node3D exportRoot)
    {
        if (node is MeshInstance3D originalMesh && originalMesh.IsVisibleInTree() && originalMesh.Mesh != null)
        {
            MeshInstance3D cleanMesh = new MeshInstance3D();
            cleanMesh.Name = originalMesh.Name;
            cleanMesh.Mesh = originalMesh.Mesh;
            cleanMesh.MaterialOverride = originalMesh.MaterialOverride;

            for (int i = 0; i < originalMesh.GetSurfaceOverrideMaterialCount(); i++)
            {
                cleanMesh.SetSurfaceOverrideMaterial(i, originalMesh.GetSurfaceOverrideMaterial(i));
            }

            exportRoot.AddChild(cleanMesh);

            cleanMesh.Transform = originalMesh.GlobalTransform;
        }

        foreach (Node child in node.GetChildren())
        {
            ExtractCleanMeshes(child, exportRoot);
        }
    }

    private static void FindKitchenComponents(Node currentNode, List<IKitchenComponent> foundComponents)
    {
        if (currentNode is IKitchenComponent component)
        {
            foundComponents.Add(component);
            return;
        }

        foreach (Node child in currentNode.GetChildren())
        {
            FindKitchenComponents(child, foundComponents);
        }
    }
}