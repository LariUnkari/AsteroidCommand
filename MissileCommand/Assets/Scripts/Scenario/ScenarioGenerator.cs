using UnityEngine;
using System.Collections.Generic;

public class ScenarioGenerator
{
    public static ScenarioRound GenerateRound(int roundIndex, ScenarioPreset preset)
    {
        return new ScenarioRound(roundIndex, GenerateWaves(roundIndex, preset));
    }

    public static ScenarioWave[] GenerateWaves(int roundIndex, ScenarioPreset preset)
    {
        float t = preset.m_waveCountCurve.Evaluate(roundIndex / (preset.m_waveCountRoundMax - 1f));
        int waveCount = t < 1f ? Mathf.FloorToInt(Mathf.Lerp(preset.m_waveCountMin, preset.m_waveCountMax, t)) : preset.m_waveCountMax;

        ScenarioWave wave;
        ScenarioWave[] waves = new ScenarioWave[waveCount];
        float[] wavePickRolls = new float[waveCount];
        float[] waveEnemyCountRolls = new float[waveCount];

        t = preset.m_waveIntervalCurve.Evaluate(roundIndex / (preset.m_waveIntervalRoundMax - 1f));
        float waveInterval = Mathf.Lerp(preset.m_waveIntervalMin, preset.m_waveIntervalMax, t);

        Debug.Log(DebugUtilities.AddTimestampPrefix("Round " + (roundIndex + 1) + " will have " + waveCount + " waves in it, interval=" + waveInterval));

        float enemyWaveCountWeightTotal, enemySpeed;
        int i, p, waveCountToSpawnEnemyIn, enemyCountTotal, remainingEnemyCount;
        List<int> pickedWaveIndices;
        int[] pickedWaveEnemyCounts;
        ScenarioPreset.Enemy enemyPreset;

        ScenarioWave.Enemy[] waveEnemies;

        // TODO: Optimize this, it's stupid now
        for (int e = 0; e < preset.m_enemyPresets.Length; e++)
        {
            enemyPreset = preset.m_enemyPresets[e];
            
            if (roundIndex < enemyPreset.m_firstRoundToSpawn - 1)
                continue;

            // Determine how many enemies to spawn in total
            if (roundIndex == enemyPreset.m_firstRoundToSpawn - 1)
                enemyCountTotal = enemyPreset.m_spawnCountMin;
            else
            {
                t = enemyPreset.m_spawnCountCurve.Evaluate((roundIndex - (enemyPreset.m_firstRoundToSpawn - 1f)) / (enemyPreset.m_spawnCountRoundMax - enemyPreset.m_firstRoundToSpawn));
                enemyCountTotal = t < 1f ? Mathf.FloorToInt(Mathf.Lerp(enemyPreset.m_spawnCountMin, enemyPreset.m_spawnCountMax, t)) : enemyPreset.m_spawnCountMax;
            }

            Debug.Log(DebugUtilities.AddTimestampPrefix("Round " + (roundIndex + 1) + " will spawn a total of " + enemyCountTotal + " instances of enemy " + enemyPreset.m_enemyPrefab.name));
            
            // Roll for each wave after first to determine which ones to pick and the weight value for the enemy count
            for (i = 0; i < waveCount; i++)
            {
                // First wave is always picked
                wavePickRolls[i] = i == 0 ? 2f : Random.value * enemyPreset.m_waveProbabilityCurve.Evaluate((float)i / waveCount);
                waveEnemyCountRolls[i] = Random.value;
                //Debug.Log(DebugUtilities.AddTimestampPrefix("Round " + (roundIndex + 1) + " Wave[" + i + "] pick roll: " + wavePickRolls[i] + ", enemy count roll: " + waveEnemyCountRolls[i]));
            }
            
            waveCountToSpawnEnemyIn = Mathf.FloorToInt(enemyPreset.m_waveCountMultiplier * waveCount);
            pickedWaveIndices = new List<int>(waveCountToSpawnEnemyIn);
            pickedWaveEnemyCounts = new int[waveCountToSpawnEnemyIn];

            enemyWaveCountWeightTotal = 0f;

            // Gather the highest rolled indices of the picked waves
            for (i = 0; i < waveCountToSpawnEnemyIn; i++)
            {
                pickedWaveIndices.Add(0);

                t = 0f;
                for (p = 0; p < waveCount; p++)
                {
                    if (wavePickRolls[p] > t)
                    {
                        t = wavePickRolls[p];
                        pickedWaveIndices[i] = p;
                    }
                }

                //Debug.Log(DebugUtilities.AddTimestampPrefix("Wave[" + pickedWaveIndices[i] + "] picked, roll: " + wavePickRolls[pickedWaveIndices[i]]));

                // Add to enemy count weight total so the amount can be distributed properly
                enemyWaveCountWeightTotal += waveEnemyCountRolls[pickedWaveIndices[i]];

                // Strike down the best pick from the rolls so it won't be considered again
                wavePickRolls[pickedWaveIndices[i]] = -1f;
            }

            pickedWaveIndices.Sort(SortWaves);

            remainingEnemyCount = enemyCountTotal;

            Debug.Log(DebugUtilities.AddTimestampPrefix("Round " + (roundIndex + 1) + " will spawn these enemies in " + waveCountToSpawnEnemyIn + " waves"));

            // Calculate how many enemies to spawn in each chosen wave
            for (i = 0; i < waveCountToSpawnEnemyIn; i++)
            {
                p = pickedWaveIndices[i];

                // Get the wave in question or create it
                if (waves[p] == null)
                {
                    wave = new ScenarioWave(i, p * waveInterval);
                    waves[p] = wave;
                }
                else
                    wave = waves[p];

                // Last wave should have all the remaining enemies, otherwise floor to int from roll values
                if (i == waveCountToSpawnEnemyIn - 1)
                    pickedWaveEnemyCounts[i] = remainingEnemyCount;
                else
                {
                    pickedWaveEnemyCounts[i] = Mathf.Max(Mathf.FloorToInt(enemyCountTotal * waveEnemyCountRolls[p] / enemyWaveCountWeightTotal), 1);
                    remainingEnemyCount -= pickedWaveEnemyCounts[i];
                }

                t = Mathf.Pow(enemyPreset.m_speedMultiplierGain, roundIndex);
                if (enemyPreset.m_speedMultiplierMax > 1f)
                    t = Mathf.Clamp(t, 1f, enemyPreset.m_speedMultiplierMax);

                Debug.Log(DebugUtilities.AddTimestampPrefix("Wave[" + p + "] will spawn " + pickedWaveEnemyCounts[i] + " enemies of type " + enemyPreset.m_enemyPrefab.name + " with speed multiplier of " + t + " at time " + wave.WaveTime));

                // Generate the enemies in this wave
                waveEnemies = new ScenarioWave.Enemy[pickedWaveEnemyCounts[i]];

                for (p = 0; p < pickedWaveEnemyCounts[i]; p++)
                {
                    // Calculate the speed for each enemy individually
                    enemySpeed = t * Mathf.Lerp(enemyPreset.m_speedMin, enemyPreset.m_speedMax, enemyPreset.m_speedCurve.Evaluate(Random.value));
                    waveEnemies[p] = new ScenarioWave.Enemy(enemyPreset.m_enemyPrefab, enemyPreset.m_modelPrefab, enemySpeed);
                }

                wave.AddEnemies(waveEnemies);
            }
        }

        return waves;
    }

    private static int SortWaves(int a, int b)
    {
        if (a < b) return -1;
        if (a > b) return 1;
        return 0;
    }
}
