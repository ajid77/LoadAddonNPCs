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
    private int ticksSinceLastSpawn = 0;
    private const int spawnInterval = 600; // Adjust this value to change spawn frequency (in ticks)

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
        ticksSinceLastSpawn++;

        // Check if enough ticks have passed to spawn another NPC
        if (ticksSinceLastSpawn >= spawnInterval)
        {
            foreach (var npc in addonNPCs)
            {
                Model model = new Model(npc);
                if (model.IsValid && model.IsInCdImage)
                {
                    model.Request();
                    while (!model.IsLoaded) Script.Wait(100);

                    // Spawn the NPC at a random location near the player
                    Ped ped = World.CreatePed(model, Game.Player.Character.Position.Around(5f));
                    ped.Task.WanderAround();
                }
            }

            // Reset the ticks counter
            ticksSinceLastSpawn = 0;
        }
    }
}
