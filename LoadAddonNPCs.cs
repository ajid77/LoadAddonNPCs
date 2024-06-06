using System;
using System.Collections.Generic;
using System.IO;
using GTA;
using GTA.Native;
using GTA.Math;
using GTA.UI;

public class LoadAddonNPCs : Script
{
    private List<string> addonNPCs = new List<string>();

    public LoadAddonNPCs()
    {
        // Load the list of addon NPCs from a file
        LoadNPCsFromFile("scripts/addonNPCs.txt");

        // Hook into the Tick event
        Tick += OnTick;
    }

    private void LoadNPCsFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    addonNPCs.Add(line.Trim());
                }
            }
        }
        else
        {
            Notification.Show("NPC file not found!");
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        foreach (var npc in addonNPCs)
        {
            Model model = new Model(npc);
            if (model.IsValid && model.IsInCdImage)
            {
                model.Request();
                while (!model.IsLoaded) Script.Wait(100);

                // Spawn the NPC at a random location near the player
                Ped ped = World.CreatePed(model, Game.Player.Character.Position + new Vector3(0, 5, 0));
                ped.Task.WanderAround();
            }
        }

        // Clear the list so NPCs are not spawned repeatedly
        addonNPCs.Clear();
    }
}
