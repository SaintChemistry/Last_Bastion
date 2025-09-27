using Godot;
using System;
using System.Collections.Generic;

public static class DebugCollision
{
    // Define your layer names in order (Godot supports up to 32 bits)
    private static readonly string[] LayerNames =
    {
        "Walls",       // 1 << 0 = 1
        "Cannons",     // 1 << 1 = 2
        "Enemies",     // 1 << 2 = 4
        "Projectiles"  // 1 << 3 = 8
        // Add more later if needed
    };

    public static void PrintCollision(Node node)
    {
        if (node is CollisionObject2D col)
        {
            string layers = BitsToNames(col.CollisionLayer);
            string masks = BitsToNames(col.CollisionMask);

            GD.Print($"{node.Name} → Layer: {col.CollisionLayer} [{layers}], " +
                     $"Mask: {col.CollisionMask} [{masks}]");
        }
        else
        {
            GD.Print($"{node.Name} → (not a CollisionObject2D)");
        }
    }

    private static string BitsToNames(uint bits)
    {
        List<string> names = new();
        for (int i = 0; i < LayerNames.Length; i++)
        {
            if ((bits & (1u << i)) != 0)
                names.Add(LayerNames[i]);
        }
        return names.Count > 0 ? string.Join(",", names) : "None";
    }
}
