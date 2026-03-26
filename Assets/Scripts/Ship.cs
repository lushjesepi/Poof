using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Tracks multi-stage repair progress for a broken ship.
/// Each stage costs resources from the provided player <see cref="Inventory"/>.
/// </summary>
[DisallowMultipleComponent]
public class Ship : MonoBehaviour
{
    [Serializable]
    public class ResourceRequirement
    {
        public ResourceType type;
        public int amount = 0;
    }

    [Serializable]
    public class RepairStage
    {
        [Tooltip("Optional label shown in debug/logs.")] public string stageName;
        public List<ResourceRequirement> requiredResources = new List<ResourceRequirement>();
    }

    [Header("Repair Stages")]
    [Tooltip("Stage 0 is the first repair. Progress advances one stage per successful repair attempt.")]
    [SerializeField] private List<RepairStage> repairStages = new List<RepairStage>();

    [Tooltip("Which repair stage the ship is currently on (0 = broken stage 0, repairStageIndex + 1 = next to complete).")]
    [SerializeField] private int repairStageIndex = 0;

    [Header("Optional Visuals (for setup convenience)")]
    [Tooltip("If set, visuals are toggled based on repairStageIndex: index 0 shows when broken, index N shows when fully repaired.")]
    [SerializeField] private GameObject[] stageVisuals;

    [Header("Events")]
    public UnityEvent OnStageRepaired;
    public UnityEvent OnShipRepaired;

    public int CurrentStageIndex => repairStageIndex;
    public int TotalStages => repairStages != null ? repairStages.Count : 0;
    public bool IsFullyRepaired => repairStageIndex >= repairStages.Count;

    private void Start()
    {
        ClampRepairIndex();
        ApplyStageVisuals();
    }

    /// <summary>
    /// Returns true if the provided inventory has all required resources for the current stage.
    /// </summary>
    public bool CanRepair(Inventory playerInventory)
    {
        if (playerInventory == null)
            return false;

        if (IsFullyRepaired)
            return false;

        RepairStage stage = GetCurrentStage();
        if (stage == null)
            return false;

        foreach (var req in stage.requiredResources)
        {
            if (req == null)
                continue;

            if (!playerInventory.HasResource(req.type, req.amount))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Attempts to repair the ship by consuming the current stage's resources.
    /// If successful, advances <see cref="CurrentStageIndex"/> by 1 and updates visuals.
    /// </summary>
    public bool TryRepair(Inventory playerInventory)
    {
        if (!CanRepair(playerInventory))
            return false;

        RepairStage stage = GetCurrentStage();
        if (stage == null)
            return false;

        // Consume resources only after all checks pass.
        foreach (var req in stage.requiredResources)
        {
            if (req == null)
                continue;

            // HasResource(...) above guarantees enough resources; RemoveResource should succeed.
            playerInventory.RemoveResource(req.type, req.amount);
        }

        repairStageIndex++;
        ApplyStageVisuals();

        OnStageRepaired?.Invoke();
        if (IsFullyRepaired)
            OnShipRepaired?.Invoke();

        return true;
    }

    public RepairStage GetCurrentStage()
    {
        if (IsFullyRepaired)
            return null;
        if (repairStageIndex < 0 || repairStageIndex >= repairStages.Count)
            return null;
        return repairStages[repairStageIndex];
    }

    private void ClampRepairIndex()
    {
        if (repairStages == null)
            repairStages = new List<RepairStage>();

        repairStageIndex = Mathf.Max(0, repairStageIndex);

        // If the serialized index is beyond bounds, treat it as already fully repaired.
        if (repairStages.Count == 0)
            repairStageIndex = 0;
        else if (repairStageIndex > repairStages.Count)
            repairStageIndex = repairStages.Count;
    }

    private void ApplyStageVisuals()
    {
        if (stageVisuals == null || stageVisuals.Length == 0)
            return;

        // Expected mapping:
        // stageVisuals[0] = broken visuals for stageIndex = 0
        // stageVisuals[N] = fully repaired visuals (where N = repairStages.Count)
        int maxVisualIndex = repairStages.Count;
        int visualIndex = Mathf.Clamp(repairStageIndex, 0, maxVisualIndex);

        for (int i = 0; i < stageVisuals.Length; i++)
        {
            if (stageVisuals[i] == null)
                continue;

            stageVisuals[i].SetActive(i == visualIndex);
        }
    }

    private void Reset()
    {
        // Provide a sensible default for the first repair stage.
        // You can edit/extend additional stages in the Inspector.
        if (repairStages == null)
            repairStages = new List<RepairStage>();

        if (repairStages.Count == 0)
        {
            repairStages.Add(new RepairStage
            {
                stageName = "Repair Stage 1",
                requiredResources = new List<ResourceRequirement>
                {
                    new ResourceRequirement { type = ResourceType.Wood, amount = 10 },
                    new ResourceRequirement { type = ResourceType.Stone, amount = 8 },
                    new ResourceRequirement { type = ResourceType.Iron, amount = 5 },
                }
            });

            repairStages.Add(new RepairStage
            {
                stageName = "Repair Stage 2",
                requiredResources = new List<ResourceRequirement>
                {
                    new ResourceRequirement { type = ResourceType.Wood, amount = 8 },
                    new ResourceRequirement { type = ResourceType.Stone, amount = 10 },
                    new ResourceRequirement { type = ResourceType.Iron, amount = 6 },
                }
            });

            repairStages.Add(new RepairStage
            {
                stageName = "Repair Stage 3",
                requiredResources = new List<ResourceRequirement>
                {
                    new ResourceRequirement { type = ResourceType.Wood, amount = 12 },
                    new ResourceRequirement { type = ResourceType.Stone, amount = 14 },
                    new ResourceRequirement { type = ResourceType.Iron, amount = 8 },
                }
            });
        }
    }
}

