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
    private List<string> addonAnimals = new List<string>();
    private int ticksSinceLastSpawnNPC = 0;
    private int ticksSinceLastSpawnAnimal = 0;
    private int spawnIntervalNPC = 1200; // 20 seconds in ticks (assuming 60 ticks per second)
    private int spawnIntervalAnimal = 1800; // 30 seconds in ticks (assuming 60 ticks per second)
    private Random random = new Random();

    public LoadAddonNPCs()
    {
        // Load the list of addon NPCs and animals from files
        LoadNPCsFromFile("scripts/addonNPCs.txt");
        LoadAnimalsFromFile("scripts/addonAnimals.txt");

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

    private void LoadAnimalsFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    addonAnimals.Add(line.Trim());
                }
            }
        }
        else
        {
            Notification.Show("Animal file not found!");
        }
    }

    private void OnTick(object sender, EventArgs e)
    {
        ticksSinceLastSpawnNPC++;
        ticksSinceLastSpawnAnimal++;

        // Check if enough ticks have passed to spawn another NPC
        if (ticksSinceLastSpawnNPC >= spawnIntervalNPC)
        {
            SpawnRandomNPC();
            ticksSinceLastSpawnNPC = 0; // Reset the tick counter
            SetRandomSpawnIntervalNPC(); // Set a new random spawn interval for NPCs
        }

        // Check if enough ticks have passed to spawn another animal
        if (ticksSinceLastSpawnAnimal >= spawnIntervalAnimal)
        {
            SpawnRandomAnimal();
            ticksSinceLastSpawnAnimal = 0; // Reset the tick counter
            SetRandomSpawnIntervalAnimal(); // Set a new random spawn interval for animals
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
                // Calculate a position in front of the player
                Vector3 spawnPosition = GetGroundPositionInFrontOfPlayer(3f);

                // Spawn the NPC at the calculated ground-level position
                Ped ped = World.CreatePed(model, spawnPosition);

                // Assign a random action to the NPC
                AssignRandomAction(ped);

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

    private void SpawnRandomAnimal()
    {
        if (addonAnimals.Count == 0)
        {
            Notification.Show("No animals to spawn!");
            return;
        }

        // Choose a random animal from the list
        string randomAnimal = addonAnimals[random.Next(addonAnimals.Count)];

        Model model = new Model(randomAnimal);
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
                // Calculate a position in front of the player
                Vector3 spawnPosition = GetGroundPositionInFrontOfPlayer(3f);

                // Spawn the animal at the calculated ground-level position
                Ped animal = World.CreatePed(model, spawnPosition);

                // Animals usually wander around
                animal.Task.WanderAround();

                model.MarkAsNoLongerNeeded(); // Clean up model to free memory
            }
            else
            {
                Notification.Show($"Failed to load model: {randomAnimal}");
            }
        }
        else
        {
            Notification.Show($"Invalid model: {randomAnimal}");
        }
    }

    private Vector3 GetGroundPositionInFrontOfPlayer(float distance)
    {
        Vector3 playerPosition = Game.Player.Character.Position;
        Vector3 forwardVector = Game.Player.Character.ForwardVector;
        Vector3 positionInFront = playerPosition + forwardVector * distance;

        // Ensure the position is on the ground
        Vector3 groundPosition = World.GetGroundHeight(positionInFront) != 0 
            ? new Vector3(positionInFront.X, positionInFront.Y, World.GetGroundHeight(positionInFront))
            : positionInFront;

        return groundPosition;
    }

    private void AssignRandomAction(Ped ped)
    {
        int action = random.Next(4); // Generate a random number between 0 and 3

        switch (action)
        {
            case 0:
                ped.Task.WanderAround();
                break;
            case 1:
                ped.Task.StandStill(-1); // Stand still indefinitely
                break;
            case 2:
                ped.Task.ReactAndFlee(Game.Player.Character);
                break;
            case 3:
                if (!ped.IsPlayer) // Ensure the NPC is not the player
                {
                    ped.Task.FightAgainst(Game.Player.Character);
                }
                break;
        }
    }

    private void SetRandomSpawnIntervalNPC()
    {
        // Generate a random spawn interval between 18 and 22 seconds (in ticks)
        spawnIntervalNPC = random.Next(1080, 1320); // Assuming 60 ticks per second
    }

    private void SetRandomSpawnIntervalAnimal()
    {
        // Generate a random spawn interval between 25 and 35 seconds (in ticks)
        spawnIntervalAnimal = random.Next(1500, 2100); // Assuming 60 ticks per second
    }
}
