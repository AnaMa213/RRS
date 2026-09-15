---
type: "query"
date: "2026-09-14T21:39:09.696692+00:00"
question: "Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle"
contributor: "graphify"
outcome: "useful"
source_nodes: ["LocalVehicleCameraRig", "RunFlowController", ".HandleRageTargetLockControls()", ".ApplyRageTargetLock()", ".ResolveNextInCycle()"]
---

# Q: Corriger la camera du lock rage pour rester au POV du joueur et faire de T un toggle, Y un cycle

## Answer

Expanded from original query via vocab: [camera, local, look, override, rage, target, lock, cycle, input, key, unlock, vehicle]. Le parcours relie RunFlowController.HandleRageTargetLockControls a LocalVehicleCameraRig.SetRageTargetLookOverride et AiRageTargetResolution.ResolveNextInCycle. L invariant retenu est de modifier seulement CinemachineCamera.LookAt tout en conservant Follow sur le vehicule local; T desactive maintenant le lock actif avant toute nouvelle resolution, Y conserve son chemin de cycle.

## Outcome

- Signal: useful

## Source Nodes

- LocalVehicleCameraRig
- RunFlowController
- .HandleRageTargetLockControls()
- .ApplyRageTargetLock()
- .ResolveNextInCycle()