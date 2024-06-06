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
    private int spawnInterval = 1200; // 20 seconds in ticks (assuming 60 ticks per second)
    private Random random = new Random();

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
            SpawnRandomNPC();
            ticksSinceLastSpawn = 0; // Reset the tick counter
            SetRandomSpawnInterval(); // Set a new random spawn interval
        }
    }

    private void SpawnRandomNPC()
    {
        if (addonNPCs.Count == 0)
        {
            Notification.Show("No NPCs to spawn!");
            return;
        }

        // Choose a random NPC from the list
        string randomNPC = addonNPCs[random.Next(addonNPCs.Count)];

        Model model = new Model(randomNPC);
        if (model.IsValid && model.IsInCdImage)
        {
            model.Request();
            DateTime end = DateTime.Now + TimeSpan.FromSeconds(10);
            while (!model.IsLoaded && DateTime.Now < end)
            {
                Script.Wait(50);
            }

            if (model.IsLoaded)
            {
                // Get a random position around the player's position at ground level
                Vector3 spawnPosition = World.GetNextPositionOnStreet(Game.Player.Character.Position.Around(10f));

                // Spawn the NPC at the random ground-level position
                Ped ped = World.CreatePed(model, spawnPosition);
                ped.Task.WanderAround();

                model.MarkAsNoLongerNeeded(); // Clean up model to free memory
            }
            else
            {
                Notification.Show($"Failed to load model: {randomNPC}");
            }
        }
        else
        {
            Notification.Show($"Invalid model: {randomNPC}");
        }
    }

    private void SetRandomSpawnInterval()
    {
        // Generate a random spawn interval between 18 and 22 seconds (in ticks)
        spawnInterval = random.Next(1080, 1320); // Assuming 60 ticks per second
    }
}